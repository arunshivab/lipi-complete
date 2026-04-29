-- Smoke Tests for LiPi Database
-- Quick verification of schema creation and basic functionality

-- Schema verification
SELECT schema_name FROM information_schema.schemata 
WHERE schema_name NOT IN ('pg_catalog','information_schema','public') 
ORDER BY schema_name;

-- Table count by schema
SELECT 
    table_schema,
    COUNT(*) as table_count
FROM information_schema.tables
WHERE table_schema NOT IN ('pg_catalog','information_schema','public')
GROUP BY table_schema
ORDER BY table_schema;

-- Verify key tables exist in core schema
SELECT 
    'core' as schema_name,
    COUNT(*) as table_count
FROM information_schema.tables
WHERE table_schema = 'core';

-- Verify key tables exist in identity schema
SELECT 
    'identity' as schema_name,
    COUNT(*) as table_count
FROM information_schema.tables
WHERE table_schema = 'identity';

-- Verify audit triggers exist
SELECT COUNT(*) as trigger_count FROM information_schema.triggers 
WHERE trigger_schema IN ('core', 'identity', 'abdm', 'audit', 'security', 'compliance', 'certs', 'sigma');

-- Verify RLS policies exist
SELECT COUNT(*) as policy_count FROM pg_policies;

-- Verify UUID v7 function
SELECT proname FROM pg_proc WHERE proname = 'uuid_v7';

-- Verify hash function for audit
SELECT proname FROM pg_proc WHERE proname = 'fn_compute_hash';

-- Test clinic-scoped RLS (if data exists)
SET lipi.clinic_id = '00000000-0000-0000-0000-000000000001';
SELECT COUNT(*) FROM identity.users;
RESET lipi.clinic_id;

-- Summary
SELECT 'Database provisioning complete!' as status;
