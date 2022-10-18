import { Route } from "./Route";

describe("Route.compare()", () => {
  it("should not compare identical tokens", () => {
    const left = new Route({ route: "/foo/bar" });
    const right = new Route({ route: "/foo/bar" });

    expect(Route.compare(left, right)).toEqual(0);
    expect(Route.compare(right, left)).toEqual(0);
  });

  it("should compare string tokens", () => {
    const left = new Route({ route: "/foo/bar" });
    const right = new Route({ route: "/foo/baz" });

    expect(Route.compare(left, right)).toEqual(-1);
    expect(Route.compare(right, left)).toEqual(1);
  });

  it("should prefer token with longer suffix", () => {
    const left = new Route({ route: "/foo/bar" });
    const right = new Route({ route: "/foo" });

    expect(Route.compare(left, right)).toEqual(-1);
    expect(Route.compare(right, left)).toEqual(1);
  });

  it("should prefer string token to variable token", () => {
    const left = new Route({ route: "/:foo/bar" });
    const right = new Route({ route: "/:foo/:bar" });

    expect(Route.compare(left, right)).toEqual(-1);
    expect(Route.compare(right, left)).toEqual(1);
  });

  it("should prefer variable token to undefined token", () => {
    const left = new Route({ route: "/foo/:bar" });
    const right = new Route({ route: "/foo" });

    expect(Route.compare(left, right)).toEqual(-1);
    expect(Route.compare(right, left)).toEqual(1);
  });

  it("should prefer variable token to empty slash", () => {
    const left = new Route({ route: "/:foo" });
    const right = new Route({ route: "/" });

    expect(Route.compare(left, right)).toEqual(-1);
    expect(Route.compare(right, left)).toEqual(1);
  });

  it("should sort routes in lexicographical order", () => {
    const routes = [
      "/",
      "/abc",
      "/foo",
      "/abc/:def",
      "/abc/def",
      "/foo/:bar",
      "/:foo/:bar",
      "/:foo/bar"
    ]
      .map(route => new Route({ route }))
      .sort(Route.compare)
      .map(route => route.route);

    expect(routes).toEqual([
      "/abc/def",
      "/abc/:def",
      "/abc",
      "/foo/:bar",
      "/foo",
      "/:foo/bar",
      "/:foo/:bar",
      "/"
    ]);
  });
});
