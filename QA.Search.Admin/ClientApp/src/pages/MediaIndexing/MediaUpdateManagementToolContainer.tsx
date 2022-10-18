import {
  QpIndexingResponse,
  QpIndexingController,
  IndexingState,
  TargetQP
} from "../../backend.generated";
import React, { useEffect, useContext, useState, useRef } from "react";
import createContainer from "constate";
import createContextFactory from "../IndexingManagement/IndexingManagementToolContainerFactory";

export default createContainer(createContextFactory(TargetQP.IndexingMediaUpdate));
