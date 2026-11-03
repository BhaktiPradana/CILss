# CI Yield - Daily Data Synchronization Hub (Windows Application)

Aplikasi desktop Windows (.NET 9 Windows Forms) untuk melakukan ekstraksi, kalkulasi, akumulasi harian, dan sinkronisasi **First Pass Yield (FPY)** serta **Overall Yield (Final Yield)** per **DeviceCode** dari tiga sistem database produksi plant Excelitas ke database sentral `CI_LSS`.

---

## 📌 Alur & Arsitektur Sinkronisasi

```
[Sumber Data Plant]                                             [Target Data Hub]
-------------------                                             -----------------
1. MLS (Lighting)
   Server: ETCBTMA03\DIGITMONT
   DB    : digimon_lighting (Windows Auth)
         │
2. OPTIC / VISION
   Server: ETCBTMA03\DIGITMONT
   DB    : digimon_vision   (Windows Auth) ──────► [YieldSyncApp] ──────► Server: BTMSQL01\DIGITMONT_DEV
         │                                       (Windows App)            DB    : CI_LSS
3. AIS (Detection)                                                        User  : CI_Hub_User
   Server: ETCBTMSQL01\DIGITMONT                                          Auth  : SQL Server Auth
   DB    : dbDigitmontDetection (Windows Auth)                            Tabel : [dbo].[CI_Yield]
```

---

## 📊 Struktur Tabel Target `[dbo].[CI_Yield]`

Tabel ini secara otomatis dibuat oleh aplikasi jika belum ada di database `CI_LSS` (atau dapat dieksekusi via script `Create_CI_Yield.sql`):

```sql
CREATE TABLE [dbo].[CI_Yield] (
    [Id]               BIGINT IDENTITY(1,1) NOT NULL,
    [DeviceCode]       NVARCHAR(100)        NOT NULL,
    [Date]             DATE                 NOT NULL,
    [FirstPassYield]   DECIMAL(7, 4)        NOT NULL DEFAULT (0),
    [OverallYield]     DECIMAL(7, 4)        NOT NULL DEFAULT (0),
    [Division]         NVARCHAR(50)         NULL, -- 'MLS', 'OPTIC', 'AIS'
    [TotalQtyIn]       FLOAT                NULL DEFAULT (0),
    [TotalQtyOut]      FLOAT                NULL DEFAULT (0),
    [TotalDefectFirst] FLOAT                NULL DEFAULT (0),
    [TotalDefectFinal] FLOAT                NULL DEFAULT (0),
    [SourceServer]     NVARCHAR(100)        NULL,
    [SourceDatabase]   NVARCHAR(100)        NULL,
    [LastSyncDate]     DATETIME             NOT NULL DEFAULT (GETDATE()),
    CONSTRAINT [PK_CI_Yield] PRIMARY KEY CLUSTERED ([Id] ASC)
);

CREATE UNIQUE NONCLUSTERED INDEX [UQ_CI_Yield_Device_Date_Div] 
    ON [dbo].[CI_Yield] ([DeviceCode] ASC, [Date] ASC, [Division] ASC);

CREATE NONCLUSTERED INDEX [IX_CI_Yield_Date] ON [dbo].[CI_Yield] ([Date] ASC);
CREATE NONCLUSTERED INDEX [IX_CI_Yield_DeviceCode] ON [dbo].[CI_Yield] ([DeviceCode] ASC);
```

---

## 🧮 Logika Perhitungan Harian per DeviceCode

Data ditarik dari tabel MES `dmPChart` dan `dmPChartDefect` lalu diakumulasikan per tanggal dan `DeviceCode`:

1. **Total Input (`TotalQtyIn`)**: $\sum \text{QuantityIn}$
2. **Total Output Akhir (`TotalQtyOut`)**: $\sum \text{QuantityOut}$
3. **Cacat Putaran Pertama (`TotalDefectFirst`)**: $\sum \text{QuantityDefectFirst}$
4. **Cacat Akhir (`TotalDefectFinal`)**: $\sum \text{QuantityDefect}$

Formula yang dihitung oleh aplikasi:
* **First Pass Yield (FPY %)**:
  $$\text{FPY} = \frac{\text{TotalQtyIn} - \text{TotalDefectFirst}}{\text{TotalQtyIn}} \times 100\%$$
* **Overall Yield (Final Yield %)**:
  $$\text{Overall Yield} = \frac{\text{TotalQtyOut}}{\text{TotalQtyIn}} \times 100\%$$

---

## 🚀 Fitur Unggulan YieldSyncApp

