export abstract class Controller {
  controller: AbortController;

  constructor() {
    Object.defineProperty(this, "jsonParseReviver", {
      get() {
        return parseJsonDates;
      },
      set() { }
    });
    this.controller = new AbortController();
  }

  protected async transformOptions(options: RequestInit): Promise<RequestInit> {
    const response = await fetch("/api/antiforgery/token", {
      method: "GET",
      credentials: "same-origin",
      headers: { ...options.headers },
      signal: this.controller.signal
    });

    if (response.ok) {
      const xsrfToken = getCookie("X-XSRF-TOKEN");
      if (xsrfToken) {
        options.headers = {
          ...options.headers,
          "X-XSRF-TOKEN": xsrfToken,
          "Content-Type": "application/json"
        };
      }
    }

    return Promise.resolve(options);
  }
}

const dataRateRegexForUnspecifiedKind = /^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(.\d+)*$/;
const iso8601DateRegex = /^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(?:((\.\d+)?Z)|((\.\d+)?\+\d{2}:\d{2}))?$/;

/**
 * Примеры для проверки
 * 2018-02-14T23:32:56.5987719+03:00
 * 2018-02-10T09:42:14.4575689Z
 * 2018-03-12T10:46:32.123
 */
function parseJsonDates(_key: string, value: any) {
  if (typeof value === "string" && dataRateRegexForUnspecifiedKind.test(value)) {
    return new Date(value + "Z");
  }
  if (typeof value === "string" && iso8601DateRegex.test(value)) {
    return new Date(value);
  }
  return value;
}

function getCookie(name: string) {
  const cookie = decodeURIComponent(document.cookie)
    .split("; ")
    .map(cookie => cookie.split("="))
    .find(cookie => cookie[0] === name);

  return cookie ? cookie[1] : null;
}
