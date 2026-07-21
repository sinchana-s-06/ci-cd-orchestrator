using System.Net.Http.Headers;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using OrchestratorAPI.Analyzer;
using OrchestratorAPI.Data;
using OrchestratorAPI.DecisionEngine;
using OrchestratorAPI.Execution;
using OrchestratorAPI.GitHub;
using OrchestratorAPI.State;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// -----------------------------
// Serilog
// -----------------------------
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// -----------------------------
// GitHub Configuration
// -----------------------------
builder.Services
    .AddOptions<GitHubOptions>()
    .Bind(
        builder.Configuration.GetSection(
            GitHubOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

// -----------------------------
// Application Services
// -----------------------------
builder.Services.AddScoped<PipelineStateService>();
builder.Services.AddScoped<ChangeAnalyzer>();
builder.Services.AddScoped<DecisionService>();
builder.Services.AddScoped<PipelineExecutor>();

// -----------------------------
// Shared GitHub HttpClient Configuration
// -----------------------------
static void ConfigureGitHubClient(
    IServiceProvider serviceProvider,
    HttpClient client)
{
    var configuration =
        serviceProvider.GetRequiredService<IConfiguration>();

    var token = configuration["GitHub:Token"];

    client.BaseAddress = new Uri("https://api.github.com/");

    client.DefaultRequestHeaders.UserAgent.ParseAdd(
        "Intelligent-CICD-Orchestrator");

    client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue(
            "application/vnd.github+json"));

    client.DefaultRequestHeaders.Add(
        "X-GitHub-Api-Version",
        "2026-03-10");

    if (!string.IsNullOrWhiteSpace(token))
    {
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                token);
    }
}

// -----------------------------
// GitHub Services
// -----------------------------
builder.Services.AddHttpClient<GitHubActionsService>(
    ConfigureGitHubClient);

builder.Services.AddHttpClient<GitHubStatusService>(
    ConfigureGitHubClient);

builder.Services.AddHostedService<GitHubRunSyncService>();

// -----------------------------
// Database
// -----------------------------
var connectionString =
    builder.Configuration.GetConnectionString(
        "OrchestratorDatabase")
    ?? throw new InvalidOperationException(
        "Connection string 'OrchestratorDatabase' was not found.");

builder.Services.AddDbContext<AppDbContext>(options =>
  options.UseSqlite(connectionString));

// -----------------------------
// CORS
// -----------------------------
var allowedOrigins =
    builder.Configuration
        .GetSection("Cors:AllowedOrigins")
        .Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowUI", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// -----------------------------
// ASP.NET Services
// -----------------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

var app = builder.Build();

// -----------------------------
// Middleware
// -----------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();
app.UseCors("AllowUI");
app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () => Results.Ok(
    new
    {
        service = "Intelligent CI/CD Orchestrator",
        status = "Running"
    }));

app.MapHealthChecks(
    "/health",
    new HealthCheckOptions
    {
        ResponseWriter = async (context, report) =>
        {
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsJsonAsync(
                new
                {
                    status = report.Status.ToString(),
                    timestamp = DateTime.UtcNow,
                    checks = report.Entries.Select(entry => new
                    {
                        name = entry.Key,
                        status = entry.Value.Status.ToString()
                    })
                });
        }
    });
   
app.Run();