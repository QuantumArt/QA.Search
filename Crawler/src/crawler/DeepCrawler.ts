import { URL } from "url";
import puppeteer, { Page, Browser, ResourceType } from "puppeteer";
import { configurePage } from "../utils/page";
import normalizeUrl from "normalize-url";
import { HttpStatusError, EmptyResponseError } from "../utils/error";
import { UniqueStack } from "../utils/UniqueStack";
import { delay } from "../utils/promise";
import { onShutdown } from "../utils/process";
import { deepCrawlerLogger as logger } from "../utils/loggers";
import { getHost } from "../utils/url";

interface DeepCrawlerOptions {
  links: string[];
  /** @default 1 */
  concurrencyLevel?: number;
  /** @default 1000 */
  retryDelayMsec?: number;
  /** @default 10000 */
  navigationTimeoutMsec?: number;
  outgoingHosts?: string[];
  blockedResources?: ResourceType[];
  handlePage?: (page: Page) => Promise<void>;
  handleFile?: (url: string) => Promise<void>;
}

const noop = () => Promise.resolve();

export class DeepCrawler {
  private _handlePage: (page: Page) => Promise<void>;
  private _handleFile: (url: string) => Promise<void>;
  private _visitedLinks: Set<string>;
  private _linkStack: UniqueStack<string>;
  private _hosts: Set<string>;
  private _outgoingHosts: Set<string>;
  private _tasks: Promise<number>[];
  private _pages: Page[];
  private _browser: Browser;
  private _retryDelayMsec: number;
  private _unsubscribeFromShutdown: () => void;

  static async launch(options: DeepCrawlerOptions) {
    const crawler = new DeepCrawler();
    await crawler._init(options);
    try {
      await crawler._crawl();
    } finally {
      await crawler._dispose();
    }
  }

  private async _init({
    links,
    outgoingHosts = [],
    concurrencyLevel = 1,
    retryDelayMsec = 1000,
    navigationTimeoutMsec = 10000,
    handlePage = noop,
    handleFile = noop,
    blockedResources = []
  }: DeepCrawlerOptions) {
    this._handlePage = handlePage;
    this._handleFile = handleFile;
    this._linkStack = new UniqueStack<string>(...links);
    this._hosts = new Set(links.map(getHost));
    this._outgoingHosts = new Set(links.concat(outgoingHosts).map(getHost));
    this._visitedLinks = new Set();
    this._retryDelayMsec = retryDelayMsec;
    // populate pool by resolved tasks
    this._tasks = [...Array(concurrencyLevel)].map(async (_, i) => i);
    this._browser = await puppeteer.launch({
      args: ["--no-sandbox", "--disable-setuid-sandbox"]
    });
    this._pages = await Promise.all(
      [...Array(concurrencyLevel)].map(async () => {
        const page = await this._browser.newPage();
        page.setDefaultNavigationTimeout(navigationTimeoutMsec);
        await configurePage(page, blockedResources);
        return page;
      })
    );
    this._unsubscribeFromShutdown = onShutdown(() => this._dispose());
  }

  private async _dispose() {
    this._unsubscribeFromShutdown();
    await Promise.all(this._pages.map(page => page.close()));
    await this._browser.close();
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
    const page = this._pages[jobIndex];
    try {
      logger.info({ jobIndex, href: currentLink }, "Loading page");

      const response = await page.goto(currentLink);

      const pageUrl = normalizeUrl(page.url());

      logger.info({ jobIndex, href: pageUrl }, "Page loaded");

      if (response) {
        if (response.ok()) {
          // filter out redirects to other hosts
          if (this._hosts.has(getHost(pageUrl)) && !this._visitedLinks.has(pageUrl)) {
            this._visitedLinks.add(pageUrl);

            if (await this._checkRobotsNofollow(page)) {
              return jobIndex;
            }

            await this._processCanonicalLink(page);

            const fileLinks: string[] = [];

            await this._processAnchorLinks(page, fileLinks);

            await this._processResults(page, fileLinks);

            logger.info({ jobIndex, href: pageUrl }, "Page handled");
          }
        } else {
          throw new HttpStatusError(response.status());
        }
      } else {
        throw new EmptyResponseError();
      }
    } catch (err) {
      logger.warn(
        { err, jobIndex, href: currentLink, retriesLeft },
        `Browser error during page parsing`
      );

      if (
        retriesLeft > 0 &&
        !(err instanceof HttpStatusError && err.statusCode >= 400 && err.statusCode < 500)
      ) {
        await delay(this._retryDelayMsec);
        await this._evaluatePage(jobIndex, currentLink, retriesLeft - 1);
      }
    }
    return jobIndex;
  }

  private async _checkRobotsNofollow(page: Page) {
    return await page.evaluate(() => {
      const meta = document.querySelector<HTMLMetaElement>("meta[name=robots]");
      return meta && meta.content === "nofollow";
    });
  }

  private async _processCanonicalLink(page: Page) {
    let canonical = await page.evaluate(() => {
      const link = document.querySelector<HTMLLinkElement>("link[rel=canonical]");
      return link && link.href;
    });

    if (canonical && (canonical.startsWith("https://") || canonical.startsWith("http://"))) {
      canonical = normalizeUrl(canonical);
      if (this._hosts.has(getHost(canonical))) {
        await page.evaluate(canonical => {
          window.history.replaceState(
            window.history.state,
            document.title,
            canonical.split("#")[0]
          );
        }, canonical);
      }
    }
  }

  private async _processAnchorLinks(page: Page, fileLinks: string[]) {
    // collect all links except nofollow
    const links: string[] = await page.evaluate(() =>
      [...document.querySelectorAll<HTMLAnchorElement>("a[href]:not([rel=nofollow])")].map(
        a => a.href
      )
    );

    // split links to pages and files
    links.forEach(link => {
      if (link && (link.startsWith("https://") || link.startsWith("http://"))) {
        link = normalizeUrl(link);
        const { host, pathname } = new URL(link);
        if (/\.[a-z]+$/i.test(pathname)) {
          fileLinks.push(link.split("#")[0]);
        } else if (this._outgoingHosts.has(host)) {
          this._linkStack.push(link.split("#")[0]);
        }
      }
    });
  }

  private async _processResults(page: Page, fileLinks: string[]) {
    const pageUrl = normalizeUrl(page.url());
    try {
      await this._handlePage(page);
    } catch (err) {
      logger.error({ err, pageLink: pageUrl }, "Error handling page");
    }
    await Promise.all(
      fileLinks.map(async fileLink => {
        try {
          await this._handleFile(fileLink);
        } catch (err) {
          logger.error({ err, pageLink: pageUrl, fileLink }, "Error handling file");
        }
      })
    );
  }
}
