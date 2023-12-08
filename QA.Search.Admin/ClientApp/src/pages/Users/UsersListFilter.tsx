import React, { useEffect, useContext } from "react";
import { Card, InputGroup, HTMLSelect } from "@blueprintjs/core";
import { useForm, useField } from "react-final-form-hooks";

import { UserRole } from "../../backend.generated";
import UsersContainer from "./UsersContainer";

const DEBOUNCE_TIME_MS = 300;

const UsersListFilter = () => {
  const { form } = useForm({
    onSubmit: values => {
      initUsersList(values);
    }
  });
  const email = useField("email", form);
  const role = useField("role", form);
  const { initUsersList } = useContext(UsersContainer.Context);

  useEffect(() => {
    let timeout: NodeJS.Timeout;
    form.subscribe(
      () => {
        if (timeout) {
          clearTimeout(timeout);
        }

        timeout = setTimeout(form.submit, DEBOUNCE_TIME_MS);
      },
      { values: true }
    );
  }, []);

  return (
    <Card elevation={0} style={{ display: "flex", marginBottom: "15px" }}>
      <div style={{ width: "79%", marginRight: "1%" }}>
        <InputGroup {...email.input} large={true} placeholder="Поиск по Email" type="text" />
      </div>
      <div style={{ width: "20%" }}>
        <HTMLSelect
          {...role.input}
          fill={true}
          large={true}
          placeholder="Выберите роль"
          options={[
            { label: "Любая роль", value: "" },
            ...Object.keys(UserRole)
              .filter(key => isNaN(Number(key)))
              .filter(key => typeof UserRole[key] === "number" || typeof UserRole[key] === "string")
              .map(key => {
                return {
                  label: key,
                  value: UserRole[key]
                };
              })
          ]}
        />
      </div>
    </Card>
  );
};

export default UsersListFilter;
