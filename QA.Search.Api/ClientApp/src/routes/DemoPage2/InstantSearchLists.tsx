import React from "react";
import { Divider } from "@blueprintjs/core";
import { QueryCompletion } from "../../SearchClient";

export function renderQueryCompletion(suggestions: QueryCompletion[], onSelect: (event: any, key: string) => void) {
  return (
    <>
      {suggestions.map(item => {
        return (
          <div class="instant-search__card" onClick={e => onSelect(e, item.key)}>
            <div class="instant-search__short-card">
              {item.key}
              </div>
            <div class="instant-search__full-card">
              {item.key}
              </div>
          </div>
        );
      })}
      <Divider />
    </>
  );
}