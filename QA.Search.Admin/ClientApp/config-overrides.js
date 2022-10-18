/* config-overrides.js */
const MonacoWebpackPlugin = require("monaco-editor-webpack-plugin");

module.exports = function override(config, env) {
  if (!config.plugins) {
    config.plugins = [];
  }

  // add MonacoWebpackPlugin
  config.plugins.push(new MonacoWebpackPlugin({ languages: ["json"] }));

  // remove ModuleScopePlugin
  config.plugins = config.plugins.filter(
    plugin => plugin.constructor.name !== "ModuleScopePlugin"
  );

  // Extract and process inline CSS literals in JavaScript files
  config.module.rules.push({
    test: /\.(js|mjs|jsx|ts|tsx)$/,
    use: [
      {
        loader: "astroturf/loader",
        options: { extension: ".scss" }
      }
    ]
  });

  return config;
};
