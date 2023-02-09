import fs from "fs";
import { FastCrawler } from "../src/crawler/FastCrawler";

const fileName = `debug/media.domain.ru_${new Date()
  .toISOString()
  .slice(0, 19)
  .replace(/[:T]/g, "-")}.txt`;

FastCrawler.launch({
  links: ["http://demositerus.dev.qsupport.ru"],
  outgoingHosts: ["http://demositerus.dev.qsupport.ru"],
  concurrencyLevel: 10,
  blockedResources: ["stylesheet"],
  async handlePage(pageLink) {
    fs.appendFileSync(
      fileName,
      `[${new Date()
        .toISOString()
        .slice(0, 19)
        .replace("T", " ")}] ${pageLink}\n`
    );
  }
});
