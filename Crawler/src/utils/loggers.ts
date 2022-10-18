import bunyan from "bunyan";

const logger = bunyan.createLogger({
  name: "Crawler",
  serializers: {
    err: bunyan.stdSerializers.err
  }
});

export const fastCrawlerLogger = logger.child({ process: "FastCrawler" });
export const deepCrawlerLogger = logger.child({ process: "DeepCrawler" });
export const pageIndexerLogger = logger.child({ process: "PageIndexer" });

process.on("unhandledRejection", err => {
  logger.fatal({ err }, `Unhandled promise rejection`);
  process.exit(-1); //mandatory (as per the Node docs)
});

process.on("uncaughtException", err => {
  logger.fatal({ err }, `Uncaught exception`);
  process.exit(1); //mandatory (as per the Node docs)
});
