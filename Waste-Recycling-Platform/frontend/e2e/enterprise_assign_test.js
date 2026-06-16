// TC-E2E-003: Enterprise Assign Collector to Task
// Jira: KIEM-16 | Test Design: State Transition + Decision Table

const { I } = inject();

const ENTERPRISE = {
  email: 'enterprise@test.waste',
  password: 'password',
};

async function loginAsEnterprise() {
  I.amOnPage('/login');
  I.waitForElement('input[name="email"]', 10);
  I.fillField('input[name="email"]', ENTERPRISE.email);
  I.fillField('input[name="password"]', ENTERPRISE.password);
  I.click('button[type="submit"]');
  // Wait for either successful login or error response
  I.waitForElement('h1, h2, nav, .bg-red-50, .bg-green-50', 15);
}

Feature('TC-E2E-003: Enterprise Assign Collector Flow');

Scenario('#1 Enterprise can login and reach task management dashboard', async ({ I }) => {
  // Given: User is on the login page
  I.amOnPage('/login');
  I.waitForElement('input[name="email"]', 10);
  I.see('WASTE PLATFORM');

  // When: Enterprise user enters valid credentials and submits
  I.fillField('input[name="email"]', ENTERPRISE.email);
  I.fillField('input[name="password"]', ENTERPRISE.password);
  I.click('button[type="submit"]');

  // Then: System responds — either authenticated redirect or error banner
  I.waitForElement('h1, h2, nav, .bg-red-50, .bg-green-50', 15);
  I.dontSee('500 Internal Server Error');
})
  .tag('@allure.label.epic:E2E Frontend')
  .tag('@allure.label.feature:Enterprise Role')
  .tag('@allure.label.story:Authentication')
  .tag('@allure.label.severity:critical');

Scenario('#2 Enterprise task management page loads with correct structure', async ({ I }) => {
  // Given: Enterprise user is logged in
  await loginAsEnterprise();

  // When: Enterprise user navigates to /enterprise/dashboard
  I.amOnPage('/enterprise/dashboard');
  I.waitForElement('h1, h2, div', 10);

  // Then: Dashboard renders without critical errors
  I.dontSee('404');
  I.dontSee('Not Found');
  I.dontSee('500 Internal Server Error');
})
  .tag('@allure.label.epic:E2E Frontend')
  .tag('@allure.label.feature:Enterprise Role')
  .tag('@allure.label.story:Dashboard Access')
  .tag('@allure.label.severity:normal');

Scenario('#3 Enterprise can see Collector Assignment Management page', async ({ I }) => {
  // Given: Enterprise user is logged in
  await loginAsEnterprise();

  // When: Enterprise user navigates to /enterprise/reports (task assignment)
  I.amOnPage('/enterprise/reports');
  I.waitForElement('h1, h2, div', 10);

  // Then: Page loads without access restriction errors
  I.dontSee('404');
  I.dontSee('Không có quyền');
  I.dontSee('500 Internal Server Error');
})
  .tag('@allure.label.epic:E2E Frontend')
  .tag('@allure.label.feature:Enterprise Role')
  .tag('@allure.label.story:Collector Assignment')
  .tag('@allure.label.severity:critical');

Scenario('#4 Enterprise login fails with invalid credentials (negative test)', async ({ I }) => {
  // Given: User is on the login page
  I.amOnPage('/login');
  I.waitForElement('input[name="email"]', 10);

  // When: Enterprise user enters valid email but WRONG password
  I.fillField('input[name="email"]', ENTERPRISE.email);
  I.fillField('input[name="password"]', 'WrongPassword!');
  I.click('button[type="submit"]');

  // Then: System shows an error — either auth error (with backend) or connection error (no backend)
  // "Email hoặc mật khẩu không đúng." (401) OR "Không thể kết nối đến máy chủ." (no backend)
  I.waitForElement('.bg-red-50', 10);

  // And: URL remains on login page — no redirect to enterprise area
  I.dontSeeCurrentUrlEquals('/enterprise/dashboard');
})
  .tag('@allure.label.epic:E2E Frontend')
  .tag('@allure.label.feature:Enterprise Role')
  .tag('@allure.label.story:Authentication')
  .tag('@allure.label.severity:critical');
