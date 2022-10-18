import { Button, Icon, Tab, Tabs, Navbar } from "@blueprintjs/core";
import * as monaco from "monaco-editor/esm/vs/editor/editor.api";
import React, { useCallback, useEffect, useMemo, useRef } from "react";
import MonacoEditor from "react-monaco-editor";
import ReactMarkdown from "react-markdown";
import MappingsTab from "../components/MappingsTab";
import Navigaton from "../components/Navigation";
import CodeBlock from "../components/CodeBlock";
import {
  TableMappings,
  collectAllFieldNames,
  collectAllNestedPaths,
  getFieldNamesForTables,
  buildFilterSchema,
  buildFieldWeights,
  buildFieldSnippets
} from "../models/TableMappings";
import SuggestSchema from "../schemas/suggest.json";
import { useSetState } from "../utils/hooks";
import { formatJson } from "../utils/json";
import "../utils/monaco";
import { acquireLoading, releaseLoading } from "../utils/nprogress";
import Toaster, { getHttpStatusIntent, getResponseStatusMessage } from "../utils/toaster";
import { isString } from "../utils/types";
import { mapObject, mergeForKeys } from "../utils/collections";
import SuggestDocs from "../docs/Suggest.md";
import FilteringDocs from "../docs/Filtering.md";
import ContextualDocs from "../docs/Contextual.md";
import "./SuggestPage.scss";

interface State {
  tabId: string;
  code: string;
  modelUri?: string;
  results?: string;
  tables?: TableMappings;
}

