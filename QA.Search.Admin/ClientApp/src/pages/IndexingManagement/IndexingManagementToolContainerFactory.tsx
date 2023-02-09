import {
  QpIndexingResponse,
  QpIndexingController,
  IndexingState,
  TargetQP
} from "../../backend.generated";
import React, { useEffect, useContext, useState, useRef } from "react";

export type IndexingManagementToolModel = {
  pageState: IndexingToolState;
  indexingServiceState: QpIndexingResponse | null;
  currentIndexingButton: IndexingButton;
  indexingOperationsAreEnabled: boolean;
};

export enum IndexingToolState {
  None,
  Loading,
  Normal,
  Error
}

/** Доступная кнопка операций индексирования */
export enum IndexingButton {
  Start,
  Stop,
  None
}

/** Возвращает функцию, создающую контейнер */
function CreateContextFactory(targetQP: TargetQP) {
  function CreateContext() {
    const defaultState: IndexingManagementToolModel = {
      indexingServiceState: null,
      pageState: IndexingToolState.None,
      indexingOperationsAreEnabled: false,
      currentIndexingButton: IndexingButton.None
    };

    const [state, setState] = useState(defaultState);
    const intervalRef = useRef<NodeJS.Timeout | null>(null);

    useEffect(() => {
      setState(currentState => ({ ...currentState, pageState: IndexingToolState.Loading }));
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
      updateServiceState();
      intervalRef.current = setInterval(updateServiceState, 5000);
    }

    function stopUpdating() {
      if (!intervalRef.current) {
        return;
      }
      clearInterval(intervalRef.current!);
      intervalRef.current = null;
    }

    async function updateServiceState() {
      try {
        const serviceState = await new QpIndexingController().getIndexingStatus(targetQP);
        if (!serviceState) {
          throw new Error();
        }
        setState(currentState => {
          const iOpsAreEnabled =
            currentState.pageState === IndexingToolState.Loading // После окончания первой загрузки
              ? true
              : getIndexingOperationEnabled(serviceState);

          var newState = {
            ...currentState,
            indexingServiceState: { ...serviceState },
            pageState: IndexingToolState.Normal,
            indexingOperationsAreEnabled: iOpsAreEnabled,
            currentIndexingButton: getIndexingButton(serviceState)
          };
          return newState;
        });
      } catch {
        setState(currentState => ({ ...currentState, pageState: IndexingToolState.Error }));
      }
    }

    async function indexingStart() {
      console.log("indexingStart");
      try {
        stopUpdating();
        await new QpIndexingController().startIndexing(targetQP);
        startUpdating();
      } catch {
        setState(currentState => {
          return {
            ...currentState,
            pageState: IndexingToolState.Error
          };
        });
      }
    }

    async function indexingStop() {
      console.log("indexingStop");
      try {
        stopUpdating();
        await new QpIndexingController().stopIndexing(targetQP);
        startUpdating();
      } catch {
        setState(currentState => {
          return {
            ...currentState,
            pageState: IndexingToolState.Error
          };
        });
      }
    }

    /** Определить какая кнопка должна быть показана */
    function getIndexingButton(serviceState: QpIndexingResponse | null): IndexingButton {
      if (!serviceState) {
        return IndexingButton.None;
      }
      switch (serviceState.state) {
        case IndexingState.Stopped:
        case IndexingState.AwaitingRun:
          return IndexingButton.Start;
        case IndexingState.Running:
        case IndexingState.AwaitingStop:
          return IndexingButton.Stop;
        default:
          return IndexingButton.Start;
      }
    }

    function getIndexingOperationEnabled(serviceState: QpIndexingResponse | null): boolean {
      if (!serviceState) {
        return false;
      }
      switch (serviceState.state) {
        case IndexingState.Stopped:
        case IndexingState.Running:
          return true;
        case IndexingState.AwaitingRun:
        case IndexingState.AwaitingStop:
          return false;
        default:
          return true;
      }
    }

    return {
      state,
      indexingStart,
      indexingStop
    };
  }

  return CreateContext;
}

export default CreateContextFactory;
