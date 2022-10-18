import React, { useState, useCallback, useMemo, useRef } from "react";
import { InputGroup, Button, Card, Elevation } from "@blueprintjs/core";
import useClickAway from "react-use/lib/useClickAway";
import { withRouter, RouteComponentProps } from "react-router";
import qs from "query-string";
import { useDebounce } from "../../utils/hooks";
import Navigaton from "../../components/Navigation";
import SearchClient, { QueryCompletion } from "../../SearchClient";
import { renderQueryCompletion } from "./InstantSearchLists";
import "./InstantSearch.scss";

interface Props extends RouteComponentProps {
  delayMsec?: number;
}

const InstantSearch = withRouter(({ history, delayMsec = 100 }: Props) => {
  const [isActive, setActive] = useState(false);
  const [userInput, setUserInput] = useState("");
  const [queryCompletions, setQueryCompletions] = useState<QueryCompletion[]>([]);
  const [currentRegionAlias, setCurrentRegionAlias] = useState("moskva");

  const queryString = useMemo(() => userInput.replace(/\s+/g, " ").trim(), [userInput]);

  const isOpen = useMemo(
    () => (isActive && queryCompletions.length > 0),
    [queryCompletions, isActive]
  );

  useDebounce({
    async action(signal) {
      if (!queryString) return;
      try {
        const completions = await new SearchClient().queryCompletions(signal, {
          $query: queryString,
          $limit: 10,
          $region: currentRegionAlias
        });
        setQueryCompletions(completions.documents);

      } catch (err) {
        if (err instanceof DOMException && err.name === "AbortError") {
        } else {
          console.dir(err);
          throw err;
        }
      }
    },
    msec: delayMsec,
    deps: [queryString]
  });

  const extendedSearch = useCallback(
    (event, query?: string) => {
      event.preventDefault();
      setActive(false);

      query = query || queryString;
      setUserInput("");

      setQueryCompletions([])
      history.push(`/demo2?${qs.stringify({ query: query })}`);
      registerSuggestionsQuery(query, currentRegionAlias);
    },
    [history, queryString]
  );

  const registerSuggestionsQuery = (query, region) => {
    new SearchClient().registerQueryCompletion({
      $query: query,
      $region: region
    });
  };

  const panelRef = useRef<HTMLDivElement>(null);
  const resultsRef = useRef<HTMLDivElement>(null);

  useClickAway(panelRef, event => {
    const resultsEl = resultsRef.current;
    if (resultsEl && !resultsEl.contains(event.target as HTMLElement)) {
      setActive(false);
    }
  });

  const setUserQuery =
    (event, query: string) => {
      event.preventDefault();
      setUserInput(query);
      extendedSearch(event, query);
    };

  return (
    <>
      <Navigaton.RightPanel>
        <div className="instant-search__panel" ref={panelRef}>
          <InputGroup
            large
            fill
            type="search"
            placeholder="Поиск..."
            value={userInput}
            onFocus={() => setActive(true)}
            onChange={e => setUserInput(e.target.value)}
            rightElement={
              <Button
                minimal
                intent="primary"
                icon="search"
                title="Расширенный поиск"
                onClick={extendedSearch}
              />
            }
            onKeyPress={event => event.key === "Enter" && extendedSearch(event)}
          />
        </div>
      </Navigaton.RightPanel>
      {isOpen && (
        <div className="instant-search__results" ref={resultsRef}>
          <Card interactive elevation={Elevation.TWO}>
            {queryCompletions.length > 0 && renderQueryCompletion(queryCompletions, setUserQuery)}
          </Card>
        </div>
      )}
    </>
  );
});

export default InstantSearch;
