# CI Sync Hub - Unified Synchronization Hub

**CI Sync Hub** is the consolidated enterprise desktop application and background sync engine for CI LSS Batam (Excelitas Technologies). It unifies all legacy Windows sync tools (`DatabaseSyncApp`, `YieldSyncApp`, and diagnostic tools) into a single, high-performance, robust client with an intuitive tabbed UI, live telemetry terminal, automated background scheduling, and headless CLI support.

---

## 🎯 Key Capabilities & Features

### 1. Daily FPY & Overall Yield Synchronization
- Extracts First Pass Yield (FPY), Overall Yield, total quantities, and defects per device code per day.
- Supports 3 manufacturing divisions simultaneously:
  - **MLS**: `ETCBTMA03\DIGITMONT` -> Database: `digimon_lighting`
  - **OPTIC**: `ETCBTMA03\DIGITMONT` -> Database: `digimon_vision`
  - **AIS**: `ETCBTMSQL01\DIGITMONT` -> Database: `dbDigitmontDetection`
- Upserts extracted records into `BTMSQL01\DIGITMONT_DEV` -> `CI_LSS.dbo.CI_Yield` with deduplication and index safety.
- Granular date range options: **Today**, **Yesterday**, **Last 7 Days**, **Last 30 Days**, **All Time**, or **Custom Date Range**.

### 2. Employee Master Synchronization
- Reads all active employees (`TERMINATION_STATUS IS NULL`) directly from the HR database:
  - **HR Server**: `ETCRESSQL02` -> Database: `BTMTime` -> Table: `[EMPLOYEE]`
- Performs high-speed bulk ingestion via `SqlBulkCopy` with temp staging and merge into:
  - Target: `BTMSQL01\DIGITMONT_DEV` -> Database: `CI_LSS` -> Table: `[dbo].[CI_Employee]`

### 3. One-Click "SYNC ALL" Automation
- Orchestrates a complete synchronization pipeline with a single click:
  1. Synchronizes the active Employee Master from BTMTime.
  2. Synchronizes daily FPY and Overall Yield for all 3 divisions (MLS, OPTIC, AIS).
- Live progress and visual status cards update dynamically.

### 4. Background Daily Scheduler & System Tray
- Integrated background timer configurable directly from the UI or `appsettings.json` (e.g. daily at 06:00 AM).
- Minimizes seamlessly to the Windows System Tray with balloon notifications.
- Context menu options on the tray icon to run sync tasks on demand or restore the window.

### 5. Multi-Endpoint Connection Diagnostics
- Built-in diagnostic tester that pings and verifies credentials and connectivity across all 5 databases:
  1. Target CI_LSS (`BTMSQL01\DIGITMONT_DEV`)
  2. MLS Digitmont (`ETCBTMA03\DIGITMONT`)
  3. OPTIC Digitmont (`ETCBTMA03\DIGITMONT`)
  4. AIS Digitmont (`ETCBTMSQL01\DIGITMONT`)
  5. BTMTime HR (`ETCRESSQL02`)

### 6. Headless CLI Execution for Windows Task Scheduler
You can trigger CI Sync Hub in headless/silent mode via batch scripts or Windows Task Scheduler without launching the GUI:

```cmd
# Run Full Sync (Employees + Yield)
CISyncHub.exe --sync-all

# Run Yield Sync only
CISyncHub.exe --sync-yield

# Run Employee Master Sync only
CISyncHub.exe --sync-employee

# Run Connection Diagnostics only
CISyncHub.exe --test-conn
```

---

## ⚙️ Configuration (`appsettings.json`)

```json
{
  "ConnectionStrings": {
    "Destination": "Server=BTMSQL01\\DIGITMONT_DEV;Database=CI_LSS;User Id=CI_Hub_User;Password=CiHubB4tam@excelitas;TrustServerCertificate=True;MultipleActiveResultSets=False",
    "Source_MLS": "Server=ETCBTMA03\\DIGITMONT;Database=digimon_lighting;Integrated Security=SSPI;TrustServerCertificate=True;",
    "Source_OPTIC": "Server=ETCBTMA03\\DIGITMONT;Database=digimon_vision;Integrated Security=SSPI;TrustServerCertificate=True;",
    "Source_AIS": "Server=ETCBTMSQL01\\DIGITMONT;Database=dbDigitmontDetection;Integrated Security=SSPI;TrustServerCertificate=True;",
    "Source_BTMTime": "Server=ETCRESSQL02;Database=BTMTime;User Id=BTM_IT_RO;Password=ITreadonly!;TrustServerCertificate=True;"
  },
  "SyncSettings": {
    "DefaultDaysBack": 7,
    "BatchSize": 2000,
    "CommandTimeoutSeconds": 300,
    "EnableDailySchedule": true,
    "DailyScheduleHour": 6,
    "DailyScheduleMinute": 0,
    "ScheduleMode": "All"
  }
}
```

---

## 🚀 Building & Running

```bash
# Build
dotnet build CISyncHub/CISyncHub.csproj

# Run GUI
dotnet run --project CISyncHub/CISyncHub.csproj
```
