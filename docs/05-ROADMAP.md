# LiPi HIS — Roadmap (LOCKED)

Single source of truth. Updated when phases ship or new phases lock.

---

## Phase 2 — Component Library

| Phase | Status | Components |
|---|---|---|
| 2.1 | ✅ Done | LipiButton family (Button, ButtonSpinner, LucideIcon) |
| 2.2 | ✅ Done | Text input family (TextBox, TextArea, NumberInput, Select, Combobox, MultiSelect, DatePicker, DateRangePicker) |
| 2.5 | ✅ Done | Selection family (Checkbox, CheckboxGroup, Radio, RadioGroup, Toggle) |
| 2.5.5 | ✅ Done | LabelPosition cross-family retrofit (all 13 input components, `--lipi-label-w: 180px`) |
| 2.6.1 | 🚧 Build chat | Layout — Tabs + Alert + Card |
| 2.6.2 | 📝 Specs ready | Overlays — Modal + Drawer + DynamicTabs |
| 2.7 | ⏳ Queued | Feedback (Skeleton, Badge, Spinner, Toast, ValidationSummary) |
| 2.8 | ⏳ Queued | Data Display (Table, List, Pagination, EmptyState) |
| 2.9 | ⏳ Queued | Navigation (Breadcrumb, StepIndicator, ContextMenu) |
| 2.10 | ⏳ Queued | Infrastructure Audit & Decisions (see DEFERRED-ITEMS.md) |

## Phase 3 — Brand identity

| Phase | Status | Components |
|---|---|---|
| 3.0 | ⏳ Queued | Custom LiPi Icons + Spinner |

## Phase 4+ — Clinical modules (after 2.x complete)

| # | Module | Status |
|---|---|---|
| 01 | User Registration | ✅ Built |
| 02 | Clinic Registration | ✅ Built |
| 03 | Organisation Registration | ✅ Built |
| 04 | Patient Registration | 80% — redesign after Phase 2.6.x |
| 05 | Appointments | 60% — redesign after Phase 2.6.x |
| 06 | OPD | Spec only |
| 07 | IPD | Spec only |
| 08 | Radiology | Spec only |
| 09 | Pharmacy | Spec only |
| 10 | CSSD | Spec only |
| 11 | OT | Spec only |
| 12 | Dental | Spec only |
| 13 | Lab | Spec only |
| 14 | Cathlab | Spec only |
| 15 | Chemotherapy | Spec only |
| 16 | Radiotherapy | Spec only |
| 17 | Medical Physics QA | Spec only |
| 18 | Dialysis | Spec only |
| 19 | Insurance / TPA | Spec only |
| 20 | Billing | Spec only |
| 21 | Asset Management | Spec only |
| 22 | Compliance Management | Spec only |
| 23 | Purchase | Spec only |
| 24 | Ticket Management | Spec only |
| 25 | PACS | Spec only |

---

## Active design session (strategic chat)

**Layout architecture redesign** for clinical workstation:
- ✅ TopNav + BottomNav shell **locked** (clinical-ergonomic validation)
- ⏳ Patient banner placement (Question A — 3 options shown)
- ⏳ Universal search position (Question B — 3 options shown)
- ⏳ Tab strip empty-state behaviour (Question C — 3 options shown)
- ⏳ BottomNav module display (Question D — 3 options shown)
- ⏳ Status indicators (Question E — 3 options shown)
- ⏳ Patient context three-layer architecture (Banner / Drawer / Page) — user reviewing

Next decisions awaited from user.
