import { URL } from "url";
import got, { HTTPError } from "got";
import SAXParser, { StartTagToken } from "parse5-sax-parser";
import normalizeUrl from "normalize-url";
import { UniqueStack } from "../utils/UniqueStack";
import { delay } from "../utils/promise";
import { fastCrawlerLogger as logger } from "../utils/loggers";
import { getHost } from "../utils/url";

interface FastCrawlerOptions {
  links: string[];
  /** @default 10 */
  concurrencyLevel?: number;
  /** @default 1000 */
  retryDelayMsec?: number;
  /** @default 20000 */
  requestTimeoutMsec?: number;
  outgoingHosts?: string[];
  handlePage?: (url: string) => Promise<void>;
  handleFile?: (url: string) => Promise<void>;
}

const noop = () => Promise.resolve();

class Parser extends SAXParser {
  isSkipped = false;
  url: URL = null;
  fileLinks: string[] = [];
  resolve: () => void;
  reject: (error: Error) => void;

  skip() {
    this.isSkipped = true;
    this.stop();
    this.resolve();
  }

  throw(error: Error) {
    this.stop();
    this.reject(error);
  }
}

export class FastCrawler {
  private _handlePage: (url: string) => Promise<void>;
  private _handleFile: (url: string) => Promise<void>;
  private _visitedLinks: Set<string>;
  private _linkStack: UniqueStack<string>;
  private _hosts: Set<string>;
  private _outgoingHosts: Set<string>;
  private _tasks: Promise<number>[];
  private _retryDelayMsec: number;
  private _requestTimeoutMsec: number;

  static async launch(options: FastCrawlerOptions) {
    const crawler = new FastCrawler();
    await crawler._init(options);
    await crawler._crawl();
  }

  private async _init({
    links,
    outgoingHosts = [],
    concurrencyLevel = 10,
    retryDelayMsec = 1000,
    requestTimeoutMsec = 20000,
    handlePage = noop,
    handleFile = noop
  }: FastCrawlerOptions) {
    this._handlePage = handlePage;
    this._handleFile = handleFile;
    this._linkStack = new UniqueStack<string>(...links);
    this._hosts = new Set(links.map(getHost));
    this._outgoingHosts = new Set(links.concat(outgoingHosts).map(getHost));
    this._visitedLinks = new Set();
    this._retryDelayMsec = retryDelayMsec;
    this._requestTimeoutMsec = requestTimeoutMsec;
    // populate pool by resolved tasks
    this._tasks = [...Array(concurrencyLevel)].map(async (_, i) => i);
  }

  private async _crawl() {
    let currentLink = this._linkStack.pop();
    while (currentLink) {
      while (currentLink) {
        // wait for some task and replace it in pool
        const jobIndex = await Promise.race(this._tasks);
        this._tasks[jobIndex] = this._evaluatePage(jobIndex, currentLink);
        currentLink = this._linkStack.pop();
      }
      // wait for remaining tasks
      await Promise.all(this._tasks);
      // last tasks can push new links to the linkStack
      currentLink = this._linkStack.pop();
    }
  }

  private async _evaluatePage(jobIndex: number, currentLink: string, retriesLeft = 2) {
    try {
      logger.info({ jobIndex, href: currentLink }, "Parsing page");

      const parser = new Parser();
      parser.on("startTag", tag => {
        try {
          switch (tag.tagName) {
            case "meta":
              return this._evaluateMeta(tag, parser);
            case "link":
              return this._evaluateLink(tag, parser);
            case "a":
              return this._evaluateAnchor(tag, parser);
          }
        } catch (err) {
          logger.warn({ err, tag, href: currentLink }, `Unable to parse tag`);
        }
      });

      // We should check real parser.url because it can differ from currentLink
      // due to HTTP redirects. So in logs we can see that some page link is
      // already handled before FastCrawler visits it's currentLink
      await new Promise((resolve, reject) => {
        parser.resolve = resolve;
        parser.reject = reject;

        got
          .stream(currentLink, { timeout: this._requestTimeoutMsec })
          .setEncoding("utf8")
          .on("response", response => {
            parser.url = new URL(normalizeUrl(response.url));
            if (
              response.headers["content-type"].startsWith("text/html") &&
              this._hosts.has(parser.url.host) &&
              !this._visitedLinks.has(String(parser.url))
            ) {
              this._visitedLinks.add(String(parser.url));
            } else {
              parser.skip();
            }
          })
          .on("end", resolve)
          .on("error", error => {
            parser.throw(error);
          })
          .pipe(parser);
      });

      logger.info(
        { jobIndex, href: currentLink, isSkipped: parser.isSkipped },
        "Parsing completed"
      );

      if (!parser.isSkipped) {
        // parser.url can differ from currentLink due to HTTP redirects
        await this._processResults(String(parser.url), parser.fileLinks);
      }
    } catch (err) {
      logger.warn(
        { err, jobIndex, href: currentLink, retriesLeft },
        `Browser error during page parsing`
      );

      if (
        retriesLeft > 0 &&
        !(err instanceof HTTPError && err.statusCode >= 400 && err.statusCode < 500)
      ) {
        await delay(this._retryDelayMsec);
        await this._evaluatePage(jobIndex, currentLink, retriesLeft - 1);
      }
    }
    return jobIndex;
  }

