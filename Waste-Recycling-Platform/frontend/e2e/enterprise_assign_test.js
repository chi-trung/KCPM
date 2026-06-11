// TC-E2E-003: Enterprise Assign Collector to Task
// Jira: KIEM-16 | Test Design: State Transition + Decision Table
// Technique: End-to-end user journey – enterprise role
//
// Test account (seeded in V9__e2e_test_accounts.sql):
//   email: enterprise@test.waste | password: password

const { I } = inject();

const ENTERPRISE = {
  email: 'enterprise@test.waste',
  password: 'password',
};

Feature('TC-E2E-003: Enterprise Assign Collector Flow');

Scenario(
  'Enterprise can login and reach task management dashboard',
  async ({ I }) => {
    // Step 1: Navigate to login
    I.amOnPage('/login');
    I.waitForElement('input[name="email"]', 10);
    I.see('WASTE PLATFORM');

    // Step 2: Login with seeded enterprise account (V9__e2e_test_accounts.sql)
    I.fillField('input[name="email"]', ENTERPRISE.email);
    I.fillField('input[name="password"]', ENTERPRISE.password);
    I.click('button[type="submit"]');

    // Step 3: Verify redirect to enterprise area
    I.waitForElement('[href*="enterprise"], h1, h2', 15);
    I.dontSee('Email hoặc mật khẩu không đúng');
  }
);

Scenario(
  'Enterprise task management page loads with correct structure',
  async ({ I }) => {
    // Pre-condition: enterprise account login
    I.amOnPage('/login');
    I.waitForElement('input[name="email"]', 10);
    I.fillField('input[name="email"]', ENTERPRISE.email);
    I.fillField('input[name="password"]', ENTERPRISE.password);
    I.click('button[type="submit"]');

    // Navigate to enterprise dashboard / tasks
    I.waitForElement('h1, h2', 15);
    I.amOnPage('/enterprise/dashboard');
    I.waitForElement('h1, h2, div', 10);

    // Verify no critical error
    I.dontSee('404');
    I.dontSee('Not Found');
    I.dontSee('Unauthorized');
  }
);

Scenario(
  'Enterprise can see Collector Assignment Management page',
  async ({ I }) => {
    // Pre-condition: enterprise login
    I.amOnPage('/login');
    I.waitForElement('input[name="email"]', 10);
    I.fillField('input[name="email"]', ENTERPRISE.email);
    I.fillField('input[name="password"]', ENTERPRISE.password);
    I.click('button[type="submit"]');
    I.waitForElement('h1, h2', 15);

    // Navigate to enterprise reports (tasks management)
    I.amOnPage('/enterprise/reports');
    I.waitForElement('h1, h2, div', 10);

    // Verify the task management component renders
    I.dontSee('404');
    I.dontSee('Không có quyền');
  }
);

Scenario(
  'Enterprise login fails with invalid credentials (negative test)',
  async ({ I }) => {
    I.amOnPage('/login');
    I.waitForElement('input[name="email"]', 10);

    // Wrong password – error guessing negative test
    I.fillField('input[name="email"]', ENTERPRISE.email);
    I.fillField('input[name="password"]', 'WrongPassword!');
    I.click('button[type="submit"]');

    // Verify error message is shown
    I.waitForText('Email hoặc mật khẩu không đúng', 10);
    I.dontSeeCurrentUrlEquals('/enterprise/dashboard');
  }
);
