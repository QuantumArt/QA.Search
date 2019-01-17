import { isPlainObject, isString } from "./Types";

/**
 * Extracts JSON from DOM Element according to passed schema with css selectors.
 * ```js
 * const json = extract(document, {
 *   keywords: "meta[name=keywords] | content",
 *   tariff: {
 *     title: ".internet-tariff h3 | innerText",
 *     actions: [
 *       ".internet-tariff .action-container",
 *       {
 *         title: "h4 | innerText",
 *         link: "a.action-link | href"
 *       }
 *     ]
 *   }
 * })
 * ```
 * ```json
 * {
 *   "keywords": "тариф, услуги, фит инетрнет",
 *   "title": "Тариф «ФИТ Интернет + Интерактивное ТВ + Кино»",
 *   "actions": [
 *     {
 *       "title": "Акция «Пакет каналов «Настрой кино!»",
 *       "description": "При подключении данного тарифного ..."
 *     },
 *     {
 *       "title": "Акция «Подписка от Онлайн кинотеатра ivi.ru»"
 *       "description": "Первые 3 месяца: 840 руб./мес. ..."
 *     }
 *   ]
 * }
 * ```
 */
function extractJson<T extends Schema>(element: Document | Element, schema: T): Result<T>;
function extractJson(element: Document | Element, schema: any) {
  if (isString(schema)) {
    if (schema.includes(" | ")) {
      const [selector, prop] = schema.split(" | ");
      if (element) {
        element = element.querySelector(selector);
        if (element) {
          return element[prop];
        }
      }
      return null;
    }
    return element ? element[schema] : null;
  }
  if (Array.isArray(schema) && schema.length === 2) {
    const [selector, childSchema] = schema;
    return element
      ? [...element.querySelectorAll<Element>(selector)].map(child =>
          extractJson(child, childSchema)
        )
      : [];
  }
  if (isPlainObject(schema)) {
    if (element) {
      const object: any = {};
      for (const name in schema) {
        object[name] = extractJson(element, schema[name]);
      }
      return object;
    }
    return null;
  }
  throw new Error(`Passed schema is invalid: ${JSON.stringify(schema, null, 2)}`);
}

export default extractJson;

type Schema = SimpleSchema | [string, SimpleSchema] | (string | SimpleSchema)[];

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
