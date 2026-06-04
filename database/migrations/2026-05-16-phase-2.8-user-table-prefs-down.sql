-- =============================================================================
-- 2026-05-16-phase-2.8-user-table-prefs-down.sql
-- Phase 2.8 Data Display — Stage 1B (DB migration down)
--
-- Drops identity.user_table_preferences (the reverse of the matching -up.sql).
--
-- APPLY: Run on EACH CLINIC DATABASE that received the -up migration.
-- WARNING: Destroys all stored user table-state preferences. Users will see
--          table defaults on next mount until they reconfigure.
--
-- SPEC:  docs/00-COMPONENTS/2.8/00-Phase2.8-Overview.md §2.5
-- AMEND: CHANGE-LOG.md A38 (2026-05-16)
-- =============================================================================

BEGIN;

DROP INDEX IF EXISTS identity.ix_user_table_prefs_user;
DROP TABLE IF EXISTS identity.user_table_preferences;

COMMIT;
