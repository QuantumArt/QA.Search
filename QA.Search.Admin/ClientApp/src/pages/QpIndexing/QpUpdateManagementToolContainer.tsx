import {
  QpIndexingResponse,
  QpIndexingController,
  IndexingState,
  TargetQP
} from "../../backend.generated";
import React, { useEffect, useContext, useState, useRef } from "react";
import createContainer from "constate";
import CreateContextFactory from "../IndexingManagement/IndexingManagementToolContainerFactory";

export default createContainer(CreateContextFactory(TargetQP.IndexingQPUpdate));
