# DevHub Workshop: GitHub Actions & Secure SDLC

Welcome to the DevHub Workshop! This hands-on workshop teaches you how to build a complete CI/CD pipeline with security built-in from the start, following Secure Software Development Lifecycle (SSDLC) principles.

## What You'll Learn

By the end of this workshop, you'll have practical experience with:
- Building automated CI/CD pipelines using GitHub Actions
- Implementing automated testing and code quality checks
- Integrating security scanning at multiple stages (shift-left security)
- Managing dependencies and tracking vulnerabilities
- Generating Software Bill of Materials (SBOM) for supply chain security
- Automating version management with Git history

## About This Project

This is a .NET 9.0 ShoppingCart API with:
- RESTful endpoints for managing shopping carts
- SQL Server database with automated migrations (DbUp)
- Integration and unit tests (NUnit + Testcontainers)
- Intentional security vulnerabilities for learning purposes

**Note**: This application contains deliberate security issues. Finding and fixing them is part of the learning experience!

## Prerequisites

- GitHub account
- Access to 1Password (for SonarQube and DependencyTrack credentials - your instructor will provide access)
- Basic understanding of Git and GitHub
- Familiarity with .NET is helpful but not required

## Getting Started

1. **Fork this repository** to your own GitHub account
2. Navigate to `.github/workflows/ci-pipeline.yml` - this is your starting point
3. The basic pipeline structure is already in place (checkout, setup .NET, restore, build)
4. You'll enhance this pipeline step-by-step throughout the workshop

## Workshop Structure

The workshop follows the SSDLC lifecycle. Each section builds on the previous one, but you can work at your own pace.

---

## Phase 1: Foundation - Build & Version Management

### 1.1 Understanding the Baseline Pipeline

**What**: The existing pipeline performs basic build operations
**Why**: Before adding security and quality checks, we need a working foundation
**How**:
- Review `.github/workflows/ci-pipeline.yml`
- Notice it uses .NET 9.0 (important for later steps)
- Trigger the workflow by pushing a commit or opening a PR

