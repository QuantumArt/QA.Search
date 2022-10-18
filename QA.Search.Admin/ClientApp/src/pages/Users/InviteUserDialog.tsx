import React from "react";
import {
  Dialog,
  Classes,
  Button,
  Intent,
  FormGroup,
  InputGroup,
  HTMLSelect
} from "@blueprintjs/core";
import { UserRole, UsersController } from "../../backend.generated";
import { useForm, useField } from "react-final-form-hooks";
import Toaster from "../../utils/toaster";
import { getFieldError } from "../../utils/forms";

const validate = values => {
  const errors: any = {};

  if (!values.email) {
    errors.email = "Email не указан";
  } else if (!/^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}$/i.test(values.email)) {
    errors.email = "Некорректный email";
  }
  if (!values.role) {
    errors.role = "Роль не выбрана";
  }
  return errors;
};

interface Props {
  isOpen: boolean;
  onClose: () => void;
  onSuccess: () => Promise<void>;
}

const InviteUserDialog = ({ isOpen, onClose, onSuccess }: Props) => {
  const onSubmit = async values => {
    try {
      await new UsersController().createUser(values);
      form.reset();
      onClose();
      Toaster.show({
        message: "Пользователь успешно создан",
        intent: Intent.SUCCESS
      });
      await onSuccess();
    } catch (error) {
      Toaster.show({
        message: error.title || "При выполнении запроса произошла ошибка",
        intent: Intent.DANGER
      });
    }
  };

  const { form, handleSubmit, submitting } = useForm({ onSubmit, validate });
  const email = useField("email", form);
  const role = useField("role", form);

  return (
    <Dialog icon="new-person" title="Приглашение пользователя" isOpen={isOpen} onClose={onClose}>
      <form onSubmit={handleSubmit}>
        <div className={Classes.DIALOG_BODY}>
          <FormGroup
            label="Адрес электронной почты:"
            labelFor="text-input"
            helperText={getFieldError(email)}
            intent={getFieldError(email) ? Intent.DANGER : Intent.NONE}
          >
            <InputGroup
              {...email.input}
              large={true}
              placeholder="Введите адрес Email"
              type="text"
            />
          </FormGroup>
          <FormGroup
            label="Роль:"
            labelFor="text-input"
            helperText={getFieldError(role)}
            intent={getFieldError(email) ? Intent.DANGER : Intent.NONE}
          >
            <HTMLSelect
              {...role.input}
              fill={true}
              large={true}
              options={[
                { label: "Выберите роль", value: "" },
                ...Object.keys(UserRole).map(key => {
                  return {
                    label: key,
                    value: UserRole[key]
                  };
                })
              ]}
            />
          </FormGroup>
        </div>
        <div className={Classes.DIALOG_FOOTER}>
          <div className={Classes.DIALOG_FOOTER_ACTIONS}>
            <Button
              onClick={() => {
                form.reset();
                onClose();
              }}
            >
              Отмена
            </Button>
            <Button intent={Intent.PRIMARY} loading={submitting} type="submit">
              Пригласить
            </Button>
          </div>
        </div>
      </form>
    </Dialog>
  );
};

export default InviteUserDialog;
