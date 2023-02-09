import React, { useEffect, useContext } from "react";
import { Row, Col } from "react-flexbox-grid";
import { Card, InputGroup, FormGroup, HTMLSelect } from "@blueprintjs/core";
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
    <Card elevation={2}>
      <h4 className="bp3-heading">Настройки отображения</h4>
      <Row>
        <Col xs>
          <FormGroup label="Фильтр по Email:" labelFor="text-input">
            <InputGroup {...email.input} large={true} placeholder="Поиск по Email" type="text" />
          </FormGroup>
          <FormGroup label="Фильтр по Роли:" labelFor="text-input">
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
          </FormGroup>
        </Col>
      </Row>
    </Card>
  );
};

export default UsersListFilter;
