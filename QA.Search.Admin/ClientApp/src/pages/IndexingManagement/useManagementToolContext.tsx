import { useContext } from "react";
import { TargetQP } from "../../backend.generated";
import QpManagementToolContainer from "../QpIndexing/QpManagementToolContainer";
import QpUpdateManagementToolContainer from "../QpIndexing/QpUpdateManagementToolContainer";

function useManagementToolContext(targetQP: TargetQP) {
  let context = selectContext(targetQP);

  return useContext(context);

  function selectContext(targetQP: TargetQP) {
    switch (targetQP) {
      case TargetQP.IndexingQP:
        return QpManagementToolContainer.Context;
      case TargetQP.IndexingQPUpdate:
        return QpUpdateManagementToolContainer.Context;
    }
  }
}

export default useManagementToolContext;
