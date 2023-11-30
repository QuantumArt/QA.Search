import React, { ReactNode, useRef, useState, useLayoutEffect } from "react";
import { withRouter, RouteComponentProps } from "react-router-dom";
import constate from "constate";
import {
  Navbar,
  Button,
  Popover,
  Intent,
  Menu,
  MenuItem,
  PopoverInteractionKind
} from "@blueprintjs/core";
import { UserResponse } from "../backend.generated";

interface Props extends RouteComponentProps {
  user: UserResponse;
  logout: () => Promise<void>;
}

const usePanels = constate(() => {
  const ref = useRef({
    setLeftPanel(_panel: ReactNode) {},
    setRightPanel(_panel: ReactNode) {}
  });
  return ref.current;
});

const NavigationPanel = withRouter(({ user, logout }: Props) => {
  const [leftPanel, setLeftPanel] = useState<ReactNode>(null);
  const [rightPanel, setRightPanel] = useState<ReactNode>(null);

  Object.assign(usePanels(), { setLeftPanel, setRightPanel });

  return (
    <Navbar  style={{position:"sticky", top: "0"}}>
      <Navbar.Group align="left">
        <Navbar.Heading>Search Admin App</Navbar.Heading>
      </Navbar.Group>
      {leftPanel && <Navbar.Group align="left">{leftPanel}</Navbar.Group>}
      {rightPanel && <Navbar.Group align="right">{rightPanel}</Navbar.Group>}
      <Navbar.Group align="right">
        <Popover position="auto" minimal interactionKind={PopoverInteractionKind.HOVER}>
          <Button
            minimal
            icon="user"
            rightIcon="caret-down"
            intent={Intent.NONE}
            text={user.email}
          />
          <Menu>
            {/* <MenuItem icon="settings" text="Настройки" /> */}
            <MenuItem icon="log-out" text="Выход" onClick={logout} />
          </Menu>
        </Popover>
      </Navbar.Group>
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
    []
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
    []
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
