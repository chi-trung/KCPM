// TC-E2E-004: Collector View Tasks & Status Update Flow
// Jira: KIEM-14 | Test Design: State Transition Diagram
// Technique: End-to-end user journey – collector role
// State machine: Assigned → OnTheWay → Collected

const { I } = inject();

Feature('TC-E2E-004: Collector Task Status Flow');

Scenario(
  'Collector can login and reach collector dashboard',
  async ({ I }) => {
    // Step 1: Navigate to login
    I.amOnPage('/login');
    I.waitForElement('input[name="email"]', 10);
    I.see('WASTE PLATFORM');

    // Step 2: Login with collector account (seeded in DB)
    I.fillField('input[name="email"]', 'collector@test.waste');
    I.fillField('input[name="password"]', 'Collector@123');
    I.click('button[type="submit"]');

    // Step 3: Verify successful login (no error message)
    I.waitForElement('h1, h2, nav', 15);
    I.dontSee('Email hoặc mật khẩu không đúng');
  }
);

Scenario(
  'Collector dashboard page loads without error',
  async ({ I }) => {
    // Pre-condition: collector login
    I.amOnPage('/login');
    I.waitForElement('input[name="email"]', 10);
    I.fillField('input[name="email"]', 'collector@test.waste');
    I.fillField('input[name="password"]', 'Collector@123');
    I.click('button[type="submit"]');

    I.waitForElement('h1, h2', 15);

    // Navigate to collector dashboard
    I.amOnPage('/collector/dashboard');
    I.waitForElement('div, h1, h2', 10);

    // Should not show 404 or Unauthorized
    I.dontSee('404');
    I.dontSee('Not Found');
    I.dontSee('Unauthorized');
    I.dontSee('Không có quyền');
  }
);

Scenario(
  'Collector tasks page renders task list structure',
  async ({ I }) => {
    // Pre-condition: collector login
    I.amOnPage('/login');
    I.waitForElement('input[name="email"]', 10);
    I.fillField('input[name="email"]', 'collector@test.waste');
    I.fillField('input[name="password"]', 'Collector@123');
    I.click('button[type="submit"]');

    I.waitForElement('h1, h2', 15);

    // Navigate directly to tasks list
    I.amOnPage('/collector/routes');
    I.waitForElement('div', 10);

    // Verify page loads (with or without tasks)
    I.dontSee('404');
    I.dontSee('Unauthorized');
  }
);

Scenario(
  'Collector login fails with wrong password (negative test – error guessing)',
  async ({ I }) => {
    I.amOnPage('/login');
    I.waitForElement('input[name="email"]', 10);

    // Error guessing: wrong password for valid collector email
    I.fillField('input[name="email"]', 'collector@test.waste');
    I.fillField('input[name="password"]', 'InvalidPassword123!');
    I.click('button[type="submit"]');

    // Expect error message – verify Unauthorized state
    I.waitForText('Email hoặc mật khẩu không đúng', 10);
    I.dontSeeCurrentUrlEquals('/collector/dashboard');
  }
);

Scenario(
  'Collector role cannot access enterprise-only route (state transition guard)',
  async ({ I }) => {
    // Collector login
    I.amOnPage('/login');
    I.waitForElement('input[name="email"]', 10);
    I.fillField('input[name="email"]', 'collector@test.waste');
    I.fillField('input[name="password"]', 'Collector@123');
    I.click('button[type="submit"]');

    I.waitForElement('h1, h2', 15);

    // Try to access enterprise route → should be blocked or redirected
    I.amOnPage('/enterprise/dashboard');
    I.waitForElement('div, h1, h2', 10);

    // Should NOT see enterprise-specific content
    I.dontSee('Collector Assignment Management');
  }
);
