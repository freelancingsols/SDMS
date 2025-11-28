# VS Code Commit Message Customization Guide

This guide explains how to customize commit message generation in VS Code, including the "Generate Commit Message" button feature.

**Important: This project uses ONE-LINE commit messages only (no paragraphs, no body, no footer).**

## Overview

VS Code offers several ways to generate and customize commit messages:
1. **GitHub Copilot Chat** - AI-powered commit message generation
2. **GitLens** - Advanced Git features with commit message templates
3. **Git Commit Message Template** - Standard Git template configuration
4. **VS Code Settings** - Built-in commit message formatting

## Commit Message Format (One-Line Only)

**Format:** `<type>(<scope>): <subject>`

**Rules:**
- ✅ ONE LINE ONLY (max 72 characters)
- ✅ No body, no footer, no paragraphs
- ✅ Subject: imperative mood, lowercase, no period, max 50 chars
- ✅ Scope: optional (auth, api, b2c, b2b, ui, workflow, etc.)

**Examples:**
- `feat(auth): add OAuth2 login support`
- `fix(b2c): resolve logout redirect URI trailing slash issue`
- `ci(workflow): add path filters to prevent unnecessary CI runs`
- `docs: update commit message customization guide`

---

## Method 1: GitHub Copilot Chat (Recommended)

### Setup
1. Install **GitHub Copilot Chat** extension
2. Open Source Control panel (Ctrl+Shift+G)
3. Stage your changes
4. Click the "Generate Commit Message" button (sparkle icon) or use Copilot Chat

### Customize Commit Message Format (One-Line Only)

Create a `.vscode/settings.json` file in your repository:

```json
{
  // ONE-LINE commit messages only
  "github.copilot.chat.commitMessagePrompt": "Generate a ONE-LINE commit message following Conventional Commits format. Format: <type>(<scope>): <subject>. Types: feat, fix, docs, style, refactor, test, chore, perf, ci, build. Scope: auth, api, b2c, b2b, ui, workflow, etc. Subject: imperative mood, lowercase, no period, max 50 chars. NO body, NO footer, NO paragraphs - just one line. Examples: feat(auth): add OAuth2 login support | fix(b2c): resolve logout redirect URI issue"
}
```

---

## Method 2: Git Commit Message Template

### Create Template File

Create a `.gitmessage` file in your repository root or home directory:

```bash
# .gitmessage
# <type>(<scope>): <subject>
#
# <body>
#
# <footer>
#
# Types:
#   feat:     A new feature
#   fix:      A bug fix
#   docs:     Documentation only changes
#   style:    Changes that do not affect the meaning of the code
#   refactor: A code change that neither fixes a bug nor adds a feature
#   perf:     A code change that improves performance
#   test:     Adding missing tests or correcting existing tests
#   chore:    Changes to the build process or auxiliary tools
#   ci:       Changes to CI configuration files and scripts
#   build:    Changes that affect the build system or dependencies
#
# Scope: Optional, what part of codebase (e.g., auth, api, ui)
# Subject: Imperative mood, lowercase, no period
# Body: Optional, explain what and why
# Footer: Optional, breaking changes or issue references
#
# Example:
# feat(auth): add OAuth2 login support
#
# Implemented OAuth2 authentication flow with Google and Auth0 providers.
# Added token refresh mechanism and error handling.
#
# Closes #123
```

### Configure Git to Use Template

**Global (all repositories):**
```bash
git config --global commit.template ~/.gitmessage
```

**Repository-specific:**
```bash
git config commit.template .gitmessage
```

**Or in VS Code settings.json:**
```json
{
  "git.template": ".gitmessage"
}
```

---

## Method 3: GitLens Extension

### Setup
1. Install **GitLens** extension
2. Configure commit message format in settings

### Settings

Add to `.vscode/settings.json`:

```json
{
  "gitlens.advanced.messages": {
    "suppressCommitHasNoPreviousCommitWarning": false,
    "suppressCommitNotFoundWarning": false,
    "suppressFileNotUnderSourceControlWarning": false,
    "suppressGitVersionWarning": false,
    "suppressLineUncommittedWarning": false,
    "suppressNoRepositoryWarning": false
  },
  
  // GitLens commit message format
  "gitlens.format": {
    "commit": "${message}",
    "commitMessage": "${message}"
  }
}
```

