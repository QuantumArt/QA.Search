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

  config.module.rules = config.module.rules.map(rule => {
    if (rule.oneOf instanceof Array) {
      return {
        ...rule,
        // create-react-app let every file which doesn't match to any filename test falls back to file-loader,
        // so we need to add raw-loader before that fallback.
        // see: https://github.com/facebookincubator/create-react-app/blob/master/packages/react-scripts/config/webpack.config.dev.js#L220-L236
        oneOf: [
          {
            test: /\.(md|txt)$/,
            loader: "raw-loader",
            exclude: /node_modules/
          },
          ...rule.oneOf
        ]
      };
    }

    return rule;
  });

  return config;
};
