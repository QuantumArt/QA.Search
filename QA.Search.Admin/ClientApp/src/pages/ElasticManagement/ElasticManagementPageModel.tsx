import { ElasticManagementPageResponse } from "../../backend.generated";
export type ElasticManagementPageModel = {
  pageState: PageState;
  data: ElasticManagementPageResponse | null;
  operationsAreEnabled: boolean;
  indexesFilter: string | null;
  editorModel: NewIndexEditorModel;
};

export enum PageState {
  None,
  Loading,
  Normal,
  Error,
  NewIndexCreation
}

export type NewIndexEditorModel = {
  indexName: string;
  isError: boolean;
  isSuccess: boolean;
  isInProgress: boolean;
};
