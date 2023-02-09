import { Domain, DomainGroup, Route, Link } from "../../src/entities";
import { getRepository, getManager } from "typeorm";
import { registerConnection } from "../../src/utils/connection";
import domainGroups from "./data";

(async () => {
  const connection = await registerConnection();

  try {
    await truncate(Link);
    await truncate(Route);
    await truncate(Domain);
    await truncate(DomainGroup);

    await populate();
  } finally {
    await connection.close();
  }
})();

async function truncate(Entity) {
  const repository = getRepository(Entity);
  const { tablePath } = repository.metadata;

  await repository.query(`TRUNCATE TABLE ${tablePath} RESTART IDENTITY CASCADE;`)
}

async function populate() {
  const entities = [];
  entities.push(...domainGroups);
  domainGroups.forEach(domainGroup => {
    if (domainGroup.domains) {
      entities.push(...domainGroup.domains);
    }
    if (domainGroup.routes) {
      entities.push(...domainGroup.routes);
    }
  });

  await getManager().save(entities);
}
