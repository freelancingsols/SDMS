# Automated Error Fixing with GitHub Actions and Cursor

## Overview

While Cursor doesn't have a direct built-in integration with GitHub Actions, there are several approaches to enable automated error detection and fixing.

## Option 1: GitHub Actions API Integration (Recommended)

### How It Works

1. **GitHub Actions exposes logs via REST API**
2. **Webhook triggers on workflow failures**
3. **Custom script processes errors and creates fixes**

### Implementation Steps

#### Step 1: Create a GitHub App or Personal Access Token

1. Go to GitHub Settings → Developer settings → Personal access tokens
2. Create a token with permissions:
   - `repo` (full control)
   - `actions:read` (read workflow runs)
   - `actions:write` (rerun workflows)

#### Step 2: Add Error Detection to Workflows

Add a step that captures errors and sends them to an API:

```yaml
- name: Capture and Report Errors
  if: failure()
  run: |
    # Capture error logs
    ERROR_LOG=$(cat ${{ github.event.workflow_run.logs_url }} 2>/dev/null || echo "Error occurred")
    
    # Send to API endpoint (you'd need to create this)
    curl -X POST https://your-api-endpoint.com/github-errors \
      -H "Content-Type: application/json" \
      -d "{
        \"repository\": \"${{ github.repository }}\",
        \"workflow\": \"${{ github.workflow }}\",
        \"run_id\": \"${{ github.run_id }}\",
        \"error_log\": \"$ERROR_LOG\",
        \"commit_sha\": \"${{ github.sha }}\"
      }"
```

#### Step 3: Create Error Processing Service

You'd need to create a service that:
1. Receives error logs
2. Uses AI/LLM to analyze errors
3. Generates fixes
4. Creates pull requests with fixes

## Option 2: GitHub Actions with Cursor CLI Integration

### Using Cursor's API (if available)

If Cursor exposes an API, you could:

```yaml
- name: Auto-fix Errors
  if: failure()
  run: |
    # Capture error
    ERROR=$(cat error.log)
    
    # Call Cursor API (hypothetical)
    curl -X POST https://api.cursor.sh/fix \
      -H "Authorization: Bearer $CURSOR_API_KEY" \
      -d "{
        \"error\": \"$ERROR\",
        \"file\": \"${{ github.event.pull_request.head.ref }}\",
        \"context\": \"${{ github.repository }}\"
      }"
```

## Option 3: GitHub Webhook + Custom Service

### Setup Webhook

1. **Repository Settings** → **Webhooks** → **Add webhook**
2. **Payload URL**: Your service endpoint
3. **Events**: Select "Workflow runs"
4. **Active**: Checked

### Webhook Payload Structure

```json
{
  "action": "completed",
  "workflow_run": {
    "id": 123456,
    "name": "CI - B2C WebApp",
    "status": "failure",
    "conclusion": "failure",
    "logs_url": "https://api.github.com/repos/owner/repo/actions/runs/123456/logs",
    "head_sha": "abc123..."
  }
}
```

### Service Implementation (Node.js Example)

```javascript
// webhook-service.js
const express = require('express');
const { Octokit } = require('@octokit/rest');
const app = express();

app.use(express.json());

app.post('/webhook', async (req, res) => {
  const { workflow_run } = req.body;
  
  if (workflow_run.status === 'completed' && workflow_run.conclusion === 'failure') {
    // Fetch error logs
    const octokit = new Octokit({ auth: process.env.GITHUB_TOKEN });
    const logs = await octokit.actions.downloadWorkflowRunLogs({
      owner: 'your-org',
      repo: 'your-repo',
      run_id: workflow_run.id
    });
    
    // Analyze error (using AI service)
    const fix = await analyzeAndFix(logs);
    
    // Create fix branch and PR
    await createFixPR(fix, workflow_run.head_sha);
  }
  
  res.status(200).send('OK');
});

async function analyzeAndFix(errorLog) {
  // Use OpenAI, Anthropic, or similar to analyze error
  // Return suggested fixes
}

async function createFixPR(fix, baseSha) {
  // Create branch, apply fixes, create PR
}

app.listen(3000);
```

## Option 4: GitHub Actions Self-Healing Workflow

### Auto-Retry with Fix Attempts

