-- ============================================================
-- V9: E2E Test Accounts Seed
-- Tạo accounts cố định cho CodeceptJS E2E tests
-- 
-- | Vai trò     | Email                    | Mật khẩu      |
-- |------------|--------------------------|---------------|
-- | Enterprise | enterprise@test.waste    | Enterprise@123 |
-- | Collector  | collector@test.waste     | Collector@123  |
-- | Citizen    | citizen@test.waste       | Citizen@123    |
--
-- BCrypt hash được generate offline với cost=11
-- enterprise@test.waste / Enterprise@123
-- collector@test.waste  / Collector@123
-- citizen@test.waste    / Citizen@123
--
-- Ghi chú: Nếu email đã tồn tại thì INSERT IGNORE bỏ qua,
-- không throw error.
-- ============================================================

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- ──────────────────────────────────────────────────────────────
-- Enterprise E2E Test Account
-- email: enterprise@test.waste | password: Enterprise@123
-- BCrypt hash (cost=11): $2b$11$E2eXyz...
-- ──────────────────────────────────────────────────────────────
-- NOTE: Dùng hash của 'password' từ V6 seed để đơn giản hoá,
-- chúng ta sẽ dùng password='password' cho cả 3 account này
-- trong E2E test (update file test phía dưới).

INSERT IGNORE INTO users (id, email, password_hash, full_name, phone, role, is_active, created_at)
VALUES (
    'e2e00001-e2e0-e2e0-e2e0-e2e000000001',
    'enterprise@test.waste',
    '$2b$11$tN7EUn/GW3UfJFw4OFtpKewSWNBk5wmj8VmJHm.sVFWcL.dpx63PK',  -- password = 'password'
    'E2E Enterprise Test',
    '0283900001',
    'Enterprise',
    1,
    NOW()
);

INSERT IGNORE INTO enterprises (id, user_id, company_name, capacity_kg_per_day, is_verified, status, created_at)
VALUES (
    'e2ent01-e2en-e2en-e2en-e2enterprise1',
    'e2e00001-e2e0-e2e0-e2e0-e2e000000001',
    'E2E Test Enterprise Co.',
    1000,
    1,
    'active',
    NOW()
);

-- ──────────────────────────────────────────────────────────────
-- Collector E2E Test Account (thuộc Enterprise e2ent01)
-- email: collector@test.waste | password: password
-- ──────────────────────────────────────────────────────────────
INSERT IGNORE INTO users (id, email, password_hash, full_name, phone, role, is_active, created_at)
VALUES (
    'e2e00002-e2e0-e2e0-e2e0-e2e000000002',
    'collector@test.waste',
    '$2b$11$tN7EUn/GW3UfJFw4OFtpKewSWNBk5wmj8VmJHm.sVFWcL.dpx63PK',  -- password = 'password'
    'E2E Collector Test',
    '0911900001',
    'Collector',
    1,
    NOW()
);

INSERT IGNORE INTO collectors (id, user_id, enterprise_id, is_available, created_at)
VALUES (
    'e2ecol1-e2ec-e2ec-e2ec-e2ecollector1',
    'e2e00002-e2e0-e2e0-e2e0-e2e000000002',
    'e2ent01-e2en-e2en-e2en-e2enterprise1',
    1,
    NOW()
);

-- ──────────────────────────────────────────────────────────────
-- Citizen E2E Test Account
-- email: citizen@test.waste | password: password
-- ──────────────────────────────────────────────────────────────
INSERT IGNORE INTO users (id, email, password_hash, full_name, phone, role, is_active, created_at)
VALUES (
    'e2e00003-e2e0-e2e0-e2e0-e2e000000003',
    'citizen@test.waste',
    '$2b$11$tN7EUn/GW3UfJFw4OFtpKewSWNBk5wmj8VmJHm.sVFWcL.dpx63PK',  -- password = 'password'
    'E2E Citizen Test',
    '0901900001',
    'Citizen',
    1,
    NOW()
);

SET FOREIGN_KEY_CHECKS = 1;

SELECT 'E2E test accounts seeded successfully!' as status;
SELECT email, role, full_name FROM users WHERE email LIKE '%test.waste%';
