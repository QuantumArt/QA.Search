import { createConnection, Connection } from "typeorm";
import { onShutdown } from "./process";
import { dbConnectionOptions } from "../settings";

let createConnectionTask: Promise<Connection>;

/** Create TypeORM connection if it's not created yet. */
export async function registerConnection() {
  if (!createConnectionTask) {
    createConnectionTask = createConnection(dbConnectionOptions);
    const connection = await createConnectionTask;
    onShutdown(() => connection.close());
  }
  return await createConnectionTask;
}
