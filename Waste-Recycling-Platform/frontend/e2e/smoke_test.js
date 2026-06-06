Feature('Frontend smoke');

Scenario('public home page and auth entry points render', async ({ I }) => {
  I.amOnPage('/');
  I.see('Thu gom rác thông minh');
  I.see('Bắt đầu ngay');

  I.click('Bắt đầu ngay');
  I.seeCurrentUrlEquals('/register');
  I.waitForElement('input[name="name"]', 10);
  I.seeElement('input[name="name"]');
  I.seeElement('input[name="email"]');

  I.amOnPage('/login');
  I.see('Đăng Nhập');
  I.waitForElement('input[name="email"]', 10);
  I.seeElement('input[name="email"]');
});

Scenario('public guide and locations pages render', async ({ I }) => {
  I.amOnPage('/guide');
  I.see('Hướng dẫn phân loại rác');
  I.see('Cẩm nang phân loại');
  I.see('Quy định 2025');

  I.amOnPage('/locations');
  I.see('Tra cứu điểm thu gom');
  I.see('Tìm thấy');
  I.seeElement('input[placeholder*="Tìm kiếm tên địa điểm"]');
});

Scenario('public leaderboard page renders core filters', async ({ I }) => {
  I.amOnPage('/leaderboard');
  I.see('Bảng Xếp Hạng');
  I.see('Cá nhân');
  I.see('Khu vực');
  I.seeElement('input[placeholder*="Tìm kiếm nhanh"]');
});