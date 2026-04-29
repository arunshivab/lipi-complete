# LiPi Armoki HIS — Complete Setup & Run Guide

## What This Is

LiPi is a **Blazor Server SSR** healthcare information system built on:
- **.NET 10** (Blazor Server, ASP.NET Core)
- **PostgreSQL 16** (two databases: master registry + clinic identity)
- **Entity Framework Core 8** with Npgsql

---

## Prerequisites — Install These First

| Tool | Version | Download |
|------|---------|----------|
| .NET SDK | 10.0+ | https://dotnet.microsoft.com/download |
| PostgreSQL | 16+ | https://www.postgresql.org/download |
| Git | any | https://git-scm.com |

> **Verify installed:** Open a terminal and run:
> ```bash
> dotnet --version   # should say 10.x.x
> psql --version     # should say 16.x
> ```

---

## Step 1 — Set Up PostgreSQL Databases

LiPi uses **two databases**:
- `lipi_master` — organizations, clinics, global users (the registry)
- `lipi_dev` — per-clinic identity, users, roles (for development, one DB is fine)

### 1a. Create the databases

Open a terminal and connect to PostgreSQL as the postgres superuser:

```bash
psql -U postgres
```

Then run:

```sql
-- Create a dedicated app user
CREATE USER lipi_app WITH PASSWORD 'changeme123';

-- Create the two databases
CREATE DATABASE lipi_master  OWNER lipi_app;
CREATE DATABASE lipi_dev     OWNER lipi_app;

-- Grant privileges
GRANT ALL PRIVILEGES ON DATABASE lipi_master TO lipi_app;
GRANT ALL PRIVILEGES ON DATABASE lipi_dev    TO lipi_app;

\q
```

> **For local dev**, you can use a single database — just set both connection strings  
> in `appsettings.Development.json` to point to the same DB.

---

### 1b. Run the SQL schema scripts

The SQL files are in `database/`. Run them in this order:

**Common extensions (run once, on both databases):**
```bash
psql -U lipi_app -d lipi_master -f database/00_common/001_extensions.sql
psql -U lipi_app -d lipi_master -f database/00_common/002_uuid_v7.sql
psql -U lipi_app -d lipi_master -f database/00_common/003_audit_triggers.sql
psql -U lipi_app -d lipi_master -f database/00_common/004_reference_domains.sql

psql -U lipi_app -d lipi_dev    -f database/00_common/001_extensions.sql
psql -U lipi_app -d lipi_dev    -f database/00_common/002_uuid_v7.sql
psql -U lipi_app -d lipi_dev    -f database/00_common/003_audit_triggers.sql
psql -U lipi_app -d lipi_dev    -f database/00_common/004_reference_domains.sql
```

**Master registry schema:**
```bash
psql -U lipi_app -d lipi_master -f database/master/001_schema_master.sql
```

**Clinic schemas (run on lipi_dev):**
```bash
psql -U lipi_app -d lipi_dev -f database/clinic/01_core.sql
psql -U lipi_app -d lipi_dev -f database/clinic/02_identity.sql
psql -U lipi_app -d lipi_dev -f database/clinic/03_abdm.sql
psql -U lipi_app -d lipi_dev -f database/clinic/04_audit.sql
psql -U lipi_app -d lipi_dev -f database/clinic/05_security.sql
psql -U lipi_app -d lipi_dev -f database/clinic/06_compliance.sql
psql -U lipi_app -d lipi_dev -f database/clinic/07_certs.sql
psql -U lipi_app -d lipi_dev -f database/clinic/08_sigma.sql
```

---

### 1c. Seed initial data

You need at least one Organization and one Clinic in the master DB before the app can work.

```bash
psql -U lipi_app -d lipi_master
```

```sql
-- Insert a training organization
INSERT INTO master.organizations
  (id, legal_name, org_type, country_code, registered_address, primary_contact,
   status, onboarded_at, extension_data, created_at, updated_at, row_version)
VALUES
  ('00000000-0000-0000-0000-000000000001',
   'Training Organization', 'training', 'IN',
   '{"line1":"Training","city":"Mumbai","state":"Maharashtra","pinCode":"400001","country":"India"}',
   '{"name":"Admin","phone":"9999999999","email":"admin@lipi.local"}',
   'active', now(), '{}', now(), now(), 1);

-- Insert the training clinic
INSERT INTO master.clinics
  (id, organization_id, code, name, clinic_type, city, state, country_code,
   address, timezone, status, extension_data, created_at, updated_at, row_version)
VALUES
  ('00000000-0000-0000-0000-000000000002',
   '00000000-0000-0000-0000-000000000001',
   'training', 'Training', 'training', 'Mumbai', 'Maharashtra', 'IN',
   '{"line1":"Training Facility"}', 'Asia/Kolkata',
   'active', '{}', now(), now(), 1);
```

