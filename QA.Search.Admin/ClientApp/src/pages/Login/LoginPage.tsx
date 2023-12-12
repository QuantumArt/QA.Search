import React, { useState } from "react";
import { Button, InputGroup, Intent, Tooltip, FormGroup } from "@blueprintjs/core";
import { Link, withRouter, RouteComponentProps } from "react-router-dom";
import { useForm, useField } from "react-final-form-hooks";
import Toaster from "../../utils/toaster";
import { AccountController } from "../../backend.generated";
import { getFieldError } from "../../utils/forms";
import CardLayout from "../../components/CardLayout";

const validate = values => {
  const errors: any = {};

  if (!values.email) {
    errors.email = "Email не указан";
  } else if (!/^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}$/i.test(values.email)) {
    errors.email = "Некорректный email";
  }
  if (!values.password) {
    errors.password = "Пароль не указан";
  }
  return errors;
};

interface Props extends RouteComponentProps {
  getUserInfo(): void;
}

const LoginPage = ({ history, getUserInfo }: Props) => {
  const onSubmit = async values => {
    try {
      await new AccountController().login(values);
      getUserInfo();
      history.push("/");
    } catch (err) {
      Toaster.show({
        intent: Intent.DANGER,
        message: err.Title || "При выполнении запроса произошла ошибка"
      });
    }
  };

  const [showPassword, setShowPassword] = useState(false);
  const { form, handleSubmit, submitting } = useForm({ onSubmit, validate });
  const email = useField("email", form);
  const password = useField("password", form);

  const lockButton = (
    <Tooltip content={`${showPassword ? "Скрыть" : "Показать"} пароль`}>
      <Button
        icon={showPassword ? "unlock" : "lock"}
        intent={Intent.WARNING}
        minimal={true}
        onClick={() => setShowPassword(!showPassword)}
      />
    </Tooltip>
  );

  return (
    <CardLayout>
      <h2 className="bp3-heading">Search Admin App</h2>
      <form onSubmit={handleSubmit}>
        <div>
          <FormGroup
            helperText={getFieldError(email)}
            intent={getFieldError(email) ? Intent.DANGER : Intent.NONE}
          >
            <InputGroup
              {...email.input}
              intent={getFieldError(email) ? Intent.DANGER : Intent.NONE}
              large={true}
              placeholder="Введите адрес Email"
              type="text"
            />
          </FormGroup>
        </div>
        <div>
          <FormGroup
            helperText={getFieldError(password)}
            intent={getFieldError(password) ? Intent.DANGER : Intent.NONE}
          >
            <InputGroup
              {...password.input}
              intent={getFieldError(password) ? Intent.DANGER : Intent.NONE}
              large={true}
              placeholder="Введите пароль"
              type={showPassword ? "text" : "password"}
              rightElement={lockButton}
            />
          </FormGroup>
        </div>
        <div>
          <Button
            loading={submitting}
            type="submit"
            icon="log-in"
            large={true}
            intent={Intent.PRIMARY}
            fill={true}
          >
            Войти
          </Button>
        </div>
        <br />
        <div className="flex-justify-content-flex-end">
          <Link to="/resetPassword">Восстановить пароль</Link>
        </div>
      </form>
    </CardLayout>
  );
};

export default withRouter(LoginPage);
