import React, { useState } from "react";
import { Dialog, Classes, Button, Intent } from "@blueprintjs/core";
import { UserResponse } from "../../backend.generated";

interface Props {
  user: UserResponse | null;
  isOpen: boolean;
  onClose: () => void;
  deleteUser: (id: number) => Promise<void>;
}

const DeleteUserDialog = ({ user, isOpen, onClose, deleteUser }: Props) => {
  const [loading, setLoading] = useState(false);
  const onSubmit = async () => {
    if (user == null) return;

    setLoading(true);
    await deleteUser(user.id);
    setLoading(false);
    onClose();
  };

  return (
    <Dialog icon="eraser" title="Удаление пользователя" isOpen={isOpen} onClose={onClose}>
      <div className={Classes.DIALOG_BODY}>
        <p>
          Вы действительно хотите удалить пользователя <strong>{user && user.email}</strong>?
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

export default DeleteUserDialog;
