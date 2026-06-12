// TC-E2E-002: Citizen Register & Create Waste Report
// Jira: KIEM-21 | Test Design: State Transition + Error Guessing

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
  I.amOnPage('/login');
  I.waitForElement('input[name="email"]', 10);
  I.fillField('input[name="email"]', CITIZEN.email);
  I.fillField('input[name="password"]', CITIZEN.password);
  I.click('button[type="submit"]');
  I.waitForText('Tạo Báo Cáo', 15);
}

Feature('TC-E2E-002: Citizen Report Flow');

Scenario('#1 Citizen can register a new account and reach citizen dashboard', async ({ I }) => {
  // Given: User is on the registration page
  I.amOnPage('/register');
  I.waitForElement('input[name="name"]', 10);
  I.see('WASTE PLATFORM');

  // When: Citizen fills in the registration form with valid data
  I.fillField('input[name="name"]', TEST_CITIZEN.name);
  I.fillField('input[name="email"]', TEST_CITIZEN.email);
  I.selectOption('select[name="role"]', 'citizen');
  I.fillField('input[name="password"]', TEST_CITIZEN.password);
  I.fillField('input[name="confirmPassword"]', TEST_CITIZEN.password);
  I.click('button[type="submit"]');

  // Then: System shows success message and redirects to citizen dashboard
  I.waitForText('Đăng ký thành công', 10);
  I.waitForElement('a[href="/citizen/create-report"], a[href*="citizen"], h1', 15);
})
  .tag('@allure.label.epic:E2E Frontend')
  .tag('@allure.label.feature:Citizen Role')
  .tag('@allure.label.story:Registration')
  .tag('@allure.label.severity:critical');

Scenario('#2 Citizen can navigate to create-report page and see the form', async ({ I }) => {
  // Given: Citizen is logged in
  await loginAsCitizen();

  // When: Citizen navigates to the create-report page
  I.amOnPage('/citizen/create-report');
  I.waitForElement('h1', 10);

  // Then: Report creation form is displayed with required fields
  I.see('Tạo Báo Cáo');
  I.seeElement('input[placeholder*="địa chỉ"]');
  I.seeElement('textarea');
  I.seeElement('button[type="submit"]');
})
  .tag('@allure.label.epic:E2E Frontend')
  .tag('@allure.label.feature:Citizen Role')
  .tag('@allure.label.story:Waste Report Creation')
  .tag('@allure.label.severity:critical');

Scenario('#3 Create-report form shows validation error when submitted empty', async ({ I }) => {
  // Given: Citizen is logged in
  await loginAsCitizen();

  // When: Citizen navigates to create-report page and clicks Submit without filling fields
  I.amOnPage('/citizen/create-report');
  I.waitForElement('button[type="submit"]', 10);
  I.click('button[type="submit"]');

  // Then: Form shows validation error — address and image are required
  I.waitForText('Vui lòng điền đầy đủ', 5);
})
  .tag('@allure.label.epic:E2E Frontend')
  .tag('@allure.label.feature:Citizen Role')
  .tag('@allure.label.story:Form Validation')
  .tag('@allure.label.severity:normal');

Scenario('#4 Citizen reports list page is accessible after login', async ({ I }) => {
  // Given: User is on the login page
  I.amOnPage('/login');
  I.waitForElement('input[name="email"]', 10);

  // When: Citizen logs in with valid credentials
  I.fillField('input[name="email"]', 'quantranhoang24@gmail.com');
  I.fillField('input[name="password"]', 'Quan1109');
  I.click('button[type="submit"]');
  I.waitForText('Tạo Báo Cáo', 15);

  // When: Citizen navigates to the reports list page
  I.amOnPage('/citizen/reports');
  I.waitForElement('div, h1, h2', 10);

  // Then: Reports page loads successfully without errors
  I.dontSee('404');
  I.dontSee('Not Found');
})
  .tag('@allure.label.epic:E2E Frontend')
  .tag('@allure.label.feature:Citizen Role')
  .tag('@allure.label.story:Report History')
  .tag('@allure.label.severity:normal');
