import { useState, useEffect, useRef, useCallback } from "react";
import { isFunction } from "./types";

type Init<S> = S | (() => S);
type Update<S> = (patch: Patch<S>) => void;
type Patch<S> = Partial<S> | ((state: S) => Partial<S>);

export function useSetState<S extends object>(initial: Init<S> = {} as S): [S, Update<S>] {
  const [state, setState] = useState(initial);
  const patchState = useCallback((patch: any) => {
    setState(prevState => ({
      ...prevState,
      ...(isFunction(patch) ? patch(prevState) : patch)
    }));
  }, []);
  return [state, patchState];
}

type TimerOptions = {
  action: (abortSignal: AbortSignal) => Promise<void> | void;
  msec: number;
  deps: readonly any[];
};

export function useDebounce({ action, msec, deps }: TimerOptions) {
  const isFirstRenderRef = useRef(true);

  const effect = isFirstRenderRef.current
    ? () => {
        isFirstRenderRef.current = false;
      }
    : () => {
        const controller = new AbortController();
        const timeout = setTimeout(() => action(controller.signal), msec);
        return () => {
          controller.abort();
          clearTimeout(timeout);
        };
      };

  useEffect(effect, deps);
}
