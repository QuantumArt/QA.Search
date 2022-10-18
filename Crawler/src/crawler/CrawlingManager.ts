import { delay } from "../utils/promise";
import { Domain, Route, Link } from "../entities";
import bunyan from "bunyan";
import url from "url";
import { getRepository } from "typeorm";

export abstract class CrawlingManager {
  private _queueCheckInterval: number;
  protected _logger: bunyan;

  constructor(queueCheckInterval: number, logger: bunyan) {
    this._queueCheckInterval = queueCheckInterval;
    this._logger = logger;
  }

  async crawlUntilCancelled() {
    while (true) {
      try {
        await this.crawlSingleDomain();

        const queueLength = await this.getQueueLength();
        this._logger.info(`Queue length: ${queueLength}`);

        if (queueLength === 0) {
          this._logger.info(`Sleep for ${Math.round(this._queueCheckInterval / 1000)} sec`);
          await delay(this._queueCheckInterval);
        }
      } catch (err) {
        this._logger.error({ err }, `Unknown error in crawlUntilCancelled loop`);
        this._logger.info(`Sleep for ${Math.round(this._queueCheckInterval / 1000)} sec`);
        await delay(this._queueCheckInterval);
      }
    }
  }

  private async crawlSingleDomain() {
    const domain = await this.getQueuedDomain();

    if (!domain) return;

    const startCrawlingUtc = new Date();

    if (domain.domainGroup.routes.length > 0) {
      const sortedRoutes = domain.domainGroup.routes.slice().sort(Route.compare);
      this._logger.info({ href: domain.origin }, `Start crawling domain ${domain.origin}`);
      await this.crawlDomain(domain.origin, sortedRoutes);
      this._logger.info({ href: domain.origin }, `Finish crawling domain ${domain.origin}`);
    }

    await this.updateDomainLastCrawlingDate(domain, startCrawlingUtc);
  }

  protected async handlePage(href: string, routes: Route[]) {
    const { pathname } = url.parse(href, true);
    const route = routes.find(route => route.pattern.test(pathname));

    if (!route) {
      this._logger.info({ href }, "No matching routes found");
      return;
    }

    const repository = getRepository(Link);
    const link = await repository.findOne({ hash: Link.getHash(href), url: href });

    if (!link) {
      await repository.save(
        new Link({
          url: href,
          nextIndexingUtc: new Date(),
          isActive: true
        })
      );
      this._logger.info({ href }, "Created new link");
    } else if (!link.isActive) {
      link.isActive = true;
      await repository.save(link);
      this._logger.info({ href }, "Activated link");
    } else {
      this._logger.info({ href }, "Active link already exists");
    }
  }

  protected abstract async getQueueLength(): Promise<number>;
  protected abstract async getQueuedDomain(): Promise<Domain>;
  protected abstract async crawlDomain(origin: string, routes: Route[]): Promise<void>;
  protected abstract async updateDomainLastCrawlingDate(
    domain: Domain,
    startCrawlingUtc: Date
  ): Promise<void>;
}
