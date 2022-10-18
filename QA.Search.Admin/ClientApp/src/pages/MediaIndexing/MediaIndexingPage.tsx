import React, { useState } from "react";
import { Grid, Row, Col } from "react-flexbox-grid";
import { Tabs, Tab, Navbar } from "@blueprintjs/core";
import MediaUpdateManagementToolContainer from "./MediaUpdateManagementToolContainer";
import MediaManagementToolContainer from "./MediaManagementToolContainer";
import IndexingManagementTool from "../IndexingManagement/IndexingManagementTool";
import { TargetQP } from "../../backend.generated";
import Navigation from "../../components/Navigation";

function MediaIndexingPage() {
  const [selectedTab, setState] = useState("tab1");

  return (
    <Grid fluid>
      <Navigation.LeftPanel>
        <Navbar.Divider />
        <Tabs
          animate={true}
          id="navbar"
          large={true}
          onChange={e => setState(e.toString())}
          selectedTabId={selectedTab}
        >
          <Tab id="tab1" title="Media" />
          <Tab id="tab2" title="Media Update" />
        </Tabs>
      </Navigation.LeftPanel>

      <Row around="xs" style={{ paddingTop: "20px" }}>
        <Col xs={12}>
          {selectedTab === "tab1" && (
            <MediaManagementToolContainer.Provider>
              <IndexingManagementTool targetQP={TargetQP.IndexingMedia} />
            </MediaManagementToolContainer.Provider>
          )}
          {selectedTab === "tab2" && (
            <MediaUpdateManagementToolContainer.Provider>
              <IndexingManagementTool targetQP={TargetQP.IndexingMediaUpdate} />
            </MediaUpdateManagementToolContainer.Provider>
          )}
        </Col>
      </Row>
    </Grid>
  );
}
export default MediaIndexingPage;
