import extractJson from "~/Utils/ExtractJson";

describe("extract() utility", () => {
  it("should extract Element's text", () => {
    const div = document.createElement("div");
    div.innerHTML = `<span>foo</span> bar`;

    const json = extractJson(div, "textContent");

    expect(json).toEqual("foo bar");
  });
});
