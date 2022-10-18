import pathToRegexp, { Token } from "path-to-regexp";
import { Entity, PrimaryGeneratedColumn, ManyToOne, Column } from "typeorm";
import { DomainGroup } from "./DomainGroup";

@Entity("crawler.Routes")
export class Route {
  @PrimaryGeneratedColumn()
  id: number;

  @Column({ update: false })
  domainGroupId: number;

  private _route: string;
  private _pattern: RegExp;
  private _tokens: Token[];

  @Column({ update: false })
  get route() {
    return this._route;
  }
  set route(value: string) {
    if (value.endsWith("/")) {
      value = value.slice(0, -1);
    }
    if (!value.startsWith("/")) {
      value = "/" + value;
    }
    this._route = value;
    this._pattern = pathToRegexp(value);
    this._tokens = pathToRegexp.parse(value);
  }

  get pattern() {
    return this._pattern;
  }

  @Column({ update: false })
  scanPeriodMsec: number;

  @Column({ type: "simple-json", update: false })
  indexingConfig: IndexingConfig;

  @ManyToOne(() => DomainGroup, domainGroup => domainGroup.domains)
  domainGroup: DomainGroup;

  constructor(props?: Partial<Route>) {
    Object.assign(this, props);
  }

  static compare(left: Route, right: Route) {
    for (let i = 0; i < left._tokens.length; i++) {
      const leftToken = left._tokens[i];
      const rightToken = right._tokens[i];
      if (leftToken !== rightToken) {
        if (leftToken === "/") {
          return 1;
        } else if (rightToken === "/") {
          return -1;
        } else if (typeof leftToken === "string") {
          if (typeof rightToken === "string") {
            if (leftToken.startsWith(rightToken)) {
              return -1;
            } else if (rightToken.startsWith(leftToken)) {
              return 1;
            } else if (leftToken < rightToken) {
              return -1;
            } else {
              return 1;
            }
          } else {
            return -1;
          }
        } else if (typeof rightToken === "string") {
          return 1;
        } else if (!rightToken) {
          return -1;
        }
      }
    }
    if (left._tokens.length < right._tokens.length) {
      return 1;
    }
    return 0;
  }
}

export interface IndexingConfig {
  extractSchema: Object;
  transformScript?: string;
  indexName: string;
}
