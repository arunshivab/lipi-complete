-- =====================================================================
-- LiPi :: 00_common :: 002_uuid_v7
-- UUIDv7 generator (48-bit Unix-ms timestamp + 74 random bits + version/variant).
-- Time-ordered UUIDs keep B-tree indexes compact and reduce WAL churn on high-volume inserts.
-- =====================================================================

CREATE SCHEMA IF NOT EXISTS core;

CREATE OR REPLACE FUNCTION core.uuid_v7()
RETURNS uuid
LANGUAGE plpgsql
VOLATILE
AS $$
DECLARE
    v_time_ms  bigint;
    v_rand     bytea;
    v_bytes    bytea;
BEGIN
    v_time_ms := (extract(epoch from clock_timestamp()) * 1000)::bigint;
    v_rand    := gen_random_bytes(10);

    -- 6 bytes timestamp (big-endian ms) || 10 bytes random
    v_bytes := set_byte(
                set_byte(
                 set_byte(
                  set_byte(
                   set_byte(
                    set_byte(v_rand, 0, ((v_time_ms >> 40) & 255)::int),
                            0, ((v_time_ms >> 40) & 255)::int),
                           0, ((v_time_ms >> 40) & 255)::int),
                          0, ((v_time_ms >> 40) & 255)::int),
                         0, ((v_time_ms >> 40) & 255)::int),
                        0, ((v_time_ms >> 40) & 255)::int);

    -- clearer re-build: allocate 16 bytes explicitly
    v_bytes := decode(lpad(to_hex(v_time_ms), 12, '0') || encode(v_rand, 'hex'), 'hex');

    -- version = 7  (top 4 bits of byte 6)
    v_bytes := set_byte(v_bytes, 6, ((get_byte(v_bytes, 6) & 15) | 112));
    -- variant = RFC 4122 (top 2 bits of byte 8 = 10)
    v_bytes := set_byte(v_bytes, 8, ((get_byte(v_bytes, 8) & 63) | 128));

    RETURN encode(v_bytes, 'hex')::uuid;
END;
$$;

COMMENT ON FUNCTION core.uuid_v7() IS
  'RFC 9562 UUIDv7. Time-ordered; use as DEFAULT for every primary key across LiPi.';
