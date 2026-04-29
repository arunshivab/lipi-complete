@echo off
echo.
echo === LiPi Dev Database Setup ===
echo.

REM ── Find psql automatically ──────────────────────────────────────────
set PSQL_EXE=

for %%V in (17 16 15 14 13) do (
    if exist "C:\Program Files\PostgreSQL\%%V\bin\psql.exe" (
        set PSQL_EXE=C:\Program Files\PostgreSQL\%%V\bin\psql.exe
    )
)

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

pushd ..\..
set DB_DIR=%CD%\database
popd

echo.
echo Database folder: %DB_DIR%
echo.

echo [1/12] Extensions...
%PSQL% -d lipi_dev -f "%DB_DIR%\00_common\001_extensions.sql"
if errorlevel 1 goto :error

echo [2/12] UUID v7...
%PSQL% -d lipi_dev -f "%DB_DIR%\00_common\002_uuid_v7.sql"
if errorlevel 1 goto :error

echo [3/12] Audit triggers...
%PSQL% -d lipi_dev -f "%DB_DIR%\00_common\003_audit_triggers.sql"
if errorlevel 1 goto :error

echo [4/12] Reference domains...
%PSQL% -d lipi_dev -f "%DB_DIR%\00_common\004_reference_domains.sql"
if errorlevel 1 goto :error

echo [5/12] Core schema...
%PSQL% -d lipi_dev -f "%DB_DIR%\clinic\01_core.sql"
if errorlevel 1 goto :error

echo [6/12] Identity schema...
%PSQL% -d lipi_dev -f "%DB_DIR%\clinic\02_identity.sql"
if errorlevel 1 goto :error

echo [7/12] ABDM schema...
%PSQL% -d lipi_dev -f "%DB_DIR%\clinic\03_abdm.sql"
if errorlevel 1 goto :error

echo [8/12] Audit schema...
%PSQL% -d lipi_dev -f "%DB_DIR%\clinic\04_audit.sql"
if errorlevel 1 goto :error

echo [9/12] Security schema...
%PSQL% -d lipi_dev -f "%DB_DIR%\clinic\05_security.sql"
if errorlevel 1 goto :error

echo [10/12] Compliance schema...
%PSQL% -d lipi_dev -f "%DB_DIR%\clinic\06_compliance.sql"
if errorlevel 1 goto :error

echo [11/12] Certs schema...
%PSQL% -d lipi_dev -f "%DB_DIR%\clinic\07_certs.sql"
if errorlevel 1 goto :error

echo [12/12] Sigma schema...
%PSQL% -d lipi_dev -f "%DB_DIR%\clinic\08_sigma.sql"
if errorlevel 1 goto :error

echo.
echo Verifying schemas created...
%PSQL% -d lipi_dev -c "\dn"

echo.
echo ==========================================
echo   lipi_dev setup COMPLETE!
echo ==========================================
goto :end

:error
echo.
echo *** ERROR — script stopped. See message above. ***

:end
set PGPASSWORD=
pause
