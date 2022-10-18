import { Client } from "elasticsearch";
import { elasticSearchOptions as settings } from "../settings";
import { onShutdown } from "./process";

export const elastic = new Client(settings);

onShutdown(() => elastic.close());
