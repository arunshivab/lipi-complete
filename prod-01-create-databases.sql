-- ================================================================
-- LiPi HIS — Step 1: Create production databases
-- Run as superuser (postgres) against the PostgreSQL server
-- ================================================================

-- Drop existing databases
DROP DATABASE IF EXISTS lipi_dev;
DROP DATABASE IF EXISTS lipi_master;
DROP DATABASE IF EXISTS lipi_training;

-- Create master database
CREATE DATABASE lipi_master
    ENCODING = 'UTF8'
    LC_COLLATE = 'en_US.UTF-8'
    LC_CTYPE = 'en_US.UTF-8'
    TEMPLATE = template0;

COMMENT ON DATABASE lipi_master IS 'LiPi HIS — Master registry: orgs, clinics, platform users';

-- Create training clinic database
CREATE DATABASE lipi_training
    ENCODING = 'UTF8'
    LC_COLLATE = 'en_US.UTF-8'
    LC_CTYPE = 'en_US.UTF-8'
    TEMPLATE = template0;

COMMENT ON DATABASE lipi_training IS 'LiPi HIS — Training clinic database';
