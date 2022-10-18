import * as monaco from "monaco-editor";
import { registerRulesForLanguage } from "monaco-ace-tokenizer";
import GroovyHighlightRules from "monaco-ace-tokenizer/lib/ace/definitions/groovy";

monaco.languages.register({ id: "groovy" });

registerRulesForLanguage("groovy", new GroovyHighlightRules());
