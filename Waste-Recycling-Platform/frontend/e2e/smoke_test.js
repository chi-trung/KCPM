// TC-E2E-001: Frontend Smoke Tests — Public Pages
// Jira: KIEM-21 | Test Design: Checklist / Exploratory
// No login required — tests public-facing routes only

Feature('TC-E2E-001: Frontend Smoke');

Scenario('#1 Home page and auth entry points render correctly', async ({ I }) => {
  // Given: User visits the public home page
  I.amOnPage('/');

  // Then: Home page shows platform branding and CTA button
  I.see('Thu gom rác thông minh');
  I.see('Bắt đầu ngay');

  // When: User clicks the CTA button
  I.click('Bắt đầu ngay');

  // Then: User is redirected to /register with registration form
  I.seeCurrentUrlEquals('/register');
  I.waitForElement('input[name="name"]', 10);
  I.seeElement('input[name="name"]');
  I.seeElement('input[name="email"]');

  // When: User navigates to /login
  I.amOnPage('/login');

  // Then: Login page renders with credentials form and submit button
  I.waitForElement('input[name="email"]', 10);
  I.seeElement('input[name="email"]');
  I.seeElement('input[name="password"]');
  I.seeElement('button[type="submit"]');

  // Verify login page contains key branding text
  I.see('WASTE PLATFORM');
})
  .tag('@allure.label.epic:E2E Frontend')
  .tag('@allure.label.feature:Public Pages')
  .tag('@allure.label.story:Home & Auth Routes')
  .tag('@allure.label.severity:blocker');

Scenario('#2 Public guide and locations pages render', async ({ I }) => {
  // Given: User navigates to the waste sorting guide page
  I.amOnPage('/guide');

  // Then: Guide page shows categorization content
  I.see('Hướng dẫn phân loại rác');
  I.see('Cẩm nang phân loại');
  I.see('Quy định 2025');

  // When: User navigates to the collection locations page
  I.amOnPage('/locations');

  // Then: Locations page renders with search functionality
  I.see('Tra cứu điểm thu gom');
  I.see('Tìm thấy');
  I.seeElement('input[placeholder*="Tìm kiếm tên địa điểm"]');
})
  .tag('@allure.label.epic:E2E Frontend')
  .tag('@allure.label.feature:Public Pages')
  .tag('@allure.label.story:Informational Pages')
  .tag('@allure.label.severity:normal');

Scenario('#3 Public leaderboard page renders core filters', async ({ I }) => {
  // Given: User navigates to the leaderboard page
  I.amOnPage('/leaderboard');

  // Then: Leaderboard page shows ranking content with filter options
  I.see('Bảng Xếp Hạng');
  I.see('Cá nhân');
  I.see('Khu vực');
  I.seeElement('input[placeholder*="Tìm kiếm nhanh"]');
})
  .tag('@allure.label.epic:E2E Frontend')
  .tag('@allure.label.feature:Public Pages')
  .tag('@allure.label.story:Leaderboard')
  .tag('@allure.label.severity:normal');