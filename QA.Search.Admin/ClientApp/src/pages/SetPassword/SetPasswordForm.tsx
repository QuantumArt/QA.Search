import React, { useState } from "react";
import { useForm, useField } from "react-final-form-hooks";
import { AccountController, UserResponse } from "../../backend.generated";
import Toaster from "../../utils/toaster";
import { Intent, FormGroup, InputGroup, Button, Tooltip } from "@blueprintjs/core";
import { Row, Col } from "react-flexbox-grid";
import { getFieldError } from "../../utils/forms";
import { Link } from "react-router-dom";

const PASSWORD_REQUIREMENTS =
  "Пароль должен быть не менее 8 символов," + "содержать заглавные, строчные буквы и цифры";

const validate = values => {
  const errors: any = {};

  if (!values.password) {
    errors.password = "Пароль не указан";
  } else if (!/^(?=.*\d)(?=.*[a-z])(?=.*[A-Z])[0-9a-zA-Z!@#\$%\^&\*]{8,}$/i.test(values.password)) {
    errors.password = PASSWORD_REQUIREMENTS;
  }

  if (values.password !== values.confirmation) {
    errors.confirmation = "Пароли не совпадают";
  }

  return errors;
};

interface Props {
  uid: string;
  user: UserResponse;
  onSuccess: () => void;
}

const SetPasswordForm = ({ uid, user, onSuccess }: Props) => {
  const onSubmit = async values => {
    try {
      await new AccountController().changePassword({
        emailId: uid,
        password: values.password
      });
      onSuccess();
    } catch (error) {
      Toaster.show({
        message: error.title || "При выполнении запроса произошла ошибка",
        intent: Intent.DANGER
      });
    }
  };

  const [showPassword, setShowPassword] = useState(false);
  const { form, handleSubmit, submitting } = useForm({ onSubmit, validate });
  const password = useField("password", form);
  const confirmation = useField("confirmation", form);

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
    <form onSubmit={handleSubmit}>
      <h2 className="bp3-heading">Изменение пароля</h2>
      <Row start="xs">
        <Col xs={12}>
          <FormGroup>
            <InputGroup value={user.email || ""} disabled={true} large={true} type="text" />
          </FormGroup>
        </Col>
      </Row>
      <Row start="xs">
        <Col xs={12}>
          <FormGroup
            helperText={getFieldError(password) || PASSWORD_REQUIREMENTS}
            intent={getFieldError(password) ? Intent.DANGER : Intent.NONE}
          >
            <InputGroup
              {...password.input}
              intent={getFieldError(password) ? Intent.DANGER : Intent.NONE}
              large={true}
              placeholder="Новый пароль"
              type={showPassword ? "text" : "password"}
              rightElement={lockButton}
            />
          </FormGroup>
        </Col>
      </Row>
      <Row start="xs">
        <Col xs={12}>
          <FormGroup
            helperText={getFieldError(confirmation)}
            intent={getFieldError(confirmation) ? Intent.DANGER : Intent.NONE}
          >
            <InputGroup
              {...confirmation.input}
              intent={getFieldError(confirmation) ? Intent.DANGER : Intent.NONE}
              large={true}
              placeholder="Подтверждение"
              type={showPassword ? "text" : "password"}
              rightElement={lockButton}
            />
          </FormGroup>
        </Col>
      </Row>
      <Row>
        <Col xs={12}>
          <Button
            text="Сохранить"
            loading={submitting}
            type="submit"
            large={true}
            intent={Intent.PRIMARY}
            fill={true}
          />
        </Col>
      </Row>
      <br />
      <Row end="xs">
        <Col xs>
          <Link to="/login">Отмена</Link>
        </Col>
      </Row>
    </form>
  );
};

export default SetPasswordForm;
