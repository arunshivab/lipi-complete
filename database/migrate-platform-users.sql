-- ================================================================
-- LiPi HIS — Platform Users Migration
-- Run against lipi_dev in pgAdmin
-- Creates master.platform_users and master.clinic_memberships
-- Creates identity.clinic_profiles
-- ================================================================

BEGIN;

-- 1. master.platform_users
CREATE TABLE IF NOT EXISTS master.platform_users (
    id                  UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    username            TEXT        NOT NULL,
    password_hash       TEXT,
    first_name          TEXT        NOT NULL DEFAULT '',
    middle_name         TEXT,
    last_name           TEXT        NOT NULL DEFAULT '',
    display_name        TEXT        NOT NULL DEFAULT '',
    title               TEXT,
    gender              TEXT,
    date_of_birth       TEXT,
    blood_group         TEXT,
    nationality         TEXT        DEFAULT 'Indian',
    email               TEXT        NOT NULL DEFAULT '',
    phone               TEXT,
    phone_country_code  TEXT        DEFAULT '+91',
    nmc_reg_number      TEXT,
    aerb_rp_number      TEXT,
    extension_data      JSONB       NOT NULL DEFAULT '{}',
    user_type           TEXT        NOT NULL DEFAULT 'staff',
    status              TEXT        NOT NULL DEFAULT 'active',
    must_change_password BOOLEAN    NOT NULL DEFAULT true,
    is_mfa_enforced     BOOLEAN     NOT NULL DEFAULT false,
    failed_login_count  INT         NOT NULL DEFAULT 0,
    locked_until        TIMESTAMP,
    last_login_at       TIMESTAMP,
    created_at          TIMESTAMP   NOT NULL DEFAULT now(),
    updated_at          TIMESTAMP   NOT NULL DEFAULT now(),
    deleted_at          TIMESTAMP,
    created_by          UUID,
    row_version         INT         NOT NULL DEFAULT 1
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_platform_users_username
    ON master.platform_users(username) WHERE deleted_at IS NULL;
CREATE INDEX IF NOT EXISTS ix_platform_users_email
    ON master.platform_users(email) WHERE deleted_at IS NULL;
CREATE INDEX IF NOT EXISTS ix_platform_users_user_type
    ON master.platform_users(user_type) WHERE deleted_at IS NULL;

-- 2. master.clinic_memberships
CREATE TABLE IF NOT EXISTS master.clinic_memberships (
    id              UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    platform_user_id UUID       NOT NULL REFERENCES master.platform_users(id) ON DELETE CASCADE,
    clinic_id       UUID        NOT NULL REFERENCES master.clinics(id) ON DELETE CASCADE,
    status          TEXT        NOT NULL DEFAULT 'active',
    created_at      TIMESTAMP   NOT NULL DEFAULT now(),
    created_by      UUID
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_clinic_memberships_user_clinic
    ON master.clinic_memberships(platform_user_id, clinic_id);
CREATE INDEX IF NOT EXISTS ix_clinic_memberships_clinic
    ON master.clinic_memberships(clinic_id);

-- 3. identity.clinic_profiles
CREATE TABLE IF NOT EXISTS identity.clinic_profiles (
    id               UUID        PRIMARY KEY DEFAULT core.uuid_v7(),
    platform_user_id UUID        NOT NULL,
    clinic_id        UUID        NOT NULL,
    designation      TEXT,
    department       TEXT,
    staff_type       TEXT,
    employee_code    TEXT,
    joining_date     TEXT,
    status           TEXT        NOT NULL DEFAULT 'active',
    extension_data   JSONB       NOT NULL DEFAULT '{}',
    created_at       TIMESTAMP   NOT NULL DEFAULT now(),
    updated_at       TIMESTAMP   NOT NULL DEFAULT now(),
    deleted_at       TIMESTAMP,
    created_by       UUID
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_clinic_profiles_user_clinic
    ON identity.clinic_profiles(platform_user_id, clinic_id) WHERE deleted_at IS NULL;

-- 4. Migrate existing Admin/SysAdmin/SiteAdmin to platform_users
-- (only if they don't already exist there)
INSERT INTO master.platform_users
    (id, username, password_hash, first_name, last_name, display_name,
     email, user_type, status, must_change_password, created_at, updated_at, extension_data)
SELECT
    gen_random_uuid(),
    u.username,
    u.password_hash,
    COALESCE((u.extension_data::jsonb)->>'firstName', u.username),
    COALESCE((u.extension_data::jsonb)->>'lastName', ''),
    COALESCE((u.extension_data::jsonb)->>'displayName', u.username),
    COALESCE(u.email, u.username || '@lipi.local'),
    CASE r.code
        WHEN 'global_admin' THEN 'global_admin'
        WHEN 'sys_admin'    THEN 'sys_admin'
        WHEN 'site_admin'   THEN 'site_admin'
        ELSE 'staff'
    END,
    u.status,
    u.must_change_password,
    u.created_at,
    u.updated_at,
    COALESCE(u.extension_data::jsonb, '{}')
FROM identity.users u
JOIN identity.user_roles ur ON ur.user_id = u.id
JOIN identity.roles r ON r.id = ur.role_id
WHERE u.deleted_at IS NULL
  AND r.code IN ('global_admin', 'sys_admin', 'site_admin')
ON CONFLICT (username) WHERE deleted_at IS NULL DO NOTHING;

-- 5. Create clinic memberships for migrated users
INSERT INTO master.clinic_memberships (platform_user_id, clinic_id, status, created_at)
SELECT
    pu.id,
    c.id,
    'active',
    now()
FROM master.platform_users pu
CROSS JOIN master.clinics c
WHERE c.deleted_at IS NULL
  AND pu.user_type IN ('global_admin', 'sys_admin')
ON CONFLICT (platform_user_id, clinic_id) DO NOTHING;

-- Verify
SELECT
    pu.username,
    pu.user_type,
    pu.status,
    COUNT(cm.id) AS clinic_count
FROM master.platform_users pu
LEFT JOIN master.clinic_memberships cm ON cm.platform_user_id = pu.id
WHERE pu.deleted_at IS NULL
GROUP BY pu.username, pu.user_type, pu.status
ORDER BY pu.user_type, pu.username;

COMMIT;
