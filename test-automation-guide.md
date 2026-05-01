# LiPi HIS — Test Automation Guide

> **Purpose**: Automatically validate generated code against module specs  
> **Stack**: C# 12 + xUnit + Selenium WebDriver + Npgsql  
> **Run after**: Every code generation session

---

## OVERVIEW

The test automation runs **3 layers of checks**:

1. **Database Validation** — Tables, columns, FKs, constraints match `[N].3-Database-Schema.md`
2. **UI Validation** — Form fields, IDs, validations match `[N].2-Pages-Validations.md`
3. **Behavior Validation** — Confirmation dialogs, audit events match `[N].1-Design-Specs.md`

---

## PROJECT SETUP

```
test-runner/
├── LiPi.Tests.csproj
├── DatabaseTests/
│   ├── DatabaseValidator.cs
│   ├── SchemaParser.cs
│   └── SqlAssertions.cs
├── UITests/
│   ├── UIValidator.cs
│   ├── FormFieldChecker.cs
│   └── AccessibilityChecker.cs
├── BehaviorTests/
│   ├── BehaviorValidator.cs
│   ├── ConfirmationDialogChecker.cs
│   └── AuditEventChecker.cs
├── SpecParser/
│   ├── DesignSpecParser.cs   ← Parses [N].1-Design-Specs.md
│   ├── ValidationSpecParser.cs ← Parses [N].2-Pages-Validations.md
│   └── DatabaseSpecParser.cs ← Parses [N].3-Database-Schema.md
└── Program.cs                ← Entry point
```

---

## EXAMPLE: Database Validator

```csharp
// DatabaseTests/DatabaseValidator.cs
using Npgsql;
using LiPi.Tests.SpecParser;

public class DatabaseValidator
{
    private readonly string _connectionString;
    private readonly DatabaseSpec _spec;
    
    public DatabaseValidator(string connectionString, DatabaseSpec spec)
    {
        _connectionString = connectionString;
        _spec = spec;
    }
    
    public async Task<TestResult> ValidateAsync()
    {
        var result = new TestResult { Module = _spec.ModuleName };
        
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        
        foreach (var table in _spec.Tables)
        {
            // Check table exists
            var exists = await TableExistsAsync(conn, table.Schema, table.Name);
            result.Add(new Check
            {
                Name = $"Table {table.Schema}.{table.Name} exists",
                Passed = exists
            });
            
            if (!exists) continue;
            
            // Check columns
            foreach (var col in table.Columns)
            {
                var actual = await GetColumnAsync(conn, table.Schema, table.Name, col.Name);
                result.Add(new Check
                {
                    Name = $"  Column {col.Name} ({col.Type})",
                    Passed = actual?.DataType == col.Type
                });
            }
            
            // Check indexes
            foreach (var idx in table.Indexes)
            {
                var indexExists = await IndexExistsAsync(conn, idx.Name);
                result.Add(new Check
                {
                    Name = $"  Index {idx.Name}",
                    Passed = indexExists
                });
            }
            
            // Check foreign keys
            foreach (var fk in table.ForeignKeys)
            {
                var fkExists = await FkExistsAsync(conn, fk.Name);
                result.Add(new Check
                {
                    Name = $"  FK {fk.Name}",
                    Passed = fkExists
                });
            }
        }
        
        return result;
    }
    
    private async Task<bool> TableExistsAsync(NpgsqlConnection conn, string schema, string table)
    {
        var sql = @"
            SELECT EXISTS (
                SELECT FROM information_schema.tables 
                WHERE table_schema = @schema AND table_name = @table
            )";
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("schema", schema);
        cmd.Parameters.AddWithValue("table", table);
        return (bool)(await cmd.ExecuteScalarAsync())!;
    }
    
    // ... GetColumnAsync, IndexExistsAsync, FkExistsAsync
}
```

---

## EXAMPLE: UI Validator (Selenium)

