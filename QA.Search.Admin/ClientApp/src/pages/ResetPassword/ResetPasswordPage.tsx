import React, { useState } from "react";
import { Button, InputGroup, Intent, FormGroup } from "@blueprintjs/core";
import { useForm, useField } from "react-final-form-hooks";
import Toaster from "../../utils/toaster";
import { AccountController } from "../../backend.generated";
import { getFieldError } from "../../utils/forms";
import CardLayout from "../../components/CardLayout";
import ResetPasswordSuccess from "./ResetPasswordSuccess";
import { Link } from "react-router-dom";

const validate = values => {
  const errors: any = {};

  if (!values.email) {
    errors.email = "Email не указан";
  } else if (!/^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}$/i.test(values.email)) {
    errors.email = "Некорректный email";
  }

  return errors;
};

const ResetPasswordPage = () => {
  const [isSuccess, setIsSuccess] = useState(false);
  const onSubmit = async values => {
    try {
      await new AccountController().sendResetPasswordLink({ email: values.email });
      console.log(values);
      setIsSuccess(true);
    } catch (error) {
      Toaster.show({
        message: error.title || "При выполнении запроса произошла ошибка",
        intent: Intent.DANGER
      });
    }
  };

  const { form, handleSubmit, submitting } = useForm({ onSubmit, validate });
  const email = useField("email", form);

  if (isSuccess) {
    return (
      <CardLayout>
        <ResetPasswordSuccess />
      </CardLayout>
    );
  }

  return (
    <CardLayout>
      <h2 className="bp3-heading">Восстановление пароля</h2>
      <form onSubmit={handleSubmit}>
        <div>
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
        </div>
        <div>
          <div>
            <Button
              loading={submitting}
              type="submit"
              icon="log-in"
              large={true}
              intent={Intent.PRIMARY}
              fill={true}
            >
              Восстановить пароль
            </Button>
          </div>
        </div>
        <br />
        <Link to="/login">Отмена</Link>
      </form>
    </CardLayout>
  );
};

export default ResetPasswordPage;
