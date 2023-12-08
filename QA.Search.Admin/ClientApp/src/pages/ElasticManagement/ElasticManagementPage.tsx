import React, { useContext } from "react";

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
      <div className="main-container-padding">
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

  if (state.pageState === PageState.Loading) {
    return <Loading />;
  }

  const normalPageState = (
    <>
      <div>
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
        <div className="main-container-padding">
          <div>
            <img src="https://pngicon.ru/file/uploads/cat_hungry.png" />
          </div>
        </div>
      )}
    </>
  );

  return (
    <div className="main-container-padding">
      {state.pageState == PageState.Normal && normalPageState}
      {state.pageState == PageState.NewIndexCreation && (
        <div className="main-container-padding">
          <NewIndexEditor />
        </div>
      )}
    </div>
  );
}

export default ElasticManagementPage;
