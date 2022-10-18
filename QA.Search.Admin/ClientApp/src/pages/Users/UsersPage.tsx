import React, { useEffect, useContext } from "react";
import { Grid, Row, Col } from "react-flexbox-grid";

import UsersContainer from "./UsersContainer";
import UsersList from "./UsersList";
import UsersListFilter from "./UsersListFilter";

const UsersPage = () => {
  const { initUsersList } = useContext(UsersContainer.Context);
  useEffect(() => {
    initUsersList();
  }, []);

  return (
    <Grid fluid>
      <Row around="xs" style={{ paddingTop: "20px" }}>
        <Col xs>
          <UsersList />
        </Col>
        <Col xs={3}>
          <UsersListFilter />
        </Col>
      </Row>
    </Grid>
  );
};

export default UsersPage;
