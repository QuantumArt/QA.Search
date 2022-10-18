import React, { useContext } from "react";
import { Grid, Row, Col } from "react-flexbox-grid";

import {
  Button,
  NonIdealState,
  InputGroup,
  Card,
  Intent,
  Tooltip,
  FormGroup,
  Callout
} from "@blueprintjs/core";

import { PageState } from "./ElasticManagementPageModel";
import ElasticManagementPageContainer from "./ElasticManagementPageContainer";

function NewIndexEditor() {
  const {
    state,
    switchCreateIndexMode,
    setNewIndexName,
    createNewIndex,
    indexCreationIsEnabled
  } = useContext(ElasticManagementPageContainer.Context);

  if (state.pageState === PageState.Error) {
    return null;
  }

  return (
    <Card elevation={2} style={{ paddingTop: "20px" }}>
      <h5 className="bp3-heading">Создание нового индекса</h5>
      <FormGroup
        label="Название нового индекса"
        labelFor="name-input"
        labelInfo="(Префикс и дата будут подставлены автоматом)"
      >
        <InputGroup
          id="name-input"
          large={true}
          placeholder="введите название нового индекса"
          value={state.editorModel.indexName || ""}
          onChange={event => setNewIndexName(event.target.value)}
        />
      </FormGroup>

      {state.editorModel.isError && (
        <Callout intent={Intent.DANGER} icon="error" title="Ошибка">
          При попытке создания индекса произошла ошибка
        </Callout>
      )}

      {state.editorModel.isSuccess && (
        <Callout intent={Intent.SUCCESS} icon="tick" title="Готово">
          Новый индекс был успешно создан
          <br />
          <br />
          <FormGroup>
            <Button
              large
              text="Продолжить"
              intent={Intent.SUCCESS}
              onClick={() => switchCreateIndexMode(false)}
            />
          </FormGroup>
        </Callout>
      )}
      {!state.editorModel.isSuccess && (
        <FormGroup style={{ paddingTop: "20px" }}>
          <Button
            disabled={state.editorModel.isInProgress || !indexCreationIsEnabled}
            large
            text="Создать индекс"
            intent={Intent.PRIMARY}
            onClick={() => createNewIndex()}
          />
          {"   "}
          <Button
            disabled={state.editorModel.isInProgress}
            large
            text="Отмена"
            intent={Intent.NONE}
            onClick={() => switchCreateIndexMode(false)}
          />
        </FormGroup>
      )}
    </Card>
  );
}

export default NewIndexEditor;
