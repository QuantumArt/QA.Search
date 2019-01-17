export function isString(arg: any): arg is string {
  return typeof arg === "string";
}

export function isFunciton(arg: any): arg is Function {
  return typeof arg === "string";
}

export function isPlainObject(arg: any): arg is { [key: string]: any } {
  return arg && typeof arg === "object" && (arg.constructor === Object || !arg.constructor);
}
