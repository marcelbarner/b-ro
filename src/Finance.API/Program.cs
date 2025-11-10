using Finance.Domain.Repositories;
using Finance.Domain.Services;
using Finance.Infrastructure.BackgroundJobs;
using Finance.Infrastructure.ExternalServices;
using Finance.Infrastructure.Persistence;
using Finance.Infrastructure.Persistence.Repositories;
using Finance.Infrastructure.Services;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components
builder.AddServiceDefaults();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Add services to the container
builder.Services.AddControllers();

// Add FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Add Memory Cache for currency service
builder.Services.AddMemoryCache();

// Add HttpClient for ECB provider
builder.Services.AddHttpClient<IExchangeRateProvider, EcbExchangeRateProvider>();

// Add DbContext with Aspire PostgreSQL connection
builder.AddNpgsqlDbContext<FinanceDbContext>("finance-db");

// Add repositories
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();

// Add currency services
builder.Services.AddScoped<ICurrencyService, CurrencyService>();
builder.Services.AddScoped<IAccountAggregationService, AccountAggregationService>();

// Add background job for exchange rate updates
builder.Services.AddHostedService<ExchangeRateUpdateJob>();

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "Finance API",
        Version = "v1",
        Description = "API for managing bank accounts and transactions"
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Map health checks endpoint from ServiceDefaults
app.MapDefaultEndpoints();

app.Run();
