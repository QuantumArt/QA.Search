import React from "react";
import { BrowserRouter } from "react-router-dom";
import { hot } from "react-hot-loader/root";
import Layout from "./Layout";
import AuthContainer from "./AuthContainer";

const App = () => (
  <BrowserRouter>
    <AuthContainer.Provider>
      <Layout />
    </AuthContainer.Provider>
  </BrowserRouter>
);

export default hot(App);
