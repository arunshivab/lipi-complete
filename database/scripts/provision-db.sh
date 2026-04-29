#!/bin/bash

# LiPi Database Provisioning Script
# Usage: ./provision-db.sh [OPTIONS]
# Options:
#   --database NAME       Database name (default: lipi_dev)
#   --host HOST          PostgreSQL host (default: localhost)
#   --port PORT          PostgreSQL port (default: 5432)
#   --username USER      PostgreSQL user (default: postgres)
#   --password PASS      PostgreSQL password (will prompt if not provided)
#   --verbose            Show detailed output

set -e

# Color codes for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Default values
DB_NAME="lipi_dev"
DB_HOST="localhost"
DB_PORT="5432"
DB_USER="postgres"
DB_PASSWORD=""
VERBOSE=0

# Parse arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        --database)
            DB_NAME="$2"
            shift 2
            ;;
        --host)
            DB_HOST="$2"
            shift 2
            ;;
        --port)
            DB_PORT="$2"
            shift 2
            ;;
        --username)
            DB_USER="$2"
            shift 2
            ;;
        --password)
            DB_PASSWORD="$2"
            shift 2
            ;;
        --verbose)
            VERBOSE=1
            shift
            ;;
        *)
            echo "Unknown option: $1"
            echo "Usage: $0 [--database NAME] [--host HOST] [--port PORT] [--username USER] [--password PASS] [--verbose]"
            exit 1
            ;;
    esac
done

# Get script directory
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

# Function to print messages
log() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

error() {
    echo -e "${RED}[ERROR]${NC} $1" >&2
    exit 1
}

warn() {
    echo -e "${YELLOW}[WARN]${NC} $1"
}

# Function to execute SQL file
execute_sql() {
    local file="$1"
    local description="$2"

    if [ ! -f "$file" ]; then
        error "SQL file not found: $file"
    fi

    log "Applying: $description ($file)"

    if [ -z "$DB_PASSWORD" ]; then
        psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" -f "$file" > /dev/null 2>&1 || error "Failed to apply: $description"
    else
        PGPASSWORD="$DB_PASSWORD" psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" -f "$file" > /dev/null 2>&1 || error "Failed to apply: $description"
    fi
}

# Prompt for password if not provided
if [ -z "$DB_PASSWORD" ]; then
    read -s -p "PostgreSQL password for $DB_USER@$DB_HOST: " DB_PASSWORD
    echo ""
fi

# Verify connection
log "Verifying database connection..."
if [ -z "$DB_PASSWORD" ]; then
    psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" -c "SELECT version();" > /dev/null 2>&1 || error "Failed to connect to database"
else
    PGPASSWORD="$DB_PASSWORD" psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" -c "SELECT version();" > /dev/null 2>&1 || error "Failed to connect to database"
fi
log "Connection successful!"

# Display provisioning plan
echo ""
echo "╔════════════════════════════════════════╗"
echo "║   LiPi Database Provisioning Plan      ║"
echo "╚════════════════════════════════════════╝"
echo "Database:    $DB_NAME"
echo "Host:        $DB_HOST:$DB_PORT"
echo "User:        $DB_USER"
echo "Schemas:     9 (common + master + 8 clinic schemas)"
echo "Tables:      ~150"
echo ""
echo "Provisioning will apply SQL files in order:"
echo "  1. Extensions (pgcrypto, btree_gist, pg_trgm, citext, etc.)"
echo "  2. UUID v7 function (time-ordered UUIDs)"
echo "  3. Audit triggers (created_at, updated_at, row_version)"
echo "  4. Reference domains (email, phone, Aadhaar, etc.)"
echo "  5. Master database (organizations, clinics, subscriptions)"
echo "  6. Core schema (geography, patients, staff, consents)"
echo "  7. Identity schema (users, RBAC, MFA, API keys)"
echo "  8. ABDM schema (ABHA, consent, HI exchange)"
echo "  9. Audit schema (hash-chained tamper-evident audit)"
echo " 10. Security schema (encryption, DPDP, break-glass)"
echo " 11. Compliance schema (licences, CAPA, inspections)"
echo " 12. Certs schema (credentials, CME, certifications)"
echo " 13. Sigma schema (processes, SPC, FMEA, projects)"
echo ""
read -p "Continue? (y/n) " -n 1 -r
echo ""
if [[ ! $REPLY =~ ^[Yy]$ ]]; then
    log "Provisioning cancelled."
    exit 0
fi

echo ""
log "Starting provisioning..."
echo ""

# Apply SQL files in order
execute_sql "$SCRIPT_DIR/00_common/001_extensions.sql" "Extensions (pgcrypto, btree_gist, pg_trgm, citext)"
execute_sql "$SCRIPT_DIR/00_common/002_uuid_v7.sql" "UUID v7 function"
execute_sql "$SCRIPT_DIR/00_common/003_audit_triggers.sql" "Audit triggers"
execute_sql "$SCRIPT_DIR/00_common/004_reference_domains.sql" "Reference domains"
execute_sql "$SCRIPT_DIR/master/001_schema_master.sql" "Master schema (organizations, clinics)"

# Clinic schemas
execute_sql "$SCRIPT_DIR/clinic/01_core.sql" "Core schema (patients, staff, facility)"
execute_sql "$SCRIPT_DIR/clinic/02_identity.sql" "Identity schema (users, RBAC, MFA)"
execute_sql "$SCRIPT_DIR/clinic/03_abdm.sql" "ABDM schema (ABHA, consent, HI exchange)"
execute_sql "$SCRIPT_DIR/clinic/04_audit.sql" "Audit schema (hash-chained audit)"
execute_sql "$SCRIPT_DIR/clinic/05_security.sql" "Security schema (encryption, DPDP)"
execute_sql "$SCRIPT_DIR/clinic/06_compliance.sql" "Compliance schema (licences, CAPA)"
execute_sql "$SCRIPT_DIR/clinic/07_certs.sql" "Certs schema (credentials, CME)"
execute_sql "$SCRIPT_DIR/clinic/08_sigma.sql" "Sigma schema (processes, SPC, FMEA)"

echo ""
log "Provisioning complete!"
echo ""

# Verify schemas
if [ -z "$DB_PASSWORD" ]; then
    SCHEMA_COUNT=$(psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" -t -c "SELECT COUNT(*) FROM information_schema.schemata WHERE schema_name NOT IN ('pg_catalog','information_schema','public');")
else
    SCHEMA_COUNT=$(PGPASSWORD="$DB_PASSWORD" psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" -t -c "SELECT COUNT(*) FROM information_schema.schemata WHERE schema_name NOT IN ('pg_catalog','information_schema','public');")
fi

log "Created $SCHEMA_COUNT schemas"

# List schemas
echo ""
echo "Schemas created:"
if [ -z "$DB_PASSWORD" ]; then
    psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" -c "SELECT schema_name FROM information_schema.schemata WHERE schema_name NOT IN ('pg_catalog','information_schema','public') ORDER BY schema_name;"
else
    PGPASSWORD="$DB_PASSWORD" psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" -c "SELECT schema_name FROM information_schema.schemata WHERE schema_name NOT IN ('pg_catalog','information_schema','public') ORDER BY schema_name;"
fi

echo ""
echo "✅ Database provisioning successful!"
echo ""
echo "Next steps:"
echo "  1. Verify with: psql -U $DB_USER -d $DB_NAME -c \"SELECT COUNT(*) FROM pg_tables WHERE table_schema NOT IN ('pg_catalog','information_schema','public');\""
echo "  2. Set up EF Core: dotnet ef dbcontext info"
echo "  3. Run tests: dotnet test"
echo ""
