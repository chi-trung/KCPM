-- ============================================================
-- V3: State machine triggers + Leaderboard view
-- MySQL 8.0+ | Waste Collection & Recycling Platform
-- NOTE: Run each DELIMITER block separately in MySQL CLI / Workbench
-- ============================================================

DELIMITER //

-- CREATE TRIGGER trg_validate_report_status
-- BEFORE UPDATE ON `waste_reports`
-- FOR EACH ROW
-- BEGIN
--     IF OLD.status <> NEW.status THEN
--         IF OLD.status = 'pending'    AND NEW.status NOT IN ('accepted','rejected') THEN
--             SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'report: pending → accepted|rejected only';
--         END IF;
--         IF OLD.status = 'accepted'   AND NEW.status <> 'assigned'   THEN
--             SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'report: accepted → assigned only';
--         END IF;
--         IF OLD.status = 'assigned'   AND NEW.status <> 'on_the_way' THEN
--             SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'report: assigned → on_the_way only';
--         END IF;
--         IF OLD.status = 'on_the_way' AND NEW.status <> 'collected'  THEN
--             SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'report: on_the_way → collected only';
--         END IF;
--         IF OLD.status IN ('collected','rejected') THEN
--             SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'report: terminal status cannot change';
--         END IF;
--     END IF;
-- END //

-- CREATE TRIGGER trg_validate_task_status
-- BEFORE UPDATE ON `collection_tasks`
-- FOR EACH ROW
-- BEGIN
--     IF OLD.status <> NEW.status THEN
--         IF OLD.status = 'assigned'   AND NEW.status <> 'on_the_way' THEN
--             SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'task: assigned → on_the_way only';
--         END IF;
--         IF OLD.status = 'on_the_way' AND NEW.status <> 'collected'  THEN
--             SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'task: on_the_way → collected only';
--         END IF;
--         IF OLD.status = 'collected' THEN
--             SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'task: collected is terminal';
--         END IF;
--     END IF;
-- END //

DELIMITER ;

-- Leaderboard VIEW (no Materialized View in MySQL — cache via app-layer cron)
CREATE OR REPLACE VIEW `v_leaderboard` AS
SELECT
    rp.citizen_id,
    u.full_name,
    u.district,
    u.ward,
    SUM(rp.points)                                                             AS total_points,
    RANK() OVER (ORDER BY SUM(rp.points) DESC)                                AS `rank`,
    RANK() OVER (PARTITION BY u.district ORDER BY SUM(rp.points) DESC)        AS district_rank
FROM `reward_points` rp
JOIN `users` u ON u.id = rp.citizen_id
GROUP BY rp.citizen_id, u.full_name, u.district, u.ward;
