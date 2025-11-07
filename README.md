# b-ro

A monorepo structure for scalable .NET development following arc42 documentation standards.

## Repository Structure

```
/
├── scripts/          # SQL, PowerShell, and utility scripts
├── docs/             # arc42 architecture documentation
├── src/              # Product source code
└── [config files]    # Build configuration and tooling
```

## Documentation

All architecture documentation follows the [arc42](https://arc42.org/) template and can be found in the [`/docs`](./docs) directory. Start with the [Documentation README](./docs/README.md).

## Getting Started

1. Ensure you have .NET 9.0 SDK installed
2. Clone the repository
3. Review the [Definition of Done](./Definition_of_Done.md) for quality standards
4. Read the documentation in `/docs`

## Development

This repository uses:
- **Central Package Management**: Package versions are managed in `Directory.Packages.props`
- **EditorConfig**: Consistent coding style across the team
- **Shared Build Properties**: Common settings in `Directory.Build.props` and `Directory.Build.targets`

## Contributing

Please ensure all contributions meet the criteria defined in [Definition_of_Done.md](./Definition_of_Done.md).