---

### 1d. Seed a first Admin role and user (so you can log in)

```bash
psql -U lipi_app -d lipi_dev
```

```sql
-- Create the global_admin role for the training clinic
INSERT INTO identity.roles
  (id, clinic_id, code, name, is_system_role, is_active, created_at, updated_at, row_version)
VALUES
  (gen_random_uuid(),
   '00000000-0000-0000-0000-000000000002',
   'global_admin', 'Global Admin', true, true, now(), now(), 1);

-- The first Admin user will be created through the UI once the app is running,
-- OR use demo mode (DemoMode:Enabled = true in appsettings.Development.json)
-- to log in first, then use the Register User form.
```

---

## Step 2 — Configure Connection Strings

Edit `src/LiPi.Web/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "IdentityConnection": "Host=localhost;Port=5432;Database=lipi_dev;Username=lipi_app;Password=changeme123;",
    "MasterConnection":   "Host=localhost;Port=5432;Database=lipi_master;Username=lipi_app;Password=changeme123;"
  },
  "DemoMode": {
    "Enabled": true,
    "Credentials": {
      "Admin":     "Admin",
      "SysAdmin":  "SysAdmin",
      "SiteAdmin": "SiteAdmin"
    }
  }
}
```

> **Important:** For local dev you can point both connection strings at the same database  
> if you ran both schemas (`master` + `clinic`) in one DB.

---

## Step 3 — Restore NuGet Packages

```bash
cd src/LiPi.Web
dotnet restore
```

This will download:
- `Isopoh.Cryptography.Argon2` — password hashing
- `Npgsql.EntityFrameworkCore.PostgreSQL` — PostgreSQL driver
- All other dependencies

---

## Step 4 — Build the Project

```bash
dotnet build
```

You should see **Build succeeded** with 0 errors.

---

## Step 5 — Run the App

```bash
dotnet run
```