const SuggestPage = () => {
  const mountedTabIds = useRef({ docs: true }).current;

  const [state, setState] = useSetState<State>({
    tabId: "docs",
    code: SUGGEST_REQUEST
  });

  const setTab = useCallback(
    (tabId: string) => {
      mountedTabIds[tabId] = true;
      setState({ tabId });
    },
    [mountedTabIds, setState]
  );

  useEffect(() => {
    async function loadMappings() {
      performance.now();
      const response = await fetch("/api/v1/mapping");
      if (response.ok) {
        const tables: TableMappings = await response.json();
        setState({ tables });
      } else {
        Toaster.show({
          message: getResponseStatusMessage(response),
          intent: getHttpStatusIntent(response.status)
        });
      }
    }

    loadMappings();
  }, [setState]);

  const fromTables = useMemo(() => {
    const match = state.code.match(
      /"\$from"\s*:\s*(?:"[\w.]+"|\[\s*"[\w.]+"(?:,\s*"[\w.]+")*\s*\])/
    );
    if (match) {
      try {
        let from = JSON.parse("{" + match[0] + "}").$from;
        if (isString(from)) {
          from = [from];
        }
        if (Array.isArray(from) && from.every(isString)) {
          return [...new Set(from)].join(",");
        }
      } catch {}
    }
    return null;
  }, [state.code]);

  const fieldNamesByTable = useMemo(() => {
    return state.tables && collectAllFieldNames(state.tables);
  }, [state.tables]);

  const nestedPathsByTable = useMemo(() => {
    return state.tables && collectAllNestedPaths(state.tables);
  }, [state.tables]);

  const fieldWeightsByTable = useMemo(() => {
    return state.tables && mapObject(state.tables, buildFieldWeights);
  }, [state.tables]);

  const fieldSnippetsByTable = useMemo(() => {
    return state.tables && mapObject(state.tables, buildFieldSnippets);
  }, [state.tables]);

  const filterSchemasByTable = useMemo(() => {
    return state.tables && mapObject(state.tables, buildFilterSchema);
  }, [state.tables]);

  useEffect(() => {
    if (
      fieldNamesByTable &&
      nestedPathsByTable &&
      fieldWeightsByTable &&
      fieldSnippetsByTable &&
      filterSchemasByTable &&
      state.modelUri
    ) {
      const extendedSchema: typeof SuggestSchema = JSON.parse(JSON.stringify(SuggestSchema));
      const { definitions } = extendedSchema;

      definitions.index["enum"] = Object.keys(fieldNamesByTable);

      if (fromTables) {
        const tables = fromTables.split(",");
        const nestedPaths = getFieldNamesForTables(tables, nestedPathsByTable);
        const wildcardNames = getFieldNamesForTables(tables, fieldNamesByTable);
        const fieldNames = wildcardNames.filter(name => !name.includes("*"));

        if (fieldNames.length > 0) {
          definitions.field["enum"] = fieldNames;
          definitions.fields.items["enum"] = wildcardNames;
          definitions.nestedPath["enum"] = nestedPaths;

          Object.assign(
            definitions.snippets.anyOf[1].properties,
            mergeForKeys(fieldSnippetsByTable, tables)
          );
          Object.assign(definitions.weights.properties, mergeForKeys(fieldWeightsByTable, tables));
          Object.assign(definitions.where.properties, mergeForKeys(filterSchemasByTable, tables));

          // delete definitions.filter;
          delete definitions.where.patternProperties;
          delete definitions.weights.patternProperties;
          delete definitions.snippets.anyOf[1].patternProperties;
        }
      }

      monaco.languages.json.jsonDefaults.setDiagnosticsOptions({
        validate: true,
        schemas: [
          {
            uri: "/api/v1/suggest",
            fileMatch: [state.modelUri!],
            schema: extendedSchema
          }
        ]
      });
    }
  }, [
    fromTables,
    fieldNamesByTable,
    nestedPathsByTable,
    fieldWeightsByTable,
    fieldSnippetsByTable,
    filterSchemasByTable,
    state.modelUri
  ]);

  const onChange = useCallback(
    (code: string) => {
      sessionStorage.setItem(SUGGEST_REQUEST_KEY, code);
      localStorage.setItem(SUGGEST_REQUEST_KEY, code);
      setState({ code });
    },
    [setState]
  );

  const onFormat = useCallback(() => {
    let code = state.code;
    try {
      code = formatJson(JSON.parse(code));
      sessionStorage.setItem(SUGGEST_REQUEST_KEY, code);
      localStorage.setItem(SUGGEST_REQUEST_KEY, code);
      setState({ code });
    } catch {}
  }, [setState, state.code]);

  const onExecute = useCallback(async () => {
    acquireLoading();
    const startTime = performance.now();
    const response = await fetch("/api/v1/suggest", {
      method: "POST",
      headers: {
        Accept: "application/json",
        "Content-Type": "application/json"
      },
      body: state.code
    });
    Toaster.show({
      message: getResponseStatusMessage(response, performance.now() - startTime),
      intent: getHttpStatusIntent(response.status)
    });
    if ([200, 400, 404, 408, 502].includes(response.status)) {
      const json = await response.json();

      setTab("results");
      setState({ results: formatJson(json, 100) });
    }
    releaseLoading();
  }, [setState, setTab, state.code]);

  const onEditorDidMount = useCallback(
    (editor: monaco.editor.IStandaloneCodeEditor) => {
      const model = editor.getModel()!;
      const modelUri = model.uri.toString();
      model.updateOptions({ tabSize: 2 });
      setState({ modelUri });
    },
    [setState]
  );

  return (
    <>
      <Navigaton.LeftPanel>
        <Navbar.Divider />
        <Navbar.Heading>Actions</Navbar.Heading>
        <Button icon="play" onClick={onExecute} text="Execute" />
        <Button icon="horizontal-bar-chart" onClick={onFormat} text="Format" />
      </Navigaton.LeftPanel>
      <Navigaton.RightPanel>
        <Tabs large className="suggest-page__tabs" selectedTabId={state.tabId} onChange={setTab}>
          <Tab id="tables">
            <div className="suggest-page__tab-title">
              <Icon icon="th-list" /> Tables
            </div>
          </Tab>
          <Tab id="results">
            <div className="suggest-page__tab-title">
              <Icon icon="list" /> Results
            </div>
          </Tab>
          <Tab id="docs">
            <div className="suggest-page__tab-title">
              <Icon icon="document" /> Docs
            </div>
          </Tab>
        </Tabs>
      </Navigaton.RightPanel>
      <div className="suggest-page__layout">
        <div className="suggest-page__editor">
          <MonacoEditor
            language="json"
            theme="vs-dark"
            value={state.code}
            options={{
              fontSize: 16,
              tabCompletion: "on",
              quickSuggestions: { strings: true, other: true, comments: false },
              minimap: { enabled: false },
              scrollbar: { verticalScrollbarSize: 17 }
            }}
            onChange={onChange}
            editorDidMount={onEditorDidMount}
          />
        </div>
        <div
          className="suggest-page__results"
          style={{ display: state.tabId === "results" ? "block" : "none" }}
        >
          {mountedTabIds["results"] && (
            <MonacoEditor
              language="json"
              theme="vs-dark"
              value={state.results}
              options={{
                fontSize: 16,
                readOnly: true,
                wordWrap: "on",
                minimap: { enabled: false },
                scrollbar: { verticalScrollbarSize: 17 }
              }}
            />
          )}
        </div>
        <div
          className="suggest-page__tables"
          style={{ display: state.tabId === "tables" ? "block" : "none" }}
        >
          {mountedTabIds["tables"] && <MappingsTab tables={state.tables} />}
        </div>
        <div
          className="suggest-page__docs"
          style={{ display: state.tabId === "docs" ? "block" : "none" }}
        >
          {mountedTabIds["docs"] && (
            <>
              <ReactMarkdown source={SuggestDocs} renderers={{ code: CodeBlock }} />
              <ReactMarkdown source={FilteringDocs} renderers={{ code: CodeBlock }} />
              <ReactMarkdown source={ContextualDocs} renderers={{ code: CodeBlock }} />
            </>
          )}
        </div>
      </div>
    </>
  );
};

const SUGGEST_REQUEST_KEY = "SearchApi.SuggestRequest";

const SUGGEST_REQUEST =
  sessionStorage.getItem(SUGGEST_REQUEST_KEY) || localStorage.getItem(SUGGEST_REQUEST_KEY) || `{}`;

export default SuggestPage;
