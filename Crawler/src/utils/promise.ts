export function delay(msec: number): Promise<void> {
  return new Promise(resolve => setTimeout(resolve, msec > 0 ? msec : 0));
}
