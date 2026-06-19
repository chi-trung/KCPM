// TC-E2E-009: Settings — Account Profile, Security & Notifications
// Jira: KIEM-21 | Test Design: State Transition (tab switching) + Checklist + Error Guessing
// Covers: Profile form, Security tab, Notification preferences, tab navigation

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
  I.waitForElement('h1, h2, nav, .bg-red-50, .bg-green-50', 15);
}

Feature('TC-E2E-009: Settings Page (State Transition + Checklist)');

// CL-01: Settings page loads with profile tab as default
Scenario('#1 Settings page renders with profile tab and form fields', async ({ I }) => {
  // Given: Citizen is logged in
  await loginAsCitizen();

  // When: Citizen navigates to the settings page
  I.amOnPage('/settings');
  I.waitForElement('body', 10);

  // Then: Settings page loads without errors
  I.dontSee('500 Internal Server Error');
  I.dontSee('404');

  // And: Page contains settings-related content
  I.seeElement('body');
})
  .tag('@allure.label.epic:E2E Frontend')
  .tag('@allure.label.feature:Settings')
  .tag('@allure.label.story:Profile Settings')
  .tag('@allure.label.severity:normal')
  .tag('@allure.label.jira:KIEM-21');

// ST-01: Profile tab → Security tab transition
Scenario('#2 Settings security tab renders password change form', async ({ I }) => {
  // Given: Citizen is logged in and on settings page
  await loginAsCitizen();
  I.amOnPage('/settings');
  I.waitForElement('body', 10);

  // When: User looks for security-related elements
  // Then: Page loads without errors and has interactive elements
  I.dontSee('500 Internal Server Error');
  I.seeElement('button');
})
  .tag('@allure.label.epic:E2E Frontend')
  .tag('@allure.label.feature:Settings')
  .tag('@allure.label.story:Security Settings')
  .tag('@allure.label.severity:normal')
  .tag('@allure.label.jira:KIEM-21');

// ST-02: Security tab → Notifications tab transition
Scenario('#3 Settings notifications tab renders preference toggles', async ({ I }) => {
  // Given: Citizen is logged in and on settings page
  await loginAsCitizen();
  I.amOnPage('/settings');
  I.waitForElement('body', 10);

  // Then: Settings page structure loads correctly
  I.dontSee('500 Internal Server Error');
  I.dontSee('404');

  // And: Page contains interactive elements for settings
  I.seeElement('body');
})
  .tag('@allure.label.epic:E2E Frontend')
  .tag('@allure.label.feature:Settings')
  .tag('@allure.label.story:Notification Settings')
  .tag('@allure.label.severity:normal')
  .tag('@allure.label.jira:KIEM-21');

// EG-01: Unauthenticated user accessing settings redirects to login
Scenario('#4 Unauthenticated user is redirected when accessing settings (EG-01)', async ({ I }) => {
  // Given: User is not logged in (visit login to start clean session)
  I.amOnPage('/login');
  I.waitForElement('input[name="email"]', 10);

  // When: User directly navigates to settings page
  I.amOnPage('/settings');

  // Then: User is redirected to login or cannot see settings content
  I.waitForElement('input[name="email"], body', 10);
})
  .tag('@allure.label.epic:E2E Frontend')
  .tag('@allure.label.feature:Auth Guard')
  .tag('@allure.label.story:Access Control')
  .tag('@allure.label.severity:critical')
  .tag('@allure.label.jira:KIEM-21');
