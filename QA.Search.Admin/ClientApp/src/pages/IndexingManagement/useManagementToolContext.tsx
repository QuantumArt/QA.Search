import { useContext } from "react";
import { TargetQP } from "../../backend.generated";
import QpManagementToolContainer from "../QpIndexing/QpManagementToolContainer";
import QpUpdateManagementToolContainer from "../QpIndexing/QpUpdateManagementToolContainer";
import MediaManagementToolContainer from "../MediaIndexing/MediaManagementToolContainer";
import MediaUpdateManagementToolContainer from "../MediaIndexing/MediaUpdateManagementToolContainer";

function useManagementToolContext(targetQP: TargetQP) {
  switch (targetQP) {
    case TargetQP.IndexingQP:
      return useContext(QpManagementToolContainer.Context);
    case TargetQP.IndexingQPUpdate:
      return useContext(QpUpdateManagementToolContainer.Context);
    case TargetQP.IndexingMedia:
      return useContext(MediaManagementToolContainer.Context);
    case TargetQP.IndexingMediaUpdate:
      return useContext(MediaUpdateManagementToolContainer.Context);
  }
}

export default useManagementToolContext;
