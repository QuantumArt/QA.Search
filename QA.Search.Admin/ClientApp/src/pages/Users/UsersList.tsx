import React, { useState, useContext } from "react";
import { Card, Button, Intent, ButtonGroup, Tooltip, Tag, Position } from "@blueprintjs/core";
import { Row, Col } from "react-flexbox-grid";
import { UserResponse, UserRole } from "../../backend.generated";

import "./UsersList.css";
import UsersContainer from "./UsersContainer";
import InviteUserDialog from "./InviteUserDialog";
import DeleteUserDialog from "./DeleteUserDialog";
import SendResetPasswordLinkDialog from "./SendResetPasswordLinkDialog";
import Loading from "../../components/Loading";

interface State {
  showDeleteUserDialog: boolean;
  showResetPasswordDialog: boolean;
  showInviteUserDialog: boolean;
  selectedUser: UserResponse | null;
}

const UsersList = () => {
  const [state, setState] = useState<State>({
    showDeleteUserDialog: false,
    showResetPasswordDialog: false,
    showInviteUserDialog: false,
    selectedUser: null
  });
  const { usersList, initUsersList, loadMoreUsers, deleteUser } = useContext(
    UsersContainer.Context
  );

  return (
    <Card elevation={2}>
      {usersList.loading && <Loading />}
      <InviteUserDialog
        isOpen={state.showInviteUserDialog}
        onClose={() => {
          setState({ ...state, showInviteUserDialog: false });
        }}
        onSuccess={initUsersList}
      />
      <DeleteUserDialog
        user={state.selectedUser}
        isOpen={state.showDeleteUserDialog}
        deleteUser={deleteUser}
        onClose={() => {
          setState({ ...state, showDeleteUserDialog: false });
        }}
      />
      <SendResetPasswordLinkDialog
        user={state.selectedUser}
        isOpen={state.showResetPasswordDialog}
        onClose={() => {
          setState({ ...state, showResetPasswordDialog: false });
        }}
      />
      <Row between="xs">
        <Col>
          <h4 className="bp3-heading">Список пользователей</h4>
        </Col>
        <Col>
          <Button
            onClick={() => {
              setState({ ...state, showInviteUserDialog: true });
            }}
            icon="new-person"
            intent={Intent.PRIMARY}
            text="Добавить"
          />
        </Col>
      </Row>
      <Row>
        <table className="bp3-html-table bp3-html-table-striped bp3-html-table-bordered users-table">
          <thead>
            <tr>
              <th className="users-table__header-cell">Адрес электронной почты</th>
              <th className="users-table__header-cell">Роль в системе</th>
              <th className="users-table__header-cell">Доступные действия</th>
            </tr>
          </thead>
          <tbody>
            {usersList.data.map(user => {
              return (
                <tr key={user.id}>
                  <td className="users-table__cell">{user.email}</td>
                  <td className="users-table__cell users-table__cell-center-content">
                    <Tag intent={Intent.NONE}>{UserRole[user.role]}</Tag>
                  </td>
                  <td className="users-table__cell users-table__cell-center-content">
                    <ButtonGroup>
                      <Tooltip
                        content="Отправить ссылку для восстановления пароля"
                        position={Position.TOP}
                      >
                        <Button
                          onClick={() => {
                            setState({
                              ...state,
                              selectedUser: user,
                              showResetPasswordDialog: true
                            });
                          }}
                          icon="key"
                        />
                      </Tooltip>
                      <Tooltip content="Удалить" position={Position.TOP}>
                        <Button
                          onClick={() => {
                            setState({
                              ...state,
                              selectedUser: user,
                              showDeleteUserDialog: true
                            });
                          }}
                          icon="trash"
                        />
                      </Tooltip>
                    </ButtonGroup>
                  </td>
                </tr>
              );
            })}
          </tbody>
        </table>
      </Row>
      <br />
      <Row between="xs">
        <Col />
        <Col>
          {usersList.data.length < Number(usersList.totalCount) && (
            <Button
              loading={usersList.loading}
              icon="refresh"
              text="Загрузить еще"
              onClick={loadMoreUsers}
            />
          )}
        </Col>
        <Col>
          {usersList.totalCount && (
            <div>
              <b>Всего записей:</b> {usersList.totalCount}
            </div>
          )}
        </Col>
      </Row>
    </Card>
  );
};

export default UsersList;
