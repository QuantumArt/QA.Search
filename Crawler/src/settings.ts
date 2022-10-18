import { ConnectionOptions } from "typeorm";
import { CrawlingOptions } from "./crawler/CrawlingOptions";
import { IndexingOptions } from "./indexer/IndexingOptions";
import { ConfigOptions } from "elasticsearch";

export const dbConnectionOptions: ConnectionOptions = {
  type: "mssql",
  host: process.env.DB_HOST || "localhost",
  username: process.env.DB_USER || "sa",
  password: process.env.DB_PASSWORD || "StrongPass1234",
  database: "qa_search",
  synchronize: false,
  logging: false,
  pool: { acquireTimeoutMillis: 20000 },
  entities: ["src/entities/**/*.ts"]
};

export const deepCrawlingOptions: CrawlingOptions = {
  concurrencyLevel: parseInt(process.env.DEEP_CRAWLER_CONCURRENCY_LEVEL) || 2,
  queueCheckInterval: parseInt(process.env.DEEP_CRAWLER_QUEUE_CHECK_INTERVAL) || 60 * 1000, // 1 min
  hostCrawlingInterval:
    parseInt(process.env.DEEP_CRAWLER_HOST_CRAWLING_INTERVAL) || 24 * 60 * 60 * 1000 // 1 day
};

export const fastCrawlingOptions: CrawlingOptions = {
  concurrencyLevel: parseInt(process.env.FAST_CRAWLER_CONCURRENCY_LEVEL) || 10,
  queueCheckInterval: parseInt(process.env.FAST_CRAWLER_QUEUE_CHECK_INTERVAL) || 60 * 1000, // 1 min
  hostCrawlingInterval:
    parseInt(process.env.FAST_CRAWLER_HOST_CRAWLING_INTERVAL) || 24 * 60 * 60 * 1000 // 1 day
};

export const indexingOptions: IndexingOptions = {
  concurrencyLevel: parseInt(process.env.INDEXER_CONCURRENCY_LEVEL) || 1,
  linksBatchSize: parseInt(process.env.INDEXER_LINKS_BATCH_SIZE) || 1000, // 1 sec
  queueCheckInterval: parseInt(process.env.INDEXER_QUEUE_CHECK_INTERVAL) || 5 * 60 * 1000, // 5 min
  testPort: parseInt(process.env.INDEXER_TEST_PORT) || 4957
};

export const elasticSearchOptions: ConfigOptions = {
  hosts: process.env.ELASTIC_HOST ? process.env.ELASTIC_HOST.split(";") : ["http://localhost:9200"]
};
