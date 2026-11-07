-- =========================================================================================
-- DEPLOYMENT SCRIPT: CI_IECycleTime_Header & Machine Matrix Support
-- Description: Ensures [dbo].[CI_IECycleTime_Header] and [dbo].[CI_MachinesProcess] exist
--              with complete columns for multi-division sync (MLS, OPTIC, AIS).
-- =========================================================================================

USE [CI_LSS];
GO

-- 1. Table CI_IECycleTime_Header
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
END
ELSE
BEGIN
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

-- 2. Indexes for fast capacity planning queries
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_CI_IECycleTime_Device_Proc' AND object_id = OBJECT_ID('dbo.CI_IECycleTime_Header'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_CI_IECycleTime_Device_Proc] 
        ON [dbo].[CI_IECycleTime_Header] ([DeviceCode] ASC, [ProcCode] ASC);
END;
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_CI_IECycleTime_MachCode' AND object_id = OBJECT_ID('dbo.CI_IECycleTime_Header'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_CI_IECycleTime_MachCode] 
        ON [dbo].[CI_IECycleTime_Header] ([MachCode] ASC);
END;
GO

-- 3. Relax foreign keys so sync won't block if master tables have naming variations
ALTER TABLE [dbo].[CI_IECycleTime_Header] NOCHECK CONSTRAINT ALL;
GO

-- 4. Table CI_MachinesProcess (Machine-to-Process Capability Matrix)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CI_MachinesProcess' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE [dbo].[CI_MachinesProcess] (
        [MachCode]         NVARCHAR(50)   NOT NULL,
        [ProcCode]         NVARCHAR(50)   NOT NULL,
        [CurrentCount]     INT            NULL,
        [MaxCount]         INT            NULL,
        [Division]         NVARCHAR(50)   NULL,
        CONSTRAINT [PK_CI_MachinesProcess] PRIMARY KEY CLUSTERED ([MachCode] ASC, [ProcCode] ASC)
    );
END
GO

PRINT 'CI_IECycleTime_Header and CI_MachinesProcess deployment completed successfully.';
