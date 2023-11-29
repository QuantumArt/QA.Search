import React, { useEffect, useContext } from "react";

import UsersContainer from "./UsersContainer";
import UsersList from "./UsersList";
import UsersListFilter from "./UsersListFilter";

const UsersPage = () => {
  const { initUsersList } = useContext(UsersContainer.Context);
  useEffect(() => {
    initUsersList();
  }, []);

  return (
 <div style={{ padding: "20px 16px" }}>
  <UsersListFilter />
  <UsersList />
 </div>
  );
};

export default UsersPage;