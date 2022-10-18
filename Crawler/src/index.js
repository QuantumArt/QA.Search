switch (process.env.CRAWLER_MODE) {
  case "fast-crawler":
    require("./fastCrawler");
    break;
  case "deep-crawler":
    require("./deepCrawler");
    break;
  case "page-indexer":
    require("./pageIndexer");
    require("./testIndexer");
    break;
  default:
    require("./fastCrawler");
    require("./pageIndexer");
    require("./testIndexer");
    break;
}
