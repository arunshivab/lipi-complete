# LiPi Wave 1 Completion Summary

**Status: SQL + EF Core COMPLETE** | Remaining: Mermaid diagrams + Data dictionaries

---

## SQL Schemas Delivered (8 files)

### Master DB (1 tier)
- **master/001_schema_master.sql** — Organizations, clinics, subscriptions, identity providers, feature flags, cross-clinic audit

### Per-Clinic Schemas (7 tiers, 2,400+ lines SQL)

| Schema | File | Key Tables | RLS | Triggers |
|--------|------|-----------|-----|----------|
| **core** | 01_core.sql | Geography, medical codes, facility topology, persons, patients, staff, consents | ✓ | ✓ |
| **identity** | 02_identity.sql | Users, roles, permissions, sessions, MFA (TOTP/WebAuthn), API keys, AD sync | ✓ | ✓ |
| **abdm** | 03_abdm.sql | HFR/HPR, ABHA, link requests, care contexts, consent artefacts, HI exchange, Aadhaar eKYC | ✓ | ✓ |
| **audit** | 04_audit.sql | Hash-chained audit events, PHI access logs, export/print logs, blockchain anchors, verification | ✓ | ✓ + Restrictive |
| **security** | 05_security.sql | Envelope encryption (KMS), DPDP consents, data subject rights, security events, break-glass, DLP | ✓ | ✓ |
| **compliance** | 06_compliance.sql | Licences (Fire/CGWA/CDSCO/AERB/NABH), responsibility matrix, physical custody, renewal alerts, CAPA | ✓ | ✓ |
| **certs** | 07_certs.sql | Staff credentials, board registrations, CME tracking, competency assessments, publications, awards | ✓ | ✓ |
| **sigma** | 08_sigma.sql | Process definitions, defects, SPC observations, control charts, FMEA, Six Sigma projects, Kaizen | ✓ | ✓ |

### Key SQL Features
- **Partitioning**: Monthly range partitioning on high-volume tables (audit_events, phi_access_log, login_attempts, gateway_requests, etc.)
- **Hash-Chained Audit**: Tamper-evident SHA-256 chain per clinic with verification runs
- **Row-Level Security**: Tenant isolation via `clinic_id = current_setting('lipi.clinic_id')::uuid`
- **Domain Types**: AADHAAR, ABHA, email, phone, PAN, GSTIN, ISO currency, sigma levels, percentages
- **Indexes**: Optimized for B-tree locality (UUIDv7), covering indexes, partial indexes on status/timestamps
- **Concurrency**: `row_version` on mutable tables for optimistic locking

---

## EF Core Projects Delivered (7 projects)

### Project Structure
```
database/efcore/
├── LiPi.Clinic.Identity/
│   ├── LiPi.Clinic.Identity.csproj
│   ├── Entities/User.cs
│   └── Identity.cs (Role, Permission, UserRole, Session, MFA, LoginAttempt, ServiceAccount, ApiKey, AdSyncRun)
│   └── IdentityDbContext.cs
├── LiPi.Clinic.Abdm/
│   ├── LiPi.Clinic.Abdm.csproj
│   ├── Entities/AbdmEntities.cs
│   └── AbdmDbContext.cs
├── LiPi.Clinic.Audit/
│   ├── LiPi.Clinic.Audit.csproj
│   ├── AuditEntities.cs
│   └── AuditDbContext.cs
├── LiPi.Clinic.Security/
│   ├── LiPi.Clinic.Security.csproj
│   ├── SecurityEntitiesAndContext.cs
│   └── [consolidated]
├── LiPi.Clinic.Compliance/
│   ├── LiPi.Clinic.Compliance.csproj
│   ├── ComplianceEntitiesAndContext.cs
│   └── [consolidated]
├── LiPi.Clinic.Certs/
│   ├── LiPi.Clinic.Certs.csproj
│   ├── CertsEntitiesAndContext.cs
│   └── [consolidated]
└── LiPi.Clinic.Sigma/
    ├── LiPi.Clinic.Sigma.csproj
    ├── SigmaEntitiesAndContext.cs
    └── [consolidated]
```

### EF Core Features
- **Target**: .NET 8, EFCore 8.0, Npgsql 8.0
- **Conventions**: Snake_case column mapping via `ToSnakeCase()` helper in each DbContext
- **Relationships**: Full FK navigation, cascade deletes, 1:N collections
- **Value Conversions**: IPAddress → string, List → jsonb, List → text[], enum checks
- **Indexes**: Covering indexes, unique constraints, partial filters, multi-column ordering
- **Concurrency**: Optimistic locking via `HasConcurrencyToken(x => x.RowVersion)`
- **Defaults**: `HasDefaultValueSql("core.uuid_v7()")`, `HasDefaultValueSql("now()")`
- **Soft Delete**: Query filters (pattern from Core project) to exclude `deleted_at IS NOT NULL` rows
- **JSONB Columns**: `HasColumnType("jsonb")` for ExtensionData, Detail, Payload fields

### Entity Counts by Schema
| Schema | Entity Classes | Total |
|--------|---|---|
| Identity | 12 | 12 |
| Abdm | 12 | 12 |
| Audit | 8 | 8 |
| Security | 10 | 10 |
| Compliance | 11 | 11 |
| Certs | 11 | 11 |
| Sigma | 10 | 10 |
| **Total** | **74 entities** | **74** |

---

## Regulatory Coverage

