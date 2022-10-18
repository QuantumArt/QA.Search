import React from "react";
import { Toaster, Intent } from "@blueprintjs/core";

export default Toaster.create({ position: "bottom-right" });

export function getHttpStatusIntent(status: number): Intent {
  if (status >= 200 && status < 300) {
    return Intent.SUCCESS;
  }
  if (status >= 300 && status < 400) {
    return Intent.PRIMARY;
  }
  if (status >= 400 && status < 500) {
    return Intent.WARNING;
  }
  if (status >= 500) {
    return Intent.DANGER;
  }
  throw new Error("Unsupported status");
}

export function getResponseStatusMessage(response: Response, msec?: number): React.ReactNode {
  return (
    <div style={{ display: "flex", justifyContent: "space-between" }}>
      <div>{response.url.substring(window.location.origin.length)}</div>
      {msec && response.ok && <div>{Math.round(msec)} ms</div>}
      <div>
        {response.status} {response.statusText}
      </div>
    </div>
  );
}
