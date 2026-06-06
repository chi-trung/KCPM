exports.config = {
  tests: './e2e/**/*_test.js',
  output: './output',
  helpers: {
    Playwright: {
      url: process.env.CODECEPT_BASE_URL || 'http://127.0.0.1:3000',
      show: process.env.CODECEPT_SHOW_BROWSER === 'true',
      browser: 'chromium',
      restart: false,
      waitForTimeout: 10000,
      waitForAction: 1000,
    },
  },
  include: {},
  noGlobals: true,
  bootstrap: null,
  teardown: null,
  hooks: [],
  gherkin: {},
  mocha: {},
  name: 'waste-platform-frontend',
  plugins: {
    allure: {
      enabled: true,
      require: 'allure-codeceptjs',
    },
    screenshot: {
      enabled: true,
      on: 'fail',
    },
    retryFailedStep: {
      enabled: true,
    },
  },
};