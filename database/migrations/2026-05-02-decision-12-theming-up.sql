-- =============================================================================
-- LiPi HIS — Migration: Decision #12 Theming Schema Additions (UP)
--
-- SPEC:     docs/00-COMPONENTS/00.2-THEMING-ARCHITECTURE.md §Database Schema Additions
-- DECISION: docs/00-PROJECT-BASELINE.md §12.4 (Multi-Theme System)
-- Phase:    Phase 1 — Theming Architecture
-- Date:     2026-05-02
-- Author:   Arun Shiva
--
-- WHAT THIS SCRIPT DOES:
--   PART A — Run on MASTER database (lipi_master):
--     • Creates update_modified_column() trigger function (master DB copy)
--     • Creates master.brand_themes lookup table (replaces CHECK constraint approach)
--     • Seeds 'lipi-default' as the v1.0 brand row
--     • Adds brand_theme_id column to master.clinics
--     • Adds FK: master.clinics.brand_theme_id → master.brand_themes.brand_id
--     • Adds supporting indexes
--
--   PART B — Run on each CLINIC database (in identity schema):
--     • Creates update_modified_column() trigger function (clinic DB copy)
--     • Creates identity.user_preferences table
--     • Creates trigger and index on user_preferences
--
-- DESIGN NOTE — WHY LOOKUP TABLE INSTEAD OF CHECK CONSTRAINT:
--   Adding new brand themes (Armoki, Client #3, etc.) should be:
--     INSERT INTO master.brand_themes (...) + add CSS file
--   NOT a schema migration. The lookup table approach makes onboarding
--   new clients a data operation, not a DDL operation.
--
-- IDEMPOTENT: Safe to run multiple times.
--   Uses IF NOT EXISTS / CREATE OR REPLACE / ON CONFLICT DO NOTHING / DO $$ throughout.
--
-- HOW TO RUN:
--   PART A: psql -h HOST -U lipi_app -d lipi_master   -f this_file.sql
--           (comment out PART B, or stop after PART A COMMIT)
--   PART B: psql -h HOST -U lipi_app -d lipi_training -f this_file.sql
--           (run separately for each clinic database)
--
-- ROLLBACK: Run 2026-05-02-decision-12-theming-down.sql
-- =============================================================================


-- =============================================================================
-- PART A — MASTER DATABASE (lipi_master)
-- =============================================================================

BEGIN;

-- ---------------------------------------------------------------------------
-- A1. Trigger function: update_modified_column  (master DB copy)
-- ---------------------------------------------------------------------------
-- The same function is also created in each clinic DB (Part B).
-- Both are independently owned by their respective databases.
-- CREATE OR REPLACE is idempotent — safe to run even if already defined.

CREATE OR REPLACE FUNCTION update_modified_column()
RETURNS TRIGGER
LANGUAGE plpgsql
AS $$
BEGIN
    NEW.updated_at = NOW();
    RETURN NEW;
END;
$$;

-- ---------------------------------------------------------------------------
-- A2. Create master.brand_themes lookup table
-- ---------------------------------------------------------------------------
-- Stores metadata for every brand theme available on the platform.
-- Adding a new client brand = INSERT row + add CSS file. No migration needed.
--
-- is_active:         FALSE = hidden from clinic brand picker; existing clinics still work
-- is_deprecated:     TRUE  = brand is phased out; show warning in admin UI
-- sort_order:        Controls display order in brand picker (lower = first; lipi-default = 1)

CREATE TABLE IF NOT EXISTS master.brand_themes (
    brand_id            VARCHAR(50)   NOT NULL,
    display_name        VARCHAR(100)  NOT NULL,
    description         TEXT,
    css_file_path       VARCHAR(200)  NOT NULL,
    logo_light_url      VARCHAR(200),
    logo_dark_url       VARCHAR(200),
    is_active           BOOLEAN       NOT NULL DEFAULT TRUE,
    is_deprecated       BOOLEAN       NOT NULL DEFAULT FALSE,
    deprecated_at       TIMESTAMPTZ,
    deprecation_reason  TEXT,
    sort_order          INTEGER       NOT NULL DEFAULT 100,
    created_at          TIMESTAMPTZ   NOT NULL DEFAULT NOW(),
    updated_at          TIMESTAMPTZ   NOT NULL DEFAULT NOW(),

    CONSTRAINT pk_brand_themes PRIMARY KEY (brand_id),

    -- Deprecation consistency: both fields must agree
    CONSTRAINT chk_brand_themes_deprecation
        CHECK (
            (is_deprecated = FALSE AND deprecated_at IS NULL)
            OR
            (is_deprecated = TRUE  AND deprecated_at IS NOT NULL)
        )
);

COMMENT ON TABLE master.brand_themes IS
    'Lookup table for all brand themes available on the platform. '
    'Adding a new client brand = INSERT row + add CSS file. No migration needed. '
    'SPEC: docs/00-COMPONENTS/00.2-THEMING-ARCHITECTURE.md §Database Schema Additions';

COMMENT ON COLUMN master.brand_themes.brand_id IS
    'Short identifier used as data-brand attribute on <body> and in CSS selectors. '
    'Example: ''lipi-default'', ''armoki''. Lowercase-kebab recommended. Max 50 chars.';
COMMENT ON COLUMN master.brand_themes.css_file_path IS
    'Path relative to wwwroot/. Example: ''themes/brand-lipi.css''.';
COMMENT ON COLUMN master.brand_themes.is_active IS
    'FALSE = hidden from clinic brand picker. Existing clinics using it still work.';
COMMENT ON COLUMN master.brand_themes.is_deprecated IS
    'TRUE = brand phased out. Admin UI shows warning. Clinics should migrate.';
COMMENT ON COLUMN master.brand_themes.sort_order IS
    'Display order in brand picker. Lower = first. lipi-default uses 1.';

-- ---------------------------------------------------------------------------
-- A3. Trigger: auto-update updated_at on brand_themes
-- ---------------------------------------------------------------------------
DROP TRIGGER IF EXISTS tr_brand_themes_updated
    ON master.brand_themes;

CREATE TRIGGER tr_brand_themes_updated
    BEFORE UPDATE ON master.brand_themes
    FOR EACH ROW
    EXECUTE FUNCTION update_modified_column();

-- ---------------------------------------------------------------------------
-- A4. Seed: lipi-default  (the v1.0 baseline brand)
-- ---------------------------------------------------------------------------
-- ON CONFLICT DO NOTHING makes this fully idempotent.
-- Do NOT use ON CONFLICT DO UPDATE — never silently overwrite manually edited metadata.

INSERT INTO master.brand_themes
    (brand_id, display_name, description, css_file_path,
     logo_light_url, logo_dark_url, sort_order)
VALUES
    ('lipi-default',
     'LiPi Default',
     'Standard LiPi navy + gold theme — imagiQa product baseline. '
     'Navy (#0F2D5E) primary with gold (#C49A22) accent.',
     'themes/brand-lipi.css',
     '/images/logos/lipi-logo.svg',
     '/images/logos/lipi-logo-dark.svg',
     1)
ON CONFLICT (brand_id) DO NOTHING;

-- v1.1+ example — DO NOT uncomment here; run as a separate data migration:
-- INSERT INTO master.brand_themes
--     (brand_id, display_name, description, css_file_path, sort_order)
-- VALUES ('armoki', 'Armoki', 'Armoki cancer hospital brand', 'themes/brand-armoki.css', 2)
-- ON CONFLICT (brand_id) DO NOTHING;

-- ---------------------------------------------------------------------------
-- A5. Index on brand_themes: active brands sorted (for brand picker query)
-- ---------------------------------------------------------------------------

CREATE INDEX IF NOT EXISTS idx_brand_themes_active_sorted
    ON master.brand_themes(sort_order)
    WHERE is_active = TRUE AND is_deprecated = FALSE;

-- ---------------------------------------------------------------------------
-- A6. Add brand_theme_id column to master.clinics
-- ---------------------------------------------------------------------------
-- Default 'lipi-default' is guaranteed to exist in brand_themes after A4.

ALTER TABLE master.clinics
    ADD COLUMN IF NOT EXISTS brand_theme_id VARCHAR(50) NOT NULL DEFAULT 'lipi-default';

-- ---------------------------------------------------------------------------
-- A7. Index on master.clinics.brand_theme_id
-- ---------------------------------------------------------------------------

CREATE INDEX IF NOT EXISTS idx_clinics_brand_theme_id
    ON master.clinics(brand_theme_id);

-- ---------------------------------------------------------------------------
-- A8. FK: master.clinics.brand_theme_id → master.brand_themes.brand_id
-- ---------------------------------------------------------------------------
-- Replaces the CHECK constraint approach. Adding a new brand = INSERT, not DDL.
-- ON UPDATE CASCADE: if a brand_id is ever renamed, all clinics follow automatically.
-- ON DELETE RESTRICT: cannot delete a brand while any clinic references it.
-- Wrapped in DO block for idempotency.

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM   pg_constraint
        WHERE  conname  = 'fk_clinics_brand_theme_id'
          AND  conrelid = 'master.clinics'::regclass
    ) THEN
        ALTER TABLE master.clinics
            ADD CONSTRAINT fk_clinics_brand_theme_id
            FOREIGN KEY (brand_theme_id)
            REFERENCES master.brand_themes(brand_id)
            ON UPDATE CASCADE
            ON DELETE RESTRICT;
    END IF;
