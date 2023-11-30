import React, { useContext } from "react";

import { Button, InputGroup, Card, Intent, Tooltip } from "@blueprintjs/core";

import { PageState } from "./ElasticManagementPageModel";
import ElasticManagementPageContainer from "./ElasticManagementPageContainer";

function IndexesFilter({switchCreateIndexMode}) {
  const { state, applyFilter, clearFilter } = useContext(ElasticManagementPageContainer.Context);

  if (state.pageState === PageState.Error) {
    return null;
  }

  const lockButton = (
    <Tooltip content="Очистить">
      <Button icon="small-cross" intent={Intent.WARNING} minimal={true} onClick={clearFilter} />
    </Tooltip>
  );

  //const handleFilterChange = handleStringChange

  return (
    <Card style={{display: "flex", justifyContent: "space-between"}}>
      <div style={{width:"99%", marginRight: "1%"}}>
        <Tooltip 
          content="Имя индекса может содержать латинские буквы, цифры, символы минуса и нижнего подчёркивания."
         className="tooltip-inherit-size">
         <InputGroup
           large={true}
            leftIcon="search"
            placeholder="название индекса для поиска или алиас"
           rightElement={lockButton}
            value={state.indexesFilter || ""}
            onChange={event => applyFilter(event.target.value.toString().toLowerCase().replace(/[^A-Za-z0-9-_]/ig, ''))}
         />
        </Tooltip>
      </div>
      <Button
                icon="plus"
                intent={Intent.NONE}
                large={true}
                onClick={switchCreateIndexMode}
              />
    </Card>
  );
}

export default IndexesFilter;
