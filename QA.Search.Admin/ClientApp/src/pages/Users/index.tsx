import React from "react";
import UsersContainer from "./UsersContainer";
import UsersPage from "./UsersPage";

export default () => (
  <UsersContainer.Provider>
    <UsersPage />
  </UsersContainer.Provider>
);
