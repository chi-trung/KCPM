// TC-E2E-010: Cross-Role Authorization Guard Tests
// Jira: KIEM-21 | Test Design: Equivalence Partitioning + State Transition + Error Guessing
// Covers: Role-based access control for all 4 roles, unauthenticated access blocking

const { I } = inject();

const CITIZEN = {
  email: 'citizen@test.waste',
  password: 'password',
};

const ENTERPRISE = {
  email: 'enterprise@test.waste',
  password: 'password',
};

const COLLECTOR = {
  email: 'collector@test.waste',
  password: 'password',
};

async function loginAs(email, password) {
  I.amOnPage('/login');
  I.waitForElement('input[name="email"]', 10);
  I.fillField('input[name="email"]', email);
  I.fillField('input[name="password"]', password);
  I.click('button[type="submit"]');
  I.waitForElement('h1, h2, nav, .bg-red-50, .bg-green-50', 15);
}

Feature('TC-E2E-010: Cross-Role Authorization Guard (EP + State Transition)');

// EP-01: Invalid partition — Citizen cannot access Admin routes
Scenario('#1 Citizen cannot access admin dashboard (EP — invalid role partition)', async ({ I }) => {
  // Given: Citizen is logged in
  await loginAs(CITIZEN.email, CITIZEN.password);

  // When: Citizen attempts to navigate to admin dashboard
  I.amOnPage('/admin/dashboard');
  I.waitForElement('body', 10);

  // Then: Admin-specific content is NOT visible (route guard redirects)
  I.dontSee('Admin Portal');
  I.dontSee('CWCRP');
  I.dontSee('Quản Lý Người Dùng');
})
  .tag('@allure.label.epic:E2E Frontend')
  .tag('@allure.label.feature:Auth Guard')
  .tag('@allure.label.story:Citizen → Admin Block')
  .tag('@allure.label.severity:blocker')
  .tag('@allure.label.jira:KIEM-21');

// EP-02: Invalid partition — Enterprise cannot access Admin routes
Scenario('#2 Enterprise cannot access admin dashboard (EP — invalid role partition)', async ({ I }) => {
  // Given: Enterprise user is logged in
  await loginAs(ENTERPRISE.email, ENTERPRISE.password);

  // When: Enterprise attempts to navigate to admin dashboard
  I.amOnPage('/admin/dashboard');
  I.waitForElement('body', 10);

  // Then: Admin content is NOT accessible
  I.dontSee('Admin Portal');
  I.dontSee('CWCRP');
  I.dontSee('Quản Lý Người Dùng');
})
  .tag('@allure.label.epic:E2E Frontend')
  .tag('@allure.label.feature:Auth Guard')
  .tag('@allure.label.story:Enterprise → Admin Block')
  .tag('@allure.label.severity:blocker')
  .tag('@allure.label.jira:KIEM-21');

// EP-03: Invalid partition — Citizen cannot access Enterprise routes
Scenario('#3 Citizen cannot access enterprise dashboard (EP — cross-role block)', async ({ I }) => {
  // Given: Citizen is logged in
  await loginAs(CITIZEN.email, CITIZEN.password);

  // When: Citizen attempts to navigate to enterprise dashboard
  I.amOnPage('/enterprise/dashboard');
  I.waitForElement('body', 10);

  // Then: Enterprise-specific content is NOT accessible
  I.dontSee('Quản lý thu gom');
  I.dontSee('Collector Assignment Management');
})
  .tag('@allure.label.epic:E2E Frontend')
  .tag('@allure.label.feature:Auth Guard')
  .tag('@allure.label.story:Citizen → Enterprise Block')
  .tag('@allure.label.severity:critical')
  .tag('@allure.label.jira:KIEM-21');

// EP-04: Invalid partition — Enterprise cannot access Collector routes
Scenario('#4 Enterprise cannot access collector dashboard (EP — cross-role block)', async ({ I }) => {
  // Given: Enterprise user is logged in
  await loginAs(ENTERPRISE.email, ENTERPRISE.password);

  // When: Enterprise attempts to navigate to collector dashboard
  I.amOnPage('/collector/dashboard');
  I.waitForElement('body', 10);

  // Then: Collector-specific content is NOT accessible
  I.dontSee('Cổng thông tin Thu gom');
  I.dontSee('Nhiệm vụ mở');
})
  .tag('@allure.label.epic:E2E Frontend')
  .tag('@allure.label.feature:Auth Guard')
  .tag('@allure.label.story:Enterprise → Collector Block')
  .tag('@allure.label.severity:critical')
  .tag('@allure.label.jira:KIEM-21');

// EG-01: Unauthenticated user — all protected routes redirect to login
Scenario('#5 Unauthenticated user is redirected to login for all protected routes (EG-01)', async ({ I }) => {
  // Given: User is not logged in — fresh session on login page
  I.amOnPage('/login');
  I.waitForElement('input[name="email"]', 10);

  // When: Unauthenticated user tries to access citizen protected route
  I.amOnPage('/citizen/dashboard');
  I.waitForElement('input[name="email"], body', 10);

  // Then: User is redirected to login (auth guard blocks access)
  I.dontSeeInCurrentUrl('/citizen/dashboard');

  // When: Unauthenticated user tries to access admin protected route
  I.amOnPage('/admin/dashboard');
  I.waitForElement('input[name="email"], body', 10);

  // Then: User is redirected to login
  I.dontSeeInCurrentUrl('/admin/dashboard');
})
  .tag('@allure.label.epic:E2E Frontend')
  .tag('@allure.label.feature:Auth Guard')
  .tag('@allure.label.story:Unauthenticated Access Block')
  .tag('@allure.label.severity:blocker')
  .tag('@allure.label.jira:KIEM-21');
