import React, { useRef, useEffect } from "react";
import hljs from "highlight.js";
import "highlight.js/styles/vs2015.css";

const CodeBlock = ({ value, language }) => {
  const ref = useRef<HTMLElement>();

  useEffect(() => {
    hljs.highlightBlock(ref.current);
  }, [value, language]);

  return (
    <pre>
      <code ref={ref as any} className={`hljs language-${language}`}>
        {value}
      </code>
    </pre>
  );
};

export default CodeBlock;
