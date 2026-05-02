-- =============================================================================
-- LiPi HIS — Migration: Decision #12 Theming Schema Additions (DOWN)
--
-- SPEC:     docs/00-COMPONENTS/00.2-THEMING-ARCHITECTURE.md §Database Schema Additions
-- DECISION: docs/00-PROJECT-BASELINE.md §12.4 (Multi-Theme System)
-- Phase:    Phase 1 — Theming Architecture
-- Date:     2026-05-02
-- Author:   Arun Shiva
--
-- WHAT THIS SCRIPT DOES (full reversal of theming-up.sql):
--   PART A — Run on MASTER database (lipi_master):
--     • Drops FK from master.clinics.brand_theme_id
--     • Drops index on master.clinics.brand_theme_id
--     • Drops brand_theme_id column from master.clinics
--     • Drops trigger on master.brand_themes
--     • Drops index on master.brand_themes
--     • Drops master.brand_themes table  ⚠️ ALL BRAND METADATA LOST
--     • Does NOT drop update_modified_column() — may be used by other tables
--
--   PART B — Run on each CLINIC database:
--     • Drops trigger on identity.user_preferences
--     • Drops identity.user_preferences table  ⚠️ ALL USER PREFERENCES LOST
--     • Does NOT drop update_modified_column()
--
-- IDEMPOTENT: Safe to run multiple times. Uses IF EXISTS throughout.
--
-- ⚠️  WARNING — THIS SCRIPT IS DESTRUCTIVE:
--   PART A: master.brand_themes DROP loses all brand theme metadata and
--           any custom branding records (Armoki etc.) added since migration.
--           All clinics will lose their brand_theme_id association.
--   PART B: identity.user_preferences DROP loses all user theme/density/
--           font preferences permanently.
--   Confirm with team before running in any environment with real data.
--
-- HOW TO RUN:
--   PART A: psql -h HOST -U lipi_app -d lipi_master   -f this_file.sql
--           (comment out PART B, or stop after PART A COMMIT)
--   PART B: psql -h HOST -U lipi_app -d lipi_training -f this_file.sql
--           (run separately for each clinic database)
-- =============================================================================


-- =============================================================================
-- PART A — MASTER DATABASE (lipi_master)
-- Drop in strict reverse order of creation to respect FK dependencies.
-- =============================================================================

BEGIN;

-- ---------------------------------------------------------------------------
-- A1. Drop FK constraint from master.clinics first
-- ---------------------------------------------------------------------------
-- Must be dropped BEFORE the referenced table (brand_themes) can be dropped.

ALTER TABLE master.clinics
    DROP CONSTRAINT IF EXISTS fk_clinics_brand_theme_id;

-- ---------------------------------------------------------------------------
-- A2. Drop index on master.clinics.brand_theme_id
-- ---------------------------------------------------------------------------
-- In PostgreSQL, dropping a column auto-drops its indexes, but being
-- explicit here keeps the down script self-documenting.

DROP INDEX IF EXISTS master.idx_clinics_brand_theme_id;

-- ---------------------------------------------------------------------------
-- A3. Drop brand_theme_id column from master.clinics
-- ---------------------------------------------------------------------------

ALTER TABLE master.clinics
    DROP COLUMN IF EXISTS brand_theme_id;

-- ---------------------------------------------------------------------------
-- A4. Drop trigger on master.brand_themes
-- ---------------------------------------------------------------------------

DROP TRIGGER IF EXISTS tr_brand_themes_updated
    ON master.brand_themes;

-- ---------------------------------------------------------------------------
-- A5. Drop index on master.brand_themes
-- ---------------------------------------------------------------------------

DROP INDEX IF EXISTS master.idx_brand_themes_active_sorted;

-- ---------------------------------------------------------------------------
-- A6. Drop master.brand_themes table
-- ---------------------------------------------------------------------------
-- ⚠️ DESTRUCTIVE: All brand theme metadata is permanently lost.
--    Any brands added beyond 'lipi-default' (e.g., armoki, client-3) are gone.

DROP TABLE IF EXISTS master.brand_themes;

-- NOTE: update_modified_column() is intentionally NOT dropped here.
-- It is a shared utility function. If other master DB tables use it,
-- dropping it would break those triggers.
-- To drop manually (only if no other table uses it):
--   DROP FUNCTION IF EXISTS update_modified_column();

COMMIT;

-- Verify Part A:
-- SELECT column_name FROM information_schema.columns
-- WHERE table_schema = 'master' AND table_name = 'clinics'
--   AND column_name = 'brand_theme_id';
-- (should return 0 rows)
--
-- SELECT table_name FROM information_schema.tables
-- WHERE table_schema = 'master' AND table_name = 'brand_themes';
-- (should return 0 rows)


-- =============================================================================
-- PART B — CLINIC DATABASE (run once per clinic database)
-- ⚠️  DESTRUCTIVE — drops user_preferences table and all preference data.
-- PART B IS UNCHANGED from original script.
-- =============================================================================

BEGIN;

-- ---------------------------------------------------------------------------
-- B1. Drop trigger first (must be dropped before table)
-- ---------------------------------------------------------------------------
DROP TRIGGER IF EXISTS tr_user_preferences_updated
    ON identity.user_preferences;

-- ---------------------------------------------------------------------------
-- B2. Drop partial index
-- ---------------------------------------------------------------------------
DROP INDEX IF EXISTS identity.idx_user_preferences_non_default_mode;

-- ---------------------------------------------------------------------------
-- B3. Drop identity.user_preferences table
-- ⚠️  All user preference data (theme, density, font_size, language) is lost.
-- ---------------------------------------------------------------------------
DROP TABLE IF EXISTS identity.user_preferences;

-- NOTE: update_modified_column() is intentionally NOT dropped here.
-- It is a shared utility function used by other tables in the identity schema.
-- To drop manually (only if no other table uses it):
--   DROP FUNCTION IF EXISTS update_modified_column();

COMMIT;

-- Verify Part B:
-- SELECT table_name FROM information_schema.tables
-- WHERE table_schema = 'identity' AND table_name = 'user_preferences';
-- (should return 0 rows)
