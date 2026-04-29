-- ================================================================
-- LiPi HIS — Step 4: Seed master database
-- Run against lipi_master after prod-02-master-schema.sql
-- ================================================================

-- ── 1. imagiqa Organisation ──────────────────────────────────────
INSERT INTO master.organizations (
    id, legal_name, trading_name, org_type, country_code,
    registered_address, primary_contact, status, onboarded_at
) VALUES (
    gen_random_uuid(),
    'Armoki Healthcare Technologies Private Limited',
    'imagiqa',
    'single_clinic',
    'IN',
    '{"line1": "imagiqa HQ", "city": "Pune", "state": "Maharashtra", "pin": "411001"}'::jsonb,
    '{"name": "Global Admin", "email": "admin@imagiqa.com", "phone": "+919999999999"}'::jsonb,
    'active',
    now()
) ON CONFLICT DO NOTHING;

-- ── 2. Training Clinic ───────────────────────────────────────────
INSERT INTO master.clinics (
    id, organization_id, code, name, clinic_type,
    city, state, country_code, address, timezone,
    has_oncology, has_rt, status, go_live_at
) VALUES (
    gen_random_uuid(),
    (SELECT id FROM master.organizations WHERE trading_name = 'imagiqa' LIMIT 1),
    'training',
    'LiPi Training Centre',
    'cancer_centre',
    'Pune', 'Maharashtra', 'IN',
    '{"line1": "Training Centre", "city": "Pune", "state": "Maharashtra", "pin": "411001"}'::jsonb,
    'Asia/Kolkata',
    true, true,
    'active',
    now()
) ON CONFLICT DO NOTHING;

-- ── 3. Clinic Database record (connection string for lipi_training) ──
-- NOTE: EncryptedConnectionString is left NULL here.
-- The app will build connection string from DbHost/DbPort/DbName.
-- Update with encrypted string via app after first run.
INSERT INTO master.clinic_databases (
    id, clinic_id, db_host, db_port, db_name, db_schema_version,
    encryption_key_arn, health_status, created_at, updated_at,
    db_username
) VALUES (
    gen_random_uuid(),
    (SELECT id FROM master.clinics WHERE code = 'training' LIMIT 1),
    'localhost',
    5432,
    'lipi_training',
    '1.0.0',
    'local-dev',
    'green',
    now(), now(),
    'postgres'
) ON CONFLICT DO NOTHING;

-- ── 4. Global Admin (platform_users) ─────────────────────────────
-- Password: Admin@123 (Argon2id hash — change on first login)
INSERT INTO master.platform_users (
    id, username, password_hash,
    first_name, last_name, display_name,
    email, user_type, status, must_change_password,
    created_at, updated_at, extension_data
) VALUES (
    gen_random_uuid(),
    'Admin',
    -- Argon2id hash of 'Admin@123' — app will verify this
    -- Leave as placeholder; run dotnet run -- --setup-global-admin to properly create
    '$argon2id$v=19$m=65536,t=3,p=4$placeholder$placeholder',
    'Global', 'Admin', 'Global Admin',
    'admin@imagiqa.com',
    'global_admin', 'active', true,
    now(), now(),
    '{"staffType": "admin"}'::jsonb
) ON CONFLICT DO NOTHING;

-- ── 5. Clinic memberships for admin users ────────────────────────
INSERT INTO master.clinic_memberships (platform_user_id, clinic_id, status, created_at)
SELECT pu.id, c.id, 'active', now()
FROM master.platform_users pu
CROSS JOIN master.clinics c
WHERE pu.user_type IN ('global_admin', 'sys_admin')
  AND c.deleted_at IS NULL
ON CONFLICT (platform_user_id, clinic_id) DO NOTHING;

-- Verify
SELECT 'organizations' AS tbl, COUNT(*) FROM master.organizations
UNION ALL SELECT 'clinics', COUNT(*) FROM master.clinics
UNION ALL SELECT 'clinic_databases', COUNT(*) FROM master.clinic_databases
UNION ALL SELECT 'platform_users', COUNT(*) FROM master.platform_users
UNION ALL SELECT 'clinic_memberships', COUNT(*) FROM master.clinic_memberships;
