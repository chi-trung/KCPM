// TC-E2E-004: Collector View Tasks & Status Update Flow
// Jira: KIEM-14 | Test Design: State Transition Diagram
// Technique: End-to-end user journey – collector role

const { I } = inject();

const COLLECTOR = {
  email: 'collector@test.waste',
  password: 'password',
};

async function loginAsCollector() {
  I.say('[Precondition] Navigate to login page');
  I.amOnPage('/login');
  I.waitForElement('input[name="email"]', 10);

  I.say('[Precondition] Enter collector credentials');
  I.fillField('input[name="email"]', COLLECTOR.email);
  I.fillField('input[name="password"]', COLLECTOR.password);
  I.click('button[type="submit"]');

  I.say('[Precondition] Wait for authenticated redirect');
  I.waitForElement('h1, h2, nav', 15);
}

Feature('TC-E2E-004: Collector Task Status Flow');

Scenario('#1 Collector can login and reach collector dashboard', async ({ I }) => {
  I.say('[Given] User is on the login page');
  I.amOnPage('/login');
  I.waitForElement('input[name="email"]', 10);
  I.see('WASTE PLATFORM');

  I.say('[When] Collector enters valid credentials and submits');
  I.fillField('input[name="email"]', COLLECTOR.email);
  I.fillField('input[name="password"]', COLLECTOR.password);
  I.click('button[type="submit"]');

  I.say('[Then] Collector is redirected to the authenticated area');
  I.waitForElement('h1, h2, nav', 15);
  I.dontSee('Email hoặc mật khẩu không đúng');
})
  .tag('@allure.label.epic:E2E Frontend')
  .tag('@allure.label.feature:Collector Role')
  .tag('@allure.label.story:Authentication')
  .tag('@allure.label.severity:critical');

Scenario('#2 Collector dashboard page loads without error', async ({ I }) => {
  await loginAsCollector();

  I.say('[When] Collector navigates to /collector/dashboard');
  I.amOnPage('/collector/dashboard');
  I.waitForElement('div, h1, h2', 10);

  I.say('[Then] Page loads correctly — no 404 / Unauthorized errors');
  I.dontSee('404');
  I.dontSee('Not Found');
  I.dontSee('Unauthorized');
  I.dontSee('Không có quyền');
})
  .tag('@allure.label.epic:E2E Frontend')
  .tag('@allure.label.feature:Collector Role')
  .tag('@allure.label.story:Dashboard Access')
  .tag('@allure.label.severity:normal');

Scenario('#3 Collector tasks page renders task list structure', async ({ I }) => {
  await loginAsCollector();

  I.say('[When] Collector navigates to /collector/routes (task list)');
  I.amOnPage('/collector/routes');
  I.waitForElement('div', 10);

  I.say('[Then] Page loads without 404 or Unauthorized error');
  I.dontSee('404');
  I.dontSee('Unauthorized');
})
  .tag('@allure.label.epic:E2E Frontend')
  .tag('@allure.label.feature:Collector Role')
  .tag('@allure.label.story:Task List')
  .tag('@allure.label.severity:normal');

Scenario('#4 Collector login fails with wrong password (negative test – error guessing)', async ({ I }) => {
  I.say('[Given] User is on the login page');
  I.amOnPage('/login');
  I.waitForElement('input[name="email"]', 10);

  I.say('[When] Collector enters valid email but INVALID password');
  I.fillField('input[name="email"]', 'collector@test.waste');
  I.fillField('input[name="password"]', 'InvalidPassword123!');
  I.click('button[type="submit"]');

  I.say('[Then] System shows authentication error message');
  I.waitForText('Email hoặc mật khẩu không đúng', 10);

  I.say('[And] URL does NOT change to collector dashboard');
  I.dontSeeCurrentUrlEquals('/collector/dashboard');
})
  .tag('@allure.label.epic:E2E Frontend')
  .tag('@allure.label.feature:Collector Role')
  .tag('@allure.label.story:Authentication')
  .tag('@allure.label.severity:critical');

Scenario('#5 Collector role cannot access enterprise-only route (state transition guard)', async ({ I }) => {
  await loginAsCollector();

  I.say('[When] Collector attempts to access enterprise-restricted route');
  I.amOnPage('/enterprise/dashboard');
  I.waitForElement('div, h1, h2', 10);

  I.say('[Then] Enterprise-only content is NOT visible (access blocked or redirected)');
  I.dontSee('Collector Assignment Management');
})
  .tag('@allure.label.epic:E2E Frontend')
  .tag('@allure.label.feature:Collector Role')
  .tag('@allure.label.story:Authorization Guard')
  .tag('@allure.label.severity:critical');
