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

/** Helper: login as enterprise account */
async function loginAsEnterprise() {
  I.say('[Precondition] Navigate to login page');
  I.amOnPage('/login');
  I.waitForElement('input[name="email"]', 10);

  I.say('[Precondition] Enter enterprise credentials');
  I.fillField('input[name="email"]', ENTERPRISE.email);
  I.fillField('input[name="password"]', ENTERPRISE.password);
  I.click('button[type="submit"]');

  I.say('[Precondition] Wait for redirect to authenticated area');
  I.waitForElement('h1, h2', 15);
}

Feature('TC-E2E-003: Enterprise Assign Collector Flow');

Scenario(
  '#1 Enterprise can login and reach task management dashboard',
  async ({ I }) => {
    I.say('[Given] User is on the login page');
    I.amOnPage('/login');
    I.waitForElement('input[name="email"]', 10);
    I.see('WASTE PLATFORM');

    I.say('[When] Enterprise user enters valid credentials and submits');
    I.fillField('input[name="email"]', ENTERPRISE.email);
    I.fillField('input[name="password"]', ENTERPRISE.password);
    I.click('button[type="submit"]');

    I.say('[Then] Enterprise user is redirected to the enterprise dashboard area');
    I.waitForElement('[href*="enterprise"], h1, h2', 15);
    I.dontSee('Email hoặc mật khẩu không đúng');
  }
);

Scenario(
  '#2 Enterprise task management page loads with correct structure',
  async ({ I }) => {
    await loginAsEnterprise();

    I.say('[When] Enterprise user navigates to /enterprise/dashboard');
    I.amOnPage('/enterprise/dashboard');
    I.waitForElement('h1, h2, div', 10);

    I.say('[Then] Dashboard renders without critical errors');
    I.dontSee('404');
    I.dontSee('Not Found');
    I.dontSee('Unauthorized');
  }
);

Scenario(
  '#3 Enterprise can see Collector Assignment Management page',
  async ({ I }) => {
    await loginAsEnterprise();

    I.say('[When] Enterprise user navigates to /enterprise/reports (task assignment)');
    I.amOnPage('/enterprise/reports');
    I.waitForElement('h1, h2, div', 10);

    I.say('[Then] Page loads without access restriction errors');
    I.dontSee('404');
    I.dontSee('Không có quyền');
  }
);

Scenario(
  '#4 Enterprise login fails with invalid credentials (negative test)',
  async ({ I }) => {
    I.say('[Given] User is on the login page');
    I.amOnPage('/login');
    I.waitForElement('input[name="email"]', 10);

    I.say('[When] Enterprise user enters valid email but WRONG password');
    I.fillField('input[name="email"]', ENTERPRISE.email);
    I.fillField('input[name="password"]', 'WrongPassword!');
    I.click('button[type="submit"]');

    I.say('[Then] System displays authentication error message');
    I.waitForText('Email hoặc mật khẩu không đúng', 10);

    I.say('[And] URL remains on login page — no redirect to enterprise area');
    I.dontSeeCurrentUrlEquals('/enterprise/dashboard');
  }
);
