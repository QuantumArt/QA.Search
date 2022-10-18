import { URL } from "url";

export function getHost(urlString: string) {
  return new URL(urlString).host;
}