```csharp
// UITests/UIValidator.cs
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

public class UIValidator
{
    private readonly IWebDriver _driver;
    private readonly UISpec _spec;
    
    public UIValidator(UISpec spec)
    {
        _spec = spec;
        _driver = new ChromeDriver();
    }
    
    public async Task<TestResult> ValidatePageAsync(string baseUrl)
    {
        var result = new TestResult { Module = _spec.ModuleName, Page = _spec.PageName };
        
        // Login first
        await LoginAsync(baseUrl);
        
        // Navigate to page
        _driver.Navigate().GoToUrl($"{baseUrl}{_spec.Route}");
        
        // Check all required fields exist
        foreach (var field in _spec.Fields)
        {
            // Check by ID
            var element = TryFindElement(By.Id(field.Id));
            result.Add(new Check
            {
                Name = $"Field {field.Id} exists",
                Passed = element != null
            });
            
            if (element == null) continue;
            
            // Check name attribute
            var name = element.GetAttribute("name");
            result.Add(new Check
            {
                Name = $"  Field {field.Id} has name='{field.Name}'",
                Passed = name == field.Name
            });
            
            // Check autocomplete
            var autocomplete = element.GetAttribute("autocomplete");
            result.Add(new Check
            {
                Name = $"  Field {field.Id} has autocomplete='{field.Autocomplete}'",
                Passed = autocomplete == field.Autocomplete
            });
            
            // Check label
            var labelXpath = $"//label[@for='{field.Id}']";
            var label = TryFindElementByXPath(labelXpath);
            result.Add(new Check
            {
                Name = $"  Field {field.Id} has associated label",
                Passed = label != null
            });
            
            // Check required indicator
            if (field.Required)
            {
                var required = element.GetAttribute("required") != null
                    || element.GetAttribute("aria-required") == "true";
                result.Add(new Check
                {
                    Name = $"  Field {field.Id} marked required",
                    Passed = required
                });
            }
        }
        
        return result;
    }
}
```

---

## EXAMPLE: Behavior Validator

```csharp
// BehaviorTests/ConfirmationDialogChecker.cs
public class ConfirmationDialogChecker
{
    public async Task<TestResult> CheckDestructiveActionsAsync(IWebDriver driver, BehaviorSpec spec)
    {
        var result = new TestResult();
        
        foreach (var action in spec.DestructiveActions)
        {
            // Click the action button
            var btn = driver.FindElement(By.CssSelector(action.ButtonSelector));
            btn.Click();
            
            // Wait for confirmation dialog
            await Task.Delay(500);
            
            // Verify dialog appeared
            var dialog = TryFindElement(driver, By.CssSelector(".confirmation-dialog"));
            result.Add(new Check
            {
                Name = $"Confirmation dialog appears for {action.Name}",
                Passed = dialog != null && dialog.Displayed
            });
            
            if (dialog == null) continue;
            
            // Verify "Cancel" button is prominent (default)
            var cancelBtn = dialog.FindElement(By.CssSelector(".btn-cancel, [data-default]"));
            result.Add(new Check
            {
                Name = $"  Cancel button prominent",
                Passed = cancelBtn != null && cancelBtn.Displayed
            });
            
            // Verify destructive button is red
            var destructiveBtn = dialog.FindElement(By.CssSelector(".btn-danger, [data-destructive]"));
            var color = destructiveBtn.GetCssValue("background-color");
            result.Add(new Check
            {
                Name = $"  Destructive button is red",
                Passed = color.Contains("244, 67, 54") || color.Contains("#F44336")
            });
            
            // Cancel and continue
            cancelBtn.Click();
        }
        
        return result;
    }
}
```

---

## RUNNING TESTS

```bash
# Build and run
cd test-runner
dotnet build
dotnet run -- --module 01-USER-REGISTRATION --base-url http://localhost:5000

# Run all modules
dotnet run -- --all

# Run with verbose output
dotnet run -- --module 04-PATIENT-REGISTRATION --verbose
```

---

## OUTPUT REPORT

