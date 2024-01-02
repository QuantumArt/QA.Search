import React, { useEffect, useState, useRef, useMemo } from "react";
import createContainer from "constate";
import { asc } from "../../utils/array";
import { ElasticManagementPageController, IndexesCardViewModel } from "../../backend.generated";
import { ElasticManagementPageModel, PageState } from "./ElasticManagementPageModel";
import dateConvertToLocal from "../../utils/time";

function CreateContext() {
  const defaultState: ElasticManagementPageModel = {
    pageState: PageState.None,
    data: null,
    operationsAreEnabled: false,
    indexesFilter: null,
    editorModel: {
      indexName: "",
      isInProgress: false,
      isError: false,
      isSuccess: false
    }
  };

  const [state, setState] = useState(defaultState);
  const intervalRef = useRef<NodeJS.Timeout | null>(null); // Ссылка на таймер обновления, которая сохраняется между рендерами

  useEffect(() => {
    setState(currentState => ({ ...currentState, pageState: PageState.Loading }));
    startUpdating();
    return () => {
      console.log("dispose");
      stopUpdating();
    };
  }, []); // Единожды, при первом рендеринге компонента

  function startUpdating() {
    if (intervalRef.current) {
      return;
    }
    updateData();
    intervalRef.current = setInterval(updateData, 5000);
  }

  function stopUpdating() {
    if (!intervalRef.current) {
      return;
    }
    clearInterval(intervalRef.current!);
    intervalRef.current = null;
  }

  async function updateData() {
    try {
      setState(currentState => ({ ...currentState, State: PageState.Loading }));
      let data = await new ElasticManagementPageController().loadData();

      if (!data || data.commonError) {
        throw new Error();
      }
      data = dateConvertToLocalInData(data);

      setState(currentState => {
        const opsAreEnabled = currentState.pageState !== PageState.Loading;
        var newPageState =
          currentState.pageState === PageState.NewIndexCreation
            ? PageState.NewIndexCreation
            : PageState.Normal;
        var newState = {
          ...currentState,
          data: { ...data },
          pageState: newPageState,
          operationsAreEnabled: opsAreEnabled
        };
        return newState;
      });
    } catch {
      setState(currentState => ({ ...currentState, pageState: PageState.Error }));
    }
  }

  function applyFilter(filter: string) {
    setState(currentState => ({ ...currentState, indexesFilter: filter }));
  }

  function clearFilter() {
    setState(currentState => ({ ...currentState, indexesFilter: null }));
  }

  const visibleCards = useMemo(() => {
    const re = new RegExp(`.*${state.indexesFilter}.*`);
    if (!state.data) {
      return [];
    }

    var filteredData = state.data!.cards!.filter(card => {
      if (!state.indexesFilter) {
        return true;
      }
      var res =
        (card.sourceIndex && card.sourceIndex.uiName && re.test(card.sourceIndex.uiName)) ||
        (card.destinationIndex &&
          card.destinationIndex.uiName &&
          re.test(card.destinationIndex.uiName)) ||
        (card.wrongIndexes &&
          card.wrongIndexes.filter(wi => wi.uiName && re.test(wi.uiName)).length > 0) ||
        (card.sourceIndex && card.sourceIndex.alias && re.test(card.sourceIndex.alias)) ||
        (card.destinationIndex &&
          card.destinationIndex.alias &&
          re.test(card.destinationIndex.alias)) ||
        (card.wrongIndexes &&
          card.wrongIndexes.filter(wi => wi.alias && re.test(wi.alias)).length > 0);
      return res;
    });

    return filteredData.sort(asc(getCardDisplayName));
  }, [state.indexesFilter, state.data && state.data.cards]);

  async function startIndexing(card: IndexesCardViewModel) {
    if (!card.sourceIndex || !card.sourceIndex.fullName) {
      return;
    }
    try {
      setState(currentState => ({ ...currentState, operationsAreEnabled: false }));
      await new ElasticManagementPageController().createReindexTask(card.sourceIndex.fullName);
      await updateData();
      setState(currentState => ({ ...currentState, operationsAreEnabled: true }));
    } catch {
      setState(currentState => ({
        ...currentState,
        pageState: PageState.Error,
        operationsAreEnabled: true
      }));
    }

    console.log("переиндексация", card.sourceIndex!.fullName);
    setState(currentState => ({ ...currentState, operationsAreEnabled: false }));
  }

  function switchCreateIndexMode(isOn: boolean) {
    setState(currentState => ({
      ...currentState,
      pageState: isOn ? PageState.NewIndexCreation : PageState.Normal,
      editorModel: currentState.editorModel.isSuccess
        ? { ...currentState.editorModel, isSuccess: false }
        : { ...currentState.editorModel }
    }));
  }

  function setNewIndexName(name: string) {
    setState(currentState => ({
      ...currentState,
      editorModel: { ...currentState.editorModel, indexName: name }
    }));
  }

  async function createNewIndex() {
    if (state.pageState !== PageState.NewIndexCreation || !state.editorModel.indexName) {
      return;
    }
    setState(currentState => ({
      ...currentState,
      editorModel: {
        ...currentState.editorModel,
        isInProgress: true
      }
    }));
    try {
      const em = state.editorModel;
      await new ElasticManagementPageController().createNewIndex(em.indexName);
      await updateData();
      setState(currentState => ({
        ...currentState,
        indexesFilter: em.indexName,
        editorModel: {
          ...currentState.editorModel,
          isInProgress: false,
          isSuccess: true,
          indexName: ""
        }
      }));
    } catch {
      setState(currentState => ({
        ...currentState,
        editorModel: {
          ...currentState.editorModel,
          isInProgress: false,
          isSuccess: false,
          isError: true
        }
      }));
    }
  }

  const indexCreationIsEnabled = useMemo(() => !!state.editorModel.indexName, [
    state.editorModel.indexName
  ]);

  async function deleteIndex(indexFullName: string | null) {
    if (!indexFullName) {
      return;
    }
    try {
      setState(currentState => ({ ...currentState, operationsAreEnabled: false }));
      await new ElasticManagementPageController().deleteIndex(indexFullName);
      await updateData();
      setState(currentState => ({ ...currentState, operationsAreEnabled: true }));
    } catch {
      setState(currentState => ({
        ...currentState,
        pageState: PageState.Error,
        operationsAreEnabled: true
      }));
    }
  }

  function dateConvertToLocalInData(data) {
    for (let i = 0; i < data.cards.length; i++) {
      if (data.cards[i].destinationIndex != null) {
        data.cards[i].destinationIndex.creationDate = dateConvertToLocal(
          data.cards[i].destinationIndex.creationDate
        );
      }

      if (data.cards[i].reindexTask != null) {
        data.cards[i].reindexTask.created = dateConvertToLocal(data.cards[i].reindexTask.created);
        data.cards[i].reindexTask.finished = dateConvertToLocal(data.cards[i].reindexTask.finished);
        data.cards[i].reindexTask.lastUpdated = dateConvertToLocal(
          data.cards[i].reindexTask.lastUpdated
        );
      }

      if (data.cards[i].sourceIndex != null) {
        data.cards[i].sourceIndex.creationDate = dateConvertToLocal(
          data.cards[i].sourceIndex.creationDate
        );
      }
    }

    return data;
  }
  //const delay = msec => new Promise(resolve => setTimeout(resolve, msec));

  return {
    state,
    visibleCards,
    applyFilter,
    clearFilter,
    startIndexing,
    switchCreateIndexMode,
    setNewIndexName,
    createNewIndex,
    indexCreationIsEnabled,
    deleteIndex
  };
}

export function getCardDisplayName(card: IndexesCardViewModel) {
  return (
    (card.sourceIndex && card.sourceIndex.uiName) ||
    (card.wrongIndexes && card.wrongIndexes.length > 0 && card.wrongIndexes[0].uiName) ||
    "n/d"
  );
}

export default createContainer(CreateContext);
