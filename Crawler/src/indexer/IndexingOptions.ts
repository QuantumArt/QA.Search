export interface IndexingOptions {
  readonly concurrencyLevel: number;
  readonly linksBatchSize: number;
  readonly queueCheckInterval: number;
  readonly testPort: number;
}
