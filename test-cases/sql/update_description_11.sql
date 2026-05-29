-- Update description for category id 11 (used by TC-WC-009)
UPDATE waste_categories
SET description = 'Updated description'
WHERE id = 11;

SELECT id, name, description FROM waste_categories WHERE id = 11;
