# Development Setup with .NET Aspire

This guide explains how to set up and run the entire BRo Finance application stack using .NET Aspire orchestration.

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) (version 9.0.306 or later)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (for PostgreSQL container)
- [Node.js 20+](https://nodejs.org/) (for Angular frontend)
- [Playwright Browsers](https://playwright.dev/dotnet/docs/browsers) (for E2E tests)

## Quick Start

### 1. Start the Full Stack

Run the entire application (API, Database, Frontend) with a single command:

```bash
dotnet run --project src/BRo.AppHost
```

This will start:
- **PostgreSQL Database** on `localhost:5432` with pgAdmin on `localhost:5050`
- **Finance.API** on a dynamically assigned port (check console output)
- **Angular Frontend** on `http://localhost:4200`

### 2. Access the Application

- **Frontend**: http://localhost:4200
- **API Swagger**: Check console output for the Finance.API port (e.g., `http://localhost:5123/swagger`)
- **pgAdmin**: http://localhost:5050 (credentials are auto-configured)
- **Aspire Dashboard**: Check console output for dashboard URL (e.g., `http://localhost:15123`)

### 3. Install Playwright Browsers (First Time Only)

Before running E2E tests for the first time:

```bash
pwsh tests/BRo.E2E.Tests/bin/Debug/net9.0/playwright.ps1 install
```

Or on Linux/macOS:
```bash
tests/BRo.E2E.Tests/bin/Debug/net9.0/playwright.sh install
```

### 4. Run E2E Tests

```bash
dotnet test tests/BRo.E2E.Tests
```

**Note**: Make sure the application is running (`dotnet run --project src/BRo.AppHost`) before executing E2E tests.

## Architecture Overview

### Aspire Projects

#### BRo.AppHost
- **Purpose**: Orchestrates all services and resources for local development
- **Location**: `src/BRo.AppHost`
- **Key Features**:
  - Manages PostgreSQL container with persistent volume
  - Configures service discovery between components
  - Provides unified startup experience
  - Integrates with Aspire Dashboard for observability

#### BRo.ServiceDefaults
- **Purpose**: Shared configuration for all services
- **Location**: `src/BRo.ServiceDefaults`
- **Key Features**:
  - Health checks configuration
  - OpenTelemetry instrumentation
  - HTTP resilience patterns
  - Service discovery client

### Service Architecture

```
┌─────────────────┐
│  Aspire AppHost │
│  (Orchestrator) │
└────────┬────────┘
         │
    ┌────┴───────────────────┐
    │                        │
┌───▼────────┐      ┌────────▼─────┐
│ PostgreSQL │◄─────┤  Finance.API │
│ Container  │      │   (Backend)  │
└────────────┘      └───────┬──────┘
                            │
                    ┌───────▼────────┐
                    │    Angular     │
                    │   (Frontend)   │
                    └────────────────┘
```

### Database Connection

The Finance.API uses **Aspire's PostgreSQL integration** for automatic connection string management:

- **Development**: Aspire provisions a PostgreSQL container automatically
- **Configuration**: Connection string is injected via service discovery
- **Name**: The database is registered as `finance-db` in AppHost
- **Persistence**: Data is stored in a Docker volume named `postgres-data`

## Development Workflow

### Making Changes

1. **Backend (Finance.API)**:
   - Edit code in `src/Finance.API`
   - Aspire will detect changes and restart the service automatically (hot reload)

2. **Frontend (Angular)**:
   - Edit code in `src/frontend`
   - Angular CLI's dev server provides hot module replacement

3. **Database Schema**:
   ```bash
   # Create a new migration
   dotnet ef migrations add MigrationName --project src/Finance.Infrastructure --startup-project src/Finance.API
   
   # Apply migrations (automatic on startup)
   dotnet run --project src/BRo.AppHost
   ```

### Debugging

#### Visual Studio / VS Code
1. Set `BRo.AppHost` as startup project
2. Press F5 to start debugging
3. Breakpoints in Finance.API will be hit automatically

#### Individual Services
You can run services independently if needed:

```bash
# API only (requires manual database setup)
dotnet run --project src/Finance.API

# Frontend only
cd src/frontend
npm start
```

### Viewing Logs and Metrics

The Aspire Dashboard provides:
- **Structured Logs**: From all services in real-time
- **Distributed Tracing**: Request flows across services
- **Metrics**: Performance and health metrics
- **Console Output**: Aggregated console logs

Access the dashboard URL shown in the console when running AppHost.

## E2E Testing

### Test Structure

E2E tests are located in `tests/BRo.E2E.Tests` and use:
- **xUnit**: Test framework
- **Playwright**: Browser automation
- **PlaywrightFixture**: Shared test setup/teardown

### Running Tests

```bash
# Run all E2E tests
dotnet test tests/BRo.E2E.Tests

# Run specific test
dotnet test tests/BRo.E2E.Tests --filter "FullyQualifiedName~HomePage_ShouldLoad"

# Run with detailed output
dotnet test tests/BRo.E2E.Tests --logger "console;verbosity=detailed"
```

### Writing New Tests

1. Create a new test class in `tests/BRo.E2E.Tests`
2. Implement `IClassFixture<PlaywrightFixture>` to get browser context
3. Use Playwright API to interact with the application:

```csharp
public class MyTests : IClassFixture<PlaywrightFixture>
{
    private readonly PlaywrightFixture _fixture;
    
    public MyTests(PlaywrightFixture fixture) 
        => _fixture = fixture;

    [Fact]
    public async Task MyTest()
    {
        var page = _fixture.Page!;
        await page.GotoAsync("http://localhost:4200/my-page");
        
        var element = page.Locator("css-selector");
        await Expect(element).ToBeVisibleAsync();
    }
}
```

## Troubleshooting

### PostgreSQL Container Issues

```bash
# Stop all containers
docker stop $(docker ps -aq)

# Remove PostgreSQL volume
docker volume rm postgres-data

# Restart Aspire
dotnet run --project src/BRo.AppHost
```

### Port Conflicts

If you get port conflicts:
1. Check `src/BRo.AppHost/AppHost.cs`
2. Modify port assignments in `WithHttpEndpoint()` calls
3. Update frontend proxy configuration in `src/frontend/proxy.conf.json`

### NPM Issues (Frontend)

```bash
# Clean install
cd src/frontend
rm -rf node_modules package-lock.json
npm install
```

### Aspire Dashboard Not Accessible

The dashboard URL is shown in the console output. If you miss it:
- Look for: `Now listening on: http://localhost:XXXXX`
- Or check: `$env:DOTNET_DASHBOARD_OTLP_ENDPOINT_URL` (PowerShell)

## Production Deployment

**Note**: Aspire is designed for local development. For production:
1. Use `docker-compose` or Kubernetes for orchestration
2. Deploy PostgreSQL as a managed service (Azure Database for PostgreSQL, AWS RDS, etc.)
3. Build Angular for production: `npm run build --prod`
4. Deploy Finance.API as a container or to a PaaS (Azure App Service, AWS Elastic Beanstalk, etc.)

See deployment documentation in `/docs/07_deployment_view.md` for details.

## Additional Resources

- [.NET Aspire Documentation](https://learn.microsoft.com/dotnet/aspire/)
- [Playwright .NET Documentation](https://playwright.dev/dotnet/)
- [PostgreSQL Integration](https://learn.microsoft.com/dotnet/aspire/database/postgresql-component)
- [Service Discovery](https://learn.microsoft.com/dotnet/aspire/service-discovery/overview)

## Related Documentation

- [Definition of Done](../Definition_of_Done.md)
- [Architecture Documentation](../docs/README.md)
- [Building Block View](../docs/05_building_block_view.md)
