using System.Net.Http.Headers;
using OrchestratorAPI.GitHub;
using Serilog;
using OrchestratorAPI.State;
using OrchestratorAPI.Analyzer;
using OrchestratorAPI.DecisionEngine;
using OrchestratorAPI.Execution;
using Microsoft.EntityFrameworkCore;
using OrchestratorAPI.Data;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<GitHubOptions>(
    builder.Configuration.GetSection(GitHubOptions.SectionName));

// Register services
builder.Services.AddScoped<PipelineStateService>();
builder.Services.AddScoped<ChangeAnalyzer>();
builder.Services.AddScoped<DecisionService>();
builder.Services.AddScoped<PipelineExecutor>();
builder.Services.AddHttpClient<GitHubActionsService>((serviceProvider, client) =>
{
    var configuration = serviceProvider
        .GetRequiredService<IConfiguration>();

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
            new AuthenticationHeaderValue("Bearer", token);
    }
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=orchestrator.db"));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowUI",
        policy =>
        {
            policy.WithOrigins("http://localhost:5179")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});
    
// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();

app.UseRouting();

app.UseCors("AllowUI");

app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () => "Orchestrator Running");

app.Run();