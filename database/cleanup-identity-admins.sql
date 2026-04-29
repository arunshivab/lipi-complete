-- Clean old global/sys admin entries from identity.users (they now live in master.platform_users)
DELETE FROM identity.user_roles 
WHERE user_id IN (
    SELECT u.id FROM identity.users u
    JOIN identity.user_roles ur ON ur.user_id = u.id
    JOIN identity.roles r ON r.id = ur.role_id
    WHERE r.code IN ('global_admin', 'sys_admin')
);

DELETE FROM identity.users
WHERE username IN ('Admin', 'SysAdmin')
  AND id IN (
    SELECT DISTINCT ur.user_id FROM identity.user_roles ur
    JOIN identity.roles r ON r.id = ur.role_id
    WHERE r.code IN ('global_admin', 'sys_admin')
  );

-- Verify
SELECT u.username, r.code AS role
FROM identity.users u
JOIN identity.user_roles ur ON ur.user_id = u.id
JOIN identity.roles r ON r.id = ur.role_id
WHERE u.deleted_at IS NULL
ORDER BY r.code, u.username;
