import React, { useContext } from "react";
import { Grid, Row, Col } from "react-flexbox-grid";

import { Button, NonIdealState } from "@blueprintjs/core";

import Loading from "../../components/Loading";
import { PageState } from "./ElasticManagementPageModel";
import ElasticManagementPageContainer from "./ElasticManagementPageContainer";
import IndexesFilter from "./IndexesFilter";
import IndexesCardDetails from "./IndexesCardDetails";
import NewIndexEditor from "./NewIndexEditor";

function ElasticManagementPage() {
  const { state, visibleCards, switchCreateIndexMode } = useContext(
    ElasticManagementPageContainer.Context
  );

  if (state.pageState === PageState.Error) {
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

  if (state.pageState === PageState.Loading) {
    return <Loading />;
  }

  const normalPageState = (
    <>
      <div style={{ paddingTop: "20px" }}>
          <IndexesFilter switchCreateIndexMode={() => switchCreateIndexMode(true)} />
      </div>
      <div style={{ paddingTop: "20px", maxWidth: "650px", margin: "auto" }}>
        {visibleCards &&
          visibleCards.map((cm, i) => (
            <div key={String(i)} style={{ paddingTop: "20px" }}>
              <IndexesCardDetails indexesCard={cm} />
            </div>
          ))}
      </div>
      {visibleCards && visibleCards.length === 0 && (
        <div style={{ paddingTop: "20px" }}>
          <div>
            <img src="https://pngicon.ru/file/uploads/cat_hungry.png" />
          </div>
        </div>
      )}
    </>
  );

  return (
    <Grid fluid>
      {state.pageState == PageState.Normal && normalPageState}
      {state.pageState == PageState.NewIndexCreation && (
        <Row around="xs" style={{ paddingTop: "20px" }}>
          <Col xs={12}>
            <NewIndexEditor />
          </Col>
        </Row>
      )}
    </Grid>
  );
}

export default ElasticManagementPage;
