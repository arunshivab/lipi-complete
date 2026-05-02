# LiPi HIS — CHANGE LOG

> **Purpose**: Track all design/spec changes after v1.0 launch.  
> **Rule**: v1.0 decisions in `00-PROJECT-BASELINE.md` are LOCKED. New changes go HERE.

---

## v1.0 — BASE (May 2, 2026) 🔒 LOCKED

All 12 design decisions finalized. See `00-PROJECT-BASELINE.md`.

### Locked Decisions
1. CSS Architecture: 00-baseline.css + per-module CSS + theme files
2. CLAUDE.md: Extracted to 00-PROJECT-BASELINE.md, deprecated
3. Database Docs: Hybrid (database/ + docs/00-DATABASE/)
4. Module Scope: All 25 modules
5. DOB Confidence: SysAdmin + SiteAdmin can override Verified
6. Duplicate Detection: Aadhaar → ABHA → Name+DOB → Name+DOB+Mobile
7. Record Merge Cooling: 24h/7d/30d/30d+ by role
8. Scheduling Coordinator: Standalone role, multiple per clinic
9. Teleconsult: PARKED (revisit after 10+ modules)
10. Public Calling Board: BASE FEATURE + per-module customization, multi-screen
11. Waitlist Confirmation: Manual
12. **Component Library + Multi-Theme** (added May 2, 2026):
    - Custom Razor components with `Lipi` prefix
    - 41 components total for v1.0 (P0: 15, P1: 15, P2: 11)
    - Two-dimensional theme system (brand × mode)
    - v1.0 ships with: 1 brand (`lipi-default`) × 2 modes (light + dark)
    - Default mode for new users: Light
    - Apple HIG density baseline
    - `/admin/style-guide` page as living showcase

### Production-Ready Modules (v1.0)
- ✅ Admin (Users, Clinics, Orgs, Settings)
- ✅ Authentication (Login, OTP, Password Reset)
- 🟡 Patient Registration (80%)
- 🟡 Appointments (60%)

### Component Library Status (v1.0)
- 📋 Architecture Specs: In progress (Decision #12)
- 📋 41 Razor Components: Build phase
- 📋 Style Guide Page: Build phase
- 📋 Theme System: Build phase
- 📋 Existing pages refactored to use Lipi components: Pending

---

## v1.1 — PLANNED (Future)

### Pending Items (Move from PARKED → v1.1)
- [ ] Teleconsult feature (after 10+ modules complete)
- [ ] Auto patient identifier verification (Aadhaar, ABHA via DigiLocker)
- [ ] Real-time bed management (IPD)
- [ ] PACS integration (Radiology)
- [ ] **Armoki brand theme** (after Armoki finalizes brand identity)
- [ ] **Auto theme mode** (follows OS preference)
- [ ] **High-contrast theme mode** (accessibility)
- [ ] **Density toggle** (user preference: comfortable/compact/spacious)

### Pending Decisions (Not Locked)
- [ ] Insurance TPA workflows (decision pending)
- [ ] Multi-language support priority (Hindi/Tamil/Marathi/etc.)
- [ ] Mobile app strategy (native/PWA/responsive only)

---

## CHANGE TEMPLATE (Use for new entries)

```markdown
## v1.X — [DATE]

### Changed
- [Module: NN-Name] What changed and why
- Refer to: docs/[N]-MODULE/[N].1-Design-Specs.md (sections updated)

### Added
- [Module: NN-Name] New feature

### Deprecated
- [Module: NN-Name] What's being phased out

### Removed
- [Module: NN-Name] Removed feature/spec
```

---

## RULES FOR EDITING THIS FILE

1. ✅ NEVER edit v1.0 entries — they are locked
2. ✅ All changes go to a NEW version section (v1.1, v1.2, etc.)
3. ✅ Reference specific files/sections that changed
4. ✅ Date every entry
5. ✅ Get sign-off from Arun (project owner) before adding entries
