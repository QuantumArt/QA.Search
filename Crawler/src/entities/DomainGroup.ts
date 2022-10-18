import { Entity, Column, PrimaryGeneratedColumn, OneToMany } from "typeorm";
import { Domain } from "./Domain";
import { Route, IndexingConfig } from "./Route";

@Entity("crawler.DomainGroups")
export class DomainGroup {
  @PrimaryGeneratedColumn()
  id: number;

  @Column({ update: false })
  name: string;

  @Column({ type: "simple-json", update: false })
  indexingConfig: IndexingConfig;

  @OneToMany(() => Domain, domain => domain.domainGroup)
  domains: Domain[];

  @OneToMany(() => Route, route => route.domainGroup)
  routes: Route[];

  constructor(props?: Partial<DomainGroup>) {
    Object.assign(this, props);
  }
}
