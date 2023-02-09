import { Entity, PrimaryColumn, ManyToOne, Column } from "typeorm";
import { DomainGroup } from "./DomainGroup";

@Entity("domains")
export class Domain {
  @PrimaryColumn({ update: false })
  origin: string;

  @Column({ update: false })
  domainGroupId: number;

  @Column()
  lastFastCrawlingUtc: Date;
 
  @Column()
  lastDeepCrawlingUtc: Date;

  @ManyToOne(() => DomainGroup, domainGroup => domainGroup.domains)
  domainGroup: DomainGroup;

  constructor(props?: Partial<Domain>) {
    Object.assign(this, props);
  }
}
