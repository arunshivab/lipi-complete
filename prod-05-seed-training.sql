-- ================================================================
-- LiPi HIS — Step 5: Seed training clinic database
-- Run against lipi_training after prod-03-training-schema.sql
-- ================================================================

-- ── Default roles ────────────────────────────────────────────────
DO $$
DECLARE
    clinic_id UUID := '00000000-0000-0000-0000-000000000001'::uuid; -- placeholder, update if needed
BEGIN
    -- Get actual training clinic ID from master (if using FDW) or hardcode
    -- For now use a fixed UUID that matches what master seeded
    -- After running, verify with: SELECT id FROM master.clinics WHERE code='training'
END $$;

-- Insert roles for training clinic
-- clinic_id will be the UUID from master.clinics where code='training'
-- Run this after noting the clinic UUID from prod-04 verification query

-- NOTE: Replace TRAINING_CLINIC_ID below with actual UUID from master.clinics
-- SELECT id FROM master.clinics WHERE code = 'training';

DO $$
DECLARE
    cid UUID;
BEGIN
    -- We use a placeholder UUID for training clinic
    -- The app seeds roles via ClinicSeeder on startup
    RAISE NOTICE 'Training clinic roles will be seeded by the application on first run.';
    RAISE NOTICE 'Start the app and the ClinicSeeder will create roles automatically.';
END $$;

-- ── login_attempts partitions for current + next months ──────────
CREATE TABLE IF NOT EXISTS identity.login_attempts_2026_06 PARTITION OF identity.login_attempts
    FOR VALUES FROM ('2026-06-01') TO ('2026-07-01');
CREATE TABLE IF NOT EXISTS identity.login_attempts_2026_07 PARTITION OF identity.login_attempts
    FOR VALUES FROM ('2026-07-01') TO ('2026-08-01');

-- Verify
SELECT table_name FROM information_schema.tables
WHERE table_schema = 'identity'
ORDER BY table_name;
