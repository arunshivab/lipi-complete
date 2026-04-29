-- =====================================================================
-- LiPi :: 00_common :: 001_extensions
-- Run FIRST on every database (master + per-clinic).
-- =====================================================================

CREATE EXTENSION IF NOT EXISTS "pgcrypto";      -- gen_random_uuid, digest(), hmac()
CREATE EXTENSION IF NOT EXISTS "btree_gist";    -- range exclusion constraints (appointment overlap, bed occupancy)
CREATE EXTENSION IF NOT EXISTS "pg_trgm";       -- fuzzy name / MRN search
CREATE EXTENSION IF NOT EXISTS "unaccent";      -- accent-insensitive search for Indian names
CREATE EXTENSION IF NOT EXISTS "citext";        -- case-insensitive TEXT (emails, usernames)
CREATE EXTENSION IF NOT EXISTS "postgres_fdw";  -- clinic-to-warehouse FDW links
