import React, { useState } from "react";
import { Dialog, Classes, Button, Intent } from "@blueprintjs/core";
import { UserResponse, AccountController } from "../../backend.generated";
import Toaster from "../../utils/toaster";

interface Props {
  user: UserResponse | null;
  isOpen: boolean;
  onClose: () => void;
}

const SendResetPasswordLinkDialog = ({ user, isOpen, onClose }: Props) => {
  const [loading, setLoading] = useState(false);
  const onSubmit = async () => {
    if (!user) return;

    setLoading(true);

    try {
      await new AccountController().sendResetPasswordLink({ email: user.email || "" });
      Toaster.show({
        message: "Ссылка для восстановления пароля отправлена",
        intent: Intent.SUCCESS
      });
      onClose();
    } catch (error) {
      Toaster.show({
        message: error.title || "При выполнении запроса произошла ошибка",
        intent: Intent.DANGER
      });
    }
    setLoading(false);
  };

  return (
    <Dialog icon="key" title="Восстановление пароля" isOpen={isOpen} onClose={onClose}>
      <div className={Classes.DIALOG_BODY}>
        <p>
          Вы действительно хотите отправить письмо со ссылкой для восстановление пароля пользователю{" "}
          <strong>{user && user.email}</strong>?
        </p>
      </div>
      <div className={Classes.DIALOG_FOOTER}>
        <div className={Classes.DIALOG_FOOTER_ACTIONS}>
          <Button onClick={onClose}>Отмена</Button>
          <Button intent={Intent.PRIMARY} loading={loading} onClick={onSubmit}>
            Подтвердить
          </Button>
        </div>
      </div>
    </Dialog>
  );
};

export default SendResetPasswordLinkDialog;
