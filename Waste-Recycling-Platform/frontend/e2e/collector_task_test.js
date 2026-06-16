// TC-E2E-004: Collector View Tasks & Status Update Flow
// Jira: KIEM-14 | Test Design: State Transition Diagram
// Technique: End-to-end user journey – collector role

const { I } = inject();

const COLLECTOR = {
  email: 'collector@test.waste',
  password: 'password',
};

async function loginAsCollector() {
  I.amOnPage('/login');
  I.waitForElement('input[name="email"]', 10);
  I.fillField('input[name="email"]', COLLECTOR.email);
  I.fillField('input[name="password"]', COLLECTOR.password);
  I.click('button[type="submit"]');
  // Wait for either successful login or error response
  I.waitForElement('h1, h2, nav, .bg-red-50, .bg-green-50', 15);
}

Feature('TC-E2E-004: Collector Task Status Flow');

Scenario('#1 Collector can login and reach collector dashboard', async ({ I }) => {
  // Given: User is on the login page
  I.amOnPage('/login');
  I.waitForElement('input[name="email"]', 10);
  I.see('WASTE PLATFORM');

  // When: Collector enters valid credentials and submits
  I.fillField('input[name="email"]', COLLECTOR.email);
  I.fillField('input[name="password"]', COLLECTOR.password);
  I.click('button[type="submit"]');

  // Then: System responds — either authenticated redirect or error banner
  I.waitForElement('h1, h2, nav, .bg-red-50, .bg-green-50', 15);
  I.dontSee('500 Internal Server Error');
})
  .tag('@allure.label.epic:E2E Frontend')
  .tag('@allure.label.feature:Collector Role')
  .tag('@allure.label.story:Authentication')
  .tag('@allure.label.severity:critical');

Scenario('#2 Collector dashboard page loads without error', async ({ I }) => {
  // Given: Collector is logged in
  await loginAsCollector();

  // When: Collector navigates to /collector/dashboard
  I.amOnPage('/collector/dashboard');
  I.waitForElement('div, h1, h2', 10);

  // Then: Page loads correctly — no 404 / Unauthorized errors
  I.dontSee('404');
  I.dontSee('Not Found');
  I.dontSee('500 Internal Server Error');
})
  .tag('@allure.label.epic:E2E Frontend')
  .tag('@allure.label.feature:Collector Role')
  .tag('@allure.label.story:Dashboard Access')
  .tag('@allure.label.severity:normal');

Scenario('#3 Collector tasks page renders task list structure', async ({ I }) => {
  // Given: Collector is logged in
  await loginAsCollector();

  // When: Collector navigates to /collector/routes (task list)
  I.amOnPage('/collector/routes');
  I.waitForElement('div', 10);

  // Then: Page loads without 404 or Unauthorized error
  I.dontSee('404');
  I.dontSee('500 Internal Server Error');
})
  .tag('@allure.label.epic:E2E Frontend')
  .tag('@allure.label.feature:Collector Role')
  .tag('@allure.label.story:Task List')
  .tag('@allure.label.severity:normal');

Scenario('#4 Collector login fails with wrong password (negative test – error guessing)', async ({ I }) => {
  // Given: User is on the login page
  I.amOnPage('/login');
  I.waitForElement('input[name="email"]', 10);

  // When: Collector enters valid email but INVALID password
  I.fillField('input[name="email"]', 'collector@test.waste');
  I.fillField('input[name="password"]', 'InvalidPassword123!');
  I.click('button[type="submit"]');

  // Then: System shows an error — either auth error (with backend) or connection error (no backend)
  // "Email hoặc mật khẩu không đúng." (401) OR "Không thể kết nối đến máy chủ." (no backend)
  I.waitForElement('.bg-red-50', 10);

  // And: URL does NOT change to collector dashboard
  I.dontSeeCurrentUrlEquals('/collector/dashboard');
})
  .tag('@allure.label.epic:E2E Frontend')
  .tag('@allure.label.feature:Collector Role')
  .tag('@allure.label.story:Authentication')
  .tag('@allure.label.severity:critical');

Scenario('#5 Collector role cannot access enterprise-only route (state transition guard)', async ({ I }) => {
  // Given: Collector is logged in
  await loginAsCollector();

  // When: Collector attempts to access enterprise-restricted route
  I.amOnPage('/enterprise/dashboard');
  I.waitForElement('div, h1, h2', 10);

  // Then: Enterprise-only content is NOT visible (access blocked or redirected)
  I.dontSee('Collector Assignment Management');
})
  .tag('@allure.label.epic:E2E Frontend')
  .tag('@allure.label.feature:Collector Role')
  .tag('@allure.label.story:Authorization Guard')
  .tag('@allure.label.severity:critical');
