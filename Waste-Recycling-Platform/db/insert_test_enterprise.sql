-- Insert test Enterprise user and data
-- Password: "password" (hashed with BCrypt)

-- Insert User (Enterprise)
INSERT INTO users (id, full_name, email, password_hash, phone, role, is_active, created_at)
VALUES (
  '550e8400-e29b-41d4-a716-446655440000',
  'Test Enterprise Co.',
  'enterprise@example.com',
  '$2a$11$K7Ci2xKI8cGjLCVo8JQB..GCVDrLI5w3RK7pNeLiMhPP2GFcT9dJy',
  '0123456789',
  'enterprise',
  1,
  NOW()
);

-- Insert Enterprise profile
INSERT INTO enterprises (id, user_id, company_name, capacity_kg_per_day, is_verified, created_at)
VALUES (
  '550e8400-e29b-41d4-a716-446655440001',
  '550e8400-e29b-41d4-a716-446655440000',
  'Test Enterprise LLC',
  1000,
  1,
  NOW()
);

-- Insert Citizen (for testing waste reports)
INSERT INTO users (id, full_name, email, password_hash, phone, role, is_active, created_at)
VALUES (
  '550e8400-e29b-41d4-a716-446655440002',
  'Test Citizen',
  'citizen@example.com',
  '$2a$11$K7Ci2xKI8cGjLCVo8JQB..GCVDrLI5w3RK7pNeLiMhPP2GFcT9dJy',
  '0987654321',
  'citizen',
  1,
  NOW()
);

-- Insert Collector User
INSERT INTO users (id, full_name, email, password_hash, phone, role, is_active, created_at)
VALUES (
  '550e8400-e29b-41d4-a716-446655440004',
  'Test Collector',
  'collector@example.com',
  '$2a$11$K7Ci2xKI8cGjLCVo8JQB..GCVDrLI5w3RK7pNeLiMhPP2GFcT9dJy',
  '0911111111',
  'collector',
  1,
  NOW()
);

-- Insert Collector Profile
INSERT INTO collectors (id, user_id, enterprise_id, is_available, created_at)
VALUES (
  '550e8400-e29b-41d4-a716-446655440003',
  '550e8400-e29b-41d4-a716-446655440004',
  '550e8400-e29b-41d4-a716-446655440001',
  1,
  NOW()
);

-- Insert Enterprise Waste Types (Plastic=1, Metal=2)
INSERT INTO enterprise_waste_types (id, enterprise_id, waste_category_id)
VALUES 
  ('550e8400-e29b-41d4-a716-446655440005', '550e8400-e29b-41d4-a716-446655440001', 1),
  ('550e8400-e29b-41d4-a716-446655440006', '550e8400-e29b-41d4-a716-446655440001', 2);

-- Insert Test Waste Reports (Pending status - available for enterprise)
INSERT INTO waste_reports (id, citizen_id, waste_category_id, latitude, longitude, address, description, status, ai_suggestion, created_at)
VALUES 
  ('550e8400-e29b-41d4-a716-446655440010', '550e8400-e29b-41d4-a716-446655440002', 1, 10.776889, 106.700981, '789 Test Address District 1', 'Plastic trash near store', 'pending', 'Contains plastic bottles', NOW()),
  ('550e8400-e29b-41d4-a716-446655440011', '550e8400-e29b-41d4-a716-446655440002', 1, 10.777000, 106.701000, '790 Another Address', 'More plastic waste', 'pending', 'Plastic bags', NOW()),
  ('550e8400-e29b-41d4-a716-446655440012', '550e8400-e29b-41d4-a716-446655440002', 2, 10.778000, 106.702000, '791 Metal Street', 'Metal cans found', 'pending', 'Aluminum cans', NOW());

SELECT 'Test data inserted successfully!' as result;
