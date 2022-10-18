import { registerConnection } from "./utils/connection";
import { DeepCrawlingManager } from "./crawler/DeepCrawlingManager";

(async () => {
  await registerConnection();

  await new DeepCrawlingManager().crawlUntilCancelled();
})();