---

## Method 4: VS Code Built-in Settings

### Commit Message Formatting

Add to `.vscode/settings.json` or user settings:

```json
{
  // Enable commit message validation
  "git.enableCommitSigning": false,
  
  // Commit message length warning
  "git.inputValidation": "always",
  "git.inputValidationLength": 72,
  "git.inputValidationSubjectLength": 50,
  
  // Auto-format commit messages
  "git.format.enabled": true,
  
  // Commit message editor settings
  "editor.wordWrap": "on",
  "editor.rulers": [50, 72],
  
  // Snippet for commit messages
  "editor.snippetSuggestions": "top"
}
```

---

## Method 5: Custom Snippets for Commit Messages

Create `.vscode/commit-message.code-snippets`:

```json
{
  "Conventional Commit": {
    "prefix": "commit",
    "body": [
      "${1|feat,fix,docs,style,refactor,test,chore,perf,ci,build|}(${2:scope}): ${3:subject}",
      "",
      "${4:body}",
      "",
      "${5:footer}"
    ],
    "description": "Conventional Commit format"
  },
  "Feature Commit": {
    "prefix": "feat",
    "body": [
      "feat(${1:scope}): ${2:description}",
      "",
      "${3:details}"
    ],
    "description": "Feature commit"
  },
  "Bug Fix Commit": {
    "prefix": "fix",
    "body": [
      "fix(${1:scope}): ${2:description}",
      "",
      "Fixes ${3:issue-number}"
    ],
    "description": "Bug fix commit"
  }
}
```

---

## Recommended Commit Message Format (One-Line Only)

### Conventional Commits Standard (Simplified)

```
<type>(<scope>): <subject>
```

**NO body, NO footer - just one line!**

### Types
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `style`: Code style changes (formatting, no logic change)
- `refactor`: Code refactoring
- `perf`: Performance improvements
- `test`: Adding or updating tests
- `chore`: Maintenance tasks
- `ci`: CI/CD changes
- `build`: Build system changes

### Examples (One-Line Only)

```
feat(auth): add OAuth2 login support
fix(b2c): resolve logout redirect URI trailing slash issue
ci(workflow): add path filters to prevent unnecessary CI runs
docs: update commit message customization guide
refactor(auth): optimize logging configuration
```

---

## Quick Setup for Your Project

1. **Create `.vscode/settings.json` in repository root:**

```json
{
  "github.copilot.chat.commitMessagePrompt": "Generate a commit message following Conventional Commits format. Use types: feat, fix, docs, style, refactor, test, chore, perf, ci, build. Keep subject under 50 characters.",
  "git.inputValidation": "always",
  "git.inputValidationLength": 72,
  "git.inputValidationSubjectLength": 50,
  "editor.rulers": [50, 72]
}
```

2. **Create `.gitmessage` template:**

```bash
# <type>(<scope>): <subject>
#
# <body>
#
# <footer>
```

3. **Configure Git:**

```bash
git config commit.template .gitmessage
```

---

## Tips

1. **Use Copilot Chat**: Type `@workspace` and ask "Generate a commit message for my staged changes"
2. **Keyboard Shortcut**: Set up a shortcut for "Generate Commit Message"
3. **Validation**: Enable input validation to enforce message length
4. **Rulers**: Show rulers at 50 and 72 characters for guidance
5. **Snippets**: Use snippets for quick commit message templates

---

## Troubleshooting

### "Generate Commit Message" button not showing
- Ensure GitHub Copilot Chat extension is installed
- Check that you have staged changes
- Verify Copilot subscription is active

### Template not appearing
- Check Git config: `git config --list | grep commit.template`
- Verify template file path is correct
- Restart VS Code after configuration

### Commit message format not applying
- Check VS Code settings for commit message related options
- Verify extension settings (GitLens, Copilot)
- Check if workspace settings override user settings

---

## References

- [Conventional Commits](https://www.conventionalcommits.org/)
- [Git Commit Message Template](https://git-scm.com/docs/git-commit#_commit_templates)
- [VS Code Git Settings](https://code.visualstudio.com/docs/sourcecontrol/overview)
- [GitHub Copilot Chat](https://github.com/features/copilot)

