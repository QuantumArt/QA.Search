import React, { memo, Fragment, useState, useMemo, useCallback } from "react";
import { Code, Pre, Collapse, Button, MenuItem, Icon } from "@blueprintjs/core";
import { MultiSelect } from "@blueprintjs/select";
import { TableMappings, ObjectMapping } from "../models/TableMappings";
import { copyToClipboard } from "../utils/clipboard";
import { isArray, isObject } from "../utils/types";
import "./MappingsTab.scss";

interface Props {
  tables: TableMappings | undefined;
}

const AliasMultiSelect = MultiSelect.ofType<string>();

function MappingsTab({ tables = {} }: Props) {
  const [selectedTables, setSelectedTables] = useState({});
  const [openTables, setOpenTables] = useState({});

  const selectedItems = useMemo(() => {
    return Object.keys(tables)
      .filter(alias => selectedTables[alias])
      .sort();
  }, [tables, selectedTables]);

  const toggleSelectItem = useCallback((alias: string) => {
    setSelectedTables(selectedTables => ({
      ...selectedTables,
      [alias]: !selectedTables[alias]
    }));
  }, []);

  const renderSelectItem = useCallback(
    (alias: string, { modifiers, handleClick }) => {
      if (!modifiers.matchesPredicate) {
        return null;
      }
      return (
        <MenuItem
          active={modifiers.active}
          icon={selectedTables[alias] ? "tick" : "blank"}
          key={alias}
          text={alias}
          onClick={handleClick}
        />
      );
    },
    [selectedTables]
  );

  return (
    <div className="mappings-tab">
      <AliasMultiSelect
        large
        placeholder="Tables..."
        items={Object.keys(tables).sort()}
        itemPredicate={(query, item) => item.includes(query)}
        selectedItems={selectedItems}
        tagRenderer={item => item}
        itemRenderer={renderSelectItem}
        onItemSelect={toggleSelectItem}
        noResults={<MenuItem disabled text="No results..." />}
        tagInputProps={{ minimal: true, large: true, onRemove: toggleSelectItem }}
        popoverProps={{ minimal: true, usePortal: false }}
      />
      {(selectedItems.length === 0 ? Object.keys(tables).sort() : selectedItems).map(alias => (
        <Fragment key={alias}>
          <Button
            minimal
            icon="clipboard"
            title="Copy to clipboard"
            onClick={() => copyToClipboard(alias)}
          />
          <Button
            minimal
            text={alias}
            title={openTables[alias] ? "Collapse" : "Expand"}
            onClick={() => setOpenTables({ ...openTables, [alias]: !openTables[alias] })}
          />
          <Collapse isOpen={Boolean(openTables[alias])}>
            <Pre>
              <Code>
                <CodeBlock mapping={tables[alias]} />
              </Code>
            </Pre>
          </Collapse>
        </Fragment>
      ))}
    </div>
  );
}

interface CodeBlockProps {
  field?: string;
  path?: string;
  mapping: string | ObjectMapping | [ObjectMapping];
}

function CodeBlock({ field, path, mapping }: CodeBlockProps) {
  const [expanded, setExpanded] = useState(!field);
  const [hasFocus, setFocus] = useState(false);

  if (isArray(mapping) || isObject(mapping)) {
    return (
      <div>
        {field ? (
          <div
            className="mappings-tab__collapse"
            title={expanded ? "collapse" : "expand"}
            onClick={() => setExpanded(!expanded)}
            onMouseEnter={() => setFocus(true)}
            onMouseLeave={() => setFocus(false)}
          >
            <Icon iconSize={18} icon={expanded ? "caret-down" : "caret-right"} />
            <span className="mappings-tab__complex-field">{field}</span>
            {isArray(mapping)
              ? expanded
                ? `: [{`
                : `: [{ ... }]`
              : expanded
              ? `: {`
              : `: { ... }`}
            <span
              className="mappings-tab__field-path"
              style={{ visibility: hasFocus ? "visible" : "hidden" }}
              title="Copy to clipboard"
              onClick={() => path && copyToClipboard(path)}
            >
              {path}
            </span>
          </div>
        ) : (
          <div>{isArray(mapping) ? `[{` : `{`}</div>
        )}
        <div className="mappings-tab__block" style={{ display: expanded ? "block" : "none" }}>
          {Object.entries(isArray(mapping) ? mapping[0] : mapping).map(([name, type]) => (
            <CodeBlock
              key={name}
              field={name}
              path={path ? path + "." + name : name}
              mapping={type}
            />
          ))}
        </div>
        <div>{expanded && (isArray(mapping) ? `}]` : `}`)}</div>
      </div>
    );
  }

  return (
    <div onMouseEnter={() => setFocus(true)} onMouseLeave={() => setFocus(false)}>
      <span className="mappings-tab__scalar-field">{field}</span>
      {`: `}
      <span className="mappings-tab__type">{mapping}</span>
      <span
        className="mappings-tab__field-path"
        style={{ visibility: hasFocus ? "visible" : "hidden" }}
        title="Copy to clipboard"
        onClick={() => path && copyToClipboard(path)}
      >
        {path}
      </span>
    </div>
  );
}

export default memo(MappingsTab);
