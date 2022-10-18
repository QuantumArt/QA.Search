import { isArray, isObject } from "../utils/types";

export interface ObjectMapping {
  [property: string]: string | ObjectMapping | [ObjectMapping];
}

export interface TableMappings {
  [alias: string]: ObjectMapping;
}

export interface FieldNamesByTable {
  [alias: string]: string[];
}

function collectFieldNames(
  mapping: ObjectMapping,
  stack: string[] = [],
  names = new Set<string>(["_id", "_index", "_score"])
) {
  for (const name in mapping) {
    const value = mapping[name];
    const inner = isArray(value) ? value[0] : isObject(value) ? value : null;

    if (inner) {
      stack.push(name);
      stack.push("*");
      names.add(stack.join("."));
      stack.pop();
      collectFieldNames(inner, stack, names);
      stack.pop();
    } else {
      stack.push(name);
      names.add(stack.join("."));
      stack.pop();
    }
  }
  return names;
}

export function collectAllFieldNames(mappings: TableMappings): FieldNamesByTable {
  return Object.entries(mappings).reduce((names, [alias, mapping]) => {
    names[alias] = [...collectFieldNames(mapping)];
    return names;
  }, {});
}

function collectNestedPaths(
  mapping: ObjectMapping,
  stack: string[] = [],
  names = new Set<string>()
) {
  for (const name in mapping) {
    const value = mapping[name];
    const inner = isArray(value) ? value[0] : isObject(value) ? value : null;

    if (inner) {
      stack.push(name);
      if (isArray(value)) {
        names.add(stack.join("."));
      }
      collectNestedPaths(inner, stack, names);
      stack.pop();
    }
  }
  return names;
}

export function collectAllNestedPaths(mappings: TableMappings): FieldNamesByTable {
  return Object.entries(mappings).reduce((names, [alias, mapping]) => {
    names[alias] = [...collectNestedPaths(mapping)];
    return names;
  }, {});
}

export function getFieldNamesForTables(tables: string[], fieldNamesByTable: FieldNamesByTable) {
  return [
    ...new Set(
      tables
        .map(alias => fieldNamesByTable[alias])
        .filter(Boolean)
        .flat()
    )
  ];
}

export function buildFilterSchema(mapping: ObjectMapping) {
  const schema = {};
  for (const name in mapping) {
    const value = mapping[name];
    const inner = isArray(value) ? value[0] : isObject(value) ? value : null;

    if (inner) {
      const innerSchema = buildFilterSchema(inner);

      schema[name] = {
        type: "object",
        properties: innerSchema,
        additionalProperties: false,
        minProperties: 1
      };

      for (const innerName in innerSchema) {
        schema[name + "." + innerName] = innerSchema[innerName];
      }
    } else {
      schema[name] = { $ref: "#/definitions/condition" };
    }
  }
  return schema;
}

export function buildFieldFacets(mapping: ObjectMapping) {
  const schema = {};
  for (const name in mapping) {
    const value = mapping[name];
    const inner = isArray(value) ? value[0] : isObject(value) ? value : null;

    if (inner) {
      const innerSchema = buildFieldFacets(inner);

      for (const innerName in innerSchema) {
        if (!schema[name]) {
          schema[name] = {
            type: "object",
            properties: innerSchema,
            additionalProperties: false,
            minProperties: 1
          };
        }
        schema[name + "." + innerName] = innerSchema[innerName];
      }
    } else if (value === "number" || value === "date") {
      schema[name] = { $ref: "#/definitions/facet" };
    } else {
      schema[name] = {
        anyOf: [{ enum: ["$samples"] }, { $ref: "#/definitions/samplesFacet" }]
      };
    }
  }
  return schema;
}

export function buildFieldWeights(mapping: ObjectMapping) {
  const schema = {};
  for (const name in mapping) {
    const value = mapping[name];
    const inner = isArray(value) ? value[0] : isObject(value) ? value : null;

    if (inner) {
      const innerSchema = buildFieldWeights(inner);

      for (const innerName in innerSchema) {
        if (!schema[name]) {
          schema[name] = {
            type: "object",
            properties: innerSchema,
            additionalProperties: false,
            minProperties: 1
          };
        }
        schema[name + "." + innerName] = innerSchema[innerName];
      }
    } else if (value === "text") {
      schema[name] = { type: "number", exclusiveMinimum: 0 };
    }
  }
  return schema;
}

export function buildFieldSnippets(mapping: ObjectMapping) {
  const schema = {};
  for (const name in mapping) {
    const value = mapping[name];
    const inner = isArray(value) ? value[0] : isObject(value) ? value : null;

    if (inner) {
      const innerSchema = buildFieldSnippets(inner);

      for (const innerName in innerSchema) {
        if (!schema[name]) {
          schema[name] = {
            type: "object",
            properties: innerSchema,
            additionalProperties: false,
            minProperties: 1
          };
        }
        schema[name + "." + innerName] = innerSchema[innerName];
      }
    } else if (value === "text") {
      schema[name] = { $ref: "#/definitions/snippet" };
    }
  }
  return schema;
}
