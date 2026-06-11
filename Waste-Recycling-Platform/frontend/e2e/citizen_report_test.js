// TC-E2E-002: Citizen Register & Create Waste Report
// Jira: KIEM-FE | Test Design: State Transition + Error Guessing
// Technique: End-to-end user journey – happy path + validation guard
//
// Pre-seeded account (V9__e2e_test_accounts.sql):
//   citizen@test.waste / password

const { I } = inject();

const CITIZEN = {
  email: 'citizen@test.waste',
  password: 'password',
};

const TEST_CITIZEN = {
  name: 'E2E Test Citizen',
  email: `e2e.citizen.${Date.now()}@test.waste`,
  password: 'Test@12345',
  role: 'citizen',
};

async function loginAsCitizen() {
  I.say('[Precondition] Navigate to login page');
  I.amOnPage('/login');
  I.waitForElement('input[name="email"]', 10);

  I.say('[Precondition] Enter citizen credentials');
  I.fillField('input[name="email"]', CITIZEN.email);
  I.fillField('input[name="password"]', CITIZEN.password);
  I.click('button[type="submit"]');

  I.say('[Precondition] Wait for redirect to citizen area');
  I.waitForText('Tạo Báo Cáo', 15);
}

Feature('TC-E2E-002: Citizen Report Flow');

Scenario(
  '#1 Citizen can register a new account and reach citizen dashboard',
  { epic: 'E2E Frontend', feature: 'Citizen Role', story: 'Registration', severity: 'critical' },
  async ({ I }) => {
    I.say('[Given] User is on the registration page');
    I.amOnPage('/register');
    I.waitForElement('input[name="name"]', 10);
    I.see('WASTE PLATFORM');

    I.say('[When] Citizen fills in the registration form with valid data');
    I.fillField('input[name="name"]', TEST_CITIZEN.name);
    I.fillField('input[name="email"]', TEST_CITIZEN.email);
    I.selectOption('select[name="role"]', 'citizen');
    I.fillField('input[name="password"]', TEST_CITIZEN.password);
    I.fillField('input[name="confirmPassword"]', TEST_CITIZEN.password);

    I.say('[When] Citizen submits the registration form');
    I.click('button[type="submit"]');

    I.say('[Then] System shows success message and redirects to citizen dashboard');
    I.waitForText('Đăng ký thành công', 10);
    I.waitForElement('a[href="/citizen/create-report"], a[href*="citizen"], h1', 15);
  }
);

Scenario(
  '#2 Citizen can navigate to create-report page and see the form',
  { epic: 'E2E Frontend', feature: 'Citizen Role', story: 'Waste Report Creation', severity: 'critical' },
  async ({ I }) => {
    await loginAsCitizen();

    I.say('[When] Citizen navigates to the create-report page');
    I.amOnPage('/citizen/create-report');
    I.waitForElement('h1', 10);

    I.say('[Then] Report creation form is displayed with required fields');
    I.see('Tạo Báo Cáo');
    I.seeElement('input[placeholder*="địa chỉ"]');
    I.seeElement('textarea');
    I.seeElement('button[type="submit"]');
  }
);

Scenario(
  '#3 Create-report form shows validation error when submitted empty',
  { epic: 'E2E Frontend', feature: 'Citizen Role', story: 'Form Validation', severity: 'normal' },
  async ({ I }) => {
    await loginAsCitizen();

    I.say('[When] Citizen navigates to create-report page');
    I.amOnPage('/citizen/create-report');
    I.waitForElement('button[type="submit"]', 10);

    I.say('[When] Citizen clicks Submit without filling any fields');
    I.click('button[type="submit"]');

    I.say('[Then] Form shows validation error — address and image are required');
    I.waitForText('Vui lòng điền đầy đủ', 5);
  }
);

Scenario(
  '#4 Citizen reports list page is accessible after login',
  { epic: 'E2E Frontend', feature: 'Citizen Role', story: 'Report History', severity: 'normal' },
  async ({ I }) => {
    I.say('[Given] User is on the login page');
    I.amOnPage('/login');
    I.waitForElement('input[name="email"]', 10);

    I.say('[When] Citizen logs in with valid credentials');
    I.fillField('input[name="email"]', 'quantranhoang24@gmail.com');
    I.fillField('input[name="password"]', 'Quan1109');
    I.click('button[type="submit"]');
    I.waitForText('Tạo Báo Cáo', 15);

    I.say('[When] Citizen navigates to the reports list page');
    I.amOnPage('/citizen/reports');
    I.waitForElement('div, h1, h2', 10);

    I.say('[Then] Reports page loads successfully without errors');
    I.dontSee('404');
    I.dontSee('Not Found');
  }
);
