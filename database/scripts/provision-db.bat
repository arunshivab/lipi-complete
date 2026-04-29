@echo off
REM LiPi Database Provisioning Script for Windows
REM Usage: provision-db.bat [database] [host] [port] [username]
REM Example: provision-db.bat lipi_dev localhost 5432 postgres

setlocal enabledelayedexpansion

REM Default values
set DB_NAME=%1
if "%DB_NAME%"=="" set DB_NAME=lipi_dev

set DB_HOST=%2
if "%DB_HOST%"=="" set DB_HOST=localhost

set DB_PORT=%3
if "%DB_PORT%"=="" set DB_PORT=5432

set DB_USER=%4
if "%DB_USER%"=="" set DB_USER=postgres

REM Get script directory
cd /d "%~dp0\.."
set SCRIPT_DIR=%cd%

echo.
echo ╔════════════════════════════════════════╗
echo ║   LiPi Database Provisioning Plan      ║
echo ╚════════════════════════════════════════╝
echo Database:    %DB_NAME%
echo Host:        %DB_HOST%:%DB_PORT%
echo User:        %DB_USER%
echo Schemas:     9 (common + master + 8 clinic schemas)
echo Tables:      ~150
echo.
echo Provisioning will apply SQL files in order:
echo   1. Extensions
echo   2. UUID v7 function
echo   3. Audit triggers
echo   4. Reference domains
echo   5. Master database
echo   6-13. Clinic schemas (core through sigma)
echo.
set /p CONTINUE="Continue? (y/n) "
if /i not "%CONTINUE%"=="y" (
    echo Provisioning cancelled.
    exit /b 0
)

echo.
echo [INFO] Starting provisioning...
echo.

REM Function to execute SQL file
setlocal
for %%F in (
    "00_common/001_extensions.sql:Extensions (pgcrypto, btree_gist, pg_trgm, citext)"
    "00_common/002_uuid_v7.sql:UUID v7 function"
    "00_common/003_audit_triggers.sql:Audit triggers"
    "00_common/004_reference_domains.sql:Reference domains"
    "master/001_schema_master.sql:Master schema (organizations, clinics)"
    "clinic/01_core.sql:Core schema (patients, staff, facility)"
    "clinic/02_identity.sql:Identity schema (users, RBAC, MFA)"
    "clinic/03_abdm.sql:ABDM schema (ABHA, consent, HI exchange)"
    "clinic/04_audit.sql:Audit schema (hash-chained audit)"
    "clinic/05_security.sql:Security schema (encryption, DPDP)"
    "clinic/06_compliance.sql:Compliance schema (licences, CAPA)"
    "clinic/07_certs.sql:Certs schema (credentials, CME)"
    "clinic/08_sigma.sql:Sigma schema (processes, SPC, FMEA)"
) do (
    for /f "tokens=1,2 delims=:" %%A in ("%%F") do (
        set FILE=!SCRIPT_DIR!\%%A
        set DESC=%%B

        if exist "!FILE!" (
            echo [INFO] Applying: !DESC! (!FILE!)
            REM Using PGPASSWORD environment variable if set
            psql -h %DB_HOST% -p %DB_PORT% -U %DB_USER% -d %DB_NAME% -f "!FILE!" >nul 2>&1
            if !errorlevel! neq 0 (
                echo [ERROR] Failed to apply: !DESC!
                exit /b 1
            )
        ) else (
            echo [ERROR] SQL file not found: !FILE!
            exit /b 1
        )
    )
)

echo.
echo [INFO] Provisioning complete!
echo.

REM Verify schemas
echo Schemas created:
psql -h %DB_HOST% -p %DB_PORT% -U %DB_USER% -d %DB_NAME% -c "SELECT schema_name FROM information_schema.schemata WHERE schema_name NOT IN ('pg_catalog','information_schema','public') ORDER BY schema_name;"

if !errorlevel! neq 0 (
    echo [ERROR] Failed to verify schemas
    exit /b 1
)

echo.
echo ✅ Database provisioning successful!
echo.
echo Next steps:
echo   1. Verify with: psql -U %DB_USER% -d %DB_NAME% -c "SELECT COUNT(*) FROM pg_tables WHERE table_schema NOT IN ('pg_catalog','information_schema','public');"
echo   2. Set up EF Core: dotnet ef dbcontext info
echo   3. Run tests: dotnet test
echo.

endlocal
