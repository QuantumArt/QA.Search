import { Entity, PrimaryColumn, ManyToOne, Column } from "typeorm";
import { DomainGroup } from "./DomainGroup";

@Entity("crawler.Domains")
export class Domain {
  @PrimaryColumn({ update: false })
  origin: string;

  @Column({ update: false })
  domainGroupId: number;

  /**
   * For comparsion operations we should use `.toISOString()`
   * ```js
   * LessThan(new Date().toISOString())
   * ```
   */
  @Column("datetimeoffset")
  lastFastCrawlingUtc: Date;

  /**
   * For comparsion operations we should use `.toISOString()`
   * ```js
   * LessThan(new Date().toISOString())
   * ```
   */
  @Column("datetimeoffset")
  lastDeepCrawlingUtc: Date;

  @ManyToOne(() => DomainGroup, domainGroup => domainGroup.domains)
  domainGroup: DomainGroup;

  constructor(props?: Partial<Domain>) {
    Object.assign(this, props);
  }
}
