import React, { useEffect, useContext } from "react";

import { Button, NonIdealState } from "@blueprintjs/core";
import Loading from "../../components/Loading";

import IndexingReport from "../IndexingManagement/IndexingReport";
import { TargetQP } from "../../backend.generated";

import { IndexingToolState } from "./IndexingManagementToolContainerFactory";

import useManagementToolContext from "./useManagementToolContext";
import IndexingActions from "../IndexingManagement/IndexingActions";

type Props = {
  targetQP: TargetQP;
};

function IndexingManagementTool({ targetQP }: Props) {
  const { state } = useManagementToolContext(targetQP);

  if (!state) {
    return null;
  }

  if (state.pageState === IndexingToolState.Error) {
    return (
      <div className="elastic-card-bottom-element">
        <NonIdealState
          title="Ошибка"
          icon="error"
          description={<p>При выполнении операции произошла ошибка</p>}
          action={
            <Button icon="refresh" onClick={() => window.location.reload()}>
              Перезагрузить страницу
            </Button>
          }
        />
      </div>
    );
  }

  if (state.pageState === IndexingToolState.Loading) {
    return <Loading />;
  }

  return (
    <div>
      <div className="elastic-card-bottom-element">
        <IndexingActions targetQP={targetQP} />
      </div>
      <div className="qp-cards-space-around">
        {state.pageState == IndexingToolState.Normal &&
          state.indexingServiceState &&
          state.indexingServiceState.reports && (
            <>
              {state.indexingServiceState.reports.map((rep, i) => (
                <div key={i} className="flex-basis-three-block" style={{ padding: "8px" }}>
                  <IndexingReport report={rep} />
                </div>
              ))}
            </>
          )}
      </div>
    </div>
  );
}
export default IndexingManagementTool;
