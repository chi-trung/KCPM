// TC-E2E-002: Citizen Register & Create Waste Report
// Jira: KIEM-FE | Test Design: State Transition + Error Guessing
// Technique: End-to-end user journey – happy path + validation guard

const { I } = inject();

// Fixed test account (unique enough to avoid conflicts in CI)
const TEST_CITIZEN = {
  name: 'E2E Test Citizen',
  email: `e2e.citizen.${Date.now()}@test.waste`,
  password: 'Test@12345',
  role: 'citizen',
};

Feature('TC-E2E-002: Citizen Report Flow');

Scenario(
  'Citizen can register a new account and reach citizen dashboard',
  async ({ I }) => {
    // Step 1: Navigate to register page
    I.amOnPage('/register');
    I.waitForElement('input[name="name"]', 10);
    I.see('WASTE PLATFORM');

    // Step 2: Fill registration form
    I.fillField('input[name="name"]', TEST_CITIZEN.name);
    I.fillField('input[name="email"]', TEST_CITIZEN.email);
    I.selectOption('select[name="role"]', 'citizen');
    I.fillField('input[name="password"]', TEST_CITIZEN.password);
    I.fillField('input[name="confirmPassword"]', TEST_CITIZEN.password);

    // Step 3: Submit
    I.click('button[type="submit"]');

    // Step 4: Verify success – redirected to citizen dashboard
    I.waitForText('Đăng ký thành công', 10);
    // After auto-redirect, should be on citizen area
    I.waitForElement('a[href="/citizen/create-report"], a[href*="citizen"], h1', 15);
  }
);

Scenario(
  'Citizen can navigate to create-report page and see the form',
  async ({ I }) => {
    // Pre-condition: Login with seeded citizen account
    I.amOnPage('/login');
    I.waitForElement('input[name="email"]', 10);
    I.fillField('input[name="email"]', 'quantranhoang24@gmail.com');
    I.fillField('input[name="password"]', 'Quan1109');
    I.click('button[type="submit"]');

    // Wait for redirect to citizen area
    I.waitForText('Tạo Báo Cáo', 15);

    // Navigate to create-report
    I.amOnPage('/citizen/create-report');
    I.waitForElement('h1', 10);
    I.see('Tạo Báo Cáo');

    // Verify form sections are present
    I.seeElement('input[placeholder*="địa chỉ"]');
    I.seeElement('textarea');
    I.seeElement('button[type="submit"]');
  }
);

Scenario(
  'Create-report form shows validation error when submitted empty',
  async ({ I }) => {
    // Pre-condition: must be logged in as citizen
    I.amOnPage('/login');
    I.waitForElement('input[name="email"]', 10);
    I.fillField('input[name="email"]', 'quantranhoang24@gmail.com');
    I.fillField('input[name="password"]', 'Quan1109');
    I.click('button[type="submit"]');

    I.waitForText('Tạo Báo Cáo', 15);

    // Navigate to create-report
    I.amOnPage('/citizen/create-report');
    I.waitForElement('button[type="submit"]', 10);

    // Submit without filling anything
    I.click('button[type="submit"]');

    // Expect validation error (address required, image required)
    I.waitForText('Vui lòng điền đầy đủ', 5);
  }
);

Scenario(
  'Citizen reports list page is accessible after login',
  async ({ I }) => {
    I.amOnPage('/login');
    I.waitForElement('input[name="email"]', 10);
    I.fillField('input[name="email"]', 'quantranhoang24@gmail.com');
    I.fillField('input[name="password"]', 'Quan1109');
    I.click('button[type="submit"]');

    I.waitForText('Tạo Báo Cáo', 15);

    I.amOnPage('/citizen/reports');
    I.waitForElement('div, h1, h2', 10);
    // Page should load without error
    I.dontSee('404');
    I.dontSee('Not Found');
  }
);