```
═══════════════════════════════════════════
TEST RUN — 2026-05-02 14:30:15
═══════════════════════════════════════════
Module: 01-USER-REGISTRATION
Page: /admin/users/new

DATABASE VALIDATION:
✓ Table master.platform_users exists
  ✓ Column user_id (uuid)
  ✓ Column username (varchar)
  ✓ Column email (varchar)
  ✓ Column display_name (text, generated)
  ✓ Index uq_platform_users_username
  ✗ Index uq_platform_users_email — MISSING

UI VALIDATION:
✓ Field un-firstName exists
  ✓ Field has name="firstName"
  ✓ Field has autocomplete="given-name"
  ✓ Field marked required
✓ Field un-lastName exists
  ...

BEHAVIOR VALIDATION:
✓ Confirmation dialog appears for "Suspend User"
  ✓ Cancel button prominent
  ✓ Destructive button red
✓ Audit event USER_SUSPENDED fires
  ✓ before_state captured
  ✓ after_state captured

SUMMARY:
  Total Checks: 142
  Passed: 138
  Failed: 4
  Status: FAIL — see test-results/2026-05-02-01-user.md

═══════════════════════════════════════════
```

---

## CI/CD INTEGRATION

Add to `.github/workflows/spec-validation.yml`:

```yaml
name: Spec Validation
on: [push, pull_request]

jobs:
  validate:
    runs-on: ubuntu-latest
    services:
      postgres:
        image: postgres:16
        env:
          POSTGRES_PASSWORD: test
        ports: ['5432:5432']
        options: --health-cmd pg_isready --health-interval 10s --health-timeout 5s --health-retries 5
    
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'
      
      - name: Provision databases
        run: |
          psql -h localhost -U postgres -f database/master/001_schema_master.sql
          psql -h localhost -U postgres -f database/clinic/01_core_v3.sql
      
      - name: Run spec validation
        run: |
          cd test-runner
          dotnet run -- --all
```

---

## SPEC PARSING

Each parser reads markdown spec files:

```csharp
// SpecParser/ValidationSpecParser.cs
public class ValidationSpecParser
{
    public UISpec Parse(string moduleNumber)
    {
        var path = $"docs/{moduleNumber}-*/{moduleNumber}.2-Pages-Validations.md";
        var file = Directory.GetFiles(".", path).First();
        var content = File.ReadAllText(file);
        
        // Parse markdown tables for fields
        // Look for: | Field | Type | Required | Validation | id | name | autocomplete |
        var fields = ExtractFieldsFromMarkdownTables(content);
        
        return new UISpec
        {
            ModuleName = moduleNumber,
            Fields = fields
        };
    }
    
    private List<FieldSpec> ExtractFieldsFromMarkdownTables(string markdown)
    {
        var fields = new List<FieldSpec>();
        var lines = markdown.Split('\n');
        bool inTable = false;
        bool isFieldTable = false;
        
        foreach (var line in lines)
        {
            if (line.Contains("| Field | Type | Required") && line.Contains("autocomplete"))
            {
                inTable = true;
                isFieldTable = true;
                continue;
            }
            
            if (line.StartsWith("|---")) continue; // separator
            
            if (line.StartsWith("|") && inTable && isFieldTable)
            {
                var parts = line.Split('|', StringSplitOptions.RemoveEmptyEntries)
                    .Select(p => p.Trim()).ToList();
                if (parts.Count >= 7)
                {
                    fields.Add(new FieldSpec
                    {
                        Name = parts[0],
                        Type = parts[1],
                        Required = parts[2] == "✅",
                        Validation = parts[3],
                        Id = parts[4].Trim('`'),
                        FieldName = parts[5].Trim('`'),
                        Autocomplete = parts[6].Trim('`')
                    });
                }
            }
            
            if (string.IsNullOrWhiteSpace(line)) inTable = false;
        }
        
        return fields;
    }
}
```

---

## NEXT STEPS

1. Create `test-runner/` project
2. Add NuGet packages: `Selenium.WebDriver`, `Selenium.WebDriver.ChromeDriver`, `Npgsql`, `xunit`
3. Implement parsers for all 3 spec types
4. Implement validators
5. Add CI/CD workflow
6. Run after every code generation session

---

## REFERENCES

- **Specs Folder**: `docs/`
- **System Prompt**: `system-prompt.md` (links to test runner)
- **CI/CD**: `.github/workflows/spec-validation.yml`
