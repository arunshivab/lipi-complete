# LiPi HIS — Operational Rules

How Claude works with the user across strategic and build chats. These rules govern
every interaction, not just specific features.

---

## 1. File workflow (MANDATORY)

**The user never edits files manually.** Claude must always:

1. Generate full files (or surgical updates as full files)
2. Update `deploy-downloads.ps1` with new entries every time a new file is introduced
3. Apply this to spec docs, CHANGE-LOG, source code, CSS — everything

**Never:**
- No "paste this snippet into the file" instructions
- No "insert this line between line 47 and 48" requests
- No partial files

**The deploy flow:** User drops files into `Downloads\LiPi\` and runs `deploy-downloads.ps1`.
Both build chat AND strategic chat must follow this rule.

---

## 2. Design discussion rule (MANDATORY)

**Claude always presents industry standards + best practices + market norms for every design decision.**

- Lay out all options with trade-offs
- If user goes against standards, Claude evaluates user's reasoning honestly and debates back and forth
- Both perspectives must be on the table — informed decision
- Never stay silent on standards just because user expressed a preference

---

## 3. Deviation handling rule (MANDATORY)

When build chat (or any subsequent chat) proposes a deviation from a locked decision:

- **Never silently accept**
- **Never auto-reject**
- Either it's already-discussed-in-strategic → re-confirm and enforce
- Or it's NEW information → re-discuss, weigh trade-offs, decide together

Cross-reference the locked decision, surface what changes, ask for user's call.
**Review-time approval belongs to the user, not Claude.**

---

## 4. LiPi project posture (LOCKED)

- Solo dev, no timeline pressure (Armoki in discussion phase)
- **Prioritize depth over speed**
- Calm pace = strategic edge:
  - Refactor freely as patterns emerge
  - Take time on big design discussions
  - Don't compress phases
- Discipline still required:
  - Don't drift between sessions
  - Don't expand scope endlessly
  - Don't overpolish past module needs
- Strategic + build chat dialogue serves as pair-programming substitute

---

## 5. Infrastructure roadmap principle

Email / HTTP needs will grow (Lab / Radiology reports, ABDM, DigiLocker, SMS, payments).

**Short-term:** keep `MailKit` + raw `HttpClient`. No `SmtpClient` rewrite.

**Long-term:**
- Build `LipiMail` service in dedicated phase when first structured-email module ships
- Build `LipiHttp` + `LipiApi` infrastructure in dedicated phase when first external integration arrives

**Deliberate infrastructure phases with own design discussion, not inline feature work.**

---

## 6. OnEditContextReset protocol (shipped, documented for reference)

### Tier 1 — Replace model object
New EditContext is created. Causes `_isTouched` + `_editContextError` reset in
`LipiInputBase.OnParametersSet`.

### Tier 2 — Same EditContext
Call `editContext.MarkAsUnmodified()` then `editContext.NotifyValidationStateChanged()`.
`LipiInputBase` heuristic detects `IsModified=false` + no messages and clears `_isTouched`.

### Tier 3 — Deferred
`LipiFormContext` wrapper. Own design session when first wizard form is built.
See `03-DEFERRED-ITEMS.md`.
