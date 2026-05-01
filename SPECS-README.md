# LiPi HIS — Complete Spec Package

> **Generated**: May 2, 2026  
> **Version**: v1.0 (BASE — all 11 decisions locked)  
> **Status**: READY FOR INTEGRATION

---

## WHAT'S IN THIS PACKAGE

### 📁 docs/
- `00-PROJECT-BASELINE.md` — Master spec (READ EVERY SESSION)
- `CHANGE-LOG.md` — Version history
- `README.md` — Folder structure guide
- `00-DATABASE/` — Schema documentation (3 files)
- `01-USER-REGISTRATION/` — Built module (full specs)
- `02-CLINIC-REGISTRATION/` through `25-PACS/` — All 25 modules with `.1`/`.2`/`.3` files

### 📄 Top-level Files
- `system-prompt.md` — Spec enforcement engine (add to Claude system prompt)
- `css-refactoring-roadmap.md` — Step-by-step CSS migration plan
- `test-automation-guide.md` — C# + Selenium test framework

---

## INSTALLATION

### Step 1: Copy to Project
```bash
# From this package
cp -r docs/ /your-project/lipi-complete/
cp system-prompt.md /your-project/lipi-complete/
cp css-refactoring-roadmap.md /your-project/lipi-complete/
cp test-automation-guide.md /your-project/lipi-complete/
```

### Step 2: Deprecate Old CLAUDE.md
```bash
cd /your-project/lipi-complete
mv CLAUDE.md CLAUDE.md.deprecated
echo "# DEPRECATED — See docs/00-PROJECT-BASELINE.md" > CLAUDE.md
echo "Original archived as CLAUDE.md.deprecated" >> CLAUDE.md
```

### Step 3: Update Claude Project System Prompt

Copy the contents of `system-prompt.md` into your Claude Project's system prompt.

This will enforce:
- Read specs at start of every session
- Pre-code checklist before generating
- 12 Blazor critical rules
- Post-code verification
- Scope management (BASE vs v1.1+)

---

## HOW TO USE

### Starting a New Session
1. Claude reads `docs/00-PROJECT-BASELINE.md` (auto-triggered by system prompt)
2. Claude asks: "Which module are we working on?"
3. Claude reads that module's `.1`, `.2`, `.3` files
4. Claude shows pre-code checklist
5. You confirm
6. Claude generates code

### Making Changes
- **Within v1.0 scope**: Direct implementation
- **New requirement**: Add to `CHANGE-LOG.md` under v1.1, then implement
- **Spec gap**: Discuss, then update spec file

### Refactoring CSS
Follow `css-refactoring-roadmap.md`:
1. Phase 1: Create `00-baseline.css`
2. Phase 2: Extract per-module CSS
3. Phase 3: Update App.razor
4. Phase 4: Visual regression test
5. Phase 5: Deprecate `admin.css`

### Running Tests
Follow `test-automation-guide.md`:
1. Create `test-runner/` C# project
2. Add Selenium + xUnit + Npgsql
3. Implement spec parsers
4. Run after every code session

---

## 🔒 LOCKED DECISIONS (REFERENCE)

| # | Topic | Decision |
|---|-------|----------|
| 1 | CSS Architecture | 00-baseline.css + per-module CSS |
| 2 | CLAUDE.md | Extracted to baseline + deprecated |
| 3 | Database Docs | Hybrid (database/ + docs/00-DATABASE/) |
| 4 | Module Scope | All 25 modules spec'd |
| 5 | DOB Override | SysAdmin + SiteAdmin only, mark "Overridden" |
| 6 | Duplicate Detection | Aadhaar → ABHA → Name+DOB → Name+DOB+Mobile |
| 7 | Merge Cooling | 24h/7d/30d/30d+ by role |
| 8 | Sched. Coordinator | Standalone role, multiple per clinic |
| 9 | Teleconsult | PARKED (revisit after 10+ modules) |
| 10 | Calling Board | BASE FEATURE + per-module customization (multi-screen) |
| 11 | Waitlist | Manual confirmation |

---

## NEXT STEPS

1. ✅ Review this package contents
2. ✅ Copy to your project
3. ✅ Update Claude system prompt with `system-prompt.md`
4. ✅ Refactor CSS (follow roadmap)
5. ✅ Set up test automation (follow guide)
6. ✅ For each new module: fill in template specs as you build
7. ✅ Add changes to CHANGE-LOG.md (never edit baseline)

---

## TROUBLESHOOTING

### Q: Claude isn't reading specs at start of session
**A**: Verify system-prompt.md content is in your Claude Project system prompt. Should auto-trigger.

### Q: Module 06+ specs look like templates
**A**: Yes! They are. When you start working on module 06 (OPD), fill in:
- `06.1-Design-Specs.md` — Full layout, components
- `06.2-Pages-Validations.md` — Field-by-field rules
- `06.3-Database-Schema.md` — Tables, columns, FKs

Use modules 01-04 as reference (they're filled in).

### Q: How do I update the baseline?
**A**: NEVER edit `00-PROJECT-BASELINE.md` directly. Add changes to `CHANGE-LOG.md` under a new version.

### Q: CSS refactoring is risky — what if I break things?
**A**: Follow roadmap Phase 5. Keep `admin.css.legacy` for 1 month as safety net.

---

## FILE COUNT SUMMARY

```
Total files: 79
├── 00-PROJECT-BASELINE.md (1)
├── CHANGE-LOG.md (1)
├── README.md (1)
├── system-prompt.md (1)
├── css-refactoring-roadmap.md (1)
├── test-automation-guide.md (1)
├── 00-DATABASE/ (3 files)
└── 25 modules × 3 files = 75 files
```

---

## CONTACT / OWNERSHIP

- **Product**: LiPi HIS
- **Brand Owner**: imagiQa
- **First Expected Client**: Armoki
- **Lead Developer**: Arun Shiva
- **Architecture**: Multi-tenant Blazor + PostgreSQL
- **Compliance**: HIPAA + Six Sigma

---

**This package is the SINGLE SOURCE OF TRUTH for LiPi HIS specs.**

Last Updated: May 2, 2026
