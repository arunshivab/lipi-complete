# LiPi HIS — Documentation Structure

> Read this FIRST. This explains how all spec files are organized.

---

## FOLDER STRUCTURE

```
docs/
├── 00-PROJECT-BASELINE.md          ← READ EVERY SESSION (master spec)
├── CHANGE-LOG.md                   ← Version history
├── README.md                       ← This file
│
├── 00-DATABASE/
│   ├── 00.1-Master-Schema.md       ← Master DB (orgs, clinics, platform users)
│   ├── 00.2-Clinic-Core-Schema.md  ← Per-clinic DB structure
│   └── 00.3-ER-Diagram-Overview.md ← Visual relationships
│
└── [NN]-MODULE-NAME/               ← One folder per module (01-25)
    ├── [NN].1-Design-Specs.md      ← UI design, layout, colors, components
    ├── [NN].2-Pages-Validations.md ← Pages, forms, fields, validation rules
    └── [NN].3-Database-Schema.md   ← Tables, columns, FKs, EF Core entities
```

---

## HOW TO USE

### Before generating ANY code:
1. Open `00-PROJECT-BASELINE.md` — refresh memory on locked decisions
2. Open the relevant module folder (e.g., `04-PATIENT-REGISTRATION/`)
3. Read all 3 files (.1, .2, .3) for that module
4. Show user the pre-code checklist (system prompt enforces this)
5. Wait for confirmation
6. Generate code following specs exactly

### When making changes:
1. **NEVER** edit `00-PROJECT-BASELINE.md` (it's LOCKED)
2. Add changes to `CHANGE-LOG.md` under a new version section
3. Reference specific files/sections changed
4. Get user sign-off before applying

### When starting a new module (currently 7-25 are templates):
1. Copy `MODULE-TEMPLATE/` files to your new folder
2. Fill in module-specific content
3. Update `CHANGE-LOG.md` with new module entry

---

## MODULE LIST (All 25)

| # | Module | Status | Priority |
|---|--------|--------|----------|
| 01 | User Registration | ✅ Built | Done |
| 02 | Clinic Registration | ✅ Built | Done |
| 03 | Organization Registration | ✅ Built | Done |
| 04 | Patient Registration | 🟡 80% | Active |
| 05 | Appointments | 🟡 60% | Active |
| 06 | OPD | 📋 Spec | Next |
| 07 | IPD | 📋 Spec | Next |
| 08 | Radiology | 📋 Spec | Phase 2 |
| 09 | Pharmacy | 📋 Spec | Phase 2 |
| 10 | CSSD | 📋 Spec | Phase 2 |
| 11 | OT | 📋 Spec | Phase 2 |
| 12 | Dental | 📋 Spec | Phase 3 |
| 13 | Lab | 📋 Spec | Phase 2 |
| 14 | Cathlab | 📋 Spec | Phase 3 |
| 15 | Chemotherapy | 📋 Spec | Phase 3 |
| 16 | Radiotherapy | 📋 Spec | Phase 3 |
| 17 | Medical Physics QA | 📋 Spec | Phase 3 |
| 18 | Dialysis | 📋 Spec | Phase 3 |
| 19 | Insurance/TPA | 📋 Spec | Phase 2 |
| 20 | Billing | 📋 Spec | Phase 2 |
| 21 | Asset Management | 📋 Spec | Phase 4 |
| 22 | Compliance Management | 📋 Spec | Phase 4 |
| 23 | Purchase | 📋 Spec | Phase 4 |
| 24 | Ticket Management | 📋 Spec | Phase 4 |
| 25 | PACS | 📋 Spec | Phase 4 |

**Status legend**: ✅ Built · 🟡 In Progress · 📋 Spec Only · ⏸️ Parked

---

## SPEC FILE TEMPLATE

Every module's 3 files follow this structure:

### [N].1-Design-Specs.md
- Page list and routes
- Color theme for module
- Component layout (cards, panels, tables)
- Responsive breakpoints
- Empty states, loading states, error states

### [N].2-Pages-Validations.md
- Each page: form fields, validation rules, error messages
- Field IDs, names, autocomplete values (a11y)
- Save/Cancel button behavior
- Confirmation dialogs (when, what, why)
- Audit events triggered

### [N].3-Database-Schema.md
- New tables added by this module
- Columns, types, constraints
- Foreign keys, indexes
- EF Core entity references
- Soft delete + immutable patterns
- HIPAA encryption notes

---

## REFERENCES

- **Project Baseline**: `00-PROJECT-BASELINE.md`
- **Database Schemas (Technical)**: `../database/data-dictionary/`
- **SQL Migrations**: `../database/clinic/`, `../database/master/`
- **EF Core Entities**: `../database/efcore/`
- **System Prompt**: `../system-prompt.md`
- **CSS Roadmap**: `../css-refactoring-roadmap.md`
- **Test Automation**: `../test-automation-guide.md`
