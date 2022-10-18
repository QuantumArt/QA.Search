import { Page, ResourceType } from "puppeteer";

const BLOCKED_RESOURCE_TYPES: ResourceType[] = [
  "image",
  "media",
  "font",
  "texttrack",
  "eventsource",
  "websocket",
  "manifest",
  "other"
];

export async function configurePage(page: Page, blockedResources: ResourceType[] = []) {
  blockedResources.push(...BLOCKED_RESOURCE_TYPES);
  // prevent opening new tabs
  await page.evaluateOnNewDocument(() => {
    window.open = () => null;
  });
  // prevent dialogs
  page.on("dialog", dialog => {
    dialog.dismiss();
  });

  await page.setRequestInterception(true);
  // prevent image loading
  page.on("request", request => {
    if (BLOCKED_RESOURCE_TYPES.includes(request.resourceType())) {
      request.abort();
    } else {
      request.continue();
    }
  });
}
