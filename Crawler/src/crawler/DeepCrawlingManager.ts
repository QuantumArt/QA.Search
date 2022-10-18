import { getRepository, LessThan } from "typeorm";
import normalizeUrl from "normalize-url";
import { Domain, Route } from "../entities";
import { CrawlingManager } from "./CrawlingManager";
import { DeepCrawler } from "./DeepCrawler";
import { deepCrawlingOptions as settings } from "../settings";
import { deepCrawlerLogger } from "../utils/loggers";

export class DeepCrawlingManager extends CrawlingManager {
  constructor() {
    super(settings.queueCheckInterval, deepCrawlerLogger);
  }

  protected async crawlDomain(origin: string, routes: Route[]) {
    return DeepCrawler.launch({
      links: [origin],
      outgoingHosts: ["https://domain.ru"],
      concurrencyLevel: settings.concurrencyLevel,
      blockedResources: ["stylesheet"],
      handlePage: page => this.handlePage(normalizeUrl(page.url()), routes)
    });
  }

  protected async getQueuedDomain() {
    return getRepository(Domain).findOne({
      where: [
        { lastDeepCrawlingUtc: LessThan(this.getThresholdDate().toISOString()) },
        { lastDeepCrawlingUtc: null }
      ],
      order: { lastDeepCrawlingUtc: "ASC" },
      relations: ["domainGroup", "domainGroup.routes"]
    });
  }

  protected async updateDomainLastCrawlingDate(domain: Domain, startCrawlingUtc: Date) {
    domain.lastDeepCrawlingUtc = startCrawlingUtc;
    await getRepository(Domain).save(domain);
  }

  protected async getQueueLength() {
    return getRepository(Domain).count({
      where: [
        { lastDeepCrawlingUtc: LessThan(this.getThresholdDate().toISOString()) },
        { lastDeepCrawlingUtc: null }
      ]
    });
  }

  private getThresholdDate() {
    return new Date(Date.now() - settings.hostCrawlingInterval);
  }
}
