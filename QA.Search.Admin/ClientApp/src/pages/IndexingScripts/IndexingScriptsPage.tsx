import React, { useState, useCallback } from "react";
import * as monaco from "monaco-editor/esm/vs/editor/editor.api";
import MonacoEditor from "react-monaco-editor";
import "./IndexingScriptsPage.scss";
import "../../utils/groovy";

const IndexingScriptsPage = () => {
  const [code, setCode] = useState("");

  const onEditorDidMount = useCallback((editor: monaco.editor.IStandaloneCodeEditor) => {
    const model = editor.getModel()!;
    model.updateOptions({ tabSize: 2 });
  }, []);

  return (
    <>
      <div className="indexing-scripts__container">
        <div className="indexing-scripts__content">
          <MonacoEditor
            width="calc(100vw - 440px)"
            height="calc(100vh - 100px)"
            theme="vs-dark"
            language="groovy"
            value={code}
            options={{
              fontSize: 16,
              tabCompletion: "on",
              minimap: { enabled: false },
              scrollbar: { verticalScrollbarSize: 17 }
            }}
            onChange={setCode}
            editorDidMount={onEditorDidMount}
          />
          <div>
            <div className="indexing-scripts__panel indexing-scripts__panel--left">
              <strong>LeftPanel</strong>
              <span />
            </div>
            <div className="indexing-scripts__panel indexing-scripts__panel--right">
              <span />
              <strong>RIghtPanel</strong>
            </div>
          </div>
        </div>
        <div className="indexing-scripts__sidebar" />
      </div>
    </>
  );
};

export default IndexingScriptsPage;
