# LiPi HIS — Session Prompt Templates

> Copy-paste these into new chats in your Claude Project for fast, consistent session starts.

---

## TEMPLATE 1: Fill in Specs for a Built Module

Use when: A module has working code but template specs.

```
Let's fill in the spec files for Module [NN] ([Module Name]). 
The module is built — pages exist at [/route1, /route2, /route3].

Please:
1. Read docs/[NN]-[MODULE-NAME]/[NN].1-Design-Specs.md, [NN].2, and [NN].3 (currently templates)
2. Read the actual built code at src/LiPi.Web/Pages/[folder]/[Page1].razor, [Page2].razor, [Page3].razor
3. Read database/efcore/LiPi.[Project]/Entities/[Entity].cs
4. Use Module 01 (User Registration) specs as the format reference
5. Generate the fully populated [NN].1, [NN].2, [NN].3 files matching the actual implementation

Show me each file before saving, so I can review.
```

---

## TEMPLATE 2: Start a New Module Build

Use when: Beginning code work on a new module.

```
I want to start building Module [NN] ([Module Name]).

Please follow the SYSTEM PROMPT enforcement rules:
1. Read docs/00-PROJECT-BASELINE.md (refresh on locked decisions)
2. Read docs/[NN]-[MODULE-NAME]/[NN].1-Design-Specs.md, .2, .3
3. Display the PRE-CODE CHECKLIST and wait for my confirmation
4. Once I confirm, generate code following all 12 Blazor rules

Specifically, I want to start with: [page name]
```

---

## TEMPLATE 3: Make a Change Within v1.0 Scope

Use when: Modifying existing v1.0 functionality.

```
I need to make a change to Module [NN]. 

Change: [describe what you want to change]

Please:
1. Classify this as BASE v1.0 (already in spec) or v1.1+ (new change)
2. If BASE v1.0: implement per spec
3. If v1.1+: add to docs/CHANGE-LOG.md under v1.1, update spec file, then implement
4. Show me the spec change before applying

Wait for my confirmation before generating code.
```

---

## TEMPLATE 4: Debug a Build Error

Use when: Code isn't compiling or running.

```
I'm getting a build error in [file]. Error message:

[paste error]

Code that's failing:
[paste code]

Please:
1. Cross-reference against the 12 Blazor rules in docs/00-PROJECT-BASELINE.md
2. Check if it's one of the recurring issues (CS1525, missing @rendermode, etc.)
3. Suggest the fix WITHOUT generating large code blocks — just the specific lines to change

Don't regenerate the whole file. Just show me the diff.
```

---

## TEMPLATE 5: CSS Refactoring Phase

Use when: Working on CSS migration.

```
Let's continue CSS refactoring per docs/css-refactoring-roadmap.md.

I want to do PHASE [N] this session: [phase description]

Please:
1. Read css-refactoring-roadmap.md (Phase [N] section)
2. [Specific instructions for the phase]
3. DO NOT touch files outside this phase
4. Show me each file change before applying

If I haven't completed earlier phases, stop and tell me what's needed first.
```

---

## TEMPLATE 6: Quick Spec Lookup

Use when: You just need to recall something fast.

```
Quick reference question: [your question]

Pull from docs/00-PROJECT-BASELINE.md or relevant module spec.
No code generation needed — just the answer.
```

Examples:
- "What's the CSS prefix for Module 02?"
- "What's the format for UHID?"
- "What are the status colors for table rows?"
- "What's the cooling-off period for SiteAdmin merges?"

---

## TEMPLATE 7: Add Spec for Built Page

Use when: A specific page exists in code but isn't documented.

```
The page [PageName].razor exists at [route] but isn't fully spec'd.

Please:
1. Read the page code: src/LiPi.Web/Pages/[folder]/[PageName].razor
2. Read the corresponding [code-behind].razor.cs if it exists
3. Read related entity in database/efcore/[Project]/Entities/[Entity].cs
4. Update docs/[NN]-[MODULE]/[NN].2-Pages-Validations.md to add this page's section
5. Show me the section before saving

Use Module 01 UsersNew section as format reference.
```

---

## TEMPLATE 8: Add to CHANGE-LOG

Use when: Making a v1.1+ change.

```
I want to add this change to v1.1:

[Description of change]

Affected modules: [list]

Please:
1. Update docs/CHANGE-LOG.md with the change under v1.1 section
2. Update affected module spec files (.1, .2, or .3 as needed)
3. Show me each file diff before saving
```

---

## QUICK COMMAND CHEAT SHEET

| Need | Quick command |
|------|---------------|
| Read baseline | "Read 00-PROJECT-BASELINE.md" |
| Module spec | "Read docs/[NN]-[NAME]/" |
| Find a decision | "What's locked decision #N?" |
| Validation rules | "What are the validation rules for [page]?" |
| DB schema | "Show table structure for [table]" |
| CSS prefix | "What's the CSS prefix for module [NN]?" |
| Audit codes | "What audit events fire on [action]?" |

---

## SESSION HYGIENE RULES

Before ending a long session:

1. **Commit code changes** to GitHub immediately
2. **Update CHANGE-LOG.md** if v1.1+ work was done
3. **Note any open issues** in a session-end summary
4. **Verify all generated files** are in correct paths
5. **Don't leave half-finished spec files** — either complete them or revert

After starting a new session:

1. **Verify Claude reads baseline** (test with "what's decision #6?")
2. **Confirm scope** before requesting code (BASE v1.0 vs v1.1)
3. **Use pre-code checklist** — never skip
4. **Reference spec sections** in your requests

---

## RED FLAGS DURING SESSIONS

If you notice any of these, STOP and re-align:

- ❌ Claude generates code without showing pre-code checklist
- ❌ Claude suggests `.NET 7/8/9` patterns instead of `.NET 10`
- ❌ Claude forgets `@rendermode InteractiveServer`
- ❌ Claude uses `Dictionary<string,object>` instead of `string` for ExtensionData
- ❌ Claude suggests CSS in admin.css instead of per-module file
- ❌ Claude doesn't reference spec sections in code comments
- ❌ Claude treats template specs as authoritative without flagging "template only"

If you see these → Quote the exact violation, ask Claude to re-read baseline + relevant spec, regenerate.

---

## THIS DOCUMENT

Save this as `SESSION-TEMPLATES.md` in your project root.

Update it as you discover new useful prompt patterns.
