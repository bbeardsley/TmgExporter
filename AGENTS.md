# Agent Guidelines for TmgExporter Project

This document provides comprehensive guidelines for AI agents working on the TmgExporter codebase. These rules ensure consistency, compatibility, and adherence to project standards.

## Project Overview

**TmgExporter** is a C# console application that exports data from TMG (The Master Genealogist) databases to various formats including CSV, JSON, XML, and database outputs.

## Tech Stack

### Core Platform
- **Language:** C# (with nullable reference types enabled)
- **Target Framework:** .NET 9.0
- **Project SDK:** Microsoft.NET.Sdk
- **Project Type:** Console Application (Exe)

### Build Configuration
- **ImplicitUsings:** Enabled
- **Nullable:** Enabled
- **RestorePackagesWithLockFile:** true (package lock file is used)

## Dependencies and Package Management

### Active NuGet Packages
The following packages are currently used (as of the latest csproj):

| Package | Version | Purpose |
|---------|---------|---------|
| CsvHelper | 30.0.1 | CSV file reading and writing |
| Microsoft.Extensions.Configuration.Ini | 10.0.0 | INI file parsing |
| MySqlConnector | 2.3.5 | MySQL database connectivity |
| Newtonsoft.Json | 13.0.3 | JSON serialization/deserialization |
| NLog | 5.2.7 | Logging framework |
| Npgsql | 8.0.3 | PostgreSQL database connectivity |
| Microsoft.Data.SQLite | 8.0.0 | SQLite database connectivity |
| Microsoft.Data.SqlClient | 5.1.5 | SQL Server database connectivity |
| System.Data.OleDb | 8.0.0 | OLE DB database connectivity (Windows only) |
| System.CommandLine | 2.0.0-beta4.22272.1 | Command-line parsing |

### Package Management Rules
1. **Use NuGet exclusively** for all external dependencies
2. **Check packages.lock.json** before adding/updating packages
3. **Verify .NET 9.0 compatibility** for all new packages
4. **Update GEMINI.md** if package versions change significantly
5. **Avoid duplicate functionality** - use existing packages before adding new ones

## Mandatory Development Standards

### 1. Language and Framework
- All code must be written in **C# targeting .NET 9.0**
- Use **modern C# features** appropriate for .NET 9.0 (pattern matching, records, etc.)
- Enable and respect **nullable reference types** (`Nullable` is enabled)
- Use **implicit usings** where appropriate (enabled by default)

### 2. Command-Line Parsing
- **Use System.CommandLine** (v2.0.0-beta4.22272.1) for all command-line argument parsing
- Define options with clear descriptions and appropriate types
- Support both short and long option names (e.g., `-t` and `--tmg`)
- Built-in help generation via `--help`, `-h`, or `-?`

### 2. Logging Standards
- **Use NLog exclusively** for all logging operations
- Configuration is in `App.config` with both file and console targets
- Minimum log level is **Info** (as configured)
- Log to both `TmgExporter.log` file and colored console
- Follow the log format: `${longdate} | ${level} | ${logger} | ${message}`

### 3. Data Serialization
- **JSON:** Use `Newtonsoft.Json` (v13.0.3) for all JSON operations
- **CSV:** Use `CsvHelper` (v30.0.1) for all CSV operations
- **INI:** Use `Microsoft.Extensions.Configuration.Ini` (v10.0.0) for all INI file operations
- **XML:** Use built-in .NET XML libraries

### 4. Database Connectivity
- **MySQL:** Use `MySqlConnector` (v2.3.5) - Note: This is MySqlConnector, NOT MySql.Data
- **PostgreSQL:** Use `Npgsql` (v8.0.3)
- **SQLite:** Use `Microsoft.Data.SQLite` (v8.0.0)
- Database provider configuration is in `App.config` under `system.data/DbProviderFactories`

### 5. Code Style and Conventions
- Follow **standard C# coding conventions**
- Use **PascalCase** for public members, classes, and methods
- Use **camelCase** for private fields and local variables
- Prefix private fields with underscore (`_fieldName`) if used
- Add **XML documentation comments** to all public APIs
- Keep methods focused and single-responsibility

### 6. Error Handling
- Implement **try-catch blocks** for:
  - File I/O operations
  - Database operations
  - External library calls
  - Network operations
- Provide **meaningful error messages** that help diagnose issues
- Use **NLog** to log exceptions with appropriate severity levels
- Consider user-facing vs. technical error messages

### 7. File Structure
- Keep related functionality in **separate files**
- Interface definitions: `I{Name}.cs` (e.g., `IOutput.cs`, `ISqlBuilder.cs`)
- Implementation classes: `{Name}.cs` (e.g., `CsvOutput.cs`, `SqlBuilder.cs`)
- Data models: Descriptive names (e.g., `TableInfo.cs`, `ColumnInfo.cs`)

## Critical Compatibility Notes

### ⚠️ Breaking Changes from Legacy
The GEMINI.md file references an **outdated tech stack** (NET Framework 4.5). The project has been **upgraded to .NET 9.0**. Key differences:

