import React, { useState, useMemo } from "react";
import { Button, Icon, Intent } from "@blueprintjs/core";
import { MonacoDiffEditor } from "react-monaco-editor";
import { useInterval } from "../../utils/hooks";
import { formatJson } from "../../utils/json";
import { TemplateFile, TemplateController } from "../../backend.generated";
import "./MappingTemplatesPage.scss";
import { acquireLoading, releaseLoading } from "../../utils/nprogress";

const MappingTemplatesPage = () => {
  const [templates, setTemplates] = useState<TemplateFile[]>([]);
  const [current, setCurrent] = useState<string | null>(null);

  useInterval({
    async action(signal) {
      const serverTemplates = (await new TemplateController().getTemplates()) || [];
      if (!signal.aborted) {
        setTemplates(serverTemplates);
        setCurrent(
          current =>
            (!serverTemplates.some(t => t.name === current) &&
              (serverTemplates[0] && serverTemplates[0].name)) ||
            current
        );
      }
    },
    msec: 5000,
    deps: []
  });

  const buttonIntents = useMemo(
    () =>
      templates.map(template => {
        if (!template.newContent) {
          return Intent.DANGER;
        }
        if (!template.oldContent) {
          return Intent.SUCCESS;
        }
        if (JSON.stringify(template.newContent) !== JSON.stringify(template.oldContent)) {
          return Intent.WARNING;
        }
        return Intent.NONE;
      }),
    [templates]
  );

  const currentTemlate = useMemo(() => templates.find(t => t.name === current), [
    templates,
    current
  ]);

  const oldCode = useMemo(
    () =>
      currentTemlate && currentTemlate.oldContent ? formatJson(currentTemlate.oldContent) : "",
    [currentTemlate]
  );

  const newCode = useMemo(
    () =>
      currentTemlate && currentTemlate.newContent ? formatJson(currentTemlate.newContent) : "",
    [currentTemlate]
  );

  async function applyTemplate() {
    if (oldCode === newCode) return;
    try {
      acquireLoading();
      await new TemplateController().applyTemplate(current);
    } finally {
      releaseLoading();
    }
  }

  async function deleteTemplate() {
    if (!oldCode) return;
    try {
      acquireLoading();
      await new TemplateController().deleteTemplate(current);
    } finally {
      releaseLoading();
    }
  }

  return (
    <>
      <div className="mapping-templates__container">
        <div className="mapping-templates__content">
          <MonacoDiffEditor
            width="calc(100vw - 440px)"
            height="calc(100vh - 100px)"
            theme="vs-dark"
            language="json"
            original={oldCode}
            value={newCode}
            options={{
              fontSize: 16,
              readOnly: true,
              wordWrap: "on",
              minimap: { enabled: false },
              scrollbar: { verticalScrollbarSize: 17 }
            }}
          />
          <div>
            <div className="mapping-templates__panel mapping-templates__panel--left">
              <strong>ElasticSearch</strong>
              <Button
                minimal
                rightIcon="trash"
                intent={Intent.DANGER}
                disabled={!oldCode}
                onClick={deleteTemplate}
              >
                Delete Template
              </Button>
            </div>
            <div className="mapping-templates__panel mapping-templates__panel--right">
              <Button
                minimal
                rightIcon="floppy-disk"
                intent={Intent.PRIMARY}
                disabled={oldCode === newCode}
                onClick={applyTemplate}
              >
                Apply Template
              </Button>
              <strong>AdminApp</strong>
            </div>
          </div>
        </div>
        <div className="mapping-templates__sidebar">
          {templates.map((template, i) => (
            <Button
              key={template.name || ""}
              minimal
              active={template.name === current}
              onClick={() => setCurrent(template.name || null)}
              intent={buttonIntents[i]}
            >
              <span
                style={{
                  marginLeft: `${10 * (template.newContent || template.oldContent).order}px`
                }}
              >
                {template.name}
              </span>
            </Button>
          ))}
        </div>
      </div>
    </>
  );
};

export default MappingTemplatesPage;
