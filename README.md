# Intelligent CI/CD Orchestrator for .NET Applications

An intelligent CI/CD orchestration platform that analyzes repository changes and dynamically selects the appropriate GitHub Actions workflow based on the impact of those changes.

The project demonstrates how CI/CD pipelines can move beyond executing the same workflow for every commit by introducing a decision layer between code changes and pipeline execution.

## Overview

Traditional CI/CD pipelines commonly execute the same build, test, and deployment stages regardless of the scope of a code change. While reliable, this approach can result in unnecessary pipeline execution and inefficient use of CI resources.

The Intelligent CI/CD Orchestrator analyzes incoming change information, determines its impact level, and selects one of three GitHub Actions workflows:

| Change Level | Selected Workflow |
|---|---|
| LOW | Quick Build |
| MEDIUM | Build & Test |
| HIGH | Full Pipeline |

This allows lightweight changes to use a smaller workflow while higher-impact changes receive more comprehensive validation.

## Key Features

- Change-impact analysis for incoming repository modifications
- Rule-based pipeline decision engine
- Dynamic selection between three CI/CD workflows
- GitHub Actions integration through the GitHub REST API
- Automated workflow triggering
- Pipeline execution and status tracking
- Persistent pipeline run history
- Stage-level pipeline status tracking
- Web-based monitoring dashboard
- Authentication for dashboard access
- Dockerized deployment support
- REST API for pipeline orchestration

## How It Works

The orchestration process follows five main steps:

1. Repository change information is received by the API.
2. The Change Analyzer evaluates the scope of the modification.
3. The Decision Engine classifies the change as LOW, MEDIUM, or HIGH impact.
4. The corresponding GitHub Actions workflow is selected and triggered.
5. Execution metadata and pipeline status are synchronized and displayed through the dashboard.

## Decision Logic

The current decision engine uses deterministic rules to classify repository changes.

### LOW Impact

A change is classified as LOW when:

- the modification contains documentation-only changes, or
- none of the higher-impact conditions are met.

Selected workflow:

`QUICK_BUILD`

### MEDIUM Impact

A change is classified as MEDIUM when:

- more than five files are changed.

Selected workflow:

`BUILD_AND_TEST`

### HIGH Impact

A change is classified as HIGH when:

- backend code and tests are both modified.

Selected workflow:

`FULL_PIPELINE`

The decision layer is separated from pipeline execution, allowing the classification strategy to be extended independently in future versions.

## CI/CD Workflows

The repository contains three GitHub Actions workflows.

### Quick Build

`.github/workflows/quick-build.yml`

Used for low-impact changes where a lightweight validation pipeline is sufficient.

### Build & Test

`.github/workflows/build-and-test.yml`

Used for medium-impact changes requiring both compilation and automated testing.

### Full Pipeline

`.github/workflows/full-pipeline.yml`

Used for high-impact changes requiring the complete CI/CD workflow.

## System Architecture

```text
Repository Change
       |
       v
+------------------+
|  Change Analyzer |
+------------------+
       |
       v
 LOW / MEDIUM / HIGH
       |
       v
+------------------+
| Decision Engine  |
+------------------+
       |
       v
+----------------------+
| Pipeline Executor    |
+----------------------+
       |
       v
+----------------------+
| GitHub REST API      |
+----------------------+
       |
       v
+---------------------------------------------+
|             GitHub Actions                  |
|                                             |
| Quick Build | Build & Test | Full Pipeline |
+---------------------------------------------+
       |
       v
+----------------------+
| Run Status Sync      |
+----------------------+
       |
       v
+----------------------+
| Pipeline Database    |
+----------------------+
       |
       v
+----------------------+
| Monitoring Dashboard |
+----------------------+
```
## Technology Stack

### Backend
- C#
- .NET
- ASP.NET Core
- Entity Framework Core
- REST APIs

### CI/CD & DevOps
- GitHub Actions
- GitHub REST API
- Docker
- Git

### Data & Persistence
- SQLite
- Entity Framework Core Migrations

### Frontend
- ASP.NET Core Razor Pages
- HTML/CSS
- JavaScript

## Project Structure

```text
ci-cd-orchestrator/
├── .github/
│   └── workflows/
│       ├── quick-build.yml
│       ├── build-and-test.yml
│       └── full-pipeline.yml
├── OrchestratorAPI/
│   ├── Analyzer/
│   ├── Controllers/
│   ├── Data/
│   ├── DecisionEngine/
│   ├── Execution/
│   ├── GitHub/
│   ├── Migrations/
│   ├── Models/
│   └── State/
├── OrchestratorUI/
│   ├── Pages/
│   └── wwwroot/
├── TestApp/
├── Dockerfile
└── OrchestratorSolution.slnx
```
