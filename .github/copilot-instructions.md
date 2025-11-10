# GitHub Copilot Instructions

This file contains guidelines for GitHub Copilot to ensure consistent, high-quality code generation and adherence to project standards.

## Project Context

This is a .NET 9.0 monorepo following arc42 documentation standards. The repository structure consists of:
- `/src` - Product source code
- `/docs` - arc42 architecture documentation
- `/scripts` - Utility scripts for development and pipelines

## Code Generation Rules

### 1. Follow the Definition of Done

**ALWAYS** ensure that generated code and suggestions align with the requirements in `Definition_of_Done.md`:
- Write or update arc42 documentation for architectural changes
- Include unit tests for new functionality
- Follow the `.editorconfig` style guidelines
- Implement appropriate error handling and logging
- Document technical debt and architecture decisions

### 2. Documentation Standards

When generating or modifying code:
- Update relevant arc42 documentation in `/docs`
- Add architecture decisions to `/docs/09_architecture_decisions.md`
- Document cross-cutting concerns in `/docs/08_cross_cutting_concepts.md`
- Add new terms to the glossary in `/docs/12_glossary.md`
- Update the Table of Contents in `/docs/README.md` if adding new sections

### 3. Code Style and Quality

- Use C# 12+ features appropriately
- Enable nullable reference types (`<Nullable>enable</Nullable>`)
- Prefer explicit types over `var` when clarity is improved
- Use dependency injection for loose coupling
- Follow SOLID principles
- Implement the repository pattern for data access
- Use async/await for I/O operations

### 4. Testing Requirements

When generating production code:
- Generate corresponding unit tests using xUnit
- Use descriptive test method names (e.g., `MethodName_Scenario_ExpectedBehavior`)
- Include test cases for edge cases and error conditions
- Mock dependencies using Moq or NSubstitute
- Aim for 80%+ code coverage

### 5. Project Structure

- Place new projects in `/src`
- Follow the naming convention: `CompanyName.ProjectName`
- Add projects to `solution.slnx`
- Use `Directory.Packages.props` for package version management
- Never hardcode package versions in `.csproj` files

### 6. Configuration and Settings

- Use strongly-typed configuration classes
- Validate configuration on startup
- Store secrets in Azure Key Vault or User Secrets (development)
- Document all configuration options

### 7. Error Handling

- Use specific exception types
- Log exceptions with appropriate severity levels
- Include contextual information in exception messages
- Implement retry logic for transient failures
- Use Result pattern or custom result types for expected failures

### 8. Logging

- Use structured logging with Microsoft.Extensions.Logging
- Include correlation IDs for distributed tracing
- Log at appropriate levels (Trace, Debug, Information, Warning, Error, Critical)
- Never log sensitive information (passwords, tokens, PII)

### 9. Security Best Practices

- Validate all user input
- Use parameterized queries to prevent SQL injection
- Implement authentication and authorization appropriately
- Follow the principle of least privilege
- Keep dependencies up to date

### 10. Performance Considerations

- Use `IAsyncEnumerable<T>` for streaming large datasets
- Implement caching where appropriate
- Use `Span<T>` and `Memory<T>` for performance-critical code
- Profile and measure before optimizing
- Document performance requirements and SLAs

## Specific Guidance

### Branching and Pull Request Workflow

**ALWAYS** follow this workflow for any changes to the repository:

1. **Create a Feature Branch**
   - Never commit directly to `main`
   - Create a new branch for each feature, fix, or task
   - Branch naming: `feat/description`, `fix/description`, `docs/description`, `refactor/description`
   - Example: `feat/user-authentication`, `fix/database-timeout`, `docs/arc42-building-blocks`

