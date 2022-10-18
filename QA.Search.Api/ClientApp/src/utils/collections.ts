import deepmerge from "deepmerge";

/**
 * Map object values key by key using specified mapper function
 * @param source Source dictionary
 * @param mapper Mapper function
 */
export function mapObject<T, R>(
  source: { [key: string]: T },
  mapper: (value: T, key: string) => R
): { [key: string]: R };
export function mapObject<T, R>(
  source: { [key: number]: T },
  mapper: (value: T, key: number) => R
): { [key: number]: R };
export function mapObject(source: object, mapper: Function) {
  const result = {};
  for (const key in source) {
    result[key] = mapper(source[key], key);
  }
  return result;
}

/**
 * Select some objects from dictionary by keys and deep merge these objects
 * @param dict Dictionary of objects
 * @param keys Keys for select objects
 */
export function mergeForKeys<T extends object>(dict: { [key: string]: T }, keys: string[]): T;
export function mergeForKeys<T extends object>(dict: { [key: number]: T }, keys: string[]): T;
export function mergeForKeys(dict: object, keys: any[]) {
  return deepmerge.all(keys.map(key => dict[key]).filter(Boolean));
}
