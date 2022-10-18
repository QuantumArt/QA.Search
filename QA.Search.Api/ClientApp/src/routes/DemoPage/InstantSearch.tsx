import React, { useState, useCallback, useMemo, useRef } from "react";
import { InputGroup, Button, Card, Elevation } from "@blueprintjs/core";
import useClickAway from "react-use/lib/useClickAway";
import { withRouter, RouteComponentProps } from "react-router";
import qs from "query-string";
import { useDebounce } from "../../utils/hooks";
import Navigaton from "../../components/Navigation";
import SearchClient from "../../SearchClient";
import { News, Help, MediaMaterial, TextPage, MediaPage, MobileApp } from "./models";
import {
  renderMobileApps,
  renderMedia,
  renderPages,
  renderNews,
  renderHelp
} from "./InstantSearchLists";
import "./InstantSearch.scss";

interface Props extends RouteComponentProps {
  delayMsec?: number;
}

const InstantSearch = withRouter(({ history, delayMsec = 100 }: Props) => {
  const [isActive, setActive] = useState(false);
  const [userInput, setUserInput] = useState("");
  const [foundMobileApps, setFoundMobileApps] = useState<MobileApp[]>([]);
  const [foundMaterials, setFoundMaterials] = useState<MediaMaterial[]>([]);
  const [foundMediaPages, setFoundMediaPages] = useState<MediaPage[]>([]);
  const [foundPages, setFoundPages] = useState<TextPage[]>([]);
  const [foundNews, setFoundNews] = useState<News[]>([]);
  const [foundHelp, setFoundHelp] = useState<Help[]>([]);

  const queryString = useMemo(() => userInput.replace(/\s+/g, " ").trim(), [userInput]);

  const isOpen = useMemo(
    () =>
      isActive &&
      (foundMobileApps.length > 0 ||
        foundMaterials.length > 0 ||
        foundMediaPages.length > 0 ||
        foundNews.length > 0 ||
        foundHelp.length > 0 ||
        foundPages.length > 0),
    [foundMobileApps, foundHelp, foundMaterials, foundMediaPages, foundNews, foundPages, isActive]
  );

  useDebounce({
    async action(signal) {
      if (!queryString) return;
      try {
        const [
          mobileAppsResp,
          materialsResp,
          mediaPagesResp,
          pagesResp,
          newsResp,
          helpResp
        ] = await new SearchClient().multiSuggest<
          [MobileApp, MediaMaterial, MediaPage, TextPage, News, Help]
        >(signal, [
          {
            $from: "qp.mobileapps",
            $select: ["SearchUrl", "Icon", "Title"],
            $query: queryString,
            $weights: { Title: 5, ShortDescription: 1, Description: 1 },
            $snippets: { ShortDescription: 0, Description: 1 },
            $where: {
              SearchArea: "personal"
            },
            $limit: 1
          },
          {
            $from: "media.materials",
            $select: ["SearchUrl", "Preview"],
            $query: queryString,
            $snippets: 2,
            $limit: 3
          },
          {
            $from: "media.menuandpages",
            $select: ["SearchUrl"],
            $query: queryString,
            $weights: { Title: 1, TitleOnPage: 1, Description: 1 },
            $snippets: { Title: 0, TitleOnPage: 0, Description: 0 },
            $limit: 2
          },
          {
            $from: "qp.textpages",
            $select: ["SearchUrl"],
            $query: queryString,
            $weights: {
              Title: 5,
              Description: 3,
              MetaDescription: 3,
              Keywords: 5
            },
            $snippets: 2,
            $where: {
              SearchArea: "personal",
              Regions: { Alias: "moskva" }
            },
            $limit: 3
          },
          {
            $from: "qp.news",
            $select: ["SearchUrl", "PublishDate"],
            $query: queryString,
            $weights: { Title: 1, Anounce: 1 },
            $snippets: { Title: 0, Anounce: 0 },
            $where: {
              SearchArea: "personal",
              Regions: { Alias: "moskva" },
              Groups: { Title: "Новости Абонентам" }
            },
            $orderBy: [{ PublishDate: "desc" }, "_score"],
            $limit: 3
          },
          {
            $from: "domain.ru",
            $select: ["SearchUrl"],
            $query: queryString,
            $weights: { Title: 1, Description: 1 },
            $snippets: { Title: 0, Description: 0 },
            $where: {
              SearchArea: "personal",
              Regions: { Alias: "moskva" }
            },
            $limit: 3
          }
        ]);

        setActive(true);
        setFoundMobileApps(mobileAppsResp.documents);
        setFoundMaterials(materialsResp.documents);
        setFoundMediaPages(mediaPagesResp.documents);
        setFoundPages(pagesResp.documents);
        setFoundNews(newsResp.documents);
        setFoundHelp(helpResp.documents);
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
    event => {
      event.preventDefault();
      setActive(false);
      setUserInput("");
      history.push(`/demo?${qs.stringify({ query: queryString })}`);
    },
    [history, queryString]
  );

  const panelRef = useRef<HTMLDivElement>(null);
  const resultsRef = useRef<HTMLDivElement>(null);

  useClickAway(panelRef, event => {
    const resultsEl = resultsRef.current;
    if (resultsEl && !resultsEl.contains(event.target as HTMLElement)) {
      setActive(false);
    }
  });

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
            {foundMobileApps.length > 0 && renderMobileApps(foundMobileApps)}
            {(foundMaterials.length > 0 || foundMediaPages.length > 0) &&
              renderMedia(foundMaterials, foundMediaPages)}
            {foundPages.length > 0 && renderPages(foundPages)}
            {foundNews.length > 0 && renderNews(foundNews)}
            {foundHelp.length > 0 && renderHelp(foundHelp)}
          </Card>
        </div>
      )}
    </>
  );
});

export default InstantSearch;
