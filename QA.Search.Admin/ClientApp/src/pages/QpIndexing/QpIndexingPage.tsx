import React, { useState } from "react";
import QpManagementToolContainer from "./QpManagementToolContainer";
import IndexingManagementTool from "../IndexingManagement/IndexingManagementTool";
import { Tabs, Tab, Navbar } from "@blueprintjs/core";
import { TargetQP } from "../../backend.generated";
import QpUpdateManagementToolContainer from "./QpUpdateManagementToolContainer";
import Navigation from "../../components/Navigation";

function QpIndexingPage() {
  const [selectedTab, setState] = useState("tab1");

  return (
    <div className="main-container-padding">
      <Navigation.LeftPanel>
        <Navbar.Divider />
        <Tabs
          animate={true}
          id="navbar"
          large={true}
          onChange={e => setState(e.toString())}
          selectedTabId={selectedTab}
        >
          <Tab id="tab1" title="QP" />
          <Tab id="tab2" title="QP Update" />
        </Tabs>
      </Navigation.LeftPanel>
      <div>
        {selectedTab === "tab1" && (
          <QpManagementToolContainer.Provider>
            <IndexingManagementTool targetQP={TargetQP.IndexingQP} />
          </QpManagementToolContainer.Provider>
        )}
        {selectedTab === "tab2" && (
          <QpUpdateManagementToolContainer.Provider>
            <IndexingManagementTool targetQP={TargetQP.IndexingQPUpdate} />
          </QpUpdateManagementToolContainer.Provider>
        )}
      </div>
    </div>
  );
}
export default QpIndexingPage;
