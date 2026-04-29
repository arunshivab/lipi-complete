@echo off
echo.
echo === LiPi Dev Database Setup ===
echo.
set /p PGPASS=Enter postgres password: 

set PGPASSWORD=%PGPASS%
set PSQL=psql -h localhost -p 5432 -U postgres

echo.
echo [1/12] Extensions...
%PSQL% -d lipi_dev -f "..\..\database\00_common\001_extensions.sql"
if errorlevel 1 goto :error

echo [2/12] UUID v7...
%PSQL% -d lipi_dev -f "..\..\database\00_common\002_uuid_v7.sql"
if errorlevel 1 goto :error

echo [3/12] Audit triggers...
%PSQL% -d lipi_dev -f "..\..\database\00_common\003_audit_triggers.sql"
if errorlevel 1 goto :error

echo [4/12] Reference domains...
%PSQL% -d lipi_dev -f "..\..\database\00_common\004_reference_domains.sql"
if errorlevel 1 goto :error

echo [5/12] Core schema (patients, staff, geography)...
%PSQL% -d lipi_dev -f "..\..\database\clinic\01_core.sql"
if errorlevel 1 goto :error

echo [6/12] Identity schema (users, RBAC, MFA, sessions)...
%PSQL% -d lipi_dev -f "..\..\database\clinic\02_identity.sql"
if errorlevel 1 goto :error

echo [7/12] ABDM schema...
%PSQL% -d lipi_dev -f "..\..\database\clinic\03_abdm.sql"
if errorlevel 1 goto :error

echo [8/12] Audit schema...
%PSQL% -d lipi_dev -f "..\..\database\clinic\04_audit.sql"
if errorlevel 1 goto :error

echo [9/12] Security schema...
%PSQL% -d lipi_dev -f "..\..\database\clinic\05_security.sql"
if errorlevel 1 goto :error

echo [10/12] Compliance schema...
%PSQL% -d lipi_dev -f "..\..\database\clinic\06_compliance.sql"
if errorlevel 1 goto :error

echo [11/12] Certs schema...
%PSQL% -d lipi_dev -f "..\..\database\clinic\07_certs.sql"
if errorlevel 1 goto :error

echo [12/12] Sigma schema...
%PSQL% -d lipi_dev -f "..\..\database\clinic\08_sigma.sql"
if errorlevel 1 goto :error

echo.
echo Verifying schemas...
%PSQL% -d lipi_dev -c "\dn"

echo.
echo === lipi_dev setup DONE! ===
goto :end

:error
echo.
echo *** ERROR — script stopped. Check the message above. ***

:end
set PGPASSWORD=
pause
