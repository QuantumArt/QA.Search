import React from "react";
import { NonIdealState } from "@blueprintjs/core";
import { Link } from "react-router-dom";

const SetPasswordError = ({ message }: { message: string }) => (
  <NonIdealState
    icon="error"
    title={message || "Неверная ссылка для восстановления пароля"}
    action={<Link to="/login">Вернуться к странице входа</Link>}
  />
);

export default SetPasswordError;