| Component | Old (GEMINI.md) | Current (Actual) |
|-----------|-----------------|------------------|
| Framework | .NET Framework 4.5 | .NET 9.0 |
| MySQL Package | MySql.Data v6.9.8.0 | MySqlConnector v2.3.5 |
| CSV Package | CsvHelper v2.13.5.0 | CsvHelper v30.0.1 |
| INI Parser | INIFileParser v2.2.2.0 | Microsoft.Extensions.Configuration.Ini v10.0.0 |
| JSON Package | Newtonsoft.Json v7.0.1 | Newtonsoft.Json v13.0.3 |
| NLog | v4.2.2 | v5.2.7 |
| Npgsql | v3.0.4.0 | v8.0.3 |
| SQLite Package | System.Data.SQLite.Core v1.0.98.1 | Microsoft.Data.SQLite v8.0.0 |
| CLI Parser | FluentCommandLineParser v1.4.3.0 | System.CommandLine v2.0.0-beta4.22272.1 |

### Migration Considerations
- **Do NOT** add packages targeting .NET Framework 4.5
- **Avoid** using deprecated APIs from old package versions
- **Use System.CommandLine** for command-line parsing (FluentCommandLineParser is obsolete)
- **Test thoroughly** after any package updates
- **Update documentation** if making significant changes

## Agent-Specific Instructions

### When Adding Code
1. Ensure nullable reference type annotations are correct
2. Use modern C# patterns where appropriate
3. Add XML documentation for public members
4. Use existing interfaces and patterns in the codebase
5. Log important operations using NLog

### When Modifying Dependencies
1. Check if the package is .NET 8.0 compatible
2. Review breaking changes in package release notes
3. Update packages.lock.json (will happen automatically on restore)
4. Test all affected functionality
5. Update this document if adding new packages

### When Debugging
1. Check `TmgExporter.log` for detailed logs
2. Console output is also available (colored)
3. Verify `App.config` NLog settings if logging issues occur
4. Database provider issues: check `system.data` section in App.config

### When Refactoring
1. Maintain existing interfaces (`IOutput`, `IDataTableBuilder`, `ISqlBuilder`, `IDatabaseOutput`)
2. Keep separation of concerns (output vs. building vs. business logic)
3. Preserve existing public APIs unless explicitly deprecated
4. Update tests if they exist

## Build and Development

### Building the Project
```bash
dotnet build TmgExporter.sln
# Or using justfile:
just build
```

### Running the Application
```bash
dotnet run --project TmgExporter/TmgExporter.csproj -- [arguments]
```

### Publishing Executables
```bash
# windows x64 - single file executable
just publish
```

**Common Runtime Identifiers (RID):**
- `osx-arm64` - macOS Apple Silicon (M1/M2/M3)
- `osx-x64` - macOS Intel
- `win-x64` - Windows 64-bit
- `win-x86` - Windows 32-bit
- `win-arm64` - Windows ARM
- `linux-x64` - Linux 64-bit
- `linux-arm64` - Linux ARM

### Restoring Packages
```bash
dotnet restore
```

## Configuration Files

### App.config
- NLog configuration (targets, rules, formatting)
- Database provider factory registrations
- Startup runtime configuration
- **Do not modify** unless changing logging or DB provider setup

### TmgExporter.csproj
- Package references and versions
- Build properties (target framework, nullable, etc.)
- **Update carefully** - affects entire build

### packages.lock.json
- **Auto-generated** - do not manually edit
- Tracks exact package dependency tree
- Commit changes when packages are updated

## Testing Guidelines

1. Test with various database types (MySQL, PostgreSQL, SQLite)
2. Verify all output formats (CSV, JSON, XML, Database)
3. Check logging output in both file and console
4. Validate error handling with invalid inputs
5. Test with edge cases (empty tables, null values, special characters)

## Documentation Standards

- Keep **README.md** updated with user-facing changes
- Update **GEMINI.md** if tech stack or standards change
- Update **this file (AGENTS.md)** when adding new patterns or rules
- Add inline comments for complex logic only
- Prefer self-documenting code names over excessive comments

## Security Considerations

1. Never hard-code credentials or connection strings
2. Sanitize database inputs to prevent SQL injection
3. Validate file paths to prevent directory traversal
4. Be cautious with user-provided format strings
5. Log sensitive operations but not sensitive data

## Performance Guidelines

1. Use async/await for I/O operations where beneficial
2. Dispose of database connections properly (use `using` statements)
3. Stream large files rather than loading entirely into memory
4. Consider pagination for large database queries
5. Profile before optimizing

## Version Control

- Follow conventional commit messages
- Keep commits atomic and focused
- Update version information when making releases
- Tag releases appropriately
- Document breaking changes in commit messages

---

**Last Updated:** 2025-11-19 (based on .NET 9.0 migration and System.CommandLine adoption)  
**Authoritative Source:** TmgExporter.csproj and App.config take precedence over GEMINI.md for current tech stack
