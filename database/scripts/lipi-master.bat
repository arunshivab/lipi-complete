@echo off
echo.
echo === LiPi Master Database Setup ===
echo.
set /p PGPASS=Enter postgres password: 

set PGPASSWORD=%PGPASS%
set PSQL=psql -h localhost -p 5432 -U postgres

echo.
echo [1/5] Extensions...
%PSQL% -d lipi_master -f "..\..\database\00_common\001_extensions.sql"
if errorlevel 1 goto :error

echo [2/5] UUID v7...
%PSQL% -d lipi_master -f "..\..\database\00_common\002_uuid_v7.sql"
if errorlevel 1 goto :error

echo [3/5] Audit triggers...
%PSQL% -d lipi_master -f "..\..\database\00_common\003_audit_triggers.sql"
if errorlevel 1 goto :error

echo [4/5] Reference domains...
%PSQL% -d lipi_master -f "..\..\database\00_common\004_reference_domains.sql"
if errorlevel 1 goto :error

echo [5/5] Master schema (orgs, clinics, subscriptions)...
%PSQL% -d lipi_master -f "..\..\database\master\001_schema_master.sql"
if errorlevel 1 goto :error

echo.
echo Verifying schemas...
%PSQL% -d lipi_master -c "\dn"

echo.
echo === lipi_master setup DONE! ===
goto :end

:error
echo.
echo *** ERROR — script stopped. Check the message above. ***

:end
set PGPASSWORD=
pause
