-- ============================================================
-- LiPi HIS — Admin Hard Cleanup (DEV ONLY)
-- Run in pgAdmin against lipi_dev database
-- HARD DELETES all duplicate admin accounts
-- ============================================================

BEGIN;

-- Show before state
SELECT u.username, r.code AS role, u.clinic_id, u.created_at, u.id
FROM identity.users u
JOIN identity.user_roles ur ON ur.user_id = u.id
JOIN identity.roles r ON r.id = ur.role_id
WHERE r.code IN ('global_admin','sys_admin','site_admin')
ORDER BY r.code, u.username, u.created_at;

-- Delete user_roles for duplicates first (FK constraint)
DELETE FROM identity.user_roles
WHERE user_id IN (
    SELECT id FROM identity.users
    WHERE username IN ('Admin','SysAdmin','SiteAdmin')
    AND id NOT IN (
        SELECT DISTINCT ON (username) id
        FROM identity.users
        WHERE username IN ('Admin','SysAdmin','SiteAdmin')
        ORDER BY username, created_at ASC
    )
);

-- Hard delete duplicate users
DELETE FROM identity.users
WHERE username IN ('Admin','SysAdmin','SiteAdmin')
AND id NOT IN (
    SELECT DISTINCT ON (username) id
    FROM identity.users
    WHERE username IN ('Admin','SysAdmin','SiteAdmin')
    ORDER BY username, created_at ASC
);

-- Clean up any Guid.Empty orphan admin records too
DELETE FROM identity.user_roles
WHERE user_id IN (
    SELECT id FROM identity.users
    WHERE clinic_id = '00000000-0000-0000-0000-000000000000'::uuid
);
DELETE FROM identity.users
WHERE clinic_id = '00000000-0000-0000-0000-000000000000'::uuid;

-- Verify — should show exactly 1 Admin, 1 SysAdmin, 1 SiteAdmin
SELECT u.username, r.code AS role, u.clinic_id, u.created_at
FROM identity.users u
JOIN identity.user_roles ur ON ur.user_id = u.id
JOIN identity.roles r ON r.id = ur.role_id
WHERE r.code IN ('global_admin','sys_admin','site_admin')
ORDER BY r.code, u.username;

COMMIT;
