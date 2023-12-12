import React, { useContext, useState } from "react";

import {
  Button,
  Card,
  Intent,
  Tooltip,
  HTMLTable,
  Icon,
  MenuItem,
  Menu,
  Popover,
  Position,
  ProgressBar
} from "@blueprintjs/core";

import Loading from "../../components/Loading";
import { PageState } from "./ElasticManagementPageModel";
import ElasticManagementPageContainer, {
  getCardDisplayName
} from "./ElasticManagementPageContainer";
import {
  IndexesCardViewModel,
  ElasticIndexViewModel,
  ReindexTaskStatus,
  ReindexTaskViewModel
} from "../../backend.generated";

type Props = {
  indexesCard: IndexesCardViewModel;
};

function IndexesCardDetails({ indexesCard }: Props) {
  const { state, startIndexing, deleteIndex } = useContext(ElasticManagementPageContainer.Context);
  const [taskDetailsIsOpen, setTaskDetailsIsOpen] = useState(false);

  if (state.pageState === PageState.Error) {
    return null;
  }

  const reindexButton = (card: IndexesCardViewModel) => {
    return (
      <Tooltip content="Запускает задачу переиндексации для индекса">
        <Button
          disabled={!state.operationsAreEnabled}
          icon="automatic-updates"
          intent={Intent.PRIMARY}
          minimal={false}
          onClick={() => startIndexing(card)}
        >
          Запустить переиндексацию
        </Button>
      </Tooltip>
    );
  };

  const indexRow = (
    index: ElasticIndexViewModel,
    key: number,
    hasTaskWithActiveStatus: boolean
  ) => {
    if (!index) {
      return null;
    }
    return (
      <tr key={String(key)} style={{ wordBreak: "break-word" }}>
        <td>{index.fullName}</td>
        <td>{index.alias}</td>
        <td>{index.creationDate}</td>
        <td>
          {index.readonly || hasTaskWithActiveStatus ? (
            ""
          ) : (
            <Tooltip content="Удалить индекс">
              {indexDeleteButton(index, hasTaskWithActiveStatus)}
            </Tooltip>
          )}
        </td>
      </tr>
    );
  };

  const indexDeleteButton = (index: ElasticIndexViewModel, hasTaskWithActiveStatus: boolean) => {
    if (!index || index.readonly) {
      return null;
    }
    const exampleMenu = (
      <Menu>
        <MenuItem
          disabled={!state.operationsAreEnabled || hasTaskWithActiveStatus}
          icon="warning-sign"
          text="Да, все верно, удалить"
          intent={Intent.WARNING}
          onClick={() => deleteIndex(index.fullName || "")}
        />
      </Menu>
    );
    return (
      <Popover content={exampleMenu} position={Position.RIGHT_BOTTOM}>
        <Button
          disabled={!state.operationsAreEnabled || hasTaskWithActiveStatus}
          icon="small-cross"
          text=""
          intent={Intent.DANGER}
          minimal={true}
        />
      </Popover>
    );
  };

  const indexesTable = (
    <HTMLTable bordered condensed className="indexes-card-details-html-table">
      <thead>
        <tr>
          <th>Название</th>
          <th>Алиас</th>
          <th>Создан</th>
          <th>&nbsp;</th>
        </tr>
      </thead>
      <tbody>
        {indexesCard.sourceIndex &&
          indexRow(indexesCard.sourceIndex, 1, indexesCard.hasTaskWithActiveStatus)}
        {indexesCard.destinationIndex &&
          indexRow(indexesCard.destinationIndex, 2, indexesCard.hasTaskWithActiveStatus)}
        {indexesCard.wrongIndexes &&
          indexesCard.wrongIndexes.map((wi, i) =>
            indexRow(wi, i, indexesCard.hasTaskWithActiveStatus)
          )}
      </tbody>
    </HTMLTable>
  );

  function createTaskDetails(reindexTask: ReindexTaskViewModel | null) {
    if (reindexTask === null || reindexTask === undefined) {
      return null;
    }
    if (!taskDetailsIsOpen) {
      return (
        <Button
          icon="plus"
          intent={Intent.NONE}
          minimal={false}
          onClick={() => setTaskDetailsIsOpen(true)}
        >
          Показать сведения о задаче
        </Button>
      );
    }
    function getStatusDescr(status: ReindexTaskStatus) {
      switch (status) {
        case ReindexTaskStatus.AwaitStart:
          return "Ожидает запуска";
        case ReindexTaskStatus.CancelledByWorker:
          return "Завершена службой";
        case ReindexTaskStatus.Completed:
          return "Выполнена";
        case ReindexTaskStatus.Failed:
          return "Завершилась с ошибкой";
        case ReindexTaskStatus.ReindexOneAndAliasesSwap:
          return "Первая переиндексация";
        case ReindexTaskStatus.ReindexTwo:
          return "Вторая переиндексация";
      }
    }
    const isInProgress =
      reindexTask.status == ReindexTaskStatus.ReindexOneAndAliasesSwap ||
      reindexTask.status == ReindexTaskStatus.ReindexTwo;
    return (
      <>
        <Button
          icon="minus"
          intent={Intent.NONE}
          minimal={false}
          onClick={() => setTaskDetailsIsOpen(false)}
        >
          Скрыть подробности
        </Button>
        <HTMLTable bordered condensed>
          <tbody>
            <tr>
              <td colSpan={2} className="box-shadow-none" >
                <h5>Связанная задача</h5>
              </td>
            </tr>
            {reindexTask.sourceIndex && (
              <tr>
                <td>Исходный индекс</td>
                <td>{reindexTask.sourceIndex}</td>
              </tr>
            )}
            {reindexTask.destinationIndex && (
              <tr>
                <td>Целевой индекс</td>
                <td>{reindexTask.destinationIndex}</td>
              </tr>
            )}
            <tr>
              <td>Статуc</td>
              <td>{getStatusDescr(reindexTask.status)}</td>
            </tr>
            <tr>
              <td>Создана</td>
              <td>{reindexTask.created}</td>
            </tr>
            <tr>
              <td>Обновлена</td>
              <td>{reindexTask.lastUpdated}</td>
            </tr>
            <tr>
              <td>Завершена</td>
              <td>{reindexTask.finished}</td>
            </tr>
            {isInProgress && (
              <tr>
                <td>Всего документов</td>
                <td>{reindexTask.totalDocuments}</td>
              </tr>
            )}
            {isInProgress && (
              <tr>
                <td>Создано документов</td>
                <td>{reindexTask.createdDocuments}</td>
              </tr>
            )}
            {isInProgress && (
              <tr>
                <td>Удалено документов</td>
                <td>{reindexTask.deletedDocuments}</td>
              </tr>
            )}
            {isInProgress && (
              <tr>
                <td>Обновлено документов</td>
                <td>{reindexTask.updatedDocuments}</td>
              </tr>
            )}
            {isInProgress && (
              <tr>
                <td>Прогрес</td>
                <td>{reindexTask.percentage}%</td>
              </tr>
            )}
            {isInProgress && (
              <tr>
                <td colSpan={2}>
                  <ProgressBar intent="primary" value={reindexTask.percentage / 100} />
                </td>
              </tr>
            )}
          </tbody>
        </HTMLTable>
      </>
    );
  }

  const readonlyMessage = indexesCard.isReadonly ? (
    <Tooltip content="Информация об индексе доступна только для чтения">
      <Icon icon="lock" intent={Intent.PRIMARY} />
    </Tooltip>
  ) : null;
  const wrongStateMessage =
    indexesCard.wrongIndexes && indexesCard.wrongIndexes.length > 0 ? (
      <Tooltip content="Требуется исправить состояние вручную">
        <Icon icon="build" intent={Intent.WARNING} />
      </Tooltip>
    ) : null;
  const canRunReindexStateMessage = indexesCard.canRunNewTask ? (
    <Tooltip content="Для индекса может быть запущена задача переиндексации">
      <Icon icon="tick" intent={Intent.SUCCESS} />
    </Tooltip>
  ) : null;

  return (
    <Card elevation={2} className="elastic-card-detal">
      <div className="elastic-card-top-element">
        <h5 className="bp3-heading">
          {readonlyMessage}
          {wrongStateMessage}
          {canRunReindexStateMessage}
          &nbsp;
          {getCardDisplayName(indexesCard)}
        </h5>
        <div className="text-align-right">
          {indexesCard.canRunNewTask && reindexButton(indexesCard)}
        </div>
      </div>
      <div className="elastic-card-bottom-element">{indexesTable}</div>
      {indexesCard.reindexTask && (
        <div className="elastic-card-bottom-element">
          {createTaskDetails(indexesCard.reindexTask)}
        </div>
      )}
      {!indexesCard.reindexTask && indexesCard.lastFinishedReindexTask && (
        <div className="elastic-card-bottom-element">
          {createTaskDetails(indexesCard.lastFinishedReindexTask)}
        </div>
      )}
    </Card>
  );
}

export default IndexesCardDetails;
