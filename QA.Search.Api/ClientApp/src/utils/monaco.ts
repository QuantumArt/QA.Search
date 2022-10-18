import * as monaco from "monaco-editor/esm/vs/editor/editor.api";
import { formatJson } from "./json";

monaco.languages.registerDocumentFormattingEditProvider("json", {
  provideDocumentFormattingEdits(model) {
    let text = model.getValue();
    try {
      text = formatJson(JSON.parse(text));
    } catch {}

    return [{ text, range: model.getFullModelRange() }];
  }
});
