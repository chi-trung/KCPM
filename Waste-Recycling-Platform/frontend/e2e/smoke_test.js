// TC-E2E-001: Frontend Smoke Tests — Public Pages
// Jira: KIEM-FE | Test Design: Checklist / Exploratory
// Technique: Smoke testing — verify core public pages render without error
// No login required — tests public-facing routes only

Feature('TC-E2E-001: Frontend Smoke');

Scenario(
  '#1 Home page and auth entry points render correctly',
  { epic: 'E2E Frontend', feature: 'Public Pages', story: 'Home & Auth Routes', severity: 'blocker' },
  async ({ I }) => {
    I.say('[Given] User visits the public home page');
    I.amOnPage('/');

    I.say('[Then] Home page shows platform branding and CTA button');
    I.see('Thu gom rác thông minh');
    I.see('Bắt đầu ngay');

    I.say('[When] User clicks the CTA button');
    I.click('Bắt đầu ngay');

    I.say('[Then] User is redirected to /register with registration form');
    I.seeCurrentUrlEquals('/register');
    I.waitForElement('input[name="name"]', 10);
    I.seeElement('input[name="name"]');
    I.seeElement('input[name="email"]');

    I.say('[When] User navigates to /login');
    I.amOnPage('/login');

    I.say('[Then] Login page renders with credentials form');
    I.see('Đăng Nhập');
    I.waitForElement('input[name="email"]', 10);
    I.seeElement('input[name="email"]');
  }
);

Scenario(
  '#2 Public guide and locations pages render',
  { epic: 'E2E Frontend', feature: 'Public Pages', story: 'Informational Pages', severity: 'normal' },
  async ({ I }) => {
    I.say('[Given] User navigates to the waste sorting guide page');
    I.amOnPage('/guide');

    I.say('[Then] Guide page shows categorization content');
    I.see('Hướng dẫn phân loại rác');
    I.see('Cẩm nang phân loại');
    I.see('Quy định 2025');

    I.say('[When] User navigates to the collection locations page');
    I.amOnPage('/locations');

    I.say('[Then] Locations page renders with search functionality');
    I.see('Tra cứu điểm thu gom');
    I.see('Tìm thấy');
    I.seeElement('input[placeholder*="Tìm kiếm tên địa điểm"]');
  }
);

Scenario(
  '#3 Public leaderboard page renders core filters',
  { epic: 'E2E Frontend', feature: 'Public Pages', story: 'Leaderboard', severity: 'normal' },
  async ({ I }) => {
    I.say('[Given] User navigates to the leaderboard page');
    I.amOnPage('/leaderboard');

    I.say('[Then] Leaderboard page shows ranking content with filter options');
    I.see('Bảng Xếp Hạng');
    I.see('Cá nhân');
    I.see('Khu vực');
    I.seeElement('input[placeholder*="Tìm kiếm nhanh"]');
  }
);