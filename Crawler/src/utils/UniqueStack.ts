export class UniqueStack<T> {
  _visited = new Set<T>();
  _stack = new Array<T>();

  constructor(...values: T[]) {
    values.forEach(value => this.push(value));
  }

  push(value: T): void {
    if (!this._visited.has(value)) {
      this._visited.add(value);
      this._stack.push(value);
    }
  }

  pop(): T | null {
    return this._stack.pop();
  }
}
