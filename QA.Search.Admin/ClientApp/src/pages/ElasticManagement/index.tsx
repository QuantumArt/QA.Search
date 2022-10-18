import React from "react";
import ElasticManagementPage from "./ElasticManagementPage";
import ElasticManagementPageContainer from "./ElasticManagementPageContainer";

export default () => (
  <ElasticManagementPageContainer.Provider>
    <ElasticManagementPage />
  </ElasticManagementPageContainer.Provider>
);
