export interface CrawlingOptions {
  readonly concurrencyLevel: number;
  readonly queueCheckInterval: number;
  readonly hostCrawlingInterval: number;
}
