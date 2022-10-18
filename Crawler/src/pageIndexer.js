import { handleNextLinksBatch } from "./indexer/pageIndexerCore";
import { registerConnection } from "./utils/connection";
import { delay } from "./utils/promise";
import { pageIndexerLogger as logger } from "./utils/loggers";
import { indexingOptions as settings } from "./settings";

(async () => {
  await registerConnection();

  while (true) {
    const startUnixTime = Date.now();
    while (await handleNextLinksBatch()) {}

    const delayMsec = settings.queueCheckInterval + startUnixTime - Date.now();
    if (delayMsec > 0) {
      logger.info(`Sleep for ${Math.round(delayMsec / 1000)} sec`);
      await delay(delayMsec);
    }
  }
})();
