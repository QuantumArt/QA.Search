/**
 * Creates a comparsion function for Array `.sort()` that:
 * Compares values returned by selector function in ascending order.
 */
export function asc<T>(selector?: (item: T) => any): (left: T, right: T) => number {
  if (typeof selector === "undefined") {
    return (l, r) => (l < r ? -1 : l > r ? 1 : 0);
  }
  return (left, right) => {
    const l = selector(left);
    const r = selector(right);
    return l < r ? -1 : l > r ? 1 : 0;
  };
}

/**
 * Creates a comparsion function for Array `.sort()` that:
 * Compares values returned by selector function in descending order.
 */
export function desc<T>(selector?: (item: T) => any): (left: T, right: T) => number {
  if (typeof selector === "undefined") {
    return (l, r) => (l < r ? 1 : l > r ? -1 : 0);
  }
  return (left, right) => {
    const l = selector(left);
    const r = selector(right);
    return l < r ? 1 : l > r ? -1 : 0;
  };
}
