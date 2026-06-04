# Standing Rule Addition — Library Dependency Posture

**File:** `docs/02-STANDING-RULES.md`
**Action:** Insert new section after existing "GitHub Actions rules" section
**Triggered by:** Phase 2.8 housekeeping
**Reason:** Formalize the "build in-house, not external" posture that's been implicit in operational rules and visible in deferred items, but never stated as a hard standing rule

---

## Insert this new section

```markdown
---

## Library dependency posture (MANDATORY)

LiPi builds in-house for any functional surface that is bounded and replaceable.
External libraries are reserved for infrastructure that is effectively unique
to the platform.

### Allowed (infrastructure)
- ASP.NET Core, EF Core, Npgsql, runtime BCL
- Microsoft.AspNetCore.* (Identity, Authentication, Authorization, Components.Web)
- Microsoft.EntityFrameworkCore.* (and Npgsql provider)
- (Anything required to run .NET 10 on PostgreSQL with Blazor InteractiveServer)

### Build in-house (functional)
- PDF generation, Excel/spreadsheet writers, file format encoders/decoders
- Email service abstractions (LipiMail), HTTP/API client wrappers (LipiHttp, LipiApi)
- Charting, diagramming, visualization
- Custom UI primitives (the entire `Components/Shared/Lipi*` family)
- Cryptography helpers where BCL is sufficient
- Date/time formatting, CSV writers, URL builders, validators

### Currently under review (Phase 2.10 audit queue)
External functional libraries pre-existing in `LiPi.Web.csproj` and queued for
in-house replacement consideration:
- `MailKit` (SMTP) → replace with LipiMail when first structured-email module ships
- `Microsoft.AspNetCore.Authentication.JwtBearer` → confirm scope or remove if API auth approach changes
- `SharpZipLib` (password-protected ZIP for Aadhaar offline XML) → kept; no BCL equivalent supports password-protected ZIPs. Revisit if PKZIP 2.0 in-house implementation is justified.
- `Isopoh.Cryptography.Argon2` (password hashing) → kept; Argon2 not in BCL. Revisit only if BCL adds Argon2 or compliance audit prompts.

### When new functionality requires a library
1. Strategic chat raises the option
2. Presents the standard external libraries with trade-offs
3. Confirms with the user before any new dependency is added
4. Default answer is "build in-house" unless the user explicitly approves the
   external dependency for a documented reason
5. Every approved external library is logged in `03-DEFERRED-ITEMS.md` under
   "NuGet packages to review" for Phase 2.10 audit

### When in-house build is deferred
If the in-house implementation isn't ready when a feature needs the capability:
- Spec the feature's API surface (so the call site is library-agnostic)
- Stub the implementation with a clear "pending [library-name]" exception
- Skip the StyleGuide demo for that capability until the library lands
- Add the integration step to the appropriate phase's audit list

Example: LipiTable PDF export in Phase 2.8 stubs the PDF code path until the
in-house LiPi PDF library ships (expected Phase 2.10). The export trigger UI
is built; clicking "PDF" throws "PDF library pending Phase 2.10 integration"
during the gap.

### Rationale
- **Predictable maintenance.** Every NuGet dependency is a future security patch, version-mismatch debug session, or licensing review.
- **License clarity.** In-house code is owned. External code carries license terms (some viral, some commercial-license-required at scale, some abandoned).
- **Targeted features.** A focused in-house implementation only does what LiPi needs — often 200–800 lines vs a 50,000-line library covering the long tail.
- **Distribution.** The LiPi component library is intended to be redistributable to other Blazor projects. Each external dependency is a hidden requirement those projects also have to accept.
- **Audit posture.** Phase 2.10 reviews every package; this rule keeps that review queue from growing unbounded.
```

---

## Effect on other files

After this rule lands in `02-STANDING-RULES.md`, the following updates flow through:

1. **`03-DEFERRED-ITEMS.md`** — already lists `MailKit` and `JwtBearer` under "NuGet packages to review." This rule formalizes the review obligation. Add `SharpZipLib` and `Isopoh.Cryptography.Argon2` to the list with their justifications (both kept, both no-BCL-equivalent).

2. **`04-OPERATIONAL-RULES.md §5`** — the existing "Infrastructure roadmap principle" (LipiMail / LipiHttp / LipiApi) is now backed by a hard rule, not just operational guidance. Add a forward reference from §5 to the new standing rule.

3. **`00-PROJECT-BASELINE.md`** — no edit; this is a v1.0+ standing rule that lives in `02-STANDING-RULES.md`, not in baseline.

4. **Strategic chat onboarding (system prompt)** — mention this rule explicitly in the "design discussion rule" section so it surfaces in every relevant design decision.

---

## Delivery

This file is the strategic-chat deliverable. Build chat receives it as part of Phase 2.8 housekeeping and:

1. Adds the new section to `02-STANDING-RULES.md` after the "GitHub Actions rules" section
2. Updates `03-DEFERRED-ITEMS.md` per item 1 above
3. Adds the cross-reference in `04-OPERATIONAL-RULES.md §5` per item 2 above
4. Registers the new section in `deploy-downloads.ps1` (no new file; the rule is appended to existing standing-rules file)
5. Logs the change in `CHANGE-LOG.md` under v1.X amendments

*End of standing-rule addition spec.*