END
$$;

COMMIT;

-- Verify Part A:
-- SELECT brand_id, display_name, css_file_path, is_active, sort_order
-- FROM master.brand_themes ORDER BY sort_order;
--
-- SELECT column_name, data_type, column_default
-- FROM information_schema.columns
-- WHERE table_schema = 'master' AND table_name = 'clinics'
--   AND column_name = 'brand_theme_id';
--
-- SELECT conname, contype FROM pg_constraint
-- WHERE conrelid = 'master.clinics'::regclass AND conname LIKE '%brand%';


-- =============================================================================
-- PART B — CLINIC DATABASE (run once per clinic: lipi_training, etc.)
-- PART B IS UNCHANGED from original script.
-- =============================================================================

BEGIN;

-- ---------------------------------------------------------------------------
-- B1. Trigger function: update_modified_column
-- ---------------------------------------------------------------------------
CREATE OR REPLACE FUNCTION update_modified_column()
RETURNS TRIGGER
LANGUAGE plpgsql
AS $$
BEGIN
    NEW.updated_at = NOW();
    RETURN NEW;
END;
$$;

-- ---------------------------------------------------------------------------
-- B2. Create identity.user_preferences
-- ---------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS identity.user_preferences (
    user_id     UUID         NOT NULL,
    theme_mode  VARCHAR(20)  NOT NULL DEFAULT 'light',
    density     VARCHAR(20)  NOT NULL DEFAULT 'compact',
    font_size   VARCHAR(20)  NOT NULL DEFAULT 'standard',
    language    VARCHAR(10)  NOT NULL DEFAULT 'en',
    created_at  TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    updated_at  TIMESTAMPTZ  NOT NULL DEFAULT NOW(),

    CONSTRAINT pk_user_preferences PRIMARY KEY (user_id),

    CONSTRAINT chk_user_pref_theme_mode
        CHECK (theme_mode IN ('light', 'dark', 'auto', 'high-contrast')),

    CONSTRAINT chk_user_pref_density
        CHECK (density IN ('comfortable', 'compact', 'spacious')),

    CONSTRAINT chk_user_pref_font_size
        CHECK (font_size IN ('standard', 'larger')),

    CONSTRAINT chk_user_pref_language
        CHECK (language ~ '^[a-z]{2}(-[A-Z]{2})?$')
);

