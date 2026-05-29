CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `ProductVersion` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
) CHARACTER SET=utf8mb4;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260513015845_Initial') THEN

    ALTER DATABASE CHARACTER SET utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260513015845_Initial') THEN

    CREATE TABLE `bks_member` (
        `id` char(36) COLLATE ascii_general_ci NOT NULL,
        `full_name` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
        `title` varchar(128) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL,
        `is_active` tinyint(1) NOT NULL,
        `created_at` datetime(6) NOT NULL,
        `updated_at` datetime(6) NOT NULL,
        CONSTRAINT `pk_bks_member` PRIMARY KEY (`id`)
    ) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260513015845_Initial') THEN

    CREATE TABLE `department` (
        `id` char(36) COLLATE ascii_general_ci NOT NULL,
        `code` varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
        `name` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
        `is_active` tinyint(1) NOT NULL,
        `created_at` datetime(6) NOT NULL,
        `updated_at` datetime(6) NOT NULL,
        CONSTRAINT `pk_department` PRIMARY KEY (`id`)
    ) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260513015845_Initial') THEN

    CREATE TABLE `role` (
        `id` char(36) COLLATE ascii_general_ci NOT NULL,
        `code` varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
        `name` varchar(128) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
        `created_at` datetime(6) NOT NULL,
        `updated_at` datetime(6) NOT NULL,
        CONSTRAINT `pk_role` PRIMARY KEY (`id`)
    ) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260513015845_Initial') THEN

    CREATE TABLE `app_user` (
        `id` char(36) COLLATE ascii_general_ci NOT NULL,
        `username` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
        `password_hash` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
        `full_name` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
        `email` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL,
        `department_id` char(36) COLLATE ascii_general_ci NULL,
        `is_active` tinyint(1) NOT NULL,
        `last_login_at` datetime(6) NULL,
        `created_at` datetime(6) NOT NULL,
        `updated_at` datetime(6) NOT NULL,
        CONSTRAINT `pk_app_user` PRIMARY KEY (`id`),
        CONSTRAINT `fk_app_user_department_department_id` FOREIGN KEY (`department_id`) REFERENCES `department` (`id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260513015845_Initial') THEN

    CREATE TABLE `approval_config` (
        `id` char(36) COLLATE ascii_general_ci NOT NULL,
        `scope` varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
        `level` int NOT NULL,
        `department_id` char(36) COLLATE ascii_general_ci NULL,
        `role_id` char(36) COLLATE ascii_general_ci NOT NULL,
        `is_active` tinyint(1) NOT NULL,
        `created_at` datetime(6) NOT NULL,
        `updated_at` datetime(6) NOT NULL,
        CONSTRAINT `pk_approval_config` PRIMARY KEY (`id`),
        CONSTRAINT `fk_approval_config_department_department_id` FOREIGN KEY (`department_id`) REFERENCES `department` (`id`) ON DELETE RESTRICT,
        CONSTRAINT `fk_approval_config_role_role_id` FOREIGN KEY (`role_id`) REFERENCES `role` (`id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260513015845_Initial') THEN

    CREATE TABLE `attachment` (
        `id` char(36) COLLATE ascii_general_ci NOT NULL,
        `owner_type` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
        `owner_id` char(36) COLLATE ascii_general_ci NOT NULL,
        `file_name` varchar(512) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
        `stored_path` varchar(1024) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
        `size_bytes` bigint NOT NULL,
        `content_type` varchar(128) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL,
        `uploaded_by_user_id` char(36) COLLATE ascii_general_ci NOT NULL,
        `created_at` datetime(6) NOT NULL,
        `updated_at` datetime(6) NOT NULL,
        CONSTRAINT `pk_attachment` PRIMARY KEY (`id`),
        CONSTRAINT `fk_attachment_app_user_uploaded_by_user_id` FOREIGN KEY (`uploaded_by_user_id`) REFERENCES `app_user` (`id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260513015845_Initial') THEN

    CREATE TABLE `audit_log` (
        `id` char(36) COLLATE ascii_general_ci NOT NULL,
        `entity_name` varchar(128) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
        `entity_id` char(36) COLLATE ascii_general_ci NOT NULL,
        `action` varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
        `actor_user_id` char(36) COLLATE ascii_general_ci NULL,
        `before_json` json NULL,
        `after_json` json NULL,
        `created_at` datetime(6) NOT NULL,
        `updated_at` datetime(6) NOT NULL,
        CONSTRAINT `pk_audit_log` PRIMARY KEY (`id`),
        CONSTRAINT `fk_audit_log_app_user_actor_user_id` FOREIGN KEY (`actor_user_id`) REFERENCES `app_user` (`id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260513015845_Initial') THEN

    CREATE TABLE `plan` (
        `id` char(36) COLLATE ascii_general_ci NOT NULL,
        `scope` varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
        `department_id` char(36) COLLATE ascii_general_ci NULL,
        `year` int NOT NULL,
        `month` int NOT NULL,
        `status` varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
        `created_by_id` char(36) COLLATE ascii_general_ci NOT NULL,
        `submitted_at` datetime(6) NULL,
        `approved_at` datetime(6) NULL,
        `created_at` datetime(6) NOT NULL,
        `updated_at` datetime(6) NOT NULL,
        CONSTRAINT `pk_plan` PRIMARY KEY (`id`),
        CONSTRAINT `fk_plan_app_user_created_by_id` FOREIGN KEY (`created_by_id`) REFERENCES `app_user` (`id`) ON DELETE RESTRICT,
        CONSTRAINT `fk_plan_department_department_id` FOREIGN KEY (`department_id`) REFERENCES `department` (`id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260513015845_Initial') THEN

    CREATE TABLE `refresh_token` (
        `id` char(36) COLLATE ascii_general_ci NOT NULL,
        `user_id` char(36) COLLATE ascii_general_ci NOT NULL,
        `token_hash` varchar(128) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
        `expires_at` datetime(6) NOT NULL,
        `revoked_at` datetime(6) NULL,
        `replaced_by_token_hash` varchar(128) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL,
        `created_by_ip` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL,
        `created_at` datetime(6) NOT NULL,
        `updated_at` datetime(6) NOT NULL,
        CONSTRAINT `pk_refresh_token` PRIMARY KEY (`id`),
        CONSTRAINT `fk_refresh_token_app_user_user_id` FOREIGN KEY (`user_id`) REFERENCES `app_user` (`id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260513015845_Initial') THEN

    CREATE TABLE `user_role` (
        `user_id` char(36) COLLATE ascii_general_ci NOT NULL,
        `role_id` char(36) COLLATE ascii_general_ci NOT NULL,
        `created_at` datetime(6) NOT NULL,
        CONSTRAINT `pk_user_role` PRIMARY KEY (`user_id`, `role_id`),
        CONSTRAINT `fk_user_role_app_user_user_id` FOREIGN KEY (`user_id`) REFERENCES `app_user` (`id`) ON DELETE CASCADE,
        CONSTRAINT `fk_user_role_role_role_id` FOREIGN KEY (`role_id`) REFERENCES `role` (`id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260513015845_Initial') THEN

    CREATE TABLE `approval_history` (
        `id` char(36) COLLATE ascii_general_ci NOT NULL,
        `plan_id` char(36) COLLATE ascii_general_ci NOT NULL,
        `action` varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
        `from_status` varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
        `to_status` varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
        `actor_user_id` char(36) COLLATE ascii_general_ci NOT NULL,
        `comment` varchar(2000) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL,
        `created_at` datetime(6) NOT NULL,
        `updated_at` datetime(6) NOT NULL,
        CONSTRAINT `pk_approval_history` PRIMARY KEY (`id`),
        CONSTRAINT `fk_approval_history_app_user_actor_user_id` FOREIGN KEY (`actor_user_id`) REFERENCES `app_user` (`id`) ON DELETE RESTRICT,
        CONSTRAINT `fk_approval_history_plan_plan_id` FOREIGN KEY (`plan_id`) REFERENCES `plan` (`id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260513015845_Initial') THEN

    CREATE TABLE `task` (
        `id` char(36) COLLATE ascii_general_ci NOT NULL,
        `plan_id` char(36) COLLATE ascii_general_ci NOT NULL,
        `parent_task_id` char(36) COLLATE ascii_general_ci NULL,
        `outline_index` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL,
        `display_order` int NOT NULL,
        `is_header` tinyint(1) NOT NULL,
        `title` varchar(1024) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
        `work_type` int NOT NULL,
        `work_status` varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
        `deadline` datetime(6) NULL,
        `assignee_user_id` char(36) COLLATE ascii_general_ci NULL,
        `bks_member_text` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL,
        `is_locked` tinyint(1) NOT NULL,
        `inherited_from_task_id` char(36) COLLATE ascii_general_ci NULL,
        `has_open_comment` tinyint(1) NOT NULL,
        `reason_not_completed` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL,
        `progress_text` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL,
        `created_at` datetime(6) NOT NULL,
        `updated_at` datetime(6) NOT NULL,
        CONSTRAINT `pk_task` PRIMARY KEY (`id`),
        CONSTRAINT `fk_task_app_user_assignee_user_id` FOREIGN KEY (`assignee_user_id`) REFERENCES `app_user` (`id`) ON DELETE RESTRICT,
        CONSTRAINT `fk_task_plan_plan_id` FOREIGN KEY (`plan_id`) REFERENCES `plan` (`id`) ON DELETE CASCADE,
        CONSTRAINT `fk_task_task_inherited_from_task_id` FOREIGN KEY (`inherited_from_task_id`) REFERENCES `task` (`id`) ON DELETE RESTRICT,
        CONSTRAINT `fk_task_task_parent_task_id` FOREIGN KEY (`parent_task_id`) REFERENCES `task` (`id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260513015845_Initial') THEN

    CREATE TABLE `line_comment` (
        `id` char(36) COLLATE ascii_general_ci NOT NULL,
        `task_id` char(36) COLLATE ascii_general_ci NOT NULL,
        `parent_comment_id` char(36) COLLATE ascii_general_ci NULL,
        `author_user_id` char(36) COLLATE ascii_general_ci NOT NULL,
        `author_role` varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
        `content` varchar(4000) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
        `is_resolved` tinyint(1) NOT NULL,
        `resolved_at` datetime(6) NULL,
        `resolved_by_user_id` char(36) COLLATE ascii_general_ci NULL,
        `created_at` datetime(6) NOT NULL,
        `updated_at` datetime(6) NOT NULL,
        CONSTRAINT `pk_line_comment` PRIMARY KEY (`id`),
        CONSTRAINT `fk_line_comment_app_user_author_user_id` FOREIGN KEY (`author_user_id`) REFERENCES `app_user` (`id`) ON DELETE RESTRICT,
        CONSTRAINT `fk_line_comment_app_user_resolved_by_user_id` FOREIGN KEY (`resolved_by_user_id`) REFERENCES `app_user` (`id`) ON DELETE RESTRICT,
        CONSTRAINT `fk_line_comment_line_comment_parent_comment_id` FOREIGN KEY (`parent_comment_id`) REFERENCES `line_comment` (`id`) ON DELETE RESTRICT,
        CONSTRAINT `fk_line_comment_task_task_id` FOREIGN KEY (`task_id`) REFERENCES `task` (`id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260513015845_Initial') THEN

    CREATE TABLE `task_supporting_dept` (
        `task_id` char(36) COLLATE ascii_general_ci NOT NULL,
        `department_id` char(36) COLLATE ascii_general_ci NOT NULL,
        `created_at` datetime(6) NOT NULL,
        CONSTRAINT `pk_task_supporting_dept` PRIMARY KEY (`task_id`, `department_id`),
        CONSTRAINT `fk_task_supporting_dept_department_department_id` FOREIGN KEY (`department_id`) REFERENCES `department` (`id`) ON DELETE CASCADE,
        CONSTRAINT `fk_task_supporting_dept_task_task_id` FOREIGN KEY (`task_id`) REFERENCES `task` (`id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260513015845_Initial') THEN

    INSERT INTO `department` (`id`, `code`, `created_at`, `is_active`, `name`, `updated_at`)
    VALUES ('11111111-0000-0000-0000-000000000001', 'KTNB1', TIMESTAMP '2026-01-01 00:00:00', TRUE, 'Kiểm toán nội bộ 1', TIMESTAMP '2026-01-01 00:00:00'),
    ('11111111-0000-0000-0000-000000000002', 'KTNB2', TIMESTAMP '2026-01-01 00:00:00', TRUE, 'Kiểm toán nội bộ 2', TIMESTAMP '2026-01-01 00:00:00'),
    ('11111111-0000-0000-0000-000000000003', 'KTNB3', TIMESTAMP '2026-01-01 00:00:00', TRUE, 'Kiểm toán nội bộ 3', TIMESTAMP '2026-01-01 00:00:00'),
    ('11111111-0000-0000-0000-000000000004', 'KH', TIMESTAMP '2026-01-01 00:00:00', TRUE, 'Phòng Kế hoạch', TIMESTAMP '2026-01-01 00:00:00'),
    ('11111111-0000-0000-0000-000000000005', 'GS', TIMESTAMP '2026-01-01 00:00:00', TRUE, 'Phòng Giám sát', TIMESTAMP '2026-01-01 00:00:00'),
    ('11111111-0000-0000-0000-000000000006', 'VPMN', TIMESTAMP '2026-01-01 00:00:00', TRUE, 'Văn phòng miền Nam', TIMESTAMP '2026-01-01 00:00:00'),
    ('11111111-0000-0000-0000-000000000007', 'VPMT', TIMESTAMP '2026-01-01 00:00:00', TRUE, 'Văn phòng miền Trung', TIMESTAMP '2026-01-01 00:00:00'),
    ('11111111-0000-0000-0000-000000000008', 'VPTNB', TIMESTAMP '2026-01-01 00:00:00', TRUE, 'Văn phòng Tây Nam Bộ', TIMESTAMP '2026-01-01 00:00:00'),
    ('11111111-0000-0000-0000-000000000009', 'TKTH', TIMESTAMP '2026-01-01 00:00:00', TRUE, 'Bộ phận Thư ký tổng hợp', TIMESTAMP '2026-01-01 00:00:00');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260513015845_Initial') THEN

    INSERT INTO `role` (`id`, `code`, `created_at`, `name`, `updated_at`)
    VALUES ('22222222-0000-0000-0000-000000000001', 'ADMIN', TIMESTAMP '2026-01-01 00:00:00', 'Quản trị hệ thống', TIMESTAMP '2026-01-01 00:00:00'),
    ('22222222-0000-0000-0000-000000000002', 'VAN_THU', TIMESTAMP '2026-01-01 00:00:00', 'Văn thư', TIMESTAMP '2026-01-01 00:00:00'),
    ('22222222-0000-0000-0000-000000000003', 'TRUONG_KH', TIMESTAMP '2026-01-01 00:00:00', 'Trưởng phòng Kế hoạch', TIMESTAMP '2026-01-01 00:00:00'),
    ('22222222-0000-0000-0000-000000000004', 'TRUONG_KTNB', TIMESTAMP '2026-01-01 00:00:00', 'Trưởng KTNB', TIMESTAMP '2026-01-01 00:00:00'),
    ('22222222-0000-0000-0000-000000000005', 'PHO_TRUONG_KTNB', TIMESTAMP '2026-01-01 00:00:00', 'Phó Trưởng KTNB', TIMESTAMP '2026-01-01 00:00:00'),
    ('22222222-0000-0000-0000-000000000006', 'TRUONG_PHONG', TIMESTAMP '2026-01-01 00:00:00', 'Trưởng phòng', TIMESTAMP '2026-01-01 00:00:00'),
    ('22222222-0000-0000-0000-000000000007', 'TRUONG_NHOM', TIMESTAMP '2026-01-01 00:00:00', 'Trưởng nhóm', TIMESTAMP '2026-01-01 00:00:00'),
    ('22222222-0000-0000-0000-000000000008', 'NHAN_VIEN', TIMESTAMP '2026-01-01 00:00:00', 'Nhân viên', TIMESTAMP '2026-01-01 00:00:00');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260513015845_Initial') THEN

    INSERT INTO `approval_config` (`id`, `created_at`, `department_id`, `is_active`, `level`, `role_id`, `scope`, `updated_at`)
    VALUES ('44444444-0000-0000-0000-000000000001', TIMESTAMP '2026-01-01 00:00:00', NULL, TRUE, 1, '22222222-0000-0000-0000-000000000003', 'Main', TIMESTAMP '2026-01-01 00:00:00'),
    ('44444444-0000-0000-0000-000000000002', TIMESTAMP '2026-01-01 00:00:00', NULL, TRUE, 2, '22222222-0000-0000-0000-000000000004', 'Main', TIMESTAMP '2026-01-01 00:00:00'),
    ('44444444-0000-0000-0000-000000000003', TIMESTAMP '2026-01-01 00:00:00', NULL, TRUE, 1, '22222222-0000-0000-0000-000000000007', 'Sub', TIMESTAMP '2026-01-01 00:00:00'),
    ('44444444-0000-0000-0000-000000000004', TIMESTAMP '2026-01-01 00:00:00', NULL, TRUE, 2, '22222222-0000-0000-0000-000000000006', 'Sub', TIMESTAMP '2026-01-01 00:00:00'),
    ('44444444-0000-0000-0000-000000000005', TIMESTAMP '2026-01-01 00:00:00', NULL, TRUE, 3, '22222222-0000-0000-0000-000000000005', 'Sub', TIMESTAMP '2026-01-01 00:00:00');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260513015845_Initial') THEN

    CREATE INDEX `ix_app_user_department_id` ON `app_user` (`department_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260513015845_Initial') THEN

    CREATE UNIQUE INDEX `ix_app_user_username` ON `app_user` (`username`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260513015845_Initial') THEN

    CREATE INDEX `ix_approval_config_department_id` ON `approval_config` (`department_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260513015845_Initial') THEN

    CREATE INDEX `ix_approval_config_role_id` ON `approval_config` (`role_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260513015845_Initial') THEN

    CREATE UNIQUE INDEX `ix_approval_config_scope_level_department_id_role_id` ON `approval_config` (`scope`, `level`, `department_id`, `role_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260513015845_Initial') THEN

    CREATE INDEX `ix_approval_history_actor_user_id` ON `approval_history` (`actor_user_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260513015845_Initial') THEN

    CREATE INDEX `ix_approval_history_plan_id` ON `approval_history` (`plan_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260513015845_Initial') THEN

    CREATE INDEX `ix_attachment_owner_type_owner_id` ON `attachment` (`owner_type`, `owner_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260513015845_Initial') THEN

    CREATE INDEX `ix_attachment_uploaded_by_user_id` ON `attachment` (`uploaded_by_user_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260513015845_Initial') THEN

    CREATE INDEX `ix_audit_log_actor_user_id` ON `audit_log` (`actor_user_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260513015845_Initial') THEN

    CREATE INDEX `ix_audit_log_entity_name_entity_id` ON `audit_log` (`entity_name`, `entity_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260513015845_Initial') THEN

    CREATE UNIQUE INDEX `ix_department_code` ON `department` (`code`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260513015845_Initial') THEN

    CREATE INDEX `ix_line_comment_author_user_id` ON `line_comment` (`author_user_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260513015845_Initial') THEN

    CREATE INDEX `ix_line_comment_parent_comment_id` ON `line_comment` (`parent_comment_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260513015845_Initial') THEN

    CREATE INDEX `ix_line_comment_resolved_by_user_id` ON `line_comment` (`resolved_by_user_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260513015845_Initial') THEN

    CREATE INDEX `ix_line_comment_task_id` ON `line_comment` (`task_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260513015845_Initial') THEN

    CREATE INDEX `ix_plan_created_by_id` ON `plan` (`created_by_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260513015845_Initial') THEN

    CREATE INDEX `ix_plan_department_id` ON `plan` (`department_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260513015845_Initial') THEN

    CREATE INDEX `ix_plan_scope_department_id_year_month` ON `plan` (`scope`, `department_id`, `year`, `month`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260513015845_Initial') THEN

    CREATE UNIQUE INDEX `ix_refresh_token_token_hash` ON `refresh_token` (`token_hash`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260513015845_Initial') THEN

    CREATE INDEX `ix_refresh_token_user_id` ON `refresh_token` (`user_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260513015845_Initial') THEN

    CREATE UNIQUE INDEX `ix_role_code` ON `role` (`code`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260513015845_Initial') THEN

    CREATE INDEX `ix_task_assignee_user_id` ON `task` (`assignee_user_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260513015845_Initial') THEN

    CREATE INDEX `ix_task_inherited_from_task_id` ON `task` (`inherited_from_task_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260513015845_Initial') THEN

    CREATE INDEX `ix_task_parent_task_id` ON `task` (`parent_task_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260513015845_Initial') THEN

    CREATE INDEX `ix_task_plan_id` ON `task` (`plan_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260513015845_Initial') THEN

    CREATE INDEX `ix_task_supporting_dept_department_id` ON `task_supporting_dept` (`department_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260513015845_Initial') THEN

    CREATE INDEX `ix_user_role_role_id` ON `user_role` (`role_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260513015845_Initial') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260513015845_Initial', '8.0.2');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260515034854_TaskKtnbLeaderText') THEN

    ALTER TABLE `task` ADD `ktnb_leader_text` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260515034854_TaskKtnbLeaderText') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260515034854_TaskKtnbLeaderText', '8.0.2');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260513032909_AdminStep7') THEN

    ALTER TABLE `approval_config` DROP INDEX `ix_approval_config_scope_level_department_id_role_id`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260513032909_AdminStep7') THEN

    CREATE UNIQUE INDEX `ix_approval_config_scope_level` ON `approval_config` (`scope`, `level`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260513032909_AdminStep7') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260513032909_AdminStep7', '8.0.2');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260513073707_TaskOwnerDepartmentAndInheritedTasks') THEN

    ALTER TABLE `task` ADD `owner_department_id` char(36) COLLATE ascii_general_ci NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260513073707_TaskOwnerDepartmentAndInheritedTasks') THEN

    CREATE INDEX `ix_task_owner_department_id` ON `task` (`owner_department_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260513073707_TaskOwnerDepartmentAndInheritedTasks') THEN

    ALTER TABLE `task` ADD CONSTRAINT `fk_task_department_owner_department_id` FOREIGN KEY (`owner_department_id`) REFERENCES `department` (`id`) ON DELETE RESTRICT;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260513073707_TaskOwnerDepartmentAndInheritedTasks') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260513073707_TaskOwnerDepartmentAndInheritedTasks', '8.0.2');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260513091852_TaskNoteTextAndMainPlanImport') THEN

    ALTER TABLE `task` ADD `note_text` longtext CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260513091852_TaskNoteTextAndMainPlanImport') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260513091852_TaskNoteTextAndMainPlanImport', '8.0.2');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;



START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260525072101_PersonalEvaluation') THEN

    CREATE TABLE `personal_evaluation_period` (
        `id` char(36) COLLATE ascii_general_ci NOT NULL,
        `user_id` char(36) COLLATE ascii_general_ci NOT NULL,
        `department_id` char(36) COLLATE ascii_general_ci NOT NULL,
        `report_year` int NOT NULL,
        `report_month` int NOT NULL,
        `capacity_attitude_self_score` decimal(4,1) NULL,
        `capacity_attitude_team_lead_score` decimal(4,1) NULL,
        `capacity_attitude_manager_score` decimal(4,1) NULL,
        `capacity_attitude_deputy_score` decimal(4,1) NULL,
        `capacity_attitude_head_score` decimal(4,1) NULL,
        `discipline_self_score` decimal(4,1) NULL,
        `discipline_team_lead_score` decimal(4,1) NULL,
        `discipline_manager_score` decimal(4,1) NULL,
        `discipline_deputy_score` decimal(4,1) NULL,
        `discipline_head_score` decimal(4,1) NULL,
        `inspection_self_score` decimal(4,1) NULL,
        `inspection_team_lead_score` decimal(4,1) NULL,
        `inspection_manager_score` decimal(4,1) NULL,
        `inspection_deputy_score` decimal(4,1) NULL,
        `inspection_head_score` decimal(4,1) NULL,
        `status` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
        `created_at` datetime(6) NOT NULL,
        `updated_at` datetime(6) NOT NULL,
        CONSTRAINT `pk_personal_evaluation_period` PRIMARY KEY (`id`),
        CONSTRAINT `fk_personal_evaluation_period_app_user_user_id` FOREIGN KEY (`user_id`) REFERENCES `app_user` (`id`) ON DELETE RESTRICT,
        CONSTRAINT `fk_personal_evaluation_period_department_department_id` FOREIGN KEY (`department_id`) REFERENCES `department` (`id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    CREATE TABLE `personal_evaluation_item` (
        `id` char(36) COLLATE ascii_general_ci NOT NULL,
        `period_id` char(36) COLLATE ascii_general_ci NOT NULL,
        `display_order` int NOT NULL,
        `assignment_source` varchar(500) CHARACTER SET utf8mb4 NULL,
        `task_name` varchar(500) CHARACTER SET utf8mb4 NULL,
        `task_detail` longtext CHARACTER SET utf8mb4 NULL,
        `actual_result` longtext CHARACTER SET utf8mb4 NULL,
        `note` longtext CHARACTER SET utf8mb4 NULL,
        `deadline` datetime(6) NULL,
        `completed_at` datetime(6) NULL,
        `self_progress_score` decimal(4,1) NULL,
        `self_quality_score` decimal(4,1) NULL,
        `team_lead_progress_score` decimal(4,1) NULL,
        `team_lead_quality_score` decimal(4,1) NULL,
        `manager_progress_score` decimal(4,1) NULL,
        `manager_quality_score` decimal(4,1) NULL,
        `deputy_progress_score` decimal(4,1) NULL,
        `deputy_quality_score` decimal(4,1) NULL,
        `head_progress_score` decimal(4,1) NULL,
        `head_quality_score` decimal(4,1) NULL,
        `created_at` datetime(6) NOT NULL,
        `updated_at` datetime(6) NOT NULL,
        CONSTRAINT `pk_personal_evaluation_item` PRIMARY KEY (`id`),
        CONSTRAINT `fk_personal_evaluation_item_personal_evaluation_period_period_id` FOREIGN KEY (`period_id`) REFERENCES `personal_evaluation_period` (`id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    CREATE INDEX `ix_personal_evaluation_item_period_id_display_order` ON `personal_evaluation_item` (`period_id`, `display_order`);
    CREATE INDEX `ix_personal_evaluation_period_department_id_report_year_report_~` ON `personal_evaluation_period` (`department_id`, `report_year`, `report_month`);
    CREATE UNIQUE INDEX `ix_personal_evaluation_period_user_id_report_year_report_month` ON `personal_evaluation_period` (`user_id`, `report_year`, `report_month`);

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260525072101_PersonalEvaluation', '8.0.2');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;
