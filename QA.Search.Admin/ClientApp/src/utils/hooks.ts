import { useEffect } from "react";

type TimerOptions = {
  action: (abortSignal: AbortSignal) => Promise<void> | void;
  msec: number;
  deps: any[];
};

export function useInterval({ action, msec, deps }: TimerOptions) {
  useEffect(() => {
    const controller = new AbortController();
    action(controller.signal);
    const interval = setInterval(() => action(controller.signal), msec);
    return () => {
      controller.abort();
      clearInterval(interval);
    };
  }, deps);
}
