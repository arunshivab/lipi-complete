# Database Provisioning Guide

## Overview

This guide covers setting up LiPi databases locally for development and deploying to production environments.

---

## Prerequisites

- **PostgreSQL 16+** (download from [postgresql.org](https://www.postgresql.org/download/))
- **psql CLI** (included with PostgreSQL)
- **Git** (for cloning the repository)
- **AWS CLI** (optional, for production AWS RDS deployments)

### Installation Verification

```bash
# Check PostgreSQL version
psql --version

# Should output: psql (PostgreSQL) 16.x or higher
```

---

## Local Development Setup

### Step 1: Create Development Database

```bash
# Start PostgreSQL service (if not running)
# Windows: Services > PostgreSQL > Start
# macOS: brew services start postgresql
# Linux: sudo systemctl start postgresql

# Create dev database
psql -U postgres -c "CREATE DATABASE lipi_dev ENCODING UTF8;"

# Verify
psql -U postgres -l | grep lipi_dev
```

### Step 2: Provision Schemas (In Order)

Run SQL files in this exact order:

```bash
cd /path/to/claudecode/database

# 1. Common infrastructure (required first)
psql -U postgres -d lipi_dev -f 00_common/001_extensions.sql
psql -U postgres -d lipi_dev -f 00_common/002_uuid_v7.sql
psql -U postgres -d lipi_dev -f 00_common/003_audit_triggers.sql
psql -U postgres -d lipi_dev -f 00_common/004_reference_domains.sql

# 2. Master database (single instance, multi-clinic control plane)
psql -U postgres -d lipi_dev -f master/001_schema_master.sql

# 3. Clinic database (per-clinic OLTP, in dependency order)
psql -U postgres -d lipi_dev -f clinic/01_core.sql
psql -U postgres -d lipi_dev -f clinic/02_identity.sql
psql -U postgres -d lipi_dev -f clinic/03_abdm.sql
psql -U postgres -d lipi_dev -f clinic/04_audit.sql
psql -U postgres -d lipi_dev -f clinic/05_security.sql
psql -U postgres -d lipi_dev -f clinic/06_compliance.sql
psql -U postgres -d lipi_dev -f clinic/07_certs.sql
psql -U postgres -d lipi_dev -f clinic/08_sigma.sql
```

### Step 3: Verify Installation

```bash
# List all schemas
psql -U postgres -d lipi_dev -c "SELECT schema_name FROM information_schema.schemata WHERE schema_name NOT IN ('pg_catalog','information_schema','public') ORDER BY schema_name;"

# Expected output:
# ┌────────────────┐
# │  schema_name   │
# ├────────────────┤
# │ abdm           │
# │ audit          │
# │ certs          │
# │ compliance     │
# │ core           │
# │ identity       │
# │ security       │
# │ sigma          │
# └────────────────┘

# Count total tables
psql -U postgres -d lipi_dev -c "SELECT COUNT(*) as total_tables FROM information_schema.tables WHERE table_schema NOT IN ('pg_catalog','information_schema','public');"
```

### Step 4: Test Row-Level Security (RLS)

```bash
# Set clinic context
psql -U postgres -d lipi_dev -c "SET lipi.clinic_id = 'clinic-uuid-here';"

# Query should return only rows for that clinic
psql -U postgres -d lipi_dev -c "SELECT COUNT(*) FROM identity.users;"
```

---

## EF Core Setup (C# .NET 8)

### Option A: Code-First Migrations (Recommended)

```bash
cd database/efcore/LiPi.Clinic.Identity

# Create initial migration (if not exists)
dotnet ef migrations add InitialCreate

# Apply to database
dotnet ef database update
```

### Option B: Using Existing SQL

```bash
# SQL is already applied; EF Core entities are pre-built
# Just ensure connection string points to correct database

# In appsettings.json:
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=lipi_dev;Username=postgres;Password=yourpassword;"
  }
}

# Test connection
dotnet ef dbcontext info
```

---

## Testing the Setup

### Test 1: Connection String

```bash
# Test psql connection
psql -U postgres -d lipi_dev -c "SELECT version();"
```

### Test 2: Table Creation

```bash
# Verify a few key tables exist
psql -U postgres -d lipi_dev -c "SELECT table_name FROM information_schema.tables WHERE table_schema = 'core' AND table_name IN ('patients','staff','facility') ORDER BY table_name;"
```

### Test 3: RLS Policy Check

```bash
# Verify RLS is enabled on users table
psql -U postgres -d lipi_dev -c "SELECT tablename, (SELECT count(*) FROM pg_policies WHERE tablename=t.tablename) as policy_count FROM pg_tables t WHERE schemaname='identity' AND tablename='users';"
```

### Test 4: Hash-Chain Function

```bash
# Verify audit hash function exists
psql -U postgres -d lipi_dev -c "SELECT proname FROM pg_proc WHERE proname = 'fn_compute_hash';"
```

---

## Production Deployment (AWS RDS)

### Step 1: Create RDS PostgreSQL Instance

```bash
# Using AWS CLI
aws rds create-db-instance \
  --db-instance-identifier lipi-prod-db \
  --db-instance-class db.t3.medium \
  --engine postgres \
  --engine-version 16 \
  --allocated-storage 100 \
  --storage-type gp3 \
  --master-username postgres \
  --master-user-password "STRONG_PASSWORD" \
  --publicly-accessible false \
  --vpc-security-group-ids sg-xxxxx \
  --db-subnet-group-name default
```

### Step 2: Wait for Instance to Be Available

```bash
# Check status
aws rds describe-db-instances --db-instance-identifier lipi-prod-db --query 'DBInstances[0].DBInstanceStatus'

# Wait until: "available"
```

### Step 3: Provision Production Database

```bash
# Set environment variable for RDS endpoint
export PROD_HOST="lipi-prod-db.xxxxx.us-east-1.rds.amazonaws.com"
export PROD_USER="postgres"
export PROD_PASSWORD="STRONG_PASSWORD"

# Run provisioning script
PGPASSWORD=$PROD_PASSWORD ./scripts/provision-db.sh \
  --host $PROD_HOST \
  --username $PROD_USER \
  --database lipi_prod
```

### Step 4: Enable Automated Backups

```bash
# RDS automatic backups (done via console or CLI)
aws rds modify-db-instance \
  --db-instance-identifier lipi-prod-db \
  --backup-retention-period 7 \
  --preferred-backup-window "03:00-04:00"
```

### Step 5: Enable Encryption

```bash
# Enable encryption at rest (KMS)
aws rds modify-db-instance \
  --db-instance-identifier lipi-prod-db \
  --storage-encrypted \
  --kms-key-id arn:aws:kms:region:account:key/keyid
```

---

## CI/CD Pipeline Integration

### GitHub Actions Example

Create `.github/workflows/database-deploy.yml`:

```yaml
name: Database Deployment

on:
  push:
    branches: [ main ]
    paths: [ 'database/**' ]

jobs:
  deploy-staging:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Test schema syntax
        run: |
          pip install sqlparse
          python scripts/validate-sql.py database/
      
      - name: Deploy to staging
        env:
          PGPASSWORD: ${{ secrets.STAGING_DB_PASSWORD }}
        run: |
          ./scripts/provision-db.sh \
            --host ${{ secrets.STAGING_DB_HOST }} \
            --username postgres \
            --database lipi_staging

  deploy-production:
    needs: deploy-staging
    runs-on: ubuntu-latest
    environment: production
    steps:
      - uses: actions/checkout@v3
      
      - name: Deploy to production
        env:
          PGPASSWORD: ${{ secrets.PROD_DB_PASSWORD }}
        run: |
          ./scripts/provision-db.sh \
            --host ${{ secrets.PROD_DB_HOST }} \
            --username postgres \
            --database lipi_prod
      
      - name: Run smoke tests
        run: |
          psql -h ${{ secrets.PROD_DB_HOST }} -U postgres -d lipi_prod -f scripts/smoke-tests.sql
```

---

## Troubleshooting

### Issue: Connection Refused

```bash
# Check if PostgreSQL is running
psql -U postgres -c "SELECT 1;"

# If fails, start PostgreSQL:
# Windows: net start postgresql-x64-16
# macOS: brew services start postgresql
# Linux: sudo systemctl start postgresql
```

### Issue: Permission Denied

```bash
# Verify user exists
psql -U postgres -c "SELECT usename FROM pg_user WHERE usename='postgres';"

# If missing, create:
psql -U postgres -c "CREATE USER postgres WITH CREATEDB CREATEROLE;"
```

### Issue: Duplicate Schema

```bash
# Drop and recreate
psql -U postgres -d lipi_dev -c "DROP SCHEMA IF EXISTS identity CASCADE;"
# Then re-run: psql -U postgres -d lipi_dev -f clinic/02_identity.sql
```

### Issue: Hash Function Not Found

```bash
# Verify UUID v7 function was created
psql -U postgres -d lipi_dev -c "SELECT proname FROM pg_proc WHERE proname ~ 'uuid_v7';"

# If missing, re-run:
psql -U postgres -d lipi_dev -f 00_common/002_uuid_v7.sql
```

---

## Backup & Recovery

### Backup Production Database

```bash
# Full database backup
pg_dump -U postgres -d lipi_prod -F custom -f lipi_prod_backup_$(date +%Y%m%d).dump

# Compress
gzip lipi_prod_backup_*.dump
```

### Restore from Backup

```bash
# Restore to new database
pg_restore -U postgres -d lipi_prod_restore < lipi_prod_backup_20260415.dump
```

---

## Maintenance

### Monitor Database Size

```bash
psql -U postgres -d lipi_dev -c "SELECT pg_database.datname, pg_size_pretty(pg_database_size(pg_database.datname)) FROM pg_database WHERE datname='lipi_dev';"
```

### Vacuum & Analyze (Monthly)

```bash
psql -U postgres -d lipi_prod -c "VACUUM ANALYZE;"
```

### Check Partition Health

```bash
psql -U postgres -d lipi_prod -c "SELECT schemaname, tablename FROM pg_tables WHERE schemaname='audit' AND tablename ~ '_20' ORDER BY tablename;"
```

---

## Next Steps

- [ ] Deploy to development environment
- [ ] Run integration tests with EF Core projects
- [ ] Deploy to staging environment
- [ ] Load test with production data volume
- [ ] Deploy to production
- [ ] Monitor database performance via CloudWatch / pgAdmin

---

**Support:** For issues, refer to `database/README.md` and `database/WAVE_1_COMPLETION.md`.