```yaml
name: Self-Healing CI

on:
  workflow_run:
    workflows: ["CI - B2C WebApp"]
    types: [completed]

jobs:
  auto-fix:
    if: ${{ github.event.workflow_run.conclusion == 'failure' }}
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      
      - name: Analyze Error
        id: analyze
        run: |
          # Download logs
          gh run view ${{ github.event.workflow_run.id }} --log > error.log
          
          # Extract error patterns
          ERROR=$(grep -i "error\|failed\|not found" error.log | head -20)
          echo "error=$ERROR" >> $GITHUB_OUTPUT
      
      - name: Attempt Auto-Fix
        run: |
          ERROR="${{ steps.analyze.outputs.error }}"
          
          # Common fixes
          if [[ "$ERROR" == *"package-lock.json"* ]]; then
            echo "Fixing package-lock.json issue..."
            rm -f package-lock.json
            npm install --legacy-peer-deps
          fi
          
          if [[ "$ERROR" == *"angular-devkit"* ]]; then
            echo "Fixing Angular version issue..."
            npm install @angular-devkit/build-angular@^18.0.0 --legacy-peer-deps
          fi
      
      - name: Re-run Workflow
        run: |
          gh workflow run "CI - B2C WebApp" --ref ${{ github.event.workflow_run.head_branch }}
```

## Option 5: Cursor Extension/Plugin Development

### Create Custom Cursor Extension

If you want Cursor to monitor GitHub Actions:

1. **Use Cursor Extension API** (if available)
2. **Poll GitHub API** for workflow status
3. **Display errors in Cursor** when detected
4. **Suggest fixes** using Cursor's AI

### Example Extension Structure

```typescript
// cursor-github-extension.ts
import * as vscode from 'vscode';
import { Octokit } from '@octokit/rest';

export function activate(context: vscode.ExtensionContext) {
  const octokit = new Octokit({
    auth: vscode.workspace.getConfiguration().get('github.token')
  });
  
  // Poll for workflow failures
  setInterval(async () => {
    const runs = await octokit.actions.listWorkflowRuns({
      owner: 'your-org',
      repo: 'your-repo',
      status: 'failure'
    });
    
    for (const run of runs.data.workflow_runs) {
      const logs = await octokit.actions.downloadWorkflowRunLogs({
        owner: 'your-org',
        repo: 'your-repo',
        run_id: run.id
      });
      
      // Show error in Cursor
      vscode.window.showErrorMessage(
        `GitHub Actions failed: ${run.name}`,
        'View Logs', 'Auto-Fix'
      ).then(selection => {
        if (selection === 'Auto-Fix') {
          // Trigger Cursor AI to fix
          vscode.commands.executeCommand('cursor.fix', logs);
        }
      });
    }
  }, 60000); // Check every minute
}
```

## Option 6: GitHub Actions + Cursor Desktop Integration

### Using Cursor's Local API

If Cursor exposes a local API endpoint:

```yaml
- name: Send Error to Cursor
  if: failure()
  run: |
    ERROR_LOG=$(cat error.log)
    
    # Send to local Cursor instance
    curl -X POST http://localhost:8080/cursor/fix \
      -H "Content-Type: application/json" \
      -d "{
        \"error\": \"$ERROR_LOG\",
        \"workspace\": \"${{ github.workspace }}\"
      }"
```

## Practical Recommendation

### Immediate Solution: Enhanced Error Reporting

Add better error capture to existing workflows:

```yaml
- name: Capture Build Errors
  if: failure()
  run: |
    echo "## Build Failed ❌" >> $GITHUB_STEP_SUMMARY
    echo "### Error Details" >> $GITHUB_STEP_SUMMARY
    echo '```' >> $GITHUB_STEP_SUMMARY
    cat error.log >> $GITHUB_STEP_SUMMARY 2>&1 || echo "No error log found"
    echo '```' >> $GITHUB_STEP_SUMMARY
    
    # Create issue with error details
    gh issue create \
      --title "CI Failed: ${{ github.workflow }}" \
      --body "Error in run ${{ github.run_id }}\n\n\`\`\`\n$(cat error.log)\n\`\`\`" \
      --label "ci-failure,auto-generated"
```

### Medium-Term Solution: Webhook Service

1. Set up a simple webhook receiver
2. Use GitHub API to fetch logs
3. Use AI service (OpenAI, Anthropic) to analyze
4. Generate fixes and create PRs

### Long-Term Solution: Cursor Integration

1. Wait for Cursor to expose API
2. Or develop custom extension
3. Or use Cursor's existing AI features manually

## Security Considerations

- **Never commit API keys** to repositories
- **Use GitHub Secrets** for sensitive tokens
- **Validate webhook signatures** to prevent unauthorized access
- **Rate limit** API calls to avoid abuse
- **Review auto-generated PRs** before merging

## Current Limitations

1. **Cursor doesn't have public API** for direct integration
2. **GitHub Actions logs** are large and need parsing
3. **Error context** requires understanding full codebase
4. **Auto-fixes** need human review before merging

## Best Practice: Manual Review with Better Tooling

For now, the best approach is:

1. **Enhanced error reporting** in workflows (see above)
2. **Use Cursor's AI** manually when errors occur
3. **Create templates** for common fixes
4. **Monitor GitHub Actions** dashboard regularly

## Example: Enhanced Error Workflow

See `.github/workflows/error-handler.yml` for a complete example of error capture and reporting.

