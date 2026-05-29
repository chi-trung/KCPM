-- Insert a WasteCategory for manual testing
-- Run this against the WastePlatform database used by the backend

INSERT INTO waste_categories (name, description)
VALUES ('Plastic', 'Plastic waste category');

-- To verify the inserted row and get its id:
SELECT id, name, description FROM waste_categories WHERE name = 'Plastic' ORDER BY id DESC LIMIT 1;
