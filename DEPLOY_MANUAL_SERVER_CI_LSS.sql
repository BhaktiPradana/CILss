-- =========================================================================================
-- SCRIPT DEPLOYMENT MANUAL DATABASE SERVER PERUSAHAAN (CI_LSS)
-- Target Server   : BTMSQL01\DIGITMONT_DEV atau Server Produksi CI_LSS
-- Target Database : [CI_LSS]
-- Deskripsi       : Skrip Idempoten (Aman dijalankan berulang kali) untuk membuat dan
--                   memperbarui tabel-tabel baru Capacity Planning, Machine Matrix,
--                   IE Cycle Time, dan Live Production Tracking.
-- Tanggal Dibuat  : 2026-11-07
-- =========================================================================================

USE [CI_LSS];
GO

SET NOCOUNT ON;
PRINT '========================================================================';
PRINT 'Memulai Eksekusi Skrip Pembaruan Skema Database CI_LSS...';
PRINT 'Waktu Eksekusi: ' + CONVERT(VARCHAR(30), GETDATE(), 120);
PRINT '========================================================================';
GO

-- -----------------------------------------------------------------------------------------
-- 1. TABEL: [dbo].[CI_MachinesProcess] (Machine-to-Process Capability Matrix)
--    Menghubungkan Process Code ke Machine Code fisik (Pool Mesin Pabrik)
-- -----------------------------------------------------------------------------------------
PRINT '1. Memeriksa tabel [dbo].[CI_MachinesProcess]...';

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CI_MachinesProcess' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE [dbo].[CI_MachinesProcess] (
        [MachCode]         NVARCHAR(50)   NOT NULL,
        [ProcCode]         NVARCHAR(50)   NOT NULL,
        [CurrentCount]     INT            NULL,
        [MaxCount]         INT            NULL,
        [Division]         NVARCHAR(50)   NULL,
        [LastSyncDate]     DATETIME       NOT NULL DEFAULT (GETDATE()),
        CONSTRAINT [PK_CI_MachinesProcess] PRIMARY KEY CLUSTERED ([MachCode] ASC, [ProcCode] ASC)
    );
    PRINT '   [OK] Tabel [dbo].[CI_MachinesProcess] berhasil dibuat.';
END
ELSE
BEGIN
    PRINT '   [OK] Tabel [dbo].[CI_MachinesProcess] sudah ada.';
    
    -- Pastikan kolom Division ada
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.CI_MachinesProcess') AND name = 'Division')
    BEGIN
        ALTER TABLE [dbo].[CI_MachinesProcess] ADD [Division] NVARCHAR(50) NULL;
        PRINT '   [OK] Kolom [Division] berhasil ditambahkan pada CI_MachinesProcess.';
    END

    -- Pastikan kolom CurrentCount ada
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.CI_MachinesProcess') AND name = 'CurrentCount')
    BEGIN
        ALTER TABLE [dbo].[CI_MachinesProcess] ADD [CurrentCount] INT NULL;
        PRINT '   [OK] Kolom [CurrentCount] berhasil ditambahkan pada CI_MachinesProcess.';
    END

    -- Pastikan kolom MaxCount ada
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.CI_MachinesProcess') AND name = 'MaxCount')
    BEGIN
        ALTER TABLE [dbo].[CI_MachinesProcess] ADD [MaxCount] INT NULL;
        PRINT '   [OK] Kolom [MaxCount] berhasil ditambahkan pada CI_MachinesProcess.';
    END

    -- Pastikan kolom LastSyncDate ada
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.CI_MachinesProcess') AND name = 'LastSyncDate')
    BEGIN
        ALTER TABLE [dbo].[CI_MachinesProcess] ADD [LastSyncDate] DATETIME NOT NULL DEFAULT (GETDATE());
        PRINT '   [OK] Kolom [LastSyncDate] berhasil ditambahkan pada CI_MachinesProcess.';
    END
END
GO

-- Buat indeks untuk performa pencarian pool mesin per proses
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_CI_MachinesProcess_ProcCode' AND object_id = OBJECT_ID('dbo.CI_MachinesProcess'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_CI_MachinesProcess_ProcCode] 
        ON [dbo].[CI_MachinesProcess] ([ProcCode] ASC);
    PRINT '   [OK] Indeks [IX_CI_MachinesProcess_ProcCode] berhasil dibuat.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_CI_MachinesProcess_MachCode' AND object_id = OBJECT_ID('dbo.CI_MachinesProcess'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_CI_MachinesProcess_MachCode] 
        ON [dbo].[CI_MachinesProcess] ([MachCode] ASC);
    PRINT '   [OK] Indeks [IX_CI_MachinesProcess_MachCode] berhasil dibuat.';