**📚 Learn More**: [GitHub Actions Basics](https://docs.github.com/en/actions/learn-github-actions)

### 1.2 Implement Automatic Versioning (GitVersion)

**What**: Automatically generate semantic version numbers from your Git history

**Why**:
- Manual versioning is error-prone and inconsistent
- Every build artifact needs a unique, traceable version
- Critical for tracking which version has which security fixes
- Enables reproducible builds and rollback capabilities

**How**:
GitVersion requires two steps in your pipeline:

1. **Setup GitVersion** ([documentation](https://github.com/GitTools/actions/blob/main/docs/examples/github/gitversion/setup.md))
   - Installs the GitVersion tool

2. **Execute GitVersion** ([documentation](https://github.com/GitTools/actions/blob/main/docs/examples/github/gitversion/execute.md))
   - Generates version numbers based on your Git tags and branch names
   - Outputs variables you can use in later steps (e.g., `${{ steps.gitversion.outputs.semVer }}`)

**💡 Tip for Juniors**: GitVersion analyzes your Git commits and branches to automatically calculate version numbers. For example, commits on `main` might be `1.0.0`, while a feature branch might be `1.1.0-feature.1`.

**💡 Tip for Seniors**: Consider configuring GitVersion modes (Mainline vs GitFlow) based on your branching strategy. The default configuration is in GitVersion.yml if you need to customize behavior.

---

## Phase 2: Quality Assurance - Testing & Code Quality

### 2.1 Automated Testing

**What**: Run your test suite automatically on every build

**Why** (SSDLC Perspective):
- Catch bugs before they reach production
- Prevent security regressions (tests can verify security fixes)
- Ensure new features don't break existing functionality
- Required for compliance in many security frameworks (SOC 2, ISO 27001)

**How**:
- Add a test step to your pipeline using `dotnet test`
- Reference: [GitHub's .NET Testing Tutorial](https://docs.github.com/en/actions/tutorials/build-and-test-code/net)
- Your test command: `dotnet test src/ShoppingCart.sln --configuration Release`

**📊 Success Criteria**: Pipeline should fail if any tests fail

**💡 Tip for Juniors**: The project uses NUnit for testing. Tests are in `src/ShoppingCart.Tests/`. Integration tests use Testcontainers to spin up a real SQL Server for testing.

**💡 Tip for Seniors**: Consider adding code coverage reporting using `--collect:"XPlat Code Coverage"` and failing the build if coverage drops below a threshold.

### 2.2 Code Linting (ReSharper)

**What**: Automatically check code for style issues, potential bugs, and code smells

**Why** (SSDLC Perspective):
- Identifies code patterns that often lead to security vulnerabilities
- Enforces consistent code style, making security reviews easier
- Catches common mistakes before human review
- Some findings may indicate security issues (e.g., SQL injection risks, missing null checks)

**How** (Two Approaches):

**Option A - Use the provided script** (recommended for learning):
1. Use [github-script action](https://github.com/actions/github-script) to run commands
2. Install JetBrains.ReSharper.GlobalTools: `dotnet tool install -g JetBrains.ReSharper.GlobalTools`
3. Run inspection: `jb inspectcode src/ShoppingCart.sln --output=inspection-results.json --format=Json`
4. In a separate step, analyze results with `.github/workflows/scripts/analyzeInspectCodeOutput.ps1`
5. The script will fail the build if issues are found (severity warning or higher)

**Option B - Use the marketplace action** (simpler):
- Use the [JetBrains ReSharper Inspect Code action](https://github.com/marketplace/actions/jetbrains-resharper-inspect-code)

**💡 Tip**: The PowerShell script shows how to parse JSON results and fail builds programmatically - useful for custom tooling!

### 2.3 Static Application Security Testing (SAST) - SonarQube

**What**: Deep code analysis to find security vulnerabilities, bugs, and code quality issues

**Why** (SSDLC Perspective):
- Identifies security vulnerabilities in source code (SAST is a core SSDLC practice)
- Finds issues like SQL injection, XSS, authentication problems
- Tracks technical debt and code quality trends over time
- Provides security ratings and remediation guidance
- Often required for compliance (PCI-DSS, HIPAA, etc.)

**How**:
1. **Access SonarQube**:
   - Open 1Password and find the SonarQube item
   - Click the website URL and login with the provided credentials

2. **Create Your Project**:
   - Click "Create Project"
   - Name it: `<your-initials>-shoppingCart` (e.g., `jd-shoppingCart`)
   - Follow the setup wizard provided by SonarQube

3. **Generate Access Token**:
   - Navigate to: User Icon → My Account → Security
   - Generate a new token for your pipeline
   - Store it as a GitHub Secret: `Settings → Secrets and variables → Actions → New repository secret`

4. **Add SonarQube Analysis to Pipeline**:
   - Follow the instructions provided by SonarQube for .NET projects
   - Typically involves three steps: begin analysis, build, end analysis

**🔍 What to Look For**:
- Security Hotspots (require manual review)
- Vulnerabilities (confirmed security issues)
- Code Smells (maintainability issues)

**💡 Tip for Juniors**: SonarQube will show you a security rating (A-E). Vulnerabilities are ranked by severity (Blocker, Critical, Major, Minor). Start with Blockers and Critical issues.

**💡 Tip for Seniors**: Configure Quality Gates to fail the build if new vulnerabilities are introduced. Consider setting up PR decoration to show issues inline in pull requests.

---

## Phase 3: Supply Chain Security - Dependencies

### 3.1 Software Bill of Materials (SBOM)

**What**: Generate a complete inventory of all software components and dependencies in your application

**Why** (SSDLC Perspective):
- Critical for supply chain security (know what's in your software)
- Required for responding to zero-day vulnerabilities (Log4Shell, etc.)
- Enables tracking vulnerable components across your organization
- Increasingly required by regulations (e.g., Executive Order 14028 in US)
- Helps with license compliance

**How**:

1. **Generate SBOM**:
   - Use [CycloneDX .NET SBOM Generator](https://github.com/CycloneDX/gh-dotnet-generate-sbom)
   - This creates a machine-readable SBOM in CycloneDX format
   - The SBOM lists every NuGet package, their versions, and transitive dependencies

2. **Upload to DependencyTrack**:
   - Access DependencyTrack credentials from 1Password
   - Create a new project in DependencyTrack (name it: `<your-initials>-shoppingCart`)
   - Get the project API key
   - Use [DependencyTrack upload action](https://github.com/DependencyTrack/gh-upload-sbom) to push your SBOM
   - Store credentials as GitHub Secrets

3. **Review Results**:
   - DependencyTrack will analyze your dependencies against multiple vulnerability databases
   - Check for known vulnerabilities (CVEs) in your dependencies
   - Review risk score and affected components

**📊 Success Criteria**: SBOM is generated and uploaded on every build, and you can see your project's dependencies in DependencyTrack

**💡 Tip for Juniors**: An SBOM is like an "ingredients label" for software. Just like food labels help identify allergens, SBOMs help identify vulnerable components.

**💡 Tip for Seniors**: Consider implementing policies in DependencyTrack to auto-fail builds when high-severity vulnerabilities are detected. Integrate with your ticketing system for automated remediation tracking.

### 3.2 Automated Dependency Updates (Dependabot)

**What**: Automatically create pull requests to update dependencies when new versions are released

**Why** (SSDLC Perspective):
- Keeping dependencies updated reduces security vulnerabilities
- Automates a tedious but critical security task
- Reduces the "update debt" that makes upgrades painful
- Provides early warning of breaking changes

**How**:

1. **Create a Dependabot configuration** (NOT a workflow file):
   - Create `.github/dependabot.yml` (NOT in workflows/)
   - Configure for NuGet packages
   - [Dependabot Configuration Documentation](https://docs.github.com/en/code-security/dependabot/dependabot-version-updates/configuration-options-for-the-dependabot.yml-file)

2. **Suggested Configuration**:
   ```yaml
   version: 2
   updates:
     - package-ecosystem: "nuget"
       directory: "/src"
       schedule:
         interval: "weekly"  # or "daily" for more active monitoring
       open-pull-requests-limit: 10
   ```

3. **Optional - Automate Dependabot with GitHub Actions**:
   - Create a separate workflow to automatically approve/merge low-risk updates
   - [Automating Dependabot Documentation](https://docs.github.com/en/code-security/dependabot/working-with-dependabot/automating-dependabot-with-github-actions)

**📊 Success Criteria**: Dependabot creates PRs when updates are available. Your pipeline runs on these PRs to validate they don't break anything.

**💡 Tip**: Don't auto-merge without testing! Let your pipeline validate Dependabot PRs first.

---

## Phase 4: Security Scanning - Detect Vulnerabilities

### 4.1 Secret Scanning (GitLeaks)

**What**: Scan your codebase and Git history for accidentally committed secrets (API keys, passwords, tokens)

**Why** (SSDLC Perspective):
- Leaked secrets are a leading cause of security breaches
- Secrets in Git history remain forever unless rewritten
- Attackers actively scan GitHub for leaked credentials
- Catching secrets before they're pushed is critical (shift-left security)

**How**:
1. Add [Gitleaks Action](https://github.com/marketplace/actions/gitleaks) to your pipeline
2. Run it early in your pipeline (fail fast if secrets detected)
3. Configure as a required check to prevent merging code with secrets

**⚠️ Challenge**: This application contains intentionally leaked secrets for learning purposes. **Find them and fix them!**

**How to Fix Leaked Secrets**:
- Move secrets to GitHub Secrets (Settings → Secrets and variables → Actions)
- Reference them in workflows with `${{ secrets.SECRET_NAME }}`
- For local development, use environment variables or user secrets
- NEVER commit actual secrets to Git

**💡 Tip for Juniors**: Even if you delete a file with secrets, it remains in Git history. Use `git log` and `git show` to see historical commits.

**💡 Tip for Seniors**: Consider using pre-commit hooks with Gitleaks to catch secrets before they're committed. For already-committed secrets, consider tools like BFG Repo-Cleaner or git-filter-repo, but remember: once pushed to public repos, consider secrets compromised and rotate them.

### 4.2 Dynamic Application Security Testing (DAST) - OWASP ZAP

**What**: Scan your running application for security vulnerabilities by acting like an attacker

**Why** (SSDLC Perspective):
- DAST finds vulnerabilities that only appear at runtime (complementary to SAST)
- Tests the actual deployed configuration, not just code
- Finds issues like authentication bypasses, injection flaws, misconfigurations
- Simulates real attacker behavior
- SAST + DAST together provide comprehensive coverage

**How**:
1. Your application needs to be running for ZAP to scan it
2. Options for hosting during the scan:
   - Start the app in a pipeline step: `dotnet run --project src/ShoppingCart/ShoppingCart.csproj &`
   - Use Docker Compose to run app + database together
   - Deploy to a temporary environment
3. Use [OWASP ZAP Full Scan Action](https://github.com/zaproxy/action-full-scan)
4. Configure the target URL (e.g., `http://localhost:5000` or your deployed URL)
5. Review the scan results - ZAP will find vulnerabilities in your API

**🎯 Challenge - "Final Boss"**:
- Get OWASP ZAP scanning your running application
- Review the findings - ZAP will discover several vulnerabilities
- Work to fix the issues it identifies
- Re-run the scan to verify fixes

**💡 Tip for Juniors**:
- OWASP Top 10 is the list of most common web vulnerabilities. ZAP scans for these.
- Common findings: SQL Injection, Cross-Site Scripting (XSS), Broken Authentication
- Start with High and Medium severity findings

**💡 Tip for Seniors**:
- Configure ZAP rules to reduce false positives
- Consider using ZAP's API scan mode for REST APIs (faster than full scan)
- Integrate with DefectDojo or other vulnerability management platforms
- Use ZAP authentication scripts for scanning behind login
- Consider baseline scans to only alert on new vulnerabilities

**⚠️ Note**: ZAP scans can be time-consuming. You might want to run them on a schedule (nightly) rather than on every commit, or use faster scan modes for PR checks.

---

## Complete SSDLC Pipeline Overview

When you've completed all sections, your pipeline will implement defense-in-depth:

```
┌─────────────────────────────────────────────────────────────┐
│  Pre-Commit (Local)                                         │
│  └─ GitLeaks Pre-commit Hook (Optional)                    │
└─────────────────────────────────────────────────────────────┘
                           ↓
┌─────────────────────────────────────────────────────────────┐
│  Phase 1: Build & Version                                   │
│  ├─ Checkout Code                                           │
│  ├─ Setup .NET                                              │
│  ├─ GitVersion (Generate Version)                           │
│  └─ Restore & Build                                         │
└─────────────────────────────────────────────────────────────┘
                           ↓
┌─────────────────────────────────────────────────────────────┐
│  Phase 2: Quality & SAST                                    │
│  ├─ Run Tests (Unit + Integration)                          │
│  ├─ ReSharper Linting                                       │
│  └─ SonarQube SAST Scan                                     │
└─────────────────────────────────────────────────────────────┘
                           ↓
┌─────────────────────────────────────────────────────────────┐
│  Phase 3: Supply Chain Security                             │
│  ├─ Generate SBOM (CycloneDX)                               │
│  └─ Upload to DependencyTrack                               │
└─────────────────────────────────────────────────────────────┘
                           ↓
┌─────────────────────────────────────────────────────────────┐
│  Phase 4: Security Scanning                                 │
│  ├─ GitLeaks (Secret Detection)                             │
│  └─ OWASP ZAP DAST Scan (on running app)                    │
└─────────────────────────────────────────────────────────────┘
                           ↓
┌─────────────────────────────────────────────────────────────┐
│  Deploy (Future Enhancement)                                │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│  Continuous Monitoring                                       │
│  └─ Dependabot (Weekly Dependency Updates)                  │
└─────────────────────────────────────────────────────────────┘
```

---

## Tips for Success

### For Junior Developers
- **Take it step by step**: Complete each section before moving to the next
- **Read the documentation links**: They provide context and examples
- **Check the logs**: When a workflow fails, click on the failed step to see why
- **Ask questions**: Security and DevOps can be complex - don't hesitate to ask
- **Experiment**: Try breaking things intentionally to understand how they work

### For Senior Developers
- **Think about integration**: How would these tools work together in your organization?
- **Consider the bigger picture**: What's missing? (e.g., deployment, monitoring, incident response)
- **Evaluate the tools**: Are there better alternatives for your context?
- **Security trade-offs**: Each tool has performance and maintenance costs - are they worth it?
- **Help others**: Share your experience with junior participants

### Common Pitfalls
- **Secrets in code**: Remember to use GitHub Secrets, not hardcoded values
- **Pipeline parallelization**: Some steps can run in parallel, others must be sequential
- **Caching**: Consider caching NuGet packages to speed up builds
- **Timeout limits**: DAST scans can exceed GitHub's default timeout (60 min) - adjust as needed
- **Branch protection**: Configure branch protection rules to enforce pipeline checks

---

## Resources & References

### GitHub Actions
- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [GitHub Actions Marketplace](https://github.com/marketplace?type=actions)
- [Workflow Syntax Reference](https://docs.github.com/en/actions/using-workflows/workflow-syntax-for-github-actions)

### Security Frameworks
- [OWASP Top 10](https://owasp.org/www-project-top-ten/)
- [OWASP Software Assurance Maturity Model (SAMM)](https://owaspsamm.org/)
- [NIST Secure Software Development Framework (SSDF)](https://csrc.nist.gov/Projects/ssdf)

### .NET Specific
- [.NET Security Best Practices](https://learn.microsoft.com/en-us/dotnet/standard/security/)
- [Secure a .NET Web API](https://learn.microsoft.com/en-us/aspnet/core/security/)

### Supply Chain Security
- [CycloneDX SBOM Standard](https://cyclonedx.org/)
- [CISA's SBOM Resources](https://www.cisa.gov/sbom)

---

## What's Next?

After completing this workshop, consider:

1. **Extend the pipeline**: Add deployment stages (dev → staging → production)
2. **Implement monitoring**: Add application performance monitoring (APM) and logging
3. **Add more tests**: Increase code coverage, add performance tests
4. **Harden the application**: Fix all vulnerabilities found by the scans
5. **Document findings**: Create a security report of what you found and fixed
6. **Share your knowledge**: Present your pipeline to your team

---

## Feedback & Questions

If you encounter issues or have suggestions for improving this workshop, please open an issue or discuss with your instructor.

Happy learning, and remember: **Security is not a feature, it's a foundation!** 🔒
