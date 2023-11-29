import React, { useEffect, useContext } from "react";
import { Grid, Row, Col } from "react-flexbox-grid";

import {
  Button,
  NonIdealState,

} from "@blueprintjs/core";
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
      <Grid fluid>
        <Row around="xs" style={{ paddingTop: "20px" }}>
          <Col xs={12}>
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
          </Col>
        </Row>
      </Grid>
    );
  }

  if (state.pageState === IndexingToolState.Loading) {
    return <Loading />;
  }

  return (
    <Grid fluid>
      <Row around="xs" style={{ paddingTop: "20px" }}>
        <Col xs={12}>
          <IndexingActions targetQP={targetQP} />
        </Col>
      </Row>
      <Row around="xs" style={{ paddingTop: "20px" }}>
        {state.pageState == IndexingToolState.Normal &&
          state.indexingServiceState &&
          state.indexingServiceState.reports && (
            <>
              {state.indexingServiceState.reports.map((rep, i) => (
                <Col key={i} xs={4} style={{padding:"8px"}}>
                  <IndexingReport report={rep} />
                </Col>
              ))}
            </>
          )}
      </Row>
    </Grid>
  );
}
export default IndexingManagementTool;