END
GO


-- -----------------------------------------------------------------------------------------
-- 2. TABEL: [dbo].[CI_IECycleTime_Header] (Master Standar UPH & Cycle Time Teknik)
--    Menghubungkan Device + Process + Machine + UPH + Machine/Labor Time
-- -----------------------------------------------------------------------------------------
PRINT '2. Memeriksa tabel [dbo].[CI_IECycleTime_Header]...';

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CI_IECycleTime_Header' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE [dbo].[CI_IECycleTime_Header] (
        [Id]               BIGINT IDENTITY(1,1) NOT NULL,
        [TransID]          BIGINT               NOT NULL,
        [DeviceCode]       NVARCHAR(50)         NOT NULL,
        [ProcCode]         NVARCHAR(50)         NULL,
        [MachCode]         NVARCHAR(50)         NOT NULL,
        [UPH]              FLOAT                NULL,
        [MachineTime]      FLOAT                NULL,
        [LaborTime]        FLOAT                NULL,
        [LMRatio]          FLOAT                NULL,
        [ShiftlyCapacity]  FLOAT                NULL,
        [WeeklyCapacity]   FLOAT                NULL,
        [Division]         NVARCHAR(50)         NULL,
        [SourceDatabase]   NVARCHAR(100)        NULL,
        [LastSyncDate]     DATETIME             NOT NULL DEFAULT (GETDATE()),
        CONSTRAINT [PK_CI_IECycleTime_Header_Id] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
    PRINT '   [OK] Tabel [dbo].[CI_IECycleTime_Header] berhasil dibuat.';
END
ELSE
BEGIN
    PRINT '   [OK] Tabel [dbo].[CI_IECycleTime_Header] sudah ada.';

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.CI_IECycleTime_Header') AND name = 'Division')
        ALTER TABLE [dbo].[CI_IECycleTime_Header] ADD [Division] NVARCHAR(50) NULL;

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.CI_IECycleTime_Header') AND name = 'ShiftlyCapacity')
        ALTER TABLE [dbo].[CI_IECycleTime_Header] ADD [ShiftlyCapacity] FLOAT NULL;

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.CI_IECycleTime_Header') AND name = 'WeeklyCapacity')
        ALTER TABLE [dbo].[CI_IECycleTime_Header] ADD [WeeklyCapacity] FLOAT NULL;

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.CI_IECycleTime_Header') AND name = 'SourceDatabase')
        ALTER TABLE [dbo].[CI_IECycleTime_Header] ADD [SourceDatabase] NVARCHAR(100) NULL;

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.CI_IECycleTime_Header') AND name = 'LastSyncDate')
        ALTER TABLE [dbo].[CI_IECycleTime_Header] ADD [LastSyncDate] DATETIME NOT NULL DEFAULT (GETDATE());
END
GO

-- Nonaktifkan constraint foreign key yang kaku agar sync multi-divisi tidak gagal jika ada variasi nama master
ALTER TABLE [dbo].[CI_IECycleTime_Header] NOCHECK CONSTRAINT ALL;
GO

-- Indeks komposit untuk lookup Device & Process
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_CI_IECycleTime_Device_Proc' AND object_id = OBJECT_ID('dbo.CI_IECycleTime_Header'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_CI_IECycleTime_Device_Proc] 
        ON [dbo].[CI_IECycleTime_Header] ([DeviceCode] ASC, [ProcCode] ASC);
    PRINT '   [OK] Indeks [IX_CI_IECycleTime_Device_Proc] berhasil dibuat.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_CI_IECycleTime_MachCode' AND object_id = OBJECT_ID('dbo.CI_IECycleTime_Header'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_CI_IECycleTime_MachCode] 
        ON [dbo].[CI_IECycleTime_Header] ([MachCode] ASC);
    PRINT '   [OK] Indeks [IX_CI_IECycleTime_MachCode] berhasil dibuat.';
END
GO


