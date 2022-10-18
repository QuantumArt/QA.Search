import React, { useState, useEffect } from "react";
import { withRouter, RouteComponentProps } from "react-router";
import Loading from "../../components/Loading";
import SetPasswordError from "./SetPasswordError";
import SetPasswordForm from "./SetPasswordForm";
import { UserResponse, AccountController } from "../../backend.generated";
import CardLayout from "../../components/CardLayout";

interface Props extends RouteComponentProps<{ uid: string }> {}

interface State {
  loading: boolean;
  user: null | UserResponse;
  errorText: string;
}

const SetPasswordPage = ({ history, match }: Props) => {
  const { uid } = match.params;
  const [state, setState] = useState<State>({
    loading: true,
    user: null,
    errorText: ""
  });

  async function validateUid() {
    try {
      const user = await new AccountController().checkResetPasswordLink(uid);
      setState({
        ...state,
        loading: false,
        user: user
      });
    } catch (error) {
      setState({
        ...state,
        loading: false,
        user: null,
        errorText: error.title || ""
      });
    }
  }

  useEffect(() => {
    validateUid();
  }, []);

  if (state.loading) {
    return <Loading />;
  }

  if (!state.user) {
    return (
      <CardLayout>
        <SetPasswordError message={state.errorText} />
      </CardLayout>
    );
  }

  return (
    <CardLayout>
      <SetPasswordForm uid={uid} user={state.user} onSuccess={() => history.push("/login")} />
    </CardLayout>
  );
};

export default withRouter(SetPasswordPage);
