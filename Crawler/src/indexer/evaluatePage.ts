import url from "url";
import pathToRegexp from "path-to-regexp";

/**
 * Make `evaluatePage` function for Headless Chrome Crawler,
 * which extracts JSON from DOM Element according to passed schema.
 * @param schema Schema DSL
 * @param route Express route string
 * @param location Page URL string
 */
export function makeEvaluatePage(schema: string | Object, route: string, location: string) {
  if (typeof schema !== "string") {
    schema = JSON.stringify(schema);
  }
  const context = JSON.stringify(getLocationContext(route, location));
  return new Function(`
    ${extractJson}
    ${removeTags}
    removeTags();
    return extractJson(document, ${schema}, ${context});
  `) as () => Object;
}

/**
 * Extracts information from URL path and query in object form
 * @param route Express route string
 * @param location Page URL string
 * ```js
 * getLocationContext("/:first/:second", "http://site.com/foo/bar?name=value")
 * // results to
 * {
 *   url: "http://site.com/foo/bar?query=string",
 *   first: "foo",
 *   second: "bar"
 *   name: "value"
 * }
 * ```
 */
export function getLocationContext(route: string, location: string) {
  const { pathname, query } = url.parse(location, true);
  const context = { ...query };
  const keys = [];
  const match = pathToRegexp(route, keys).exec(pathname);
  keys.forEach((key, i) => {
    context[key.name] = match[i + 1];
  });
  context.url = location;
  return context;
}

/** @internal */
interface RemoveTagsContext {
  openCommentsCount: number;
  noindexParentElement: Element;
}

/** Removes tags that should not be indexed. */
export function removeTags(
  node: Node = document.body,
  selectors = ["noindex", ".noindex", ".robots-noindex", ".robots-nocontent"],
  context: RemoveTagsContext = {
    openCommentsCount: 0,
    noindexParentElement: null
  }
) {
  if (node instanceof Element) {
    if (
      context.openCommentsCount > 0 ||
      selectors.some(selector => (node as Element).matches(selector))
    ) {
      node.remove();
    } else {
      node.childNodes.forEach(child => removeTags(child, selectors, context));

      if (node === context.noindexParentElement) {
        context.openCommentsCount = 0;
      }
    }
  } else if (node instanceof Comment) {
    if (node.data === "noindex") {
      if (context.openCommentsCount === 0) {
        context.noindexParentElement = node.parentElement;
      }
      context.openCommentsCount++;
    } else if (node.data === "/noindex") {
      if (context.openCommentsCount > 0) {
        context.openCommentsCount--;
      }
    }
    node.remove();
  }
}

/**
 * Extracts JSON from DOM Element according to passed schema with css selectors.
 * Works in browser context. So it MUST NOT DEPEND on any outer variables or functions.
 * ```js
 * const json = extractJson(document, {
 *   tariff: {
 *     location: ":url",
 *     title: ".internet-tariff h3 | innerText",
 *     keywords: "meta[name=keywords] | content",
 *     actions: [
 *       ".internet-tariff .action-container",
 *       {
 *         title: "h4 | innerText",
 *         link: "a.action-link | href"
 *       }
 *     ]
 *   }
 * }, {
 *   url: "http://domain.ru/tariffs/test-tariff",
 * })
 * ```
 * ```json
 * {
 *   "tariff": {
 *     "location":  "http://domain.ru/tariffs/test-tariff",
 *     "title": "Тариф «ФИТ Интернет + Интерактивное ТВ + Кино»",
 *     "keywords": "тариф, услуги, фит инетрнет",
 *     "actions": [
 *       {
 *         "title": "Акция «Пакет каналов «Настрой кино!»",
 *         "link": "http://domain.ru/actions/first-action"
 *       },
 *       {
 *         "title": "Акция «Подписка от Онлайн кинотеатра ivi.ru»",
 *         "link": "http://domain.ru/actions/second-action"
 *       }
 *     ]
 *   }
 * }
 * ```
 */
export function extractJson<T extends Schema>(
  element: Document | Element,
  schema: T,
  context?: Object
): Result<T>;
export function extractJson(element: Document | Element, schema: any, context?: Object) {
  if (!element) {
    throw new TypeError("Element is not defined");
  }
  if (typeof schema === "string") {
    let value: string;
    if (schema.startsWith("=")) {
      value = schema.slice(1);
    } else if (schema.includes(" | ")) {
      const [selector, prop] = schema.split(" | ");
      element = element.querySelector(selector);
      value = element && element[prop];
    } else if (schema.startsWith(":")) {
      value = context && context[schema.slice(1)];
    } else if (element instanceof Element) {
      value = schema in element ? element[schema] : element.getAttribute(schema);
    } else {
      value = element[schema];
    }
    return value === undefined ? null : value;
  }
  if (
    schema &&
    typeof schema === "object" &&
    (schema.constructor === Object || !("constructor" in schema))
  ) {
    const object = {};
    for (const name in schema) {
      object[name] = extractJson(element, schema[name], context);
    }
    return object;
  }
  if (Array.isArray(schema) && schema.length === 2) {
    const [selector, childSchema] = schema;
    return [...element.querySelectorAll<Element>(selector)].map(child =>
      extractJson(child, childSchema, context)
    );
  }
  throw new TypeError(`Passed schema is invalid: ${JSON.stringify(schema, null, 2)}`);
}

type Schema = SimpleSchema | [string, SimpleSchema];

type SimpleSchema = string | { [key: string]: Schema };

type Result<T> = T extends [string, infer U]
  ? SimpleResult<U>[]
  : T extends (string | infer U)[]
  ? SimpleResult<U>[]
  : SimpleResult<T>;

type SimpleResult<T> = T extends string
  ? string | null
  : T extends Object
  ? { [K in keyof T]: Result<T[K]> | null }
  : null;