-- -----------------------------------------------------------------------------------------
-- 3. TABEL: [dbo].[CI_DeviceProcess] (Routing Urutan Proses & Labor Hour per Device)
-- -----------------------------------------------------------------------------------------
PRINT '3. Memeriksa tabel [dbo].[CI_DeviceProcess]...';

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CI_DeviceProcess' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE [dbo].[CI_DeviceProcess] (
        [DeviceCode]       NVARCHAR(50)   NOT NULL,
        [Seq]              INT            NOT NULL,
        [ProcCode]         NVARCHAR(50)   NOT NULL,
        [LaborHour]        FLOAT          NOT NULL DEFAULT (0),
        [MachineHour]      FLOAT          NULL,
        [LimitReject]      FLOAT          NULL,
        [TAKTTime]         FLOAT          NULL,
        [EndOfProcess]     BIT            NULL DEFAULT (0),
        [FirstOfProcess]   BIT            NULL DEFAULT (0),
        CONSTRAINT [PK_CI_DeviceProcess] PRIMARY KEY CLUSTERED ([DeviceCode] ASC, [Seq] ASC)
    );
    PRINT '   [OK] Tabel [dbo].[CI_DeviceProcess] berhasil dibuat.';
END
ELSE
BEGIN
    PRINT '   [OK] Tabel [dbo].[CI_DeviceProcess] sudah ada.';

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.CI_DeviceProcess') AND name = 'LaborHour')
        ALTER TABLE [dbo].[CI_DeviceProcess] ADD [LaborHour] FLOAT NOT NULL DEFAULT (0);

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.CI_DeviceProcess') AND name = 'MachineHour')
        ALTER TABLE [dbo].[CI_DeviceProcess] ADD [MachineHour] FLOAT NULL;

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.CI_DeviceProcess') AND name = 'TAKTTime')
        ALTER TABLE [dbo].[CI_DeviceProcess] ADD [TAKTTime] FLOAT NULL;

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.CI_DeviceProcess') AND name = 'LimitReject')
        ALTER TABLE [dbo].[CI_DeviceProcess] ADD [LimitReject] FLOAT NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_CI_DeviceProcess_ProcCode' AND object_id = OBJECT_ID('dbo.CI_DeviceProcess'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_CI_DeviceProcess_ProcCode] 
        ON [dbo].[CI_DeviceProcess] ([ProcCode] ASC);
    PRINT '   [OK] Indeks [IX_CI_DeviceProcess_ProcCode] berhasil dibuat.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_CI_DeviceProcess_DeviceCode' AND object_id = OBJECT_ID('dbo.CI_DeviceProcess'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_CI_DeviceProcess_DeviceCode] 
        ON [dbo].[CI_DeviceProcess] ([DeviceCode] ASC);
    PRINT '   [OK] Indeks [IX_CI_DeviceProcess_DeviceCode] berhasil dibuat.';
END
GO


-- -----------------------------------------------------------------------------------------
-- 4. TABEL: [dbo].[CI_Yield] (Data Aktual Produksi Harian untuk Live Capacity)
-- -----------------------------------------------------------------------------------------
PRINT '4. Memeriksa tabel [dbo].[CI_Yield]...';

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CI_Yield' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
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

    PRINT '   [OK] Tabel [dbo].[CI_Yield] berhasil dibuat.';
END
ELSE
BEGIN
    PRINT '   [OK] Tabel [dbo].[CI_Yield] sudah ada.';
END
GO


-- -----------------------------------------------------------------------------------------
-- 5. PEMBARUAN KOLOM: Kolom [Area] pada [dbo].[MasterMachines]
-- -----------------------------------------------------------------------------------------
PRINT '5. Memeriksa kolom [Area] pada [dbo].[MasterMachines]...';

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'MasterMachines' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.MasterMachines') AND name = 'Area')
    BEGIN
        ALTER TABLE [dbo].[MasterMachines] ADD [Area] NVARCHAR(50) NULL;
        PRINT '   [OK] Kolom [Area] berhasil ditambahkan pada [MasterMachines].';
    END
    ELSE
    BEGIN
        PRINT '   [OK] Kolom [Area] sudah ada pada [MasterMachines].';
    END
END
GO

PRINT '========================================================================';
PRINT 'Pembaruan Skema Database CI_LSS Selesai dengan Sukses!';
PRINT 'Waktu Selesai: ' + CONVERT(VARCHAR(30), GETDATE(), 120);
PRINT '========================================================================';
GO
