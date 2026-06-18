-- ============================================================
-- V6: Seed Sample Data (Accounts, Enterprises, Reports)
-- 
-- MẬT KHẨU MẶC ĐỊNH CHO TẤT CẢ: password
-- 
-- | Vai trò     | Email                     | Mật khẩu   |
-- |------------|---------------------------|------------|
-- | Admin      | admin@gmail.com           | password   |
-- | Citizen    | nguyenvana@gmail.com      | password   |
-- | Citizen    | lethib@gmail.com          | password   |
-- | Citizen    | tranvanc@gmail.com        | password   |
-- | Enterprise | greenlife@gmail.com       | password   |
-- | Enterprise | ecofriendly@gmail.com     | password   |
-- | Enterprise | urbanwaste@gmail.com      | password   |
-- | Collector  | collector1@gmail.com      | password   |
-- | Collector  | collector2@gmail.com      | password   |
-- | Collector  | collector3@gmail.com      | password   |
-- 
-- Mật khẩu đã được mã hóa bằng BCrypt (cost=11)
-- ============================================================

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- 1. ADMIN ACCOUNT
INSERT INTO users (id, email, password_hash, full_name, role, is_active, created_at)
VALUES (
    'a1a1a1a1-a1a1-a1a1-a1a1-a1a1a1a1a1a1',
    'admin@gmail.com',
    '$2a$11$xEMK.jUrJ.15NqAFfBvF5u.JVVdpBJNfyeU5UWMAouCReQhxwX6YS',
    'System Administrator',
    'Admin',
    1,
    NOW()
);

-- 2. CITIZEN ACCOUNTS & INITIAL REPORTS
INSERT INTO users (id, email, password_hash, full_name, phone, role, district, ward, is_active, created_at)
VALUES 
(
    'c1c1c1c1-c1c1-c1c1-c1c1-c1c1c1c1c1c1',
    'nguyenvana@gmail.com',
    '$2a$11$xEMK.jUrJ.15NqAFfBvF5u.JVVdpBJNfyeU5UWMAouCReQhxwX6YS',
    'Nguyễn Văn A',
    '0901234561',
    'Citizen',
    'Quận 1',
    'Phường Bến Nghé',
    1,
    NOW()
),
(
    'c2c2c2c2-c2c2-c2c2-c2c2-c2c2c2c2c2c2',
    'lethib@gmail.com',
    '$2a$11$xEMK.jUrJ.15NqAFfBvF5u.JVVdpBJNfyeU5UWMAouCReQhxwX6YS',
    'Lê Thị B',
    '0901234562',
    'Citizen',
    'Quận 3',
    'Phường Võ Thị Sáu',
    1,
    NOW()
),
(
    'c3c3c3c3-c3c3-c3c3-c3c3-c3c3c3c3c3c3',
    'tranvanc@gmail.com',
    '$2a$11$xEMK.jUrJ.15NqAFfBvF5u.JVVdpBJNfyeU5UWMAouCReQhxwX6YS',
    'Trần Văn C',
    '0901234563',
    'Citizen',
    'Quận Bình Thạnh',
    'Phường 25',
    1,
    NOW()
);

-- Sample Reports for Nguyễn Văn A (c1)
INSERT INTO waste_reports (id, citizen_id, waste_category_id, latitude, longitude, address, description, status, ai_suggestion, created_at)
VALUES 
  (UUID(), 'c1c1c1c1-c1c1-c1c1-c1c1-c1c1c1c1c1c1', 1, 10.776889, 106.700981, '789 Đường Lê Lợi, Quận 1', 'Rác thải nhựa gần cửa hàng tiện lợi', 'Pending', 'Nhựa tái chế', NOW()),
  (UUID(), 'c1c1c1c1-c1c1-c1c1-c1c1-c1c1c1c1c1c1', 2, 10.777000, 106.701000, '123 Đường Nguyễn Huệ, Quận 1', 'Túi rác thực phẩm bốc mùi', 'Pending', 'Rác hữu cơ', NOW());

-- 3. ENTERPRISE ACCOUNTS & PROFILES
-- Enterprise 1: Green Life
INSERT INTO users (id, email, password_hash, full_name, phone, role, is_active, created_at)
VALUES (
    'e1e1e1e1-e1e1-e1e1-e1e1-e1e1e1e1e1e1',
    'greenlife@gmail.com',
    '$2a$11$xEMK.jUrJ.15NqAFfBvF5u.JVVdpBJNfyeU5UWMAouCReQhxwX6YS',
    'Green Life CEO',
    '0283800001',
    'Enterprise',
    1,
    NOW()
);
INSERT INTO enterprises (id, user_id, company_name, capacity_kg_per_day, is_verified, created_at)
VALUES (
    'ee1ee1ee-1ee1-1ee1-1ee1-1ee1ee1ee1ee',
    'e1e1e1e1-e1e1-e1e1-e1e1-e1e1e1e1e1e1',
    'Công ty Tái chế Green Life',
    5000,
    1,
    NOW()
);
INSERT INTO enterprise_waste_types (id, enterprise_id, waste_category_id)
VALUES (UUID(), 'ee1ee1ee-1ee1-1ee1-1ee1-1ee1ee1ee1ee', 1);

