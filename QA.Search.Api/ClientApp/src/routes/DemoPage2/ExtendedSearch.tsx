import React, { useCallback, useState, useMemo, useEffect, useRef } from "react";
import { Button, InputGroup, ControlGroup, Tag, H5 } from "@blueprintjs/core";
import qs from "query-string";
import { withRouter, RouteComponentProps } from "react-router";
import { useDebounce } from "../../utils/hooks";
import "./ExtendedSearch.scss";
import SearchClient, { SamplesFacet, ElasticDocument } from "../../SearchClient";
import { SearchItem } from "./models";
import useClickAway from "react-use/lib/useClickAway";

interface Props extends RouteComponentProps {
  delayMsec?: number;
}

const ITEMS_PER_PAGE = 10;

// TODO: facets

const ExtendedSearch = withRouter(({ location, history }: Props) => {
  const abortRef = useRef<AbortController>();
  const [userInput, setUserInput] = useState("");
  const [totalCount, setTotalCount] = useState(0);
  const [offset, setOffset] = useState(0);
  const [selectedIndex, setSelectedIndex] = useState("");
  const [countersPerIndex, setCountersPerIndex] = useState<SamplesFacet<string>[]>([]);
  const [searchItems, setSearchItems] = useState<SearchItem[]>([]);
  const [completionBlockOpen, setCompletionBlockOpen] = useState(false);
  const [completionPhrases, setCompletionPhrases] = useState<string[]>([]);
  const [correctedQuery, setCorrectedQuery] = useState<{
    snippet: string;
    text: string;
  } | null>(null);
  const [currentRegionAlias, setCurrentRegionAlias] = useState("moskva");

  const queryString = useMemo(() => userInput.replace(/\s+/g, " ").trim(), [userInput]);

  useEffect(() => {
    const { query } = qs.parse(location.search);
    setUserInput((query as string) || "");
  }, [location.search, history.length]);

  const extendedSearch = useCallback(
    async ({ index, query, offset }) => {
      history.replace(`/demo2?${qs.stringify({ query })}`);
      if (abortRef.current) {
        abortRef.current.abort();
      }
      abortRef.current = new AbortController();

      try {
        const resp = await new SearchClient().search<SearchItem>(abortRef.current.signal, {
          $from: index || "*",
          $select: [
            "SearchUrl",
            "Title",
            "HeaderTitle",
            "HeaderLead",
            "ListImage",
            "Date",
            "MarketingProduct.Title",
            "MarketingProduct.ListImage",
            "MarketingProduct.Images.Image",
            "MarketingProduct.Category.Title",
            "MarketingProduct.CommunicationType.Title",
            "Icon",
            "Preview",
            "PublishDate",
            "QuestionType.Group.Title",
            "IosLink"
          ],
          $roles: ["Reader"],
          $query: query,
          $snippets: { $count: 2, $length: 240 },
          $correct: {
            $query: {
              $ifFoundLte: 3
            }
          },
          $where: {
            SearchArea: [{ $eq: null }, { $eq: "personal" }],
            Regions: { Alias: [{ $eq: null }, { $eq: currentRegionAlias }] },
            Groups: { Title: [{ $eq: null }, { $eq: "Новости Абонентам" }] },
            PublishDate: [{ $eq: null }, { $gt: "2015-01-01T00:00:00" }]
          },
          $limit: ITEMS_PER_PAGE,
          $offset: offset,
          $facets: {
            _index: "$samples"
          }
        });

        if (offset > 0) {
          setSearchItems(searchItems => searchItems.concat(resp.documents));
        } else {
          setSearchItems(resp.documents);
        }
        setTotalCount(resp.totalCount);
        setCountersPerIndex(resp.facets!._index.samples!);
        if (resp.queryCorrection) {
          setCorrectedQuery(resp.queryCorrection);
        }
      } catch (err) {
        if (err instanceof DOMException && err.name === "AbortError") {
        } else {
          console.dir(err);
          throw err;
        }
      }
    },
    [history]
  );

  useEffect(() => {
    setOffset(0);
  }, [queryString]);

  const inputSearchRef = useRef<HTMLInputElement>(null);
  const completionBlockRef = useRef<HTMLDivElement>(null);

  useClickAway(inputSearchRef, event => {
    const blockEl = completionBlockRef.current;
    console.log();
    if (blockEl && !blockEl.contains(event.target as HTMLElement)) {
      setCompletionBlockOpen(false);
    }
  });

  function replaceUrlTokens(url: string) {
    const regionAlias = "{RegionAlias}";
    const urlRegionalPrefix = currentRegionAlias + ".";
    return url && url.replace(regionAlias, urlRegionalPrefix);
  }

  return (
    <div className="extended-search">
      <div className="extended-search__main">
        <ControlGroup fill className="extended-search__input">
          <InputGroup
            large
            fill
            type="search"
            placeholder="Поиск..."
            leftIcon="search"
            value={userInput}
            inputRef={inputSearchRef as any}
            onFocus={() => setCompletionBlockOpen(true)}
            onChange={e => setUserInput(e.target.value)}
            onKeyPress={event => {
              if (event.key === "Enter") {
                setOffset(0);
                extendedSearch({ index: selectedIndex, query: queryString, offset: 0 });
              }
            }}
          />
          <Button
            large
            title="Расширенный поиск"
            text="Поиск"
            onClick={() => {
              setOffset(0);
              extendedSearch({ index: selectedIndex, query: queryString, offset: 0 });
            }}
          />
        </ControlGroup>
        {completionBlockOpen && completionPhrases.length > 0 && (
          <div className="extended-search__completion-anchor">
            <div ref={completionBlockRef} className="extended-search__completion-block">
              {completionPhrases.map(phrase => (
                <div
                  key={phrase}
                  className="extended-search__completion-phrase"
                  onClick={() => {
                    setCompletionBlockOpen(false);
                    setUserInput(phrase);
                    setOffset(0);
                    extendedSearch({ index: selectedIndex, query: phrase, offset: 0 });
                  }}
                >
                  {phrase}
                </div>
              ))}
            </div>
          </div>
        )}
        {correctedQuery && (
          <div
            className="extended-search__corrected-query"
            onClick={() => {
              setOffset(0);
              setCorrectedQuery(null);
              setUserInput(correctedQuery.text);
              extendedSearch({ index: selectedIndex, query: correctedQuery.text, offset: 0 });
            }}
          >
            Возможно, вы имели в виду:{" "}
            <span
              className="extended-search__corrected-query-snippet"
              dangerouslySetInnerHTML={{ __html: correctedQuery.snippet }}
            />
          </div>
        )}
        {selectedIndex ? (
          <div className="extended-search__counters">
            <Tag
              large
              minimal
              interactive
              onRemove={() => {
                setSelectedIndex("");
                extendedSearch({ index: "", query: queryString, offset: 0 });
              }}
            >
              {indexNames[selectedIndex] || selectedIndex}{" "}
              <b>{getSelectedIndexCount(selectedIndex, countersPerIndex)}</b>
            </Tag>
          </div>
        ) : (
          <div className="extended-search__counters">
            {countersPerIndex &&
              countersPerIndex.map(el => (
                <Tag
                  key={el.value}
                  large
                  minimal
                  interactive
                  onClick={() => {
                    setSelectedIndex(el.value);
                    extendedSearch({ index: el.value, query: queryString, offset: 0 });
                  }}
                >
                  {indexNames[el.value] || el.value} <b>{el.count}</b>
                </Tag>
              ))}
          </div>
        )}
        <div className="extended-search__results">
          {searchItems.map(item => (
            <article key={item._id} className="extended-search__item">
              <H5>
                <a href={replaceUrlTokens(item.SearchUrl) || "https://domain.ru"}>
                  {item.PublishDate && `[${item.PublishDate.toLocaleDateString()}] `}
                  {item.Title ||
                    item.HeaderTitle ||
                    (item.MarketingProduct && item.MarketingProduct.Title)}
                </a>
              </H5>
              <section>
                <aside>
                  {item.Icon && (
                    <img
                      className="instant-search__card-image instant-search__app-image"
                      src={
                        item.Icon.startsWith("//")
                          ? item.Icon
                          : "http://msk.alpha.domain.ru/upload/contents/10695/" + item.Icon
                      }
                      alt="icon"
                      width="64"
                      height="64"
                    />
                  )}
                  {item.Preview && (
                    <img
                      className="instant-search__card-image instant-search__material-image"
                      src={"https://static.ssl.domain.ru/media/images/materials/" + item.Preview}
                      alt="preview"
                      width="64"
                      height="64"
                    />
                  )}
                </aside>
                <main>
                  {item.HeaderLead && <p>{item.HeaderLead}</p>}
                  <p dangerouslySetInnerHTML={getSnippetsHtml(item)}></p>
                </main>
              </section>
            </article>
          ))}
        </div>
        {offset + ITEMS_PER_PAGE < totalCount && (
          <div className="extended-search__load-more">
            <Button
              active
              large
              minimal
              onClick={() => {
                setOffset(searchItems.length);
                extendedSearch({
                  index: selectedIndex,
                  query: queryString,
                  offset: searchItems.length
                });
              }}
            >
              Показать ещё {Math.min(totalCount - searchItems.length, ITEMS_PER_PAGE)} (из{" "}
              {totalCount - searchItems.length})
            </Button>
          </div>
        )}
      </div>
    </div>
  );
});

export default ExtendedSearch;

function getSelectedIndexCount(
  index: string,
  countersPerIndex: SamplesFacet<string>[] | undefined
) {
  if (countersPerIndex) {
    const counter = countersPerIndex.find(c => c.value === index);
    if (counter) {
      return counter.count;
    }
  }
  return null;
}

function getSnippetsHtml(document: ElasticDocument) {
  const snippets = Object.values(document._snippets!)
    .flat()
    .map(snippet => snippet.trim())
    .filter(Boolean)
    .join(" | ");

  return { __html: snippets };
}

const indexNames = {
  "media.materials": "Медиа",
  "media.menuandpages": "Медиа",

  "qp.mobileapps": "Мобильные приложения",
  "qp.textpages": "Частным клиентам",
  "qp.news": "Новости",

  "domain.ru": "Помощь"
};
