# Conventions

This section defines the conventions and standards that govern development and architecture throughout the project.

## Coding Conventions

### General Principles

- Follow SOLID principles
- Prefer composition over inheritance
- Write self-documenting code with clear naming
- Keep methods small and focused (single responsibility)
- Use dependency injection for loose coupling

### C# Naming Conventions

| Element | Convention | Example |
|---------|-----------|---------|
| Namespace | PascalCase | `BRo.Finance.Domain` |
| Class | PascalCase | `TransactionService` |
| Interface | I-prefix, PascalCase | `ITransactionRepository` |
| Method | PascalCase | `GetTransactionById` |
| Property | PascalCase | `TotalAmount` |
| Field (private) | _camelCase | `_transactionRepository` |
| Parameter | camelCase | `transactionId` |
| Local Variable | camelCase | `totalAmount` |
| Constant | PascalCase | `MaxRetryAttempts` |

### TypeScript/Angular Conventions

| Element | Convention | Example |
|---------|-----------|---------|
| Component Class | PascalCase + suffix | `TransactionListComponent` |
| Component Selector | kebab-case + prefix | `bro-transaction-list` |
| Service | PascalCase + suffix | `TransactionService` |
| Interface | PascalCase | `Transaction` |
| Type | PascalCase | `TransactionType` |
| Method | camelCase | `getTransactionById` |
| Property | camelCase | `totalAmount` |
| Observable | $ suffix | `transactions$` |

## Project Structure Conventions

### Backend Projects

```
/src
  /{Domain}.API/
    - Program.cs
    - appsettings.json
    /Controllers/
    /Models/
    /Services/
    /Repositories/
  /{Domain}.Domain/
    /Entities/
    /Interfaces/
    /ValueObjects/
  /{Domain}.Infrastructure/
    /Data/
    /Repositories/
  /Shared.{Area}/
```

### Frontend Projects

```
/src/frontend/
  /app/
    /core/            # Singleton services, guards, interceptors
    /shared/          # Shared modules, components, pipes
    /features/
      /finance/       # Feature modules
      /documents/
      /storage/
    /models/          # TypeScript interfaces and types
```

## API Conventions

### REST API Design

- **Resource Naming**: Plural nouns (e.g., `/transactions`, `/documents`)
- **HTTP Methods**: 
  - GET: Retrieve resources
  - POST: Create resources
  - PUT: Replace resources
  - PATCH: Partial update
  - DELETE: Remove resources

### Endpoint Patterns

```
GET    /api/{domain}/{resource}           # List all
GET    /api/{domain}/{resource}/{id}      # Get one
POST   /api/{domain}/{resource}           # Create
PUT    /api/{domain}/{resource}/{id}      # Replace
PATCH  /api/{domain}/{resource}/{id}      # Update
DELETE /api/{domain}/{resource}/{id}      # Delete
```

### Response Format

```json
{
  "data": { },
  "success": true,
  "errors": [],
  "metadata": {
    "timestamp": "2025-11-08T10:00:00Z",
    "page": 1,
    "totalPages": 10
  }
}
```

## Testing Conventions

### Test Naming

- **Method Format**: `MethodName_Scenario_ExpectedBehavior`
- **Example**: `GetTransactionById_WhenNotFound_ReturnsNotFoundResult`

### Test Organization

```
/tests
  /{Domain}.API.Tests/
    /Controllers/
    /Services/
  /{Domain}.Domain.Tests/
    /Entities/
  /{Domain}.Integration.Tests/
```

### Test Categories

- **Unit Tests**: Test single components in isolation (mocked dependencies)
- **Integration Tests**: Test component interactions
- **End-to-End Tests**: Test complete user workflows

## Documentation Conventions

### arc42 Structure

- One main markdown file per section in `/docs` (table of contents only)
- Detailed content in subdirectories
- Consistent heading levels and formatting
- Code examples in appropriate language-specific code blocks

### Code Documentation

- XML comments for public APIs (C#)
- JSDoc comments for public services (TypeScript)
- README.md in each major project folder
- Architecture Decision Records (ADR) for significant decisions

## Git Conventions

### Branch Naming

- `feat/short-description` - New features
- `fix/short-description` - Bug fixes
- `docs/short-description` - Documentation updates
- `refactor/short-description` - Code refactoring
- `test/short-description` - Test additions/modifications

### Commit Messages

**Format**: `<type>(<scope>): <description>`

**Types**:
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `style`: Code style changes (formatting)
- `refactor`: Code refactoring
- `test`: Test changes
- `chore`: Build/tool changes
- `perf`: Performance improvements
- `ci`: CI/CD changes
- `build`: Build system changes

**Example**:
```
feat(finance): add transaction filtering by date range (relates to #42)
```

### Pull Request Template

```markdown
## Description
[Describe the changes]

## Related Issue
Closes #[issue number]

## Type of Change
- [ ] New feature
- [ ] Bug fix
- [ ] Documentation update
- [ ] Refactoring

## Checklist
- [ ] Follows Definition of Done
- [ ] Tests added/updated
- [ ] Documentation updated
- [ ] No breaking changes
```

## Docker Conventions

### Image Naming

- Format: `bro/{service}:{version}`
- Example: `bro/finance-api:1.0.0`

### Container Naming

- Format: `bro-{service}-{environment}`
- Example: `bro-finance-api-dev`

### Environment Variables

- Prefix with service name: `FINANCE_DB_CONNECTION`
- Use UPPER_SNAKE_CASE
- Document all variables in README

## Database Conventions

### Table Naming

- PascalCase, singular nouns
- Example: `Transaction`, `Document`, `StorageLocation`

### Column Naming

- PascalCase
- Example: `TransactionId`, `CreatedAt`, `TotalAmount`

### Foreign Keys

- Format: `{ReferencedTable}Id`
- Example: `UserId`, `CategoryId`

## Logging Conventions

### Log Levels

- **Trace**: Very detailed diagnostic information
- **Debug**: Debugging information for developers
- **Information**: General informational messages
- **Warning**: Warning messages for unexpected but handled situations
- **Error**: Error messages for failures
- **Critical**: Critical failures requiring immediate attention

### Log Message Format

```csharp
_logger.LogInformation(
    "Transaction {TransactionId} created by user {UserId}",
    transactionId,
    userId
);
```

### Structured Logging

- Use message templates with named parameters
- Include correlation IDs for request tracing
- Never log sensitive data (passwords, tokens, PII)
