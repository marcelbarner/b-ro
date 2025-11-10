var builder = DistributedApplication.CreateBuilder(args);

// PostgreSQL Database
var postgres = builder.AddPostgres("postgres")
    .WithPgAdmin()
    .WithDataVolume("postgres-data");

var financeDb = postgres.AddDatabase("finance-db");

// Finance API
var financeApi = builder.AddProject<Projects.Finance_API>("finance-api")
    .WithReference(financeDb)
    .WaitFor(financeDb);

// Angular Frontend
var frontend = builder.AddNpmApp("frontend", "../frontend", "start")
    .WithHttpEndpoint(env: "PORT")
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();

builder.Build().Run();
