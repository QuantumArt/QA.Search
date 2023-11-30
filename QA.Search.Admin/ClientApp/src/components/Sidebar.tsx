import React, { useState, useCallback } from "react";
import { Link, withRouter, RouteComponentProps } from "react-router-dom";
import cn from "classnames";
import { Icon } from "@blueprintjs/core";
import { UserResponse, UserRole } from "../backend.generated";
import "./Sidebar.scss";

interface Props extends RouteComponentProps {
  user: UserResponse;
}

const Sidebar = withRouter(({ location, user }: Props) => {
  const [collapsed, setCollapsed] = useState(
    () => sessionStorage.getItem("Sidebar.Collapsed") === "true"
  );

  const toggleSidebar = useCallback(() => {
    setCollapsed(!collapsed);
    sessionStorage.setItem("Sidebar.Collapsed", String(!collapsed));
  }, [collapsed]);

  return (
    <aside style={{ position:"sticky", top: "50px" }} className={cn("sidebar", { "sidebar--collapsed": collapsed })}>
      <main className="sidebar__content">
        {user.role === UserRole.Admin && (
          <Link
            to="/users"
            className={cn("sidebar__link", {
              "sidebar__link--active": location.pathname == "/users"
            })}
          >
            <Icon icon="people" iconSize={24} htmlTitle="Пользователи" />
            <div className="sidebar__link-text">Пользователи</div>
          </Link>
        )}
        {(user.role === UserRole.Admin || user.role === UserRole.User) && (
          <Link
            to="/elastic"
            className={cn("sidebar__link", {
              "sidebar__link--active": location.pathname == "/elastic"
            })}
          >
            <Icon icon="list" iconSize={24} htmlTitle="Индексы Elastic" />
            <div className="sidebar__link-text">Индексы Elastic</div>
          </Link>
        )}
        {user.role === UserRole.Admin && (
          <Link
            to="/templates"
            className={cn("sidebar__link", {
              "sidebar__link--active": location.pathname == "/templates"
            })}
          >
            <Icon icon="list-columns" iconSize={24} htmlTitle="Шаблоны Elastic" />
            <div className="sidebar__link-text">Шаблоны Elastic</div>
          </Link>
        )}
        {(user.role === UserRole.Admin || user.role === UserRole.User) && (
          <>
            <div className="sidebar__divider" />
            <Link
              to="/qp-indexing"
              className={cn("sidebar__link", {
                "sidebar__link--active": location.pathname == "/qp-indexing"
              })}
            >
              <Icon icon="automatic-updates" iconSize={24} htmlTitle="Индексация QP" />
              <div className="sidebar__link-text">Индексация QP</div>
            </Link>
          </>
        )}
      </main>
      <footer className="sidebar__footer">
        <div className="sidebar__link" onClick={toggleSidebar}>
          <Icon
            icon={collapsed ? "circle-arrow-right" : "circle-arrow-left"}
            iconSize={24}
            htmlTitle={collapsed ? "Развернуть" : "Свернуть"}
          />
          <div className="sidebar__link-text">Свернуть</div>
        </div>
      </footer>
    </aside>
  );
});

export default Sidebar;