COMMENT ON TABLE identity.user_preferences IS
    'Per-user UI preferences (theme, density, font size, language). '
    'Logical FK to master.platform_users.user_id (cross-DB, no physical constraint). '
    'SPEC: docs/00-COMPONENTS/00.2-THEMING-ARCHITECTURE.md §Database Schema Additions';

COMMENT ON COLUMN identity.user_preferences.user_id IS
    'Logical FK to master.platform_users.user_id (cross-DB, no physical constraint).';
COMMENT ON COLUMN identity.user_preferences.theme_mode IS
    'Light/dark/auto/high-contrast. Default: light. Auto and high-contrast: v1.1+.';
COMMENT ON COLUMN identity.user_preferences.density IS
    'UI density. Default: compact (Apple HIG baseline for v1.0).';
COMMENT ON COLUMN identity.user_preferences.font_size IS
    'Standard or larger (accessibility). Default: standard.';

-- ---------------------------------------------------------------------------
-- B3. Trigger: auto-update updated_at
-- ---------------------------------------------------------------------------
DROP TRIGGER IF EXISTS tr_user_preferences_updated
    ON identity.user_preferences;

CREATE TRIGGER tr_user_preferences_updated
    BEFORE UPDATE ON identity.user_preferences
    FOR EACH ROW
    EXECUTE FUNCTION update_modified_column();

-- ---------------------------------------------------------------------------
-- B4. Partial index for non-default theme mode
-- ---------------------------------------------------------------------------
CREATE INDEX IF NOT EXISTS idx_user_preferences_non_default_mode
    ON identity.user_preferences(user_id)
    WHERE theme_mode != 'light';

COMMIT;

-- Verify Part B:
-- SELECT table_name, column_name, data_type, column_default
-- FROM information_schema.columns
-- WHERE table_schema = 'identity' AND table_name = 'user_preferences'
-- ORDER BY ordinal_position;
--
-- SELECT conname, contype, pg_get_constraintdef(oid)
-- FROM pg_constraint
-- WHERE conrelid = 'identity.user_preferences'::regclass;
