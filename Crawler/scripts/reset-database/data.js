import { Domain, DomainGroup, Route } from "../../src/entities";

/* ========================================================================= */

const regionalGroup = new DomainGroup({
  name: "enter_name",
  indexingConfig: {
    indexName: "search.alias_name",
    extractSchema: {
      SearchUrl: ":url",
      SearchArea: "=personal",
      Title: "title | textContent",
      Keywords: "meta[name=keywords] | content",
      Description: "meta[name=description] | content",
      Body: "body | innerText"
    },
    transformScript: `script to transform url`
  }
});

regionalGroup.routes = [
  new Route({
    route: "/personal/podderzhka/(.*)",
    scanPeriodMsec: 12 * 60 * 60 * 1000 // 12 hours
  })
];

regionalGroup.domains = [
  "addreses array"
].map(origin => new Domain({ origin }));

export default [
  regionalGroup
];
