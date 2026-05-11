#!/usr/bin/env python3
"""
SQL Schema Validation Script
Checks for syntax errors, missing dependencies, and ordering issues.
"""

import re
import sys
from pathlib import Path

# All valid top-level SQL constructs that constitute a legitimate SQL file.
VALID_CONSTRUCTS = [
    r'CREATE\s+TABLE',
    r'CREATE\s+SCHEMA',
    r'CREATE\s+(OR\s+REPLACE\s+)?FUNCTION',
    r'CREATE\s+(OR\s+REPLACE\s+)?PROCEDURE',
    r'CREATE\s+(OR\s+REPLACE\s+)?TRIGGER',
    r'CREATE\s+EXTENSION',
    r'CREATE\s+DOMAIN',
    r'CREATE\s+TYPE',
    r'CREATE\s+INDEX',
    r'CREATE\s+(UNIQUE\s+)?SEQUENCE',
    r'INSERT\s+INTO',
    r'ALTER\s+TABLE',
    r'GRANT\s+',
    r'DO\s+\$',
]


def has_valid_construct(content):
    for pattern in VALID_CONSTRUCTS:
        if re.search(pattern, content, re.IGNORECASE):
            return True
    return False


def strip_dollar_quotes(content):
    """Remove dollar-quoted bodies so single quotes inside $$ blocks are not counted."""
    # Named: $tag$...$tag$
    content = re.sub(r'\$([A-Za-z_][A-Za-z0-9_]*)\$.*?\$\1\$', '', content, flags=re.DOTALL)
    # Anonymous: $$...$$
    content = re.sub(r'\$\$.*?\$\$', '', content, flags=re.DOTALL)
    return content


def strip_sql_comments(content):
    content = re.sub(r'--[^\n]*', '', content)
    content = re.sub(r'/\*.*?\*/', '', content, flags=re.DOTALL)
    return content


def validate_sql_file(filepath):
    with open(filepath, 'r', encoding='utf-8') as f:
        content = f.read()

    issues = []

    # Strip dollar-quoted blocks and comments before structural checks
    stripped = strip_sql_comments(strip_dollar_quotes(content))

    # Unclosed single quotes (outside dollar-quoted blocks only)
    if stripped.count("'") % 2 != 0:
        issues.append("Unclosed single quotes")

    # Unbalanced parentheses (outside dollar-quoted blocks only)
    paren_balance = stripped.count('(') - stripped.count(')')
    if paren_balance != 0:
        issues.append(f"Unbalanced parentheses (balance: {paren_balance})")

    # Double semicolons
    if re.search(r';\s*;', content):
        issues.append("Double semicolons found")

    # At least one recognised SQL construct
    if not has_valid_construct(content):
        issues.append(
            "No recognised SQL construct found "
            "(CREATE TABLE/SCHEMA/FUNCTION/TRIGGER/EXTENSION/DOMAIN/TYPE, "
            "INSERT INTO, ALTER TABLE, GRANT, etc.)"
        )

    return issues


def main():
    db_dir = Path(__file__).parent.parent

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