1. **Multi-Database Support**: Sekali klik mengekstrak dari MLS, OPTIC, dan AIS secara terpisah dengan tag `Division`.
2. **Pilihan Rentang Tanggal (Windowing)**:
   - `Today`
   - `Yesterday`
   - `Last 7 Days` *(Rekomendasi harian)*
   - `Last 30 Days`
   - `All Time (Full Sync)`
   - `Custom Range` (Tanggal awal s/d Tanggal akhir)
3. **Atomic MERGE / Upsert**:
   - Sinkronisasi menggunakan staging table `#Staging_CI_Yield` + `SqlBulkCopy` + `MERGE`.
   - Data yang sudah ada akan di-update, data baru di-insert tanpa menyebabkan baris ganda (*no duplicate*).
4. **Automasi Harian (Background Tray)**:
   - Menjalankan sinkronisasi terjadwal otomatis setiap pukul **06:00 AM**.
   - Bila jendela ditutup (*close*), aplikasi akan terminimalisasi ke **System Tray** (icon dekat jam Windows) dan tetap aktif di latar belakang.
5. **Headless Execution untuk Windows Task Scheduler**:
   - Jalankan `YieldSyncApp.exe --silent` atau `--sync-now` dari task scheduler tanpa membuka UI.
6. **Tool Test Connection**:
   - Memeriksa konektivitas jaringan, status DNS, dan kredensial ke seluruh server sebelum memulai sync.

---

## ⚙️ Konfigurasi (`appsettings.json`)

File konfigurasi terletak berdampingan dengan `YieldSyncApp.exe`:

```json
{
  "ConnectionStrings": {
    "Source_MLS": "Server=ETCBTMA03\\DIGITMONT;Database=digimon_lighting;Integrated Security=SSPI;TrustServerCertificate=True;",
    "Source_OPTIC": "Server=ETCBTMA03\\DIGITMONT;Database=digimon_vision;Integrated Security=SSPI;TrustServerCertificate=True;",
    "Source_AIS": "Server=ETCBTMSQL01\\DIGITMONT;Database=dbDigitmontDetection;Integrated Security=SSPI;TrustServerCertificate=True;",
    "Destination": "Server=BTMSQL01\\DIGITMONT_DEV;Database=CI_LSS;User Id=CI_Hub_User;Password=CiHubB4tam@excelitas;TrustServerCertificate=True;MultipleActiveResultSets=False"
  },
  "SyncSettings": {
    "DefaultDaysBack": 7,
    "BatchSize": 2000,
    "CommandTimeoutSeconds": 300,
    "EnableDailySchedule": true,
    "DailyScheduleHour": 6,
    "DailyScheduleMinute": 0
  }
}
```

---

## 🏃 Cara Menjalankan

### Cara 1: Menjalankan Langsung (GUI)
1. Buka folder: `e:\Project\LSSData\YieldSyncApp\bin\Release\net9.0-windows\`
2. Klik ganda `YieldSyncApp.exe`.
3. Pilih sumber database dan rentang tanggal yang diinginkan.
4. Klik tombol **🚀 START SYNC NOW**.

### Cara 2: Menjalankan via Windows Task Scheduler (Headless)
Tambahkan task baru di Windows Task Scheduler:
* **Program/script**: `E:\Project\LSSData\YieldSyncApp\bin\Release\net9.0-windows\YieldSyncApp.exe`
* **Add arguments**: `--silent`
* **Start in**: `E:\Project\LSSData\YieldSyncApp\bin\Release\net9.0-windows\`

---

## 🔍 Query Pengecekan Hasil Sinkronisasi di SQL Server

```sql
USE [CI_LSS];
GO

-- 1. Lihat ringkasan data yield per divisi
SELECT 
    Division,
    COUNT(DISTINCT DeviceCode) AS TotalDevices,
    MIN([Date]) AS EarliestDate,
    MAX([Date]) AS LatestDate,
    COUNT(*) AS TotalDailyRecords
FROM [dbo].[CI_Yield]
GROUP BY Division;

-- 2. Tampilkan 20 data yield harian terbaru
SELECT TOP 20
    Division,
    DeviceCode,
    [Date],
    TotalQtyIn,
    TotalQtyOut,
    TotalDefectFirst,
    FirstPassYield AS [FPY (%)],
    OverallYield   AS [Overall Yield (%)],
    LastSyncDate
FROM [dbo].[CI_Yield]
ORDER BY [Date] DESC, Division ASC, DeviceCode ASC;

-- 3. Cek tren yield untuk device tertentu
SELECT 
    [Date],
    FirstPassYield AS [FPY (%)],
    OverallYield   AS [Overall Yield (%)],
    TotalQtyIn,
    TotalQtyOut
FROM [dbo].[CI_Yield]
WHERE DeviceCode = '25500631'
ORDER BY [Date] ASC;
```

