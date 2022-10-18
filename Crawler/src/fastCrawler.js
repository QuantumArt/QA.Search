import { registerConnection } from "./utils/connection";
import { FastCrawlingManager } from "./crawler/FastCrawlingManager";

(async () => {
  await registerConnection();

  await new FastCrawlingManager().crawlUntilCancelled();
})();
