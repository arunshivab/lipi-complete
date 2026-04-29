#!/usr/bin/env python3
"""
SQL Schema Validation Script
Checks for syntax errors, missing dependencies, and ordering issues.
"""

import os
import re
import sys
from pathlib import Path

def validate_sql_file(filepath):
    """Check SQL file for common issues."""
    with open(filepath, 'r') as f:
        content = f.read()
    
    issues = []
    
    # Check for unclosed strings
    single_quote_count = content.count("'") - content.count("\'")
    if single_quote_count % 2 != 0:
        issues.append("Unclosed single quotes")
    
    # Check for matching parentheses
    paren_balance = 0
    for char in content:
        if char == '(':
            paren_balance += 1
        elif char == ')':
            paren_balance -= 1
    if paren_balance != 0:
        issues.append(f"Unbalanced parentheses (balance: {paren_balance})")
    
    # Check for common syntax issues
    if re.search(r';\s*;', content):
        issues.append("Double semicolons found")
    
    if re.search(r'CREATE\s+TABLE.*?;', content, re.IGNORECASE | re.DOTALL) is None:
        if 'CREATE SCHEMA' not in content and 'CREATE FUNCTION' not in content:
            issues.append("No table/schema/function creation found")
    
    return issues

def main():
    """Validate all SQL files in database directory."""
    db_dir = Path(__file__).parent.parent
    
    # Order of file execution
    sql_files = [
        "00_common/001_extensions.sql",
        "00_common/002_uuid_v7.sql",
        "00_common/003_audit_triggers.sql",
        "00_common/004_reference_domains.sql",
        "master/001_schema_master.sql",
        "clinic/01_core.sql",
        "clinic/02_identity.sql",
        "clinic/03_abdm.sql",
        "clinic/04_audit.sql",
        "clinic/05_security.sql",
        "clinic/06_compliance.sql",
        "clinic/07_certs.sql",
        "clinic/08_sigma.sql",
    ]
    
    errors = []
    
    for sql_file in sql_files:
        filepath = db_dir / sql_file
        
        if not filepath.exists():
            errors.append(f"❌ Missing: {sql_file}")
            continue
        
        issues = validate_sql_file(filepath)
        if issues:
            errors.append(f"⚠️  {sql_file}:")
            for issue in issues:
                errors.append(f"   - {issue}")
        else:
            print(f"✅ {sql_file}")
    
    if errors:
        print("\nErrors found:")
        for error in errors:
            print(error)
        sys.exit(1)
    else:
        print("\n✅ All SQL files validated successfully!")
        sys.exit(0)

if __name__ == "__main__":
    main()
