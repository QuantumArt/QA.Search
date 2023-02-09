import React, { useContext } from "react";
import { Grid, Row, Col } from "react-flexbox-grid";

import { Button, NonIdealState, Card, InputGroup, Tooltip, Intent } from "@blueprintjs/core";

import Loading from "../../components/Loading";
import { PageState } from "./ElasticManagementPageModel";
import ElasticManagementPageContainer from "./ElasticManagementPageContainer";
import { IndexesCardViewModel } from "../../backend.generated";
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
      <Row around="xs" style={{ paddingTop: "20px" }}>
        <Col xs={11}>
          <IndexesFilter />
        </Col>
        <Col xs={1}>
          <Card style={{ height: "100%", textAlign: "center" }}>
            <Tooltip content="Перейти к созданию нового индекса">
              <Button
                icon="plus"
                intent={Intent.NONE}
                large={true}
                onClick={() => switchCreateIndexMode(true)}
              />
            </Tooltip>
          </Card>
        </Col>
      </Row>
      <Row around="xs" style={{ paddingTop: "20px" }}>
        {visibleCards &&
          visibleCards.map((cm, i) => (
            <Col key={String(i)} xs={6} style={{ paddingTop: "20px" }}>
              <IndexesCardDetails indexesCard={cm} />
            </Col>
          ))}
      </Row>
      {visibleCards && visibleCards.length === 0 && (
        <Row around="xs" style={{ paddingTop: "20px" }} center="xs">
          <Col xs={4}>
            <img src="https://pngicon.ru/file/uploads/cat_hungry.png" />
          </Col>
        </Row>
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
