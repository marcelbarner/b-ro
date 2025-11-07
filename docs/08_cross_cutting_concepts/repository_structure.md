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

## Configuration Files

### Build Configuration
- **`solution.slnx`**: Modern XML-based solution file referencing all projects
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
