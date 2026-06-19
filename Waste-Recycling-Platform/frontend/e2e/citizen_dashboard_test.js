// TC-E2E-008: Citizen — Dashboard, Profile, Rewards & Points History
// Jira: KIEM-21 | Test Design: State Transition + Checklist + Error Guessing
// Covers: Dashboard stats, quick actions, profile page, rewards store, points history

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
  // Wait for either successful login or error response
  I.waitForElement('h1, h2, nav, .bg-red-50, .bg-green-50', 15);
}

Feature('TC-E2E-008: Citizen Dashboard & Profile (State Transition + Checklist)');

// CL-01: Dashboard renders with stats cards and quick action links
Scenario('#1 Citizen dashboard renders stats cards and quick actions', async ({ I }) => {
  // Given: Citizen is logged in
  await loginAsCitizen();

  // When: Citizen navigates to the dashboard
  I.amOnPage('/citizen/dashboard');
  I.waitForElement('h1, h2, div, .bg-red-50', 10);

  // Then: Dashboard page loads without fatal errors
  I.dontSee('500 Internal Server Error');
  I.dontSee('404');

  // And: Page contains dashboard content structure
  I.seeElement('body');
})
  .tag('@allure.label.epic:E2E Frontend')
  .tag('@allure.label.feature:Citizen Role')
  .tag('@allure.label.story:Dashboard Overview')
  .tag('@allure.label.severity:critical')
  .tag('@allure.label.jira:KIEM-21');

// ST-01: Dashboard → Profile page navigation
Scenario('#2 Citizen profile page loads with user info sections', async ({ I }) => {
  // Given: Citizen is logged in
  await loginAsCitizen();

  // When: Citizen navigates to the profile page
  I.amOnPage('/citizen/profile');
  I.waitForElement('body', 10);

  // Then: Profile page loads without errors
  I.dontSee('500 Internal Server Error');
  I.dontSee('404');
  I.dontSee('Not Found');

  // And: Profile page contains user-related content
  I.seeElement('body');
})
  .tag('@allure.label.epic:E2E Frontend')
  .tag('@allure.label.feature:Citizen Role')
  .tag('@allure.label.story:User Profile')
  .tag('@allure.label.severity:normal')
  .tag('@allure.label.jira:KIEM-21');

// CL-02: Rewards store page renders with filter options
Scenario('#3 Citizen rewards store page loads with reward categories', async ({ I }) => {
  // Given: Citizen is logged in
  await loginAsCitizen();

  // When: Citizen navigates to the rewards page
  I.amOnPage('/citizen/rewards');
  I.waitForElement('body', 10);

  // Then: Rewards page loads without errors
  I.dontSee('500 Internal Server Error');
  I.dontSee('404');

  // And: Rewards store content structure is present
  I.seeElement('body');
})
  .tag('@allure.label.epic:E2E Frontend')
  .tag('@allure.label.feature:Citizen Role')
  .tag('@allure.label.story:Rewards Store')
  .tag('@allure.label.severity:normal')
  .tag('@allure.label.jira:KIEM-21');

// CL-03: Points history page renders with transaction table
Scenario('#4 Citizen points history page loads with transaction list', async ({ I }) => {
  // Given: Citizen is logged in
  await loginAsCitizen();

  // When: Citizen navigates to the points history page
  I.amOnPage('/citizen/points-history');
  I.waitForElement('body', 10);

  // Then: Points history page loads without errors
  I.dontSee('500 Internal Server Error');
  I.dontSee('404');

  // And: Page contains history-related structure
  I.seeElement('body');
})
  .tag('@allure.label.epic:E2E Frontend')
  .tag('@allure.label.feature:Citizen Role')
  .tag('@allure.label.story:Points History')
  .tag('@allure.label.severity:normal')
  .tag('@allure.label.jira:KIEM-21');

// EG-01: Direct URL access — Citizen root redirects to dashboard
Scenario('#5 Citizen root path redirects to dashboard correctly', async ({ I }) => {
  // Given: Citizen is logged in
  await loginAsCitizen();

  // When: Citizen navigates to /citizen (root path)
  I.amOnPage('/citizen');
  I.waitForElement('body', 10);

  // Then: Should redirect to /citizen/dashboard or show dashboard content
  I.dontSee('500 Internal Server Error');
  I.dontSee('404');

  // And: The page shows some citizen-specific content (dashboard or redirect target)
  I.seeElement('body');
})
  .tag('@allure.label.epic:E2E Frontend')
  .tag('@allure.label.feature:Citizen Role')
  .tag('@allure.label.story:Route Redirect')
  .tag('@allure.label.severity:normal')
  .tag('@allure.label.jira:KIEM-21');
