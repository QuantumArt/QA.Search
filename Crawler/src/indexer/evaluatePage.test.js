import { extractJson, getLocationContext, removeTags } from "./evaluatePage";

describe("extractJson() utility", () => {
  it("should extract Element's text", () => {
    const div = document.createElement("div");
    div.innerHTML = `<span>foo</span> bar`;

    const json = extractJson(div, "textContent");

    expect(json).toEqual("foo bar");
  });

  it("should extract Element's attribute", () => {
    const a = document.createElement("a");
    a.setAttribute("href", "https://domain.ru/");

    const json = extractJson(a, "href");

    expect(json).toEqual("https://domain.ru/");
  });

  it("should extract Element's custom attribute", () => {
    const div = document.createElement("a");
    div.setAttribute("data-test", "foo bar");

    const json = extractJson(div, "data-test");

    expect(json).toEqual("foo bar");
  });

  it("should extract data by CSS selector", () => {
    const div = document.createElement("div");
    div.innerHTML = `<span class="foo">foo</span> bar`;

    const json = extractJson(div, "span.foo | textContent");

    expect(json).toEqual("foo");
  });

  it("should map selected data to different object fields", () => {
    const div = document.createElement("div");
    div.innerHTML = `<span class="foo">foo</span><span class="bar">bar</span>`;

    const json = extractJson(div, {
      foo: "span.foo | textContent",
      bar: "span.bar | className"
    });

    expect(json).toEqual({
      foo: "foo",
      bar: "bar"
    });
  });

  it("should map selected elements to array", () => {
    const div = document.createElement("div");
    div.innerHTML = `<div class="container">foo</div><div class="container">bar</div>`;

    const json = extractJson(div, [
      ".container",
      {
        class: "className",
        text: "textContent"
      }
    ]);

    expect(json).toEqual([
      { class: "container", text: "foo" },
      { class: "container", text: "bar" }
    ]);
  });

  it("should return null if property is missing", () => {
    const div = document.createElement("div");

    const json = extractJson(div, "fooBar");

    expect(json).toBeNull();
  });

  it("should return null if selector is not found", () => {
    const div = document.createElement("div");

    const json = extractJson(div, "span.foo | textContent");

    expect(json).toBeNull();
  });

  it("should map object prop to null if selector is not found", () => {
    const div = document.createElement("div");

    const json = extractJson(div, {
      text: "span.foo | textContent"
    });

    expect(json).toEqual({ text: null });
  });

  it("should extract Documents's property", () => {
    const json = extractJson(document, "contentType");

    expect(json).toEqual("text/html");
  });

  it("should throw error if element is not defined", () => {
    expect(() => extractJson(undefined, "textContent")).toThrowError(
      "Element is not defined"
    );
  });

  it("should throw error if schema is invalid", () => {
    const div = document.createElement("div");

    // @ts-ignore
    expect(() => extractJson(div, ["textContent"])).toThrowError(
      "Passed schema is invalid:"
    );
  });

  it("should insert exact values", () => {
    const json = extractJson(document, {
      string: "=foo bar",
      number: "=12345"
    });

    expect(json).toEqual({
      string: "foo bar",
      number: "12345"
    });
  });

  it("should extract values from context", () => {
    const json = extractJson(
      document,
      {
        location: ":url",
        tariff: ":alias"
      },
      {
        url: "https://domain.ru/",
        alias: "internet-100"
      }
    );

    expect(json).toEqual({
      location: "https://domain.ru/",
      tariff: "internet-100"
    });
  });

  it("should return null if context is not specified", () => {
    const json = extractJson(document, {
      location: ":url",
      tariff: ":alias"
    });

    expect(json).toEqual({
      location: null,
      tariff: null
    });
  });
});

describe("getLocationContext() utility", () => {
  it("should extract values from path and query", () => {
    const context = getLocationContext(
      "/:first/:second",
      "http://site.com/foo/bar?name=value"
    );

    expect(context.url).toEqual("http://site.com/foo/bar?name=value");
    expect(context.first).toEqual("foo");
    expect(context.second).toEqual("bar");
    expect(context.name).toEqual("value");
  });
});

describe("removeTags() utility", () => {
  it("should remove elements by passed selectors", () => {
    const div = document.createElement("div");
    div.innerHTML = `
    <div class="noindex"></div>
    <div class="robots-noindex"></div>
    <div class="robots-nocontent"></div>
    <noindex></noindex>
    <div class="keep"></div>
    <span class="keep"></span>`;

    removeTags(div);

    expect(div.innerHTML.trim()).toEqual(`<div class="keep"></div>
    <span class="keep"></span>`);
  });

  it("should remove comments", () => {
    const div = document.createElement("div");
    div.innerHTML = `
    <!--test comment-->
    <!--another comment-->
    <div class="keep"></div>
    <span class="keep"></span>`;

    removeTags(div);

    expect(div.innerHTML.trim()).toEqual(`<div class="keep"></div>
    <span class="keep"></span>`);
  });

  it("should remove elements inside <!--noindex--> comments", () => {
    const div = document.createElement("div");
    div.innerHTML = `
    <!--noindex-->
      <div class="remove"></div>
    <!--/noindex-->
    <div class="keep"></div>
    <span class="keep"></span>
    <!--noindex-->
      <div class="remove"></div>
      <!--noindex-->
        <div class="remove"></div>
      <!--/noindex-->
      <div class="remove"></div>
    <!--/noindex-->`;

    removeTags(div);

    expect(div.querySelectorAll(".keep").length).toEqual(2);
    expect(div.querySelectorAll(".remove").length).toEqual(0);
  });

  it("should handle unbalanced <!--noindex--> comments", () => {
    const div = document.createElement("div");
    div.innerHTML = `
    <div class="keep">
      <span class="keep"></span>
      <!--noindex-->
        <span class="remove"></span>
    </div>
    <div class="keep">
      <span class="keep"></span>
      <!--noindex-->
        <span class="remove"></span>
      <!--/noindex-->
      <span class="keep"></span>
      <!--/noindex-->
    </div>`;

    removeTags(div);

    expect(div.querySelectorAll(".keep").length).toEqual(5);
    expect(div.querySelectorAll(".remove").length).toEqual(0);
  });
});
