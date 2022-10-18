import { URL } from "url";
import { createHash } from "crypto";
import { Entity, Column, PrimaryGeneratedColumn, VersionColumn } from "typeorm";

@Entity("crawler.Links")
export class Link {
  @PrimaryGeneratedColumn()
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

  /**
   * For comparsion operations we should use `.toISOString()`
   * ```js
   * LessThan(new Date().toISOString())
   * ```
   */
  @Column("datetimeoffset")
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
