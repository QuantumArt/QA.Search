import { getStatusText } from "http-status-codes";

export class BusinessError extends Error {
  constructor(message: string) {
    super(message);
  }
}

export class HttpStatusError extends BusinessError {
  statusCode: number;

  constructor(statusCode: number) {
    super(`${statusCode} — ${getStatusText(statusCode)}`);
    this.statusCode = statusCode;
  }
}

export class EmptyResponseError extends BusinessError {
  constructor() {
    super("Response is empty");
  }
}
