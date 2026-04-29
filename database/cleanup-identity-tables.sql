-- ================================================================
-- LiPi HIS — Clean Identity Tables (DEV ONLY)
-- ================================================================

TRUNCATE identity.ad_sync_runs     CASCADE;
TRUNCATE identity.api_keys         CASCADE;
TRUNCATE identity.service_accounts CASCADE;
TRUNCATE identity.login_attempts   CASCADE;
TRUNCATE identity.mfa_methods      CASCADE;
TRUNCATE identity.sessions         CASCADE;
TRUNCATE identity.role_permissions CASCADE;
TRUNCATE identity.user_roles       CASCADE;
TRUNCATE identity.permissions      CASCADE;
TRUNCATE identity.password_history CASCADE;
TRUNCATE identity.users            CASCADE;
TRUNCATE identity.roles            CASCADE;

-- Verify all empty
SELECT tablename,
       (xpath('/row/c/text()',
           query_to_xml(
               format('SELECT COUNT(*) AS c FROM identity.%I', tablename),
               false, true, ''))
       )[1]::text::int AS rows
FROM pg_tables
WHERE schemaname = 'identity'
ORDER BY tablename;
