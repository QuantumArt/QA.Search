import { URL } from "url";
import puppeteer from "puppeteer";
import normalizeUrl from "normalize-url";
import { getRepository } from "typeorm";
import { Domain } from "../entities/Domain";
import { Route } from "../entities/Route";
import { BusinessError, HttpStatusError } from "../utils/error";
import { configurePage } from "../utils/page";
import { makeEvaluatePage, getLocationContext } from "./evaluatePage";

export async function tryExtractJson({
  url,
  route,
  extractSchema,
  transformScript
}: {
  url: string;
  route: string;
  extractSchema: Object;
  transformScript: string;
}) {
  url = normalizeUrl(url);

  const testRoute = new Route({
    route,
    indexingConfig: { extractSchema, transformScript, indexName: null }
  });

  await validateRoute(testRoute, url);

  return await extractJson(testRoute, url);
}

async function validateRoute(testRoute: Route, url: string) {
  const { origin, pathname } = new URL(url);

  if (!testRoute.pattern.exec(pathname)) {
    throw new BusinessError(`The url "${url}" doesn't match to route "${testRoute.route}"`);
  }

  const domain = await getRepository(Domain).findOne(origin, {
    relations: ["domainGroup", "domainGroup.routes"]
  });

  const primaryRoute = domain.domainGroup.routes.find(
    route => route.pattern.exec(pathname) && Route.compare(route, testRoute) < 0
  );

  if (primaryRoute) {
    throw new BusinessError(
      `There is a route "${primaryRoute.route}" with higher priority than passed route "${
        testRoute.route
      }"`
    );
  }
}

async function extractJson(
  { route, indexingConfig: { extractSchema, transformScript } }: Route,
  url: string
) {
  const browser = await puppeteer.launch({
    args: ["--no-sandbox", "--disable-setuid-sandbox"]
  });
  const page = await browser.newPage();

  await configurePage(page);

  try {
    const response = await page.goto(url);
    const pageUrl = normalizeUrl(page.url());

    if (response.ok()) {
      const evaluateFunc = makeEvaluatePage(extractSchema, route, pageUrl);

      let json = await page.evaluate(evaluateFunc);

      if (transformScript) {
        const context = getLocationContext(route, pageUrl);
        // TODO: maybe we need sandbox to prevent DoS-attack by infinite loops?
        json = new Function("json", "context", transformScript)(json, context) || json;
      }

      return json;
    } else {
      throw new HttpStatusError(response.status());
    }
  } finally {
    await page.close();
    await browser.close();
  }
}
