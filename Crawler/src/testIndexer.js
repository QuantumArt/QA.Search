import express from "express";
import { tryExtractJson } from "./indexer/testIndexerCore";
import { registerConnection } from "./utils/connection";
import { BusinessError } from "./utils/error";
import { indexingOptions as settings } from "./settings";

(async () => {
  await registerConnection();

  const app = express();

  app.use(express.json());

  app.post("/check-route", async (request, response) => {
    try {
      response.send(await tryExtractJson(request.body));
    } catch (error) {
      if (error instanceof BusinessError) {
        response.status(422).send(error.message);
      } else {
        response.status(500).send(error.message);
      }
    }
  });

  app.get("/healthcheck", async (_request, response) => {
    response.status(200).send("OK");
  });

  app.listen(settings.testPort);
})();
