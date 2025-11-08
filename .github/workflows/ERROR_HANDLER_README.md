# Error Handler and Reporter Workflow

## Overview

This workflow automatically detects, analyzes, and reports build failures in CI/CD pipelines. It creates comprehensive error reports with all information needed for Cursor AI to fix issues.

## Features

### ✅ Automatic Triggering
- Triggers automatically when any monitored workflow fails
- Can be manually triggered via `workflow_dispatch`
- Monitors all CI and deployment workflows

### ✅ Comprehensive Error Extraction
- **C# / .NET Errors**: Extracts all compilation errors with file paths and line numbers
- **TypeScript / Angular Errors**: Captures TypeScript and Angular compiler errors
- **NPM Errors**: Identifies package and dependency issues
- **Build Configuration Errors**: Detects angular.json and configuration issues
- **Test Failures**: Captures test execution errors
- **Linter Errors**: Extracts linting warnings and errors
- **File-Specific Errors**: Lists all files with errors and line numbers

### ✅ Comparison with Last Successful Build
- Finds the last successful build on the same branch
- Shows all file changes since last success
- Helps identify what introduced the errors

### ✅ Detailed Error Reports
Each error report includes:
1. Complete error messages with context
2. File paths and line numbers for each error
3. Error categorization by type
4. Changes since last successful build
5. Step-by-step fix suggestions
6. Instructions for Cursor AI

### ✅ GitHub Integration
- Creates GitHub issues automatically
- Uploads error reports as artifacts
- Generates workflow summaries
- Assigns issues to the workflow actor

## Monitored Workflows

The error handler monitors:
- `CI - B2C WebApp (Build, Test, and Lint)`
- `CI - Authentication WebApp (Build, Test, and Lint)`
- `Deploy B2C WebApp to Vercel`
- `Deploy Authentication WebApp to Railway`

## How It Works

1. **Trigger**: Workflow runs automatically when a monitored workflow fails
2. **Analysis**: Downloads logs, extracts errors, and finds last successful build
3. **Report Generation**: Creates comprehensive error report with:
   - All error messages
   - File paths and line numbers
   - Error categorization
   - Changes since last success
   - Fix suggestions
4. **Issue Creation**: Creates or updates a GitHub issue with the full report
5. **Artifacts**: Uploads error reports and logs as artifacts for 30 days

## Manual Usage

You can manually trigger the workflow:

1. Go to Actions → Error Handler and Reporter
2. Click "Run workflow"
3. Optionally provide:
   - Workflow Run ID (to analyze a specific failed run)
   - Workflow Name (to find the latest failure)

## Error Report Structure

Each error report contains:

1. **C# / .NET Compilation Errors** - All CS#### errors with file paths
2. **TypeScript / Angular Errors** - All TS#### errors with file paths
3. **NPM / Package Errors** - Dependency and package issues
4. **Build Configuration Errors** - angular.json and config issues
5. **Test Failures** - Test execution errors
6. **Linter Errors** - Linting warnings and errors
7. **Complete Error Log** - All error messages from logs
8. **Files with Errors** - File paths and line numbers
9. **Changes Since Last Success** - Git diff of changed files

## Fix Suggestions

The workflow automatically generates fix suggestions based on error types:

- **C# Errors**: Suggests checking using directives, attribute usage, and type definitions
- **TypeScript Errors**: Recommends version compatibility checks and configuration fixes
- **NPM Errors**: Provides commands to fix dependency issues
- **Build Configuration**: Suggests removing deprecated properties

## Using with Cursor AI

The error reports are designed to work with Cursor AI:

1. **Open the GitHub Issue** created by the workflow
2. **Copy the error report** or reference the issue
3. **Use Cursor AI** to:
   - Read the error messages
   - Identify file paths and line numbers
   - Apply suggested fixes
   - Verify changes resolve errors

## Artifacts

Error reports are saved as artifacts:
- `error-report.md` - Complete error report
- `fix-suggestions.md` - Fix suggestions
- `workflow-logs.txt` - Full workflow logs

Artifacts are retained for 30 days.

## Permissions

The workflow requires:
- `contents: read` - To read repository content
- `issues: write` - To create/update issues
- `pull-requests: write` - To comment on PRs
- `actions: read` - To read workflow runs
- `checks: read` - To read check results

## Troubleshooting

### Workflow Not Triggering

1. Check workflow names match exactly (case-sensitive)
2. Verify the monitored workflow actually failed
3. Check workflow permissions

### No Errors Found

1. Verify logs were downloaded successfully
2. Check if errors match the extraction patterns
3. Review workflow logs manually

### GitHub CLI Issues

1. Ensure `GITHUB_TOKEN` has required permissions
2. Check if GitHub CLI is available in the runner
3. Verify authentication is set up correctly

## Example Output

```markdown
# 🔴 Workflow Failure: CI - B2C WebApp (Build, Test, and Lint)

**Run ID:** 123456789
**Branch:** main
**Commit:** abc123def456
**Total Errors Found:** 15

## 📋 Comprehensive Error Report

### 1. C# / .NET Compilation Errors
error CS0182: An attribute argument must be a constant expression
  → AccountController.cs:175:40

### 2. TypeScript / Angular Errors
TS1345: An expression of type 'void' cannot be tested for truthiness
  → auth-callback.component.ts:95:16

### 3. Changes Since Last Successful Build
Modified: SDMSApps/SDMS.AuthenticationWebApp/Controllers/AccountController.cs
Modified: SDMSApps/SDMS.AuthenticationWebApp/Controllers/AuthorizationController.cs

## 🔧 Fix Suggestions

1. Fix C# attribute error by using string literal instead of constant
2. Fix TypeScript error by removing boolean check on void return type
...
```

## Support

For issues with the error handler itself, check:
- Workflow logs in Actions
- GitHub Issues created by the workflow
- Artifacts uploaded by the workflow