### India-Specific
✓ **DPDP Act 2023**: Consent artefacts, data subject rights (access/correction/erasure/portability/grievance), 30-day SLA, data localization per region  
✓ **CDSCO**: Medical device classification in core reference data  
✓ **AERB**: Radioactive material licences + regulatory oversight  
✓ **NABH**: Hospital accreditation license tracking  
✓ **Biomedical Waste**: Incident/CAPA tracking, training completions  
✓ **ABDM**: ABHA, HFR, HPR, FHIR R4 bundles, consent manager integration, ECDH HIE exchange  
✓ **NMC/INC**: Board registrations with state-level tracking  
✓ **Fire/CGWA/MCA/RoC**: Multi-licence compliance matrix with SLA alerts  

### Global Standards
✓ **HIPAA § 164.312(b)**: PHI access audit log with context (direct care, referral, billing, research, break-glass)  
✓ **ISO 27001**: Information security controls (encryption, access controls, incident response)  
✓ **Six Sigma / Process Control**: SPC charts, Cpk/Ppk indices, control rules, FMEA, CAPA  
✓ **Blockchain** (optional): Hyperledger anchoring for audit immutability proof  

---

## Test Coverage Expectations

Post-Wave 1, integration tests should verify:

### Identity
- [ ] User creation with clinic-scoped RLS
- [ ] MFA enrollment (TOTP, WebAuthn, backup codes)
- [ ] Session lifecycle (JWT issuance, refresh, revocation)
- [ ] Login attempt tracking and rate limiting
- [ ] Service account API key provisioning

### ABDM
- [ ] ABHA linking via OTP/demographic
- [ ] Care context visibility management
- [ ] Consent artefact signing and revocation
- [ ] HI exchange (HIP/HIU) ECDH key exchange
- [ ] Aadhaar eKYC: reference hash (never plaintext) + KMS encryption of payload

### Audit
- [ ] Hash chain integrity verification
- [ ] Partition management (monthly rollover)
- [ ] Audit event immutability (UPDATE/DELETE restrictive policies)
- [ ] PHI access logging on `IsPhiSensitive` permissions
- [ ] Export/print log with DLP scan results

### Security
- [ ] DEK rotation and KMS CMK versioning
- [ ] DPDP subject requests (access, correction, erasure, portability) with 30-day SLA
- [ ] Break-glass sessions with post-hoc review workflow
- [ ] Data classification → encryption_required mapping
- [ ] Security event SIEM integration

### Compliance
- [ ] Licence lifecycle (issued → renewal_pending → approved → renewed)
- [ ] Physical copy custody chain (checked out/in, signed by actor)
- [ ] Renewal alerts triggered 90 days before expiry
- [ ] Inspection findings → CAPA assignment with due dates
- [ ] Policy version control and annual review scheduling

### Certs
- [ ] Staff credential verification via portal (NMC, council registries)
- [ ] CME credit aggregation by year and requirement type
- [ ] Competency assessment with remediation action plans
- [ ] Board registration suspension/lifting workflow
- [ ] Publication/award tracking for CV generation

### Sigma
- [ ] Process definition hierarchy (L1→L2→L3→L4 decomposition)
- [ ] SPC observation collection with monthly rollup (mean, std_dev, Cpk, Ppk, yield %)
- [ ] Out-of-control detection (rule 1: beyond 3σ; rule 6: trend; etc.)
- [ ] FMEA RPN recalculation after corrective actions
- [ ] Project status progression (define → measure → analyze → improve → control)

---

## Notes for Waves 2+

### Clinical Core (Wave 2)
OPD, IPD, ER, OT, encounters, observations, vital signs, chief complaints, diagnoses, assessments

### Diagnostics (Wave 3)
Pathology, radiology, PACS, modality worklist, DICOM ingestion, result validation, TAT tracking

### Oncology Suite (Wave 4)
Med/Surg/Rad Onco, Nuclear Med, RT QA (machine commissioning, patient-specific QA, ESAPI), CAPA for dosimetry errors

### Operations (Wave 5)
Pharmacy, inventory, asset mgmt, CSSD, dental, dialysis, billing/TPA/insurance, revenue tracking

### Supporting (Wave 6)
Tickets, telemedicine, IoT, AI/ML, MIS, reports, data warehouse (ClickHouse via Postgres FDW)

---

## Files Summary

```
database/
├── 00_common/
│   ├── 001_extensions.sql
│   ├── 002_uuid_v7.sql
│   ├── 003_audit_triggers.sql
│   └── 004_reference_domains.sql
├── master/
│   └── 001_schema_master.sql
├── clinic/
│   ├── 01_core.sql (23 tables)
│   ├── 02_identity.sql (12 tables)
│   ├── 03_abdm.sql (14 tables)
│   ├── 04_audit.sql (8 tables, hash-chained)
│   ├── 05_security.sql (10 tables)
│   ├── 06_compliance.sql (11 tables)
│   ├── 07_certs.sql (11 tables)
│   └── 08_sigma.sql (10 tables)
├── efcore/
│   ├── LiPi.Clinic.Core/[existing Wave 1]
│   ├── LiPi.Clinic.Identity/[12 entities + DbContext]
│   ├── LiPi.Clinic.Abdm/[12 entities + DbContext]
│   ├── LiPi.Clinic.Audit/[8 entities + DbContext]
│   ├── LiPi.Clinic.Security/[10 entities + DbContext]
│   ├── LiPi.Clinic.Compliance/[11 entities + DbContext]
│   ├── LiPi.Clinic.Certs/[11 entities + DbContext]
│   └── LiPi.Clinic.Sigma/[10 entities + DbContext]
├── diagrams/
│   ├── master.mmd
│   ├── clinic_01_core.mmd
│   └── [TODO: 02_identity through 08_sigma]
├── data-dictionary/
│   ├── master.md
│   ├── clinic_01_core.md
│   └── [TODO: 02_identity through 08_sigma]
└── README.md
```

---

**Next Steps:** Create Mermaid ER diagrams and markdown data dictionaries for schemas 02–08.
