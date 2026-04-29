-- =====================================================================
-- LiPi :: 00_common :: 003_audit_triggers
-- Generic triggers for created_at / updated_at / row_version.
-- Per-table audit-event emission is defined in clinic/04_audit.sql
-- because it references audit.audit_events (not yet created here).
-- =====================================================================

CREATE OR REPLACE FUNCTION core.fn_set_updated_at()
RETURNS trigger
LANGUAGE plpgsql
AS $$
BEGIN
    NEW.updated_at := now();
    NEW.row_version := COALESCE(OLD.row_version, 0) + 1;
    RETURN NEW;
END;
$$;

COMMENT ON FUNCTION core.fn_set_updated_at() IS
  'Attach as BEFORE UPDATE trigger on every mutable table. Bumps updated_at and row_version.';

-- Convenience helper: attach the standard triggers to a table in one call
CREATE OR REPLACE FUNCTION core.attach_standard_triggers(p_schema text, p_table text)
RETURNS void
LANGUAGE plpgsql
AS $$
BEGIN
    EXECUTE format(
      'CREATE TRIGGER trg_%1$s_set_updated_at BEFORE UPDATE ON %2$I.%1$I
       FOR EACH ROW EXECUTE FUNCTION core.fn_set_updated_at();',
      p_table, p_schema);
END;
$$;
