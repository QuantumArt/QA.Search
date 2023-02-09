import { URL } from "url";
import { createHash } from "crypto";
import { Entity, Column, PrimaryColumn, VersionColumn } from "typeorm";

@Entity("links")
export class Link {
  @PrimaryColumn()
  hash: string;

  private _url: string;
  private _urlParts: URL;

  @Column({ update: false })
  get url() {
    return this._url;
  }
  set url(value: string) {
    this._url = value;
    this._urlParts = new URL(value);
  }

  get urlParts() {
    return this._urlParts;
  }
  
  @Column()
  nextIndexingUtc: Date;

  @Column()
  isActive: boolean;

  @VersionColumn()
  version: number;

  constructor(props?: Partial<Link>) {
    Object.assign(this, props);
  }

  static getHash(url: string) {
    return createHash("sha256")
      .update(url, "ucs2" as any)
      .digest("hex");
  }
}
