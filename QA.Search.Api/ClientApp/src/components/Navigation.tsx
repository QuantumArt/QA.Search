import React, { ReactNode, useState, useRef, useLayoutEffect } from "react";
import { Link, withRouter, RouteComponentProps } from "react-router-dom";
import constate from "constate";
import { Navbar, Button } from "@blueprintjs/core";
import { downloadFile } from "../utils/clipboard";
import "./Navigation.scss";

// @ts-ignore
import SearchClient from "!!raw-loader!../SearchClient.ts"; // eslint-disable-line

const usePanels = constate(() => {
  const ref = useRef({
    setLeftPanel(_panel: ReactNode) {},
    setRightPanel(_panel: ReactNode) {}
  });
  return ref.current;
});

const NavigationPanel = withRouter(({ location }: RouteComponentProps) => {
  const [leftPanel, setLeftPanel] = useState<ReactNode>(null);
  const [rightPanel, setRightPanel] = useState<ReactNode>(null);

  Object.assign(usePanels(), { setLeftPanel, setRightPanel });

  return (
    <Navbar className="navigation__panel">
      <Navbar.Group align="left">
        <Navbar.Heading>Search API</Navbar.Heading>
        <Link to="/demo">
          <Button
            active={location.pathname === "/demo"}
            className="bp3-minimal"
            icon="page-layout"
            text="Demo"
          />
        </Link>
        <Link to="/demo2">
          <Button
            active={location.pathname === "/demo2"}
            className="bp3-minimal"
            icon="page-layout"
            text="Demo2"
          />
        </Link>
        <Link to="/search">
          <Button
            active={location.pathname === "/search"}
            className="bp3-minimal"
            icon="search"
            text="Search"
          />
        </Link>
        <Link to="/suggest">
          <Button
            active={location.pathname === "/suggest"}
            className="bp3-minimal"
            icon="search-template"
            text="Suggest"
          />
        </Link>
        <Link to="/completion">
          <Button
            active={location.pathname === "/completion"}
            className="bp3-minimal"
            icon="text-highlight"
            text="Completion"
          />
        </Link>
        <Navbar.Divider />
        <Link to="/swagger" target="_blank">
          <Button className="bp3-minimal" icon="document-open" text="Swagger" />
        </Link>
        <Link to="/redoc" target="_blank">
          <Button className="bp3-minimal" icon="document-open" text="ReDoc" />
        </Link>
        <Button
          className="bp3-minimal"
          icon="download"
          text="TS Client"
          title="Download TypeScript Client"
          onClick={() => downloadFile("SearchClient.ts", SearchClient)}
        />
      </Navbar.Group>
      {leftPanel && <Navbar.Group align="left">{leftPanel}</Navbar.Group>}
      {rightPanel && <Navbar.Group align="right">{rightPanel}</Navbar.Group>}
    </Navbar>
  );
});

const LeftPanel = ({ children }: { children: ReactNode }) => {
  const { setLeftPanel } = usePanels();
  useLayoutEffect(() => {
    setLeftPanel(children);
  });
  useLayoutEffect(
    () => () => {
      setLeftPanel(null);
    },
    [] // eslint-disable-line react-hooks/exhaustive-deps
  );
  return null;
};

const RightPanel = ({ children }: { children: ReactNode }) => {
  const { setRightPanel } = usePanels();
  useLayoutEffect(() => {
    setRightPanel(children);
  });
  useLayoutEffect(
    () => () => {
      setRightPanel(null);
    },
    [] // eslint-disable-line react-hooks/exhaustive-deps
  );
  return null;
};

const Navigation = {
  LeftPanel,
  RightPanel,
  Panel: NavigationPanel,
  Provider: usePanels.Provider
};

export default Navigation;
