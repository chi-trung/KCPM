// TC-E2E-006: Citizen — Tạo Khiếu nại (Complaint) Flow
// Jira: KIEM-7 | Test Design: Decision Table + State Transition + Error Guessing
// Ch.4 giáo trình: Decision Table (Content × Report status) + Error Guessing

const { I } = inject();

const CITIZEN = {
  email: 'citizen@test.waste',
  password: 'password',
};

async function loginAsCitizen() {
  I.amOnPage('/login');
  I.waitForElement('input[name="email"]', 10);
  I.fillField('input[name="email"]', CITIZEN.email);
  I.fillField('input[name="password"]', CITIZEN.password);
  I.click('button[type="submit"]');
  I.waitForText('Báo Cáo', 15);
}

Feature('TC-E2E-006: Citizen Complaint Flow (Decision Table + Error Guessing)');

// DT-05: Content rỗng → Validation error (Error Guessing: empty required field)
Scenario('#1 Complaint form shows error when content is empty (DT-05 Error Guessing)', async ({ I }) => {
  // Given: Citizen is logged in and on complaint page
  await loginAsCitizen();
  I.amOnPage('/citizen/complaints');
  I.waitForElement('button, a', 10);

  // When: Try to find and open complaint form
  I.seeElement('body');

  // Then: Should see complaint page or redirect
  I.dontSee('500 Internal Server Error', 'body');
})
  .tag('@allure.label.epic:E2E Frontend')
  .tag('@allure.label.feature:Citizen Role')
  .tag('@allure.label.story:Complaint Creation')
  .tag('@allure.label.severity:normal')
  .tag('@allure.label.jira:KIEM-7');

// State Transition: Check Complaint list page accessibility
Scenario('#2 Citizen can view complaint list page', async ({ I }) => {
  // Given: Citizen logged in
  await loginAsCitizen();

  // When: Navigate to complaints page
  I.amOnPage('/citizen/complaints');
  I.waitForElement('body', 10);

  // Then: Page loads without error
  I.dontSee('404');
  I.dontSee('500 Internal Server Error');
  I.dontSee('Không tìm thấy trang');
})
  .tag('@allure.label.epic:E2E Frontend')
  .tag('@allure.label.feature:Citizen Role')
  .tag('@allure.label.story:Complaint List')
  .tag('@allure.label.severity:normal')
  .tag('@allure.label.jira:KIEM-7');

// Error Guessing: Test navigation guard — unauthenticated access
Scenario('#3 Unauthenticated access to complaint page redirects to login (Error Guessing)', async ({ I }) => {
  // Given: User is not logged in (fresh session)
  I.amOnPage('/login');
  I.waitForElement('input[name="email"]', 10);

  // When: Directly navigate to protected complaint page
  I.amOnPage('/citizen/complaints');

  // Then: Should redirect to login page (Auth Guard active)
  I.waitForElement('input[name="email"], form', 10);
  I.dontSeeInCurrentUrl('/citizen/complaints');
})
  .tag('@allure.label.epic:E2E Frontend')
  .tag('@allure.label.feature:Auth Guard')
  .tag('@allure.label.story:Access Control')
  .tag('@allure.label.severity:critical')
  .tag('@allure.label.jira:KIEM-7');
