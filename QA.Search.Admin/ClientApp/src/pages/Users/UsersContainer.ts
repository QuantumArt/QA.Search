import { useState } from "react";
import constate from "constate";
import {
  UserResponse,
  UserRole,
  UsersController,
  UsersListResponse
} from "../../backend.generated";
import Toaster from "../../utils/toaster";
import { Intent } from "@blueprintjs/core";

const PAGE_SIZE = 4;

interface UsersList {
  loading: boolean;
  data: UserResponse[];
  totalCount: number;
  emailFilter: string | null;
  roleFilter: string | null;
}

const defaultState: UsersList = {
  loading: false,
  data: [],
  totalCount: 0,
  emailFilter: null,
  roleFilter: null
};

function useUsersContainer() {
  const [state, setState] = useState(defaultState);

  async function initUsersList(filters: any = null) {
    const emailFilter = (filters && filters.email) || null;
    const roleFilter = (filters && filters.role) || null;

    if (filters && emailFilter === state.emailFilter && roleFilter === state.roleFilter) {
      return;
    }

    setState({ ...state, loading: true });

    try {
      const response = <UsersListResponse>(
        await new UsersController().listUsers(
          PAGE_SIZE,
          0,
          emailFilter ? emailFilter : undefined,
          roleFilter ? roleFilter : undefined
        )
      );

      setState({
        ...state,
        data: response.data || [],
        loading: false,
        totalCount: response.totalCount,
        emailFilter: emailFilter,
        roleFilter: roleFilter
      });
    } catch {
      Toaster.show({
        message: "При выполнении запроса произошла ошибка",
        intent: Intent.DANGER
      });
    }
  }

  async function loadMoreUsers() {
    if (state.totalCount == null || state.data.length >= state.totalCount) {
      return;
    }

    setState({ ...state, loading: true });

    try {
      const response = <UsersListResponse>(
        await new UsersController().listUsers(
          PAGE_SIZE,
          state.data.length,
          state.emailFilter ? state.emailFilter : undefined,
          state.roleFilter ? UserRole[state.roleFilter] : undefined
        )
      );

      setState({
        ...state,
        data: [...state.data, ...response.data],
        loading: false,
        totalCount: response.totalCount
      });
    } catch {
      Toaster.show({
        message: "При выполнении запроса произошла ошибка",
        intent: Intent.DANGER
      });
    }
  }

  async function deleteUser(id: number) {
    try {
      await new UsersController().deleteUser(id);
      setState({
        ...state,
        data: state.data.filter(item => item.id !== id),
        totalCount: state.totalCount - 1
      });
    } catch {
      Toaster.show({
        message: "При выполнении запроса произошла ошибка",
        intent: Intent.DANGER
      });
    }
  }

  return { usersList: state, initUsersList, loadMoreUsers, deleteUser };
}

export default constate(useUsersContainer);
