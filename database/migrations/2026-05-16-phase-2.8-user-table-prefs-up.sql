-- =============================================================================
-- 2026-05-16-phase-2.8-user-table-prefs-up.sql
-- Phase 2.8 Data Display — Stage 1B (DB migration up)
--
-- Creates identity.user_table_preferences for per-user LipiTable state persistence.
--
-- APPLY: PART B — Run on EACH CLINIC DATABASE (not master).
--        User identity itself lives in master; table preferences are clinic-scoped
--        (locality with the table data, tenant isolation, no cross-tenant TableId
--        collision). Decision recorded in CHANGE-LOG A38.
--
-- Cross-DB integrity: user_id references master.identity.users(id) but PostgreSQL
-- cannot enforce FK cross-DB. App-layer validation only via auth context.
-- This is safe because LiPi never hard-deletes users — clinic-user-deletion is
-- access revocation only, so orphan preference rows never accumulate.
--
-- SPEC:  docs/00-COMPONENTS/2.8/00-Phase2.8-Overview.md §2.5
-- AMEND: CHANGE-LOG.md A38 (2026-05-16)
-- =============================================================================

BEGIN;

-- The identity schema already exists in each clinic DB (per 02_identity.sql in the
-- clinic bootstrap), but the IF NOT EXISTS guard makes this migration safe to run
-- against fresh DBs where 02_identity.sql is applied separately.
CREATE SCHEMA IF NOT EXISTS identity;

CREATE TABLE IF NOT EXISTS identity.user_table_preferences (
    user_id    UUID         NOT NULL,
    table_id   VARCHAR(200) NOT NULL,
    prefs_json JSONB        NOT NULL DEFAULT '{}'::jsonb,
    updated_at TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    CONSTRAINT pk_user_table_preferences PRIMARY KEY (user_id, table_id)
);

CREATE INDEX IF NOT EXISTS ix_user_table_prefs_user
    ON identity.user_table_preferences (user_id);

COMMENT ON TABLE  identity.user_table_preferences IS
    'Per-user-per-table preferences for LipiTable (Phase 2.8). One row per (user, table). user_id references master.identity.users(id) without DB-level FK (cross-DB limitation).';
COMMENT ON COLUMN identity.user_table_preferences.user_id    IS 'Master.identity.users.id — app-layer enforced.';
COMMENT ON COLUMN identity.user_table_preferences.table_id   IS 'TableId parameter of LipiTable (developer-chosen stable identifier).';
COMMENT ON COLUMN identity.user_table_preferences.prefs_json IS 'TablePreferences serialized as JSONB. Schema: LiPi.Components.DataDisplay.TablePreferences.';
COMMENT ON COLUMN identity.user_table_preferences.updated_at IS 'Last write timestamp. Set by the app on every WriteAsync.';

COMMIT;
