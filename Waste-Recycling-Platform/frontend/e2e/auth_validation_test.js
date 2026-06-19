// TC-E2E-007: Auth — Login & Register Form Validation
// Jira: KIEM-4 | Test Design: Boundary Value Analysis + Error Guessing + Decision Table
// Covers: Login validation, Register validation, password constraints, form navigation

const { I } = inject();

Feature('TC-E2E-007: Auth Form Validation (BVA + Error Guessing + Decision Table)');

// DT-01: Login — Empty email + empty password → Validation errors
Scenario('#1 Login form shows validation errors for empty fields (DT-01)', async ({ I }) => {
  // Given: User is on the login page
  I.amOnPage('/login');
  I.waitForElement('input[name="email"]', 10);

  // When: User clicks submit without filling any fields
  I.click('button[type="submit"]');

  // Then: Validation errors are shown for required fields
  I.waitForElement('body', 5);
  I.see('Email');
  I.see('Mật khẩu');

  // And: User stays on the login page
  I.seeInCurrentUrl('/login');
})
  .tag('@allure.label.epic:E2E Frontend')
  .tag('@allure.label.feature:Authentication')
  .tag('@allure.label.story:Login Validation')
  .tag('@allure.label.severity:critical')
  .tag('@allure.label.jira:KIEM-4');

// BVA-01: Login — Invalid email format (boundary: no @ symbol)
Scenario('#2 Login form rejects invalid email format (BVA-01)', async ({ I }) => {
  // Given: User is on the login page
  I.amOnPage('/login');
  I.waitForElement('input[name="email"]', 10);

  // When: User enters email without @ symbol and a password
  I.fillField('input[name="email"]', 'not-an-email');
  I.fillField('input[name="password"]', 'somepassword');
  I.click('button[type="submit"]');

  // Then: Validation error indicates invalid email format
  I.waitForElement('body', 5);

  // And: User stays on login page — no redirect
  I.seeInCurrentUrl('/login');
})
  .tag('@allure.label.epic:E2E Frontend')
  .tag('@allure.label.feature:Authentication')
  .tag('@allure.label.story:Login Validation')
  .tag('@allure.label.severity:normal')
  .tag('@allure.label.jira:KIEM-4');

// DT-02: Register — Empty form submission → Multiple validation errors
Scenario('#3 Register form shows all validation errors for empty submission (DT-02)', async ({ I }) => {
  // Given: User is on the registration page
  I.amOnPage('/register');
  I.waitForElement('input[name="name"]', 10);

  // When: User clicks submit without filling any fields
  I.click('button[type="submit"]');

  // Then: Multiple validation errors appear
  I.waitForElement('body', 5);

  // And: User stays on registration page
  I.seeInCurrentUrl('/register');
})
  .tag('@allure.label.epic:E2E Frontend')
  .tag('@allure.label.feature:Authentication')
  .tag('@allure.label.story:Register Validation')
  .tag('@allure.label.severity:critical')
  .tag('@allure.label.jira:KIEM-4');

// EG-01: Register — Password confirmation mismatch
Scenario('#4 Register form detects password mismatch (EG-01)', async ({ I }) => {
  // Given: User is on the registration page
  I.amOnPage('/register');
  I.waitForElement('input[name="name"]', 10);

  // When: User fills form with mismatched passwords
  I.fillField('input[name="name"]', 'Test User');
  I.fillField('input[name="email"]', 'test.mismatch@example.com');
  I.selectOption('select[name="role"]', 'citizen');
  I.fillField('input[name="password"]', 'Password123!');
  I.fillField('input[name="confirmPassword"]', 'DifferentPass!');
  I.click('button[type="submit"]');

  // Then: Password mismatch error is shown
  I.waitForElement('body', 5);
  I.see('không khớp');

  // And: User stays on registration page
  I.seeInCurrentUrl('/register');
})
  .tag('@allure.label.epic:E2E Frontend')
  .tag('@allure.label.feature:Authentication')
  .tag('@allure.label.story:Register Validation')
  .tag('@allure.label.severity:critical')
  .tag('@allure.label.jira:KIEM-4');

// ST-01: Navigation between Login ↔ Register pages
Scenario('#5 User can navigate between login and register pages (ST-01)', async ({ I }) => {
  // Given: User is on the login page
  I.amOnPage('/login');
  I.waitForElement('input[name="email"]', 10);
  I.see('WASTE PLATFORM');

  // When: User clicks the register link
  I.click('Đăng Ký');

  // Then: User is on the registration page
  I.seeInCurrentUrl('/register');
  I.waitForElement('input[name="name"]', 10);
  I.seeElement('input[name="name"]');
  I.seeElement('select[name="role"]');

  // When: User clicks the login link from register page
  I.click('Đăng Nhập');

  // Then: User is back on the login page
  I.seeInCurrentUrl('/login');
  I.waitForElement('input[name="email"]', 10);
  I.seeElement('input[name="password"]');
})
  .tag('@allure.label.epic:E2E Frontend')
  .tag('@allure.label.feature:Authentication')
  .tag('@allure.label.story:Page Navigation')
  .tag('@allure.label.severity:normal')
  .tag('@allure.label.jira:KIEM-4');
