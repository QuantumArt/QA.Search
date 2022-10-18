const stringifyCompact = require("@aitodotai/json-stringify-pretty-compact");

export function formatJson(json: any, maxLength = 80): string {
  return stringifyCompact(json, { objectMargins: true, maxLength });
}
