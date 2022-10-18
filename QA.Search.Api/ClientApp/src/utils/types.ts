export function isString(arg: any): arg is string {
  return typeof arg === "string";
}

export function isNumber(arg: any): arg is number {
  return typeof arg === "number";
}

export function isFunction(arg: any): arg is Function {
  return typeof arg === "function";
}

export function isObject(arg: any): arg is object {
  return arg && typeof arg === "object" && !isArray(arg);
}

export function isArray(arg: any): arg is any[] {
  return Array.isArray(arg);
}