2. **Make Small, Focused Commits**
   - Keep commits small and atomic
   - Each commit should represent a single logical change
   - Link commits to issues using keywords (e.g., `Relates to #42`, `See #42`)
   - A single commit does NOT need to resolve the entire issue
   - Use Conventional Commits format: `<type>(<scope>): <description>`
   - Examples:
     - `feat(auth): add user login endpoint (relates to #42)`
     - `test(auth): add unit tests for login validation (relates to #42)`
     - `docs(api): document authentication flow (relates to #42)`

3. **Create Pull Request When Work is Complete**
   - The PR (not individual commits) resolves the issue
   - Use PR description to close issues: `Closes #42` or `Fixes #42`
   - Ensure all Definition of Done criteria are met
   - Request code review before merging

### When Creating New Projects
1. Add project to `/src` directory
2. Reference in `b-ro.sln`
3. Use Central Package Management
4. Include appropriate README.md in project folder
5. Set up project structure following clean architecture

### When Committing Changes

**BEFORE COMMITTING, ALWAYS:**
1. **Verify Code Formatting**
   - Run `dotnet format --verify-no-changes` to check formatting
   - If formatting issues exist, run `dotnet format` to fix them
   - Ensure all files follow `.editorconfig` standards

2. **Run Tests**
   - Run `dotnet test` to execute all unit and integration tests
   - Ensure all tests pass before committing
   - Fix any failing tests before proceeding
   - Verify code coverage meets the 70% threshold

3. **Build Successfully**
   - Run `dotnet build` to ensure the solution builds without errors
   - Address any compiler warnings (warnings are treated as errors in CI)

**Commit Message Format:**
- Use Conventional Commits format: `<type>(<scope>): <description>`
- Types: `feat`, `fix`, `docs`, `style`, `refactor`, `test`, `chore`, `perf`, `ci`, `build`
- Keep commits small and focused on a single logical change
- Link to related issues with `relates to #<issue>` or `see #<issue>`
- Save issue resolution for the PR description using `closes #<issue>` or `fixes #<issue>`
- Examples:
  - `feat(api): add user authentication endpoint (relates to #42)`
  - `fix(database): resolve connection timeout issue (fixes #15)`
  - `docs(arc42): update building block view (see #8)`
  - `test(services): add unit tests for user service (relates to #42)`

### When Modifying Architecture
1. Document the decision in `/docs/09_architecture_decisions.md` using ADR format
2. Update building block view in `/docs/05_building_block_view.md`
3. Update deployment view if infrastructure changes (`/docs/07_deployment_view.md`)
4. Review impact on quality goals in `/docs/10_quality_requirements.md`

### When Adding Dependencies
1. Add to `Directory.Packages.props` with version
2. Document rationale if introducing new technology
3. Verify license compatibility
4. Check for security vulnerabilities

## Prohibited Patterns

- ❌ Avoid `System.Configuration.ConfigurationManager` (use `IOptions<T>` instead)
- ❌ Don't use `DateTime.Now` (use `DateTimeOffset.UtcNow` or inject time provider)
- ❌ Never use `Task.Result` or `Task.Wait()` (use `await` instead)
- ❌ Avoid `ConfigureAwait(false)` in application code (library code only)
- ❌ Don't catch generic `Exception` unless truly necessary
- ❌ Avoid magic strings and numbers (use constants or configuration)

## Questions to Ask

Before generating code, consider:
1. Does this change require architecture documentation updates?
2. What tests are needed to verify this functionality?
3. Are there any security implications?
4. What is the performance impact?
5. How will this be deployed and configured?
6. What error scenarios need to be handled?

## Review Checklist

When reviewing generated code, ensure:
- [ ] Follows Definition of Done criteria
- [ ] Includes appropriate tests
- [ ] Updates relevant documentation
- [ ] Follows coding standards in `.editorconfig`
- [ ] Handles errors appropriately
- [ ] Includes logging
- [ ] No hard-coded values or secrets
- [ ] Dependencies are properly managed
- [ ] Performance considerations addressed

---

**Remember:** Quality over speed. It's better to generate well-documented, tested, and maintainable code than to rush through implementation.
