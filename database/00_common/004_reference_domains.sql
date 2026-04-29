-- =====================================================================
-- LiPi :: 00_common :: 004_reference_domains
-- Shared DOMAIN types for common constrained values. Used across schemas.
-- =====================================================================

CREATE DOMAIN core.d_email        AS citext  CHECK (VALUE ~ '^[^@\s]+@[^@\s]+\.[^@\s]+$');
CREATE DOMAIN core.d_phone_in     AS text    CHECK (VALUE ~ '^\+?[0-9][0-9\-\s]{5,19}$');
CREATE DOMAIN core.d_aadhaar      AS text    CHECK (VALUE ~ '^[0-9]{12}$');
CREATE DOMAIN core.d_abha         AS text    CHECK (VALUE ~ '^[0-9]{2}-[0-9]{4}-[0-9]{4}-[0-9]{4}$');
CREATE DOMAIN core.d_pan          AS text    CHECK (VALUE ~ '^[A-Z]{5}[0-9]{4}[A-Z]$');
CREATE DOMAIN core.d_gstin        AS text    CHECK (VALUE ~ '^[0-9]{2}[A-Z]{5}[0-9]{4}[A-Z][0-9][A-Z][0-9A-Z]$');
CREATE DOMAIN core.d_pincode_in   AS text    CHECK (VALUE ~ '^[1-9][0-9]{5}$');
CREATE DOMAIN core.d_iso_ccy      AS text    CHECK (VALUE ~ '^[A-Z]{3}$');
CREATE DOMAIN core.d_sigma_level  AS numeric(4,2)  CHECK (VALUE >= 0 AND VALUE <= 6.5);
CREATE DOMAIN core.d_percent      AS numeric(5,2)  CHECK (VALUE >= 0 AND VALUE <= 100);
