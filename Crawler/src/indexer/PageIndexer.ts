import puppeteer, { Page, Browser, ResourceType } from "puppeteer";
import { configurePage } from "../utils/page";
import { HttpStatusError, EmptyResponseError } from "../utils/error";
import { delay } from "../utils/promise";
import { onShutdown } from "../utils/process";
import { pageIndexerLogger as logger } from "../utils/loggers";

interface PageIndexerOptions {
  links: string[];
  /** @default 1 */
  concurrencyLevel?: number;
  /** @default 1000 */
  retryDelayMsec?: number;
  /** @default 10000 */
  navigationTimeoutMsec?: number;
  blockedResources?: ResourceType[];
  lockPage?: (currentLink: string) => Promise<boolean> | boolean;
  handlePage?: (currentLink: string, page: Page) => Promise<void> | void;
  handleError?: (currentLink: string, error: Error) => Promise<void> | void;
}

const noop = () => {};

export class PageIndexer {
  private _lockPage: (currentLink: string) => Promise<boolean> | boolean;
  private _handlePage: (currentLink: string, page: Page) => Promise<void> | void;
  private _handleError: (currentLink: string, error: Error) => Promise<void> | void;
  private _links: string[];
  private _tasks: Promise<number>[];
  private _pages: Page[];
  private _browser: Browser;
  private _retryDelayMsec: number;
  private _unsubscribeFromShutdown: () => void;

  static async launch(options: PageIndexerOptions) {
    const indexer = new PageIndexer();
    await indexer._init(options);
    try {
      await indexer._index();
    } finally {
      await indexer._dispose();
    }
  }

  private async _init({
    links,
    concurrencyLevel = 1,
    retryDelayMsec = 1000,
    navigationTimeoutMsec = 10000,
    lockPage = () => true,
    handlePage = noop,
    handleError = noop,
    blockedResources = []
  }: PageIndexerOptions) {
    this._links = links;
    this._lockPage = lockPage;
    this._handlePage = handlePage;
    this._handleError = handleError;
    this._retryDelayMsec = retryDelayMsec;
    // populate pool by resolved tasks
    this._tasks = [...Array(concurrencyLevel)].map(async (_, i) => i);
    this._browser = await puppeteer.launch({
      args: [
        "--no-sandbox",
        "--disable-setuid-sandbox",
        "--disable-gpu",
        "--disable-web-security",
        "--disable-dev-profile"
      ]
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

  private async _index() {
    for (const currentLink of this._links) {
      // wait for some task and replace it in pool
      const jobIndex = await Promise.race(this._tasks);
      this._tasks[jobIndex] = this._evaluate(jobIndex, currentLink);
    }
    // wait for remaining tasks
    await Promise.all(this._tasks);
  }

  private async _evaluate(jobIndex: number, currentLink: string) {
    if (await this._lockPage(currentLink)) {
      logger.info({ jobIndex, href: currentLink }, `Lock page`);
      await this._tryEvaluate(jobIndex, currentLink);
    }
    return jobIndex;
  }

  private async _tryEvaluate(jobIndex: number, currentLink: string, retriesLeft = 2) {
    const page = this._pages[jobIndex];
    try {
      logger.info({ jobIndex, href: currentLink, retriesLeft }, `Parsing page`);

      const response = await page.goto(currentLink);
      if (response) {
        if (response.ok()) {
          try {
            await this._handlePage(currentLink, page);
          } catch (err) {
            logger.warn(
              { err, jobIndex, href: currentLink, retriesLeft },
              `Unknown error during page indexing`
            );
          }
        } else {
          throw new HttpStatusError(response.status());
        }
      } else {
        throw new EmptyResponseError();
      }

      logger.info({ jobIndex, href: currentLink, retriesLeft }, `Parsing completed`);
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
        await this._tryEvaluate(jobIndex, currentLink, retriesLeft - 1);
      } else {
        try {
          await this._handleError(currentLink, err);
        } catch (err) {
          logger.error(
            { err, jobIndex, href: currentLink },
            `Unhandled error in handleError callback`
          );
        }
      }
    }
  }
}
