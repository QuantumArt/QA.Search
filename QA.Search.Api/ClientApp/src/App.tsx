import React, { Suspense, lazy } from "react";
import { BrowserRouter, Route, Switch, Redirect } from "react-router-dom";
import { hot } from "react-hot-loader/root";
import Loading from "./components/Loading";
import Navigation from "./components/Navigation";

const DemoPage = lazy(() => import("./routes/DemoPage"));
const DemoPage2 = lazy(() => import("./routes/DemoPage2"));
const SearchPage = lazy(() => import("./routes/SearchPage"));
const SuggestPage = lazy(() => import("./routes/SuggestPage"));
const CompletionPage = lazy(() => import("./routes/CompletionPage"));

const App = () => (
  <BrowserRouter>
    <Navigation.Provider>
      <Navigation.Panel />
      <Suspense fallback={<Loading />}>
        <Switch>
          <Route exact path="/demo" component={DemoPage} />
          <Route exact path="/demo2" component={DemoPage2} />
          <Route exact path="/search" component={SearchPage} />
          <Route exact path="/suggest" component={SuggestPage} />
          <Route exact path="/completion" component={CompletionPage} />
          <Route render={() => <Redirect to="/demo" />} />
        </Switch>
      </Suspense>
    </Navigation.Provider>
  </BrowserRouter>
);

export default hot(App);