The terminal will show something like:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
```

Open your browser at **http://localhost:5000**

---

## Step 6 — First Login

With `DemoMode:Enabled = true` (development only):

| Username | Password | Role |
|----------|----------|------|
| `Admin` | `Admin` | Sys Admin |
| `SysAdmin` | `SysAdmin` | Sys Admin |
| `SiteAdmin` | `SiteAdmin` | Site Admin |

Select **"Training"** from the clinic dropdown.

---

## Step 7 — Bootstrap Your Data (in the UI)

Once logged in as Admin, go to **Administration** and do this in order:

1. **Register Organization** (`/admin/orgs/new`)  
   → Create your hospital group (e.g., "Armoki Healthcare Pvt Ltd")

2. **Register Clinic** (`/admin/clinics/new`)  
   → Select the organization you just created  
   → Fill in address, phone, type  
   → Submit — status will be "provisioning"

3. **Register Users** (`/admin/users/new`)  
   → Select the clinic from the dropdown (loaded from DB)  
   → Fill in all details, select qualifications, set password  
   → Assign roles

---

## Project Folder Structure

```
claudecode/
├── database/
│   ├── 00_common/           ← Extensions, UUID v7, audit triggers
│   │   ├── 001_extensions.sql
│   │   ├── 002_uuid_v7.sql
│   │   ├── 003_audit_triggers.sql
│   │   └── 004_reference_domains.sql
│   ├── clinic/              ← Per-clinic schema (identity, core, ABDM, etc.)
│   │   ├── 01_core.sql
│   │   ├── 02_identity.sql
│   │   └── … (08 files total)
│   ├── master/
│   │   └── 001_schema_master.sql   ← Organizations, clinics, global users
│   ├── efcore/              ← EF Core entity projects
│   │   ├── LiPi.Clinic.Identity/   ← Users, Roles, Sessions
│   │   ├── LiPi.Clinic.Core/       ← Patients, Staff
│   │   ├── LiPi.Clinic.Abdm/       ← ABDM integration
│   │   ├── LiPi.Clinic.Audit/
│   │   ├── LiPi.Clinic.Security/
│   │   ├── LiPi.Clinic.Compliance/
│   │   ├── LiPi.Clinic.Certs/
│   │   ├── LiPi.Clinic.Sigma/
│   │   └── LiPi.Master/            ← Orgs, Clinics, Subscriptions
│   └── scripts/
│       ├── provision-db.sh          ← Linux/Mac one-shot setup
│       └── provision-db.bat         ← Windows one-shot setup
│
└── src/
    └── LiPi.Web/            ← The Blazor application
        ├── App.razor
        ├── Program.cs        ← DI setup, auth config
        ├── LiPi.Web.csproj   ← NuGet packages + project refs
        ├── appsettings.json              ← Production config (DemoMode OFF)
        ├── appsettings.Development.json  ← Dev config  (DemoMode ON)
        ├── Components/
        │   ├── Layouts/
        │   │   ├── TopNavLayout.razor   ← Main app layout
        │   │   └── AdminLayout.razor    ← Admin sidebar layout
        │   ├── Machines/                ← Login page SVG animations
        │   └── …
        ├── Pages/
        │   ├── Login.razor              ← Sign in page (loads clinics from DB)
        │   ├── Dashboard.razor
        │   ├── Module.razor             ← Clinical module placeholder
        │   ├── Admin/
        │   │   ├── Orgs.razor           ← Org list (from DB)
        │   │   ├── OrgsNew.razor        ← Register org → saves to master DB
        │   │   ├── Clinics.razor        ← Clinic list (from DB)
        │   │   ├── ClinicsNew.razor     ← Register clinic → saves to master DB
        │   │   ├── Users.razor          ← User list (from identity DB)
        │   │   ├── UsersNew.razor       ← Register user → saves to identity DB
        │   │   ├── Roles.razor          ← Role/permission matrix
        │   │   ├── Audit.razor          ← Audit log placeholder
        │   │   └── Settings.razor       ← Settings placeholder
        │   └── Patients/
        │       └── Register.razor       ← Patient registration
        ├── Services/
        │   ├── AuthService.cs           ← Argon2 login, demo mode from config
        │   ├── IAuthService.cs
        │   └── AdminData.cs             ← UI lookup data (no hardcoded IDs)
        └── wwwroot/
            ├── css/
            │   ├── app.css
            │   ├── admin.css
            │   └── dashboard.css
            └── js/
                ├── lipi-login.js
                └── lipi-topnav.js
```

---

## Common Problems & Fixes

### "relation does not exist"
The SQL schema hasn't been run. Go back to Step 1b and run the SQL files.

### "password authentication failed for user lipi_app"
Wrong password in the connection string. Edit `appsettings.Development.json` to match what you set in Step 1a.

### "Host can't be resolved" / connection refused
PostgreSQL is not running. Start it:
- **Windows:** Start > Services > PostgreSQL
- **Mac (Homebrew):** `brew services start postgresql@16`
- **Linux:** `sudo systemctl start postgresql`

### Build error: "type or namespace 'LiPi.Master' not found"
Run `dotnet restore` to pull the project references.

### Login page shows "Loading…" forever in clinic dropdown
The master DB is reachable but has no clinics yet. Insert the seed data from Step 1c.

### Demo users work but DB users can't log in
Make sure the user's `password_hash` column contains an **Argon2id** hash (starts with `$argon2id$`). Passwords created through the Register User form are hashed automatically.

---

## Production Checklist (Before Go-Live)

- [ ] Set `DemoMode:Enabled = false` in `appsettings.json`
- [ ] Change all passwords in `appsettings.Development.json` (never deploy this file)
- [ ] Set `ASPNETCORE_ENVIRONMENT=Production` on the server
- [ ] Use a strong DB password and a dedicated app user
- [ ] Enable HTTPS — set `CookieSecurePolicy.Always` in Program.cs
- [ ] Verify session timeout is 30 min (already set in Program.cs)
- [ ] Add rate limiting (see LiPi_Code_Fixes_Guide.md — Fix #6)
- [ ] Replace the simple CAPTCHA with reCAPTCHA v3 (Fix #4)
- [ ] Run the smoke tests: `psql -f database/scripts/smoke-tests.sql`

---

## Quick-Start Script (Linux / Mac)

If PostgreSQL is already running with a `postgres` superuser:

```bash
# From the project root
chmod +x database/scripts/provision-db.sh
./database/scripts/provision-db.sh

cd src/LiPi.Web
dotnet restore
dotnet run
```

Then open http://localhost:5000 and log in with `Admin` / `Admin` on `training`.