  /**
   * ```html
   * <meta name="robots" content="nofollow" />
   * ```
   */
  private _evaluateMeta(tag: StartTagToken, parser: Parser) {
    const nameAttr = tag.attrs.find(attr => attr.name === "name");
    if (nameAttr && nameAttr.value.toLowerCase() === "robots") {
      const contentAttr = tag.attrs.find(attr => attr.name === "content");
      if (contentAttr && contentAttr.value.toLowerCase() === "nofollow") {
        parser.skip();
      }
    }
  }

  /**
   * ```html
   * <link rel="canonical" href="https://mysite.com" />
   * ```
   */
  private _evaluateLink(tag: StartTagToken, parser: Parser) {
    const relAttr = tag.attrs.find(attr => attr.name === "rel");
    if (relAttr && relAttr.value.toLowerCase() === "canonical") {
      const hrefAttr = tag.attrs.find(attr => attr.name === "href");
      if (hrefAttr) {
        let link: string = hrefAttr.value.toLowerCase();
        if (link.startsWith("//")) {
          console.error(link);
          link = parser.url.protocol + link;
        } else if (link.startsWith("/") || link.startsWith(".")) {
          link = String(new URL(link, parser.url));
        }
        if (link.startsWith("https://") || link.startsWith("http://")) {
          link = normalizeUrl(link);
          const url = new URL(link);
          if (this._hosts.has(url.host)) {
            parser.url = url;
          }
        }
      }
    }
  }

  /**
   * ```html
   * <a href="https://mysite.com"></a>
   * <a rel="nofollow" href="https://mysite.com"></a>
   * ```
   */
  private _evaluateAnchor(tag: StartTagToken, parser: Parser) {
    const relAttr = tag.attrs.find(attr => attr.name === "rel");
    if (relAttr && relAttr.value.toLowerCase() === "nofollow") return;

    const hrefAttr = tag.attrs.find(attr => attr.name === "href");
    if (hrefAttr) {
      let link: string = hrefAttr.value.toLowerCase();
      if (link.startsWith("//")) {
        console.error(link);
        link = parser.url.protocol + link;
      } else if (link.startsWith("/") || link.startsWith(".")) {
        link = String(new URL(link, parser.url));
      }
      if (link.startsWith("https://") || link.startsWith("http://")) {
        link = normalizeUrl(link);
        const { host, pathname } = new URL(link);
        if (/\.[a-z]+$/i.test(pathname)) {
          parser.fileLinks.push(link.split("#")[0]);
        } else if (this._outgoingHosts.has(host)) {
          this._linkStack.push(link.split("#")[0]);
        }
      }
    }
  }

  private async _processResults(pageLink: string, fileLinks: string[]) {
    try {
      await this._handlePage(pageLink);
    } catch (err) {
      logger.error({ err, pageLink }, "Error handling page");
    }
    await Promise.all(
      fileLinks.map(async fileLink => {
        try {
          await this._handleFile(fileLink);
        } catch (err) {
          logger.error({ err, pageLink, fileLink }, "Error handling file");
        }
      })
    );
  }
}
