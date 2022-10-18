import { CrawlingManager } from "./CrawlingManager";
import { Domain, Route } from "../entities";
import { getRepository, LessThan } from "typeorm";
import { FastCrawler } from "./FastCrawler";
import { fastCrawlingOptions as settings } from "../settings";
import { fastCrawlerLogger } from "../utils/loggers";

export class FastCrawlingManager extends CrawlingManager {
  constructor() {
    super(settings.queueCheckInterval, fastCrawlerLogger);
  }

  protected async crawlDomain(origin: string, routes: Route[]) {
    return FastCrawler.launch({
      links: [origin],
      outgoingHosts: ["https://domain.ru"],
      concurrencyLevel: settings.concurrencyLevel,
      handlePage: href => this.handlePage(href, routes)
    });
  }

  protected async getQueuedDomain() {
    return getRepository(Domain).findOne({
      where: [
        { lastFastCrawlingUtc: LessThan(this.getThresholdDate().toISOString()) },
        { lastFastCrawlingUtc: null }
      ],
      order: { lastFastCrawlingUtc: "ASC" },
      relations: ["domainGroup", "domainGroup.routes"]
    });
  }

  protected async updateDomainLastCrawlingDate(domain: Domain, startCrawlingUtc: Date) {
    domain.lastFastCrawlingUtc = startCrawlingUtc;
    await getRepository(Domain).save(domain);
  }

  protected async getQueueLength() {
    return getRepository(Domain).count({
      where: [
        { lastFastCrawlingUtc: LessThan(this.getThresholdDate().toISOString()) },
        { lastFastCrawlingUtc: null }
      ]
    });
  }

  private getThresholdDate() {
    return new Date(Date.now() - settings.hostCrawlingInterval);
  }
}
