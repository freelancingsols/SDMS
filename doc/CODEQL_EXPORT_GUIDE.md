# GitHub CodeQL Code Scanning - Export Results Guide

This guide covers multiple methods to export CodeQL code scanning results from GitHub.

## Methods to Export CodeQL Results

### Method 1: Using GitHub API (Recommended for Automation)

#### Get Code Scanning Alerts via REST API

```bash
# Get all code scanning alerts for a repository
curl -H "Authorization: token YOUR_GITHUB_TOKEN" \
  -H "Accept: application/vnd.github.v3+json" \
  https://api.github.com/repos/OWNER/REPO/code-scanning/alerts

# Get alerts in SARIF format
curl -H "Authorization: token YOUR_GITHUB_TOKEN" \
  -H "Accept: application/sarif+json" \
  https://api.github.com/repos/OWNER/REPO/code-scanning/alerts
```

#### Get Specific Analysis Results

```bash
# List all analyses
curl -H "Authorization: token YOUR_GITHUB_TOKEN" \
  -H "Accept: application/vnd.github.v3+json" \
  https://api.github.com/repos/OWNER/REPO/code-scanning/analyses

# Get specific analysis by ID
curl -H "Authorization: token YOUR_GITHUB_TOKEN" \
  -H "Accept: application/vnd.github.v3+json" \
  https://api.github.com/repos/OWNER/REPO/code-scanning/analyses/ANALYSIS_ID
```

### Method 2: Using GitHub CLI (gh)

```bash
# Install GitHub CLI if not already installed
# Windows: winget install GitHub.cli
# macOS: brew install gh
# Linux: See https://cli.github.com/manual/installation

# Authenticate
gh auth login

# Export code scanning alerts to JSON
gh api repos/OWNER/REPO/code-scanning/alerts \
  --jq '.[] | {rule: .rule.id, severity: .rule.severity, message: .message.text, state: .state}' \
  > codeql-results.json

# Export as CSV
gh api repos/OWNER/REPO/code-scanning/alerts \
  --jq '.[] | [.rule.id, .rule.severity, .state, .most_recent_instance.location.path] | @csv' \
  > codeql-results.csv

# Get all analyses
gh api repos/OWNER/REPO/code-scanning/analyses > analyses.json
```

### Method 3: Export SARIF Files from CodeQL CLI

If you're running CodeQL locally or in CI/CD:

```bash
# Run CodeQL analysis and generate SARIF
codeql database create codeql-db --language=csharp --source-root=.
codeql database analyze codeql-db --format=sarif-latest --output=results.sarif

# Upload to GitHub (optional)
codeql github upload-results \
  --repository=OWNER/REPO \
  --ref=refs/heads/main \
  --commit=$(git rev-parse HEAD) \
  --sarif=results.sarif \
  --github-auth-stdin < <(echo $GITHUB_TOKEN)
```

### Method 4: Using GitHub Actions Workflow

Create a workflow to automatically export results:

```yaml
name: Export CodeQL Results

on:
  workflow_run:
    workflows: ["CodeQL"]
    types:
      - completed
  workflow_dispatch:

jobs:
  export-results:
    runs-on: ubuntu-latest
    permissions:
      security-events: read
      contents: read
    steps:
      - name: Checkout repository
        uses: actions/checkout@v4

      - name: Export CodeQL alerts
        env:
          GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
        run: |
          # Export to JSON
          gh api repos/${{ github.repository }}/code-scanning/alerts \
            --jq '.' > codeql-alerts.json
          
          # Export to CSV
          echo "Rule ID,Severity,State,Path,Message" > codeql-alerts.csv
          gh api repos/${{ github.repository }}/code-scanning/alerts \
            --jq '.[] | [.rule.id, .rule.severity, .state, .most_recent_instance.location.path, .message.text] | @csv' \
            >> codeql-alerts.csv
          
          # Export analyses
          gh api repos/${{ github.repository }}/code-scanning/analyses \
            --jq '.' > codeql-analyses.json

      - name: Upload artifacts
        uses: actions/upload-artifact@v4
        with:
          name: codeql-results
          path: |
            codeql-alerts.json
            codeql-alerts.csv
            codeql-analyses.json
          retention-days: 90
```

### Method 5: Manual Export from GitHub UI

1. Navigate to your repository on GitHub
2. Go to **Security** tab
3. Click on **Code scanning alerts**
4. Use the filters to narrow down results
5. Click the **Export** button (if available) or use browser developer tools to extract data

### Method 6: PowerShell Script for Windows

```powershell
# Export CodeQL Results using PowerShell
$repo = "OWNER/REPO"
$token = "YOUR_GITHUB_TOKEN"
$headers = @{
    "Authorization" = "token $token"
    "Accept" = "application/vnd.github.v3+json"
}

# Get all alerts
$alerts = Invoke-RestMethod -Uri "https://api.github.com/repos/$repo/code-scanning/alerts" -Headers $headers

# Export to JSON
$alerts | ConvertTo-Json -Depth 10 | Out-File "codeql-results.json"

# Export to CSV
$alerts | Select-Object -Property @(
    @{Name='RuleID'; Expression={$_.rule.id}},
    @{Name='Severity'; Expression={$_.rule.severity}},
    @{Name='State'; Expression={$_.state}},
    @{Name='Path'; Expression={$_.most_recent_instance.location.path}},
    @{Name='Message'; Expression={$_.message.text}}
) | Export-Csv -Path "codeql-results.csv" -NoTypeInformation

Write-Host "Results exported to codeql-results.json and codeql-results.csv"
```

## Required Permissions

For GitHub API access, you need:
- **Personal Access Token (PAT)** with `security_events:read` scope
- Or use `GITHUB_TOKEN` in GitHub Actions with `security-events: read` permission

## Output Formats

### JSON Format
```json
{
  "number": 1,
  "rule": {
    "id": "cs/insecure-random",
    "severity": "error",
    "description": "..."
  },
  "message": {
    "text": "Using a cryptographically weak random number generator"
  },
  "state": "open",
  "most_recent_instance": {
    "location": {
      "path": "src/Example.cs",
      "start_line": 42
    }
  }
}
```

### SARIF Format
SARIF (Static Analysis Results Interchange Format) is the standard format for static analysis results and can be imported into various tools.

## Filtering Results

You can filter results by:
- **State**: `open`, `dismissed`, `fixed`
- **Severity**: `error`, `warning`, `note`
- **Tool**: `CodeQL`, `CodeQL-Language`, etc.
- **Language**: Filter by programming language

Example API call with filters:
```bash
gh api repos/OWNER/REPO/code-scanning/alerts?state=open&severity=error
```

## Automation Tips

1. **Schedule Regular Exports**: Use GitHub Actions scheduled workflows
2. **Store Results**: Upload artifacts or commit to a dedicated branch
3. **Generate Reports**: Use jq or PowerShell to format results
4. **Track Trends**: Compare results over time to track improvements

## References

- [GitHub Code Scanning API Documentation](https://docs.github.com/en/rest/code-scanning)
- [CodeQL CLI Documentation](https://codeql.github.com/docs/codeql-cli/)
- [SARIF Format Specification](https://docs.oasis-open.org/sarif/sarif/v2.1.0/sarif-v2.1.0.html)

