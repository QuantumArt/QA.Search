import React from "react";
import { NonIdealState } from "@blueprintjs/core";
import { Link } from "react-router-dom";

const ResetPasswordSuccess = () => (
  <NonIdealState
    icon="envelope"
    title="Ссылка для восстановления пароля успешно отправлена"
    action={<Link to="/login">Вернуться к странице входа</Link>}
  />
);

export default ResetPasswordSuccess;
