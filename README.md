# Intelligent CI/CD Orchestrator for .NET Applications

An intelligent CI/CD orchestration platform that analyzes repository changes and dynamically selects the appropriate GitHub Actions workflow based on change impact.

## Overview

Traditional CI/CD pipelines often execute the same build, test, and deployment stages regardless of the scope of a code change. This project introduces a decision layer that analyzes repository changes and routes execution to an appropriate workflow, reducing unnecessary pipeline stages while preserving validation for higher-impact changes.

## Key Features

- Change-impact analysis for repository modifications
- Rule-based pipeline decision engine
- Dynamic GitHub Actions workflow selection
- GitHub REST API integration and automated workflow triggering
- Pipeline execution and stage-level status tracking
- Persistent pipeline run history
- Web-based monitoring dashboard
- Authentication and Dockerized deployment

## Pipeline Decision Logic

| Change Level | Condition | Workflow |
|---|---|---|
| LOW | Documentation-only or low-impact changes | Quick Build |
| MEDIUM | More than five files changed | Build & Test |
| HIGH | Backend and test changes detected | Full Pipeline |

The API analyzes incoming change information, classifies its impact, selects the corresponding workflow, triggers it through the GitHub REST API, and synchronizes execution status with the monitoring dashboard.

## System Architecture

```text
Repository Change
       |
       v
+------------------+
| Change Analyzer  |
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

**Backend:** C#, .NET, ASP.NET Core, Entity Framework Core, REST APIs  
**CI/CD & DevOps:** GitHub Actions, GitHub REST API, Docker, Git  
**Data:** SQLite, Entity Framework Core Migrations  
**Frontend:** ASP.NET Core Razor Pages, HTML, CSS, JavaScript

## Results

Benchmark measurements comparing Quick Build, Build & Test, and Full Pipeline execution will be added after controlled testing.
