import React, { Suspense, lazy, useCallback, useEffect } from "react";
import { Switch, Route, Redirect, withRouter, RouteComponentProps } from "react-router";
import Navigation from "./components/Navigation";
import Sidebar from "./components/Sidebar";
import Loading from "./components/Loading";
import useAuthContainer from "./AuthContainer";
import { AccountController } from "./backend.generated";

const LoginPage = lazy(() => import("./pages/Login"));
const SetPasswordPage = lazy(() => import("./pages/SetPassword"));
const ResetPasswordPage = lazy(() => import("./pages/ResetPassword"));

const UsersPage = lazy(() => import("./pages/Users"));
const QpIndexingPage = lazy(() => import("./pages/QpIndexing"));
const ElasticManagementPage = lazy(() => import("./pages/ElasticManagement"));
const MappingTemplatesPage = lazy(() => import("./pages/MappingTemplates"));
const IndexingScriptsPage = lazy(() => import("./pages/IndexingScripts"));

const Layout = ({ history }: RouteComponentProps) => {
  const { authInfo, setAuthInfo } = useAuthContainer();

  const getUserInfo = useCallback(async () => {
    if (authInfo.user) {
      return;
    }

    setAuthInfo({ ...authInfo, loading: true });

    try {
      const user = await new AccountController().info();
      setAuthInfo({ loading: false, user: user });
    } catch {
      console.log("Failed to get user info");
      setAuthInfo({ loading: false, user: null });
    }
  }, [authInfo, setAuthInfo]);

  const logout = useCallback(async () => {
    await new AccountController().logout();
    setAuthInfo({ loading: false, user: null });
    history.push("/login");
  }, [authInfo, setAuthInfo]);

  useEffect(() => {
    getUserInfo();
  }, []);

  if (authInfo.loading) {
    return <Loading />;
  }

  if (!authInfo.user) {
    return (
      <Suspense fallback={<Loading />}>
        <Switch>
          <Route exact path="/login" render={() => <LoginPage getUserInfo={getUserInfo} />} />
          <Route exact path="/setPassword/:uid" component={SetPasswordPage} />
          <Route exact path="/resetPassword" component={ResetPasswordPage} />
          <Route render={() => <Redirect to="/login" />} />
        </Switch>
      </Suspense>
    );
  }

  return (
    <Navigation.Provider>
      <Navigation.Panel logout={logout} user={authInfo.user} />
      <div style={{ display: "flex" }}>
        <Sidebar user={authInfo.user} />
        <div style={{ flex: "auto" }}>
          <Suspense fallback={<Loading />}>
            <Switch>
              <Route exact path="/users" component={UsersPage} />
              <Route exact path="/elastic" component={ElasticManagementPage} />
              <Route exact path="/templates" component={MappingTemplatesPage} />
              <Route exact path="/qp-indexing" component={QpIndexingPage} />
              <Route render={() => <Redirect to="/elastic" />} />
            </Switch>
          </Suspense>
        </div>
      </div>
    </Navigation.Provider>
  );
};

export default withRouter(Layout);
