import { FieldRenderProps } from 'react-final-form-hooks';

export const getFieldError = (field: FieldRenderProps<any>): string => {
  return field.meta.touched && !field.meta.active && field.meta.error;
}