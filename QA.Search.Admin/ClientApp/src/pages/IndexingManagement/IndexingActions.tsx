import React from "react";

import { Card, Button, ProgressBar, HTMLTable } from "@blueprintjs/core";
import "./IndexingActions.css";
import { IndexingToolState, IndexingButton } from "./IndexingManagementToolContainerFactory";
import { TargetQP } from "../../backend.generated";
import useManagementToolContext from "../IndexingManagement/useManagementToolContext";
import { Grid, Row, Col } from "react-flexbox-grid";

type Props = {
  targetQP: TargetQP;
};

function IndexingActions({ targetQP }: Props) {
  const { state, indexingStart, indexingStop } = useManagementToolContext(targetQP);

  if (!state || state.pageState !== IndexingToolState.Normal || !state.indexingServiceState) {
    return null;
  }

  return (
    <Card elevation={2}>
      <h5 className="bp3-heading">Действия</h5>
      <Grid fluid>
        <Row between="xs" style={{ paddingTop: "20px" }}>
          <Col xs={4}>
            {state.indexingServiceState && (
              <HTMLTable bordered condensed>
                <tbody>
                  <tr>
                    <td>Статус:</td>
                    <td>{state.indexingServiceState.state}</td>
                  </tr>
                  <tr>
                    <td>Сообщение:</td>
                    <td>{state.indexingServiceState.message}</td>
                  </tr>
                  <tr>
                    <td>Дата начала:</td>
                    <td>{state.indexingServiceState.startDate}</td>
                  </tr>
                  <tr>
                    <td>Дата окончания:</td>
                    <td>{state.indexingServiceState.endDate}</td>
                  </tr>
                </tbody>
              </HTMLTable>
            )}
          </Col>
          <Col xs={8}>
            {state.indexingServiceState &&
              state.indexingServiceState.scheduledDates &&
              state.indexingServiceState.scheduledDates.length > 0 && (
                <HTMLTable bordered condensed>
                  <tbody>
                    <tr>
                      <td>Плановые даты запусков:</td>
                      <td>
                        <ul>
                          {state.indexingServiceState.scheduledDates.map((v, i) => (
                            <li key={i}>{v}</li>
                          ))}
                        </ul>
                      </td>
                    </tr>
                  </tbody>
                </HTMLTable>
              )}
          </Col>
        </Row>
        <Row between="xs" style={{ paddingTop: "20px" }}>
          <Col xs={12}>
            <div className="indexing-actions__indexing-button-area">
              {state.currentIndexingButton == IndexingButton.Start && (
                <Button
                  intent="primary"
                  icon="search-text"
                  large={true}
                  disabled={!state.indexingOperationsAreEnabled}
                  onClick={indexingStart}
                >
                  Индексировать
                </Button>
              )}

              {state.currentIndexingButton == IndexingButton.Stop && (
                <>
                  <ProgressBar intent="primary" value={state.indexingServiceState.progress / 100} />

                  <p className="indexing-actions__progressbar-label">
                    <strong>Завершено:</strong>&nbsp;{state.indexingServiceState.progress}%
                  </p>
                  <Button
                    icon="small-cross"
                    large={true}
                    onClick={indexingStop}
                    disabled={!state.indexingOperationsAreEnabled}
                  >
                    Остановить индексацию
                  </Button>
                </>
              )}
            </div>
          </Col>
        </Row>
      </Grid>
    </Card>
  );
}

export default IndexingActions;