-- Enterprise 2: Eco-Friendly
INSERT INTO users (id, email, password_hash, full_name, phone, role, is_active, created_at)
VALUES (
    'e2e2e2e2-e2e2-e2e2-e2e2-e2e2e2e2e2e2',
    'ecofriendly@gmail.com',
    '$2a$11$xEMK.jUrJ.15NqAFfBvF5u.JVVdpBJNfyeU5UWMAouCReQhxwX6YS',
    'EcoFriendly Manager',
    '0283800002',
    'Enterprise',
    1,
    NOW()
);
INSERT INTO enterprises (id, user_id, company_name, capacity_kg_per_day, is_verified, created_at)
VALUES (
    'ee2ee2ee-2ee2-2ee2-2ee2-2ee2ee2ee2ee',
    'e2e2e2e2-e2e2-e2e2-e2e2-e2e2e2e2e2e2',
    'Eco-Friendly Collection',
    3500,
    1,
    NOW()
);
INSERT INTO enterprise_waste_types (id, enterprise_id, waste_category_id)
VALUES (UUID(), 'ee2ee2ee-2ee2-2ee2-2ee2-2ee2ee2ee2ee', 2);

-- 4. COLLECTOR ACCOUNTS & PROFILES
-- Collector 1 (Green Life)
INSERT INTO users (id, email, password_hash, full_name, phone, role, is_active, created_at)
VALUES (
    'c4c4c4c4-c4c4-c4c4-c4c4-c4c4c4c4c4c4',
    'collector1@gmail.com',
    '$2a$11$xEMK.jUrJ.15NqAFfBvF5u.JVVdpBJNfyeU5UWMAouCReQhxwX6YS',
    'Phạm Minh Dũng',
    '0911000001',
    'Collector',
    1,
    NOW()
);
INSERT INTO collectors (id, user_id, enterprise_id, is_available, created_at)
VALUES (
    'cc1cc1cc-1cc1-1cc1-1cc1-1cc1cc1cc1cc',
    'c4c4c4c4-c4c4-c4c4-c4c4-c4c4c4c4c4c4',
    'ee1ee1ee-1ee1-1ee1-1ee1-1ee1ee1ee1ee',
    1,
    NOW()
);

-- Collector 2 (Eco-Friendly)
INSERT INTO users (id, email, password_hash, full_name, phone, role, is_active, created_at)
VALUES (
    'c5c5c5c5-c5c5-c5c5-c5c5-c5c5c5c5c5c5',
    'collector2@gmail.com',
    '$2a$11$xEMK.jUrJ.15NqAFfBvF5u.JVVdpBJNfyeU5UWMAouCReQhxwX6YS',
    'Lý Đại Nghĩa',
    '0911000002',
    'Collector',
    1,
    NOW()
);
INSERT INTO collectors (id, user_id, enterprise_id, is_available, created_at)
VALUES (
    'cc2cc2cc-2cc2-2cc2-2cc2-2cc2cc2cc2cc',
    'c5c5c5c5-c5c5-c5c5-c5c5-c5c5c5c5c5c5',
    'ee2ee2ee-2ee2-2ee2-2ee2-2ee2ee2ee2ee',
    1,
    NOW()
);

-- Collector 3 (Works for Urban Waste)
INSERT INTO users (id, email, password_hash, full_name, phone, role, is_active, created_at)
VALUES (
    'c6c6c6c6-c6c6-c6c6-c6c6-c6c6c6c6c6c6',
    'collector3@gmail.com',
    '$2a$11$xEMK.jUrJ.15NqAFfBvF5u.JVVdpBJNfyeU5UWMAouCReQhxwX6YS',
    'Hoàng Văn Thái',
    '0911000003',
    'Collector',
    1,
    NOW()
);
INSERT INTO collectors (id, user_id, enterprise_id, is_available, created_at)
VALUES (
    'cc3cc3cc-3cc3-3cc3-3cc3-3cc3cc3cc3cc',
    'c6c6c6c6-c6c6-c6c6-c6c6-c6c6c6c6c6c6',
    'ee3ee3ee-3ee3-3ee3-3ee3-3ee3ee3ee3ee',
    1,
    NOW()
);

-- 5. SAMPLE COLLECTION TASKS
-- Assign to Enterprise 1 (Green Life) but no collector assigned yet so we can test the Assign button!
INSERT INTO collection_tasks (id, report_id, enterprise_id, collector_id, status, assigned_at)
VALUES 
(
    '11111111-1111-1111-1111-111111111111', 
    (SELECT id FROM waste_reports WHERE description = 'Rác thải nhựa gần cửa hàng tiện lợi' LIMIT 1), 
    'ee1ee1ee-1ee1-1ee1-1ee1-1ee1ee1ee1ee', 
    NULL, 
    'assigned', 
    NOW()
),
(
    '22222222-2222-2222-2222-222222222222', 
    (SELECT id FROM waste_reports WHERE description = 'Túi rác thực phẩm bốc mùi' LIMIT 1), 
    'ee1ee1ee-1ee1-1ee1-1ee1-1ee1ee1ee1ee', 
    NULL, 
    'assigned', 
    NOW()
);

SET FOREIGN_KEY_CHECKS = 1;

SELECT 'Comprehensive sample data (V6) seeded successfully!' as status;
