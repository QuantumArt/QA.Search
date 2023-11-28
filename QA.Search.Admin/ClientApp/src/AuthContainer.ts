import { useState } from "react";
import constate from "constate";
import { UserResponse } from "./backend.generated";

interface AuthInfo {
  loading: boolean;
  user: UserResponse | null;
}

const defaultState: AuthInfo = {
  loading: true,
  user: null
};

function useAuthContainer() {
  const [authInfo, setAuthInfo] = useState(defaultState);

  return { authInfo, setAuthInfo };
}

export default constate(useAuthContainer);
