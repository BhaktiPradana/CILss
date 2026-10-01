# CILeanSixSigma

Internal web application for tracking and monitoring Lean Six Sigma training: White Belt, Yellow Belt, Green Belt + TOC, and Black Belt.

## Prerequisites

- .NET SDK 9.0 or later
- SQL Server Express (tested on SQL Server 2022 Express)
- SQL Server Instance: `DESKTOP-ABQLQF1\SQLEXPRESS`

## Setting Up the Database

Run from the repository folder using Windows Authentication:

```bash
sqlcmd -S "DESKTOP-ABQLQF1\SQLEXPRESS" -E -i database/schema.sql
```

The idempotent script creates the `LssTraining` database, all tables, programs, and initial modules. The connection string can be modified via `appsettings.json` or environment variables/user secrets.

## Running the Application

```bash
dotnet run --project LssTraining.Web
```

Open the URL printed by the application (default HTTP `http://localhost:5277`).

## Features

- KPI Dashboard, four training pathways, completion charts, and recent activity logs
- Cookie authentication based on username/password with Admin/Trainer roles
- Participant CRUD with DMAIC Status Phase dropdown (Define, Measure, Analyze, Improve, Control)
- Interactive visual roadmap and R0–R5 review status matrix
- Bulk participant CSV import
- Program and module curriculum master management with target duration
- Enrollment progress and actual vs. target hours tracking
- TOC flag for Green Belt enrollments

