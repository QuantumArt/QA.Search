/**
 * Register shutdown handler on SIGINT (Ctrl+C) and SIGTERM (kill) Unix signals for graceful shutdown.
 * @param handler Shutdown handler
 * @returns Function to unregister passed handler.
 */
export function onShutdown(handler: () => Promise<void> | void) {
  async function listener() {
    shutdownTasks.push(handler());
  }

  process.prependOnceListener("SIGINT", listener).prependOnceListener("SIGTERM", listener);

  return () => {
    process.removeListener("SIGINT", listener).removeListener("SIGTERM", listener);
  };
}

async function waitAndExit(_signal: string) {
  await Promise.all(shutdownTasks);
  // TODO: log signal
  process.exit(0);
}

const shutdownTasks: (Promise<void> | void)[] = [];

process.addListener("SIGINT", waitAndExit).addListener("SIGTERM", waitAndExit);
