import NProgress from "nprogress";

let loadingCounter = 0;

export function acquireLoading() {
  if (loadingCounter === 0) {
    NProgress.start();
  }
  loadingCounter++;
  return releaseLoading;
}

export function releaseLoading() {
  loadingCounter--;
  if (loadingCounter === 0) {
    NProgress.done();
  }
}
