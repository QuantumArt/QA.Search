import { getRepository, LessThan, In, getConnection } from "typeorm";
import normalizeUrl from "normalize-url";
import { Page } from "puppeteer";
import { Link } from "../entities/Link";
import { Domain } from "../entities/Domain";
import { Route, IndexingConfig } from "../entities/Route";
import { HttpStatusError } from "../utils/error";
import { makeEvaluatePage, getLocationContext } from "./evaluatePage";
import { PageIndexer } from "./PageIndexer";
import { elastic } from "../utils/elasticsearch";
import { pageIndexerLogger as logger } from "../utils/loggers";
import { indexingOptions as settings } from "../settings";

export async function handleNextLinksBatch() {
  try {
    logger.info(`Get scheduled links`);

    const linkDomainsByUrl = await getScheduledLinkDomains();

    if (linkDomainsByUrl.size === 0) {
      return false;
    }

    logger.info(`Start indexing scheduled links`);

    await PageIndexer.launch({
      links: [...linkDomainsByUrl.keys()],
      concurrencyLevel: settings.concurrencyLevel,
      async lockPage(currentLink) {
        const { link } = linkDomainsByUrl.get(currentLink);
        return await lockLink(link);
      },
      async handlePage(currentLink, page) {
        const { link, domain } = linkDomainsByUrl.get(currentLink);
        await handlePage(page, link, domain);
      },
      async handleError(currentLink, error) {
        if (error instanceof HttpStatusError) {
          const { link, domain } = linkDomainsByUrl.get(currentLink);
          const routes = getMatchedRoutes(link, domain);
          logger.warn({ href: currentLink, statusCode: error.statusCode }, `Page not found`);
          await removeLink(link, routes);
        } else if (error.message && error.message.startsWith("net::ERR_ABORTED")) {
          const { link, domain } = linkDomainsByUrl.get(currentLink);
          const routes = getMatchedRoutes(link, domain);
          logger.warn({ href: currentLink, status: "net::ERR_ABORTED" }, `Page cannot be loaded`);
          await removeLink(link, routes);
        } else {
          throw error;
        }
      }
    });

    logger.info(`Finish indexing scheduled links`);
  } catch (err) {
    logger.error({ err }, `Failed to index scheduled links`);
  }
  return true;
}

async function getScheduledLinkDomains(): Promise<
  Map<
    string,
    {
      link: Link;
      domain?: Domain;
    }
  >
> {
  const links = await getRepository(Link).find({
    where: {
      nextIndexingUtc: LessThan(new Date().toISOString()),
      isActive: true
    },
    order: { nextIndexingUtc: "ASC" },
    take: settings.linksBatchSize
  });

  if (links.length === 0) return new Map();

  const linkOrigins = links.map(link => link.urlParts.origin);

  const uniqueOrigins = linkOrigins.filter((el, i, arr) => arr.indexOf(el) === i);

  const domains = await getRepository(Domain).find({
    where: { origin: In(uniqueOrigins) },
    relations: ["domainGroup", "domainGroup.routes"]
  });

  domains
    .map(domain => domain.domainGroup)
    .filter((el, i, arr) => arr.indexOf(el) === i)
    .forEach(domainGroup => {
      domainGroup.routes.sort(Route.compare);
    });

  return links.reduce(
    (map, link, i) =>
      map.set(link.url, {
        link,
        domain: domains.find(domain => domain.origin === linkOrigins[i])
      }),
    new Map()
  );
}

async function lockLink(link: Link) {
  const { hash, version } = link;

  const result = await getConnection()
    .createQueryBuilder()
    .update<Link>(Link)
    .where("hash = :hash", { hash })
    .andWhere("version = :version", { version })
    .output("INSERTED.version")
    .execute();

  if (result.raw.length > 0) {
    link.version = Number(result.raw[0].version);
    return true;
  }
  return false;
}

async function handlePage(page: Page, link: Link, domain?: Domain) {
  const pageUrl = normalizeUrl(page.url());
  const routes = getMatchedRoutes(link, domain);
  if (routes.length > 0) {
    await scheduleNextIndexing(link, routes[0]);

    for (const { id, route, indexingConfig } of routes) {
      const { extractSchema, transformScript, indexName } = mergeIndexingConfig(
        domain.domainGroup.indexingConfig,
        indexingConfig
      );

      logger.info(
        { href: link.url, route: { id, route }, index: indexName },
        `Start evaluating page`
      );

      let document = await page.evaluate(makeEvaluatePage(extractSchema, route, pageUrl));

      if (transformScript) {
        const context = getLocationContext(route, pageUrl);
        try {
          // TODO: maybe we need sandbox to prevent DoS-attack by infinite loops?
          document =
            new Function("json", "context", transformScript)(document, context) || document;
        } catch (err) {
          logger.error(
            { err, href: link.url, route: { id, route }, index: indexName },
            `Failed to execute transform script`
          );
        }
      }

      await elastic.index({
        index: indexName,
        id: pageUrl,
        type: "_doc",
        version: Number(Date.now()),
        versionType: "external",
        body: document
      });

      logger.info(
        { href: link.url, route: { id, route }, index: indexName },
        `Page is saved in Elastic index`
      );
    }
  } else {
    logger.warn({ href: link.url }, `No matching routes found for page`);
    await removeLink(link, routes);
  }
}

function getMatchedRoutes(link: Link, domain?: Domain) {
  const matchedRoutes: Route[] = [];
  if (domain) {
    const { routes } = domain.domainGroup;
    const { pathname } = link.urlParts;
    let index = routes.findIndex(route => route.pattern.test(pathname));
    if (index !== -1) {
      const firstRoute = routes[index];
      do {
        matchedRoutes.push(routes[index++]);
      } while (
        index < routes.length &&
        Route.compare(firstRoute, routes[index]) === 0 &&
        routes[index].pattern.test(pathname)
      );
    }
  }
  return matchedRoutes;
}

function mergeIndexingConfig(domainConfig?: IndexingConfig, routeConfig?: IndexingConfig) {
  const config = {} as IndexingConfig;
  if (domainConfig) {
    for (const name in domainConfig) {
      if (domainConfig[name] != null) {
        config[name] = domainConfig[name];
      }
    }
  }
  if (routeConfig) {
    for (const name in routeConfig) {
      if (routeConfig[name] != null) {
        config[name] = routeConfig[name];
      }
    }
  }
  return config;
}

async function scheduleNextIndexing(link: Link, { id, route, scanPeriodMsec }: Route) {
  link.nextIndexingUtc = new Date(Date.now() + scanPeriodMsec);
  await getRepository(Link).save(link);

  logger.info(
    { href: link.url, route: { id, route }, utc: link.nextIndexingUtc },
    `Link indexing is scheduled`
  );
}

async function removeLink(link: Link, routes: Route[]) {
  link.isActive = false;
  await getRepository(Link).save(link);

  logger.info({ href: link.url }, `Page is removed from indexing process`);

  for (const { id, route, indexingConfig } of routes) {
    // TODO: maybe we should use .bulk() ?
    await elastic.delete({
      index: indexingConfig.indexName,
      id: link.url,
      type: "_doc"
    });

    logger.info({ href: link.url, route: { id, route } }, `Page is removed from Elastic index`);
  }
}
