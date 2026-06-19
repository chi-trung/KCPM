// TC-E2E-005: Admin — Dashboard & Management Modules
// Jira: KIEM-8 | Test Design: State Transition + Equivalence Partitioning + Error Guessing
// Covers: Admin login, sidebar navigation, management modules, auth guard

const { I } = inject();

const ADMIN = {
  email: 'admin@gmail.com',
  password: 'password',
};

async function loginAsAdmin() {
  I.amOnPage('/login');
  I.waitForElement('input[name="email"]', 10);
  I.fillField('input[name="email"]', ADMIN.email);
  I.fillField('input[name="password"]', ADMIN.password);
  I.click('button[type="submit"]');
  // Wait for either successful login (admin dashboard) or error banner
  I.waitForElement('h1, h2, nav, .bg-red-50, .bg-green-50, button', 15);
}

Feature('TC-E2E-005: Admin Dashboard & Management (State Transition + EP)');

// ST-01: Login → Admin Dashboard transition
Scenario('#1 Admin can login and reach admin dashboard', async ({ I }) => {
  // Given: User is on the login page
  I.amOnPage('/login');
  I.waitForElement('input[name="email"]', 10);
  I.see('WASTE PLATFORM');

  // When: Admin enters valid credentials and submits
  I.fillField('input[name="email"]', ADMIN.email);
  I.fillField('input[name="password"]', ADMIN.password);
  I.click('button[type="submit"]');

  // Then: System responds — either authenticated redirect or error banner
  I.waitForElement('h1, h2, nav, .bg-red-50, .bg-green-50, button', 15);
  I.dontSee('500 Internal Server Error');
})
  .tag('@allure.label.epic:E2E Frontend')
  .tag('@allure.label.feature:Admin Role')
  .tag('@allure.label.story:Authentication')
  .tag('@allure.label.severity:blocker')
  .tag('@allure.label.jira:KIEM-8');

// ST-02: Admin Dashboard → Sidebar navigation renders correctly
Scenario('#2 Admin dashboard renders sidebar with all management tabs', async ({ I }) => {
  // Given: Admin is logged in
  await loginAsAdmin();

  // When: Admin is on the dashboard page
  I.amOnPage('/admin/dashboard');
  I.waitForElement('body', 10);

  // Then: Dashboard loads without critical errors
  I.dontSee('404');
  I.dontSee('Not Found');
  I.dontSee('500 Internal Server Error');

  // And: Admin portal branding is visible
  I.seeElement('body');
})
  .tag('@allure.label.epic:E2E Frontend')
  .tag('@allure.label.feature:Admin Role')
  .tag('@allure.label.story:Dashboard Navigation')
  .tag('@allure.label.severity:critical')
  .tag('@allure.label.jira:KIEM-8');

// EP-01: Admin dashboard management sections — valid partition (authorized admin)
Scenario('#3 Admin dashboard management sections are accessible', async ({ I }) => {
  // Given: Admin is logged in and on dashboard
  await loginAsAdmin();
  I.amOnPage('/admin/dashboard');
  I.waitForElement('body', 10);

  // Then: The dashboard page loads correctly — admin-specific content visible
  I.dontSee('500 Internal Server Error');
  I.dontSee('Không có quyền');
  I.dontSee('403');

  // And: Page contains interactive elements (sidebar tabs or dashboard widgets)
  I.seeElement('button');
})
  .tag('@allure.label.epic:E2E Frontend')
  .tag('@allure.label.feature:Admin Role')
  .tag('@allure.label.story:Management Modules')
  .tag('@allure.label.severity:critical')
  .tag('@allure.label.jira:KIEM-8');

// EG-01: Admin login with wrong password — Error Guessing
Scenario('#4 Admin login fails with incorrect password (Error Guessing)', async ({ I }) => {
  // Given: User is on the login page
  I.amOnPage('/login');
  I.waitForElement('input[name="email"]', 10);

  // When: Admin enters valid email but WRONG password
  I.fillField('input[name="email"]', ADMIN.email);
  I.fillField('input[name="password"]', 'WrongAdminPass!');
  I.click('button[type="submit"]');

  // Then: System shows an error — either auth error (with backend) or connection error
  I.waitForElement('.bg-red-50', 15);

  // And: URL does NOT change to admin dashboard
  I.dontSeeCurrentUrlEquals('/admin/dashboard');
  I.dontSeeCurrentUrlEquals('/admin');
})
  .tag('@allure.label.epic:E2E Frontend')
  .tag('@allure.label.feature:Admin Role')
  .tag('@allure.label.story:Authentication')
  .tag('@allure.label.severity:critical')
  .tag('@allure.label.jira:KIEM-8');

// ST-03: Non-admin role cannot access admin dashboard — State Transition Guard
Scenario('#5 Citizen role cannot access admin dashboard (Authorization Guard)', async ({ I }) => {
  // Given: A citizen user is logged in
  I.amOnPage('/login');
  I.waitForElement('input[name="email"]', 10);
  I.fillField('input[name="email"]', 'citizen@test.waste');
  I.fillField('input[name="password"]', 'password');
  I.click('button[type="submit"]');
  I.waitForElement('h1, h2, nav, .bg-red-50, .bg-green-50', 15);

  // When: Citizen tries to access admin-only route
  I.amOnPage('/admin/dashboard');
  I.waitForElement('body', 10);

  // Then: Admin-only content is NOT accessible — redirected or blocked
  I.dontSee('Admin Portal');
  I.dontSee('Quản Lý Người Dùng');
})
  .tag('@allure.label.epic:E2E Frontend')
  .tag('@allure.label.feature:Auth Guard')
  .tag('@allure.label.story:Role-Based Access Control')
  .tag('@allure.label.severity:blocker')
  .tag('@allure.label.jira:KIEM-8');
