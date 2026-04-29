@echo off
echo.
echo === LiPi Master Database Setup ===
echo.

REM ── Find psql automatically ──────────────────────────────────────────
set PSQL_EXE=

REM Check common PostgreSQL install locations (versions 13 through 17)
for %%V in (17 16 15 14 13) do (
    if exist "C:\Program Files\PostgreSQL\%%V\bin\psql.exe" (
        set PSQL_EXE=C:\Program Files\PostgreSQL\%%V\bin\psql.exe
    )
)

REM Also check Program Files (x86)
for %%V in (17 16 15 14 13) do (
    if exist "C:\Program Files (x86)\PostgreSQL\%%V\bin\psql.exe" (
        if not defined PSQL_EXE set PSQL_EXE=C:\Program Files (x86)\PostgreSQL\%%V\bin\psql.exe
    )
)

if not defined PSQL_EXE (
    echo.
    echo *** Could not find psql.exe automatically. ***
    echo.
    echo Please type the full path to your psql.exe, for example:
    echo   C:\Program Files\PostgreSQL\16\bin\psql.exe
    echo.
    set /p PSQL_EXE=Path to psql.exe: 
)

echo Found psql at: %PSQL_EXE%
echo.
set /p PGPASS=Enter postgres password: 

set PGPASSWORD=%PGPASS%
set PSQL="%PSQL_EXE%" -h localhost -p 5432 -U postgres

REM ── Work out where the database folder is (2 levels up from scripts\) ─
pushd ..\..
set DB_DIR=%CD%\database
popd

echo.
echo Database folder: %DB_DIR%
echo.

echo [1/5] Extensions...
%PSQL% -d lipi_master -f "%DB_DIR%\00_common\001_extensions.sql"
if errorlevel 1 goto :error

echo [2/5] UUID v7...
%PSQL% -d lipi_master -f "%DB_DIR%\00_common\002_uuid_v7.sql"
if errorlevel 1 goto :error

echo [3/5] Audit triggers...
%PSQL% -d lipi_master -f "%DB_DIR%\00_common\003_audit_triggers.sql"
if errorlevel 1 goto :error

echo [4/5] Reference domains...
%PSQL% -d lipi_master -f "%DB_DIR%\00_common\004_reference_domains.sql"
if errorlevel 1 goto :error

echo [5/5] Master schema...
%PSQL% -d lipi_master -f "%DB_DIR%\master\001_schema_master.sql"
if errorlevel 1 goto :error

echo.
echo Verifying schemas created...
%PSQL% -d lipi_master -c "\dn"

echo.
echo ==========================================
echo   lipi_master setup COMPLETE!
echo ==========================================
goto :end

:error
echo.
echo *** ERROR — script stopped. See message above. ***

:end
set PGPASSWORD=
pause
