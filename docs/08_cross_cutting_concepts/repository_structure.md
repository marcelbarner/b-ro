# Repository Structure

The repository follows a monorepo structure with the following organization:

```
/
├── scripts/          # SQL, PowerShell, and utility scripts for pipelines and developer tasks
├── docs/             # Project documentation following arc42 structure
├── src/              # Product source code
├── solution.slnx     # Central solution file
├── .editorconfig     # Coding style enforcement
├── global.json       # SDK version definition
├── Directory.Build.props      # Shared build properties
├── Directory.Build.targets    # Shared build targets
├── Directory.Packages.props   # Centralized package management
├── Definition_of_Done.md      # Quality and completion criteria
├── .github/
│   └── copilot-instructions.md   # GitHub Copilot guidelines
├── README.md         # Repository overview
└── .gitignore        # Git exclusions
```

## Folder Organization

### `/scripts`
Contains SQL scripts, PowerShell scripts, and other utilities that support:
- CI/CD pipelines
- Development automation
- Database migrations
- Build and deployment tasks

### `/docs`
Houses all project documentation following the arc42 template structure. Each section has:
- A main markdown file directly under `/docs` (e.g., `01_introduction_and_goals.md`) containing a table of contents and section description
- A corresponding subdirectory (e.g., `/docs/01_introduction_and_goals/`) with detailed content files

### `/src`
Contains all product source code organized by projects and modules.

## Project Structure Principles

### Service-Based Organization

The monorepo contains multiple services, each representing a distinct domain:

```
/src
  /Finance.API/              # Finance domain service
  /Finance.Domain/           # Finance domain models and business logic
  /Finance.Infrastructure/   # Finance data access and external integrations
  
  /Documents.API/            # Documents domain service (DMS)
  /Documents.Domain/         # Documents domain models and business logic
  /Documents.Infrastructure/ # Documents data access and storage
  
  /Storage.API/              # Storage/Inventory domain service
  /Storage.Domain/           # Storage domain models and business logic
  /Storage.Infrastructure/   # Storage data access and persistence
  
  /Shared.Common/            # Cross-cutting utilities (logging, validation, etc.)
  /Shared.Domain/            # Shared domain interfaces and base classes
  /Shared.Infrastructure/    # Shared infrastructure (auth, caching, etc.)
  
  /Frontend/                 # Angular 20 application
    /src/
      /app/
        /features/
          /finance/          # Finance feature module
          /documents/        # Documents feature module
          /storage/          # Storage feature module
        /shared/             # Shared UI components
        /core/               # Core services and guards
```

### Separation of Concerns

Each service follows a layered architecture:

- **API Layer** (`*.API`): REST endpoints, request/response models, controllers
- **Domain Layer** (`*.Domain`): Business logic, domain entities, domain services, interfaces
- **Infrastructure Layer** (`*.Infrastructure`): Data persistence, external integrations, implementations

### Shared Libraries

Common functionality is extracted into shared libraries to avoid duplication:

- **Shared.Common**: Utilities, extensions, helpers, common validators
- **Shared.Domain**: Base entities, common interfaces, shared value objects
- **Shared.Infrastructure**: Authentication, logging, caching, configuration

### Service Independence

- Each service has its own database (database-per-service pattern)
- Services communicate via RESTful APIs (no direct database access)
- Services can be deployed independently
- Shared libraries are referenced as NuGet packages (internal)

### Solution Organization

All projects are referenced in the root `b-ro.sln` file, organized by folders:

```
Solution 'b-ro'
├── Finance
│   ├── Finance.API
│   ├── Finance.Domain
│   └── Finance.Infrastructure
├── Documents
│   ├── Documents.API
│   ├── Documents.Domain
│   └── Documents.Infrastructure
├── Storage
│   ├── Storage.API
│   ├── Storage.Domain
│   └── Storage.Infrastructure
├── Shared
│   ├── Shared.Common
│   ├── Shared.Domain
│   └── Shared.Infrastructure
└── Frontend
    └── Angular App (external build)
```

### Naming Conventions

- **Project Naming**: `{Domain}.{Layer}`
- **Namespace Naming**: `BRo.{Domain}.{Layer}`
- **Assembly Naming**: `BRo.{Domain}.{Layer}.dll`

Example:
- Project: `Finance.API`
- Namespace: `BRo.Finance.API`
- Assembly: `BRo.Finance.API.dll`

## Configuration Files

### Build Configuration
- **`b-ro.sln`**: .NET solution file referencing all projects
- **`Directory.Build.props`**: Shared MSBuild properties applied to all projects
- **`Directory.Build.targets`**: Common build targets and custom build steps
- **`Directory.Packages.props`**: Centralized NuGet package version management

### Development Tools
- **`.editorconfig`**: Code style and formatting rules
- **`global.json`**: .NET SDK version pinning for consistency
- **`.gitignore`**: Standard exclusions for .NET projects

### Quality Standards
- **`Definition_of_Done.md`**: Completion criteria for all work items
- **`.github/copilot-instructions.md`**: AI coding assistant guidelines
