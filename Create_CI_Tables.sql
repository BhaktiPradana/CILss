USE [CI_LSS];
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.MasterMachines') AND name = 'Area')
BEGIN
    ALTER TABLE [dbo].[MasterMachines] ADD [Area] NVARCHAR(50) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CI_DeviceProcess' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE [dbo].[CI_DeviceProcess] (
        [DeviceCode]       NVARCHAR(50)   NOT NULL,
        [Seq]              INT            NOT NULL,
        [ProcCode]         NVARCHAR(50)   NOT NULL,
        [LaborHour]        FLOAT          NOT NULL DEFAULT (0),
        [LimitReject]      FLOAT          NULL,
        [TAKTTime]         FLOAT          NULL,
        CONSTRAINT [PK_CI_DeviceProcess] PRIMARY KEY CLUSTERED ([DeviceCode] ASC, [Seq] ASC)
    );
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CI_IECycleTime_Header' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE [dbo].[CI_IECycleTime_Header] (
        [TransID]          BIGINT         NOT NULL,
        [DeviceCode]       NVARCHAR(50)   NOT NULL,
        [ProcCode]         NVARCHAR(50)   NOT NULL,
        [MachCode]         NVARCHAR(50)   NOT NULL,
        [UPH]              FLOAT          NULL,
        [MachineTime]      FLOAT          NULL,
        [LaborTime]        FLOAT          NULL,
        [LMRatio]          FLOAT          NULL,
        CONSTRAINT [PK_CI_IECycleTime_Header] PRIMARY KEY CLUSTERED ([TransID] ASC)
    );
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_CI_DeviceProcess_ProcCode' AND object_id = OBJECT_ID('dbo.CI_DeviceProcess'))
    CREATE NONCLUSTERED INDEX [IX_CI_DeviceProcess_ProcCode] ON [dbo].[CI_DeviceProcess] ([ProcCode] ASC);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_CI_IECycleTime_Device_Proc' AND object_id = OBJECT_ID('dbo.CI_IECycleTime_Header'))
    CREATE NONCLUSTERED INDEX [IX_CI_IECycleTime_Device_Proc] ON [dbo].[CI_IECycleTime_Header] ([DeviceCode] ASC, [ProcCode] ASC);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_CI_IECycleTime_MachCode' AND object_id = OBJECT_ID('dbo.CI_IECycleTime_Header'))
    CREATE NONCLUSTERED INDEX [IX_CI_IECycleTime_MachCode] ON [dbo].[CI_IECycleTime_Header] ([MachCode] ASC);
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_CI_DeviceProcess_CI_Product')
BEGIN
    ALTER TABLE [dbo].[CI_DeviceProcess] WITH NOCHECK
    ADD CONSTRAINT [FK_CI_DeviceProcess_CI_Product] 
    FOREIGN KEY ([DeviceCode]) REFERENCES [dbo].[CI_Product] ([DeviceCode]);
END

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_CI_DeviceProcess_CI_Process')
BEGIN
    ALTER TABLE [dbo].[CI_DeviceProcess] WITH NOCHECK
    ADD CONSTRAINT [FK_CI_DeviceProcess_CI_Process] 
    FOREIGN KEY ([ProcCode]) REFERENCES [dbo].[CI_Process] ([ProcCode]);
END

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_CI_IECycleTime_Header_MasterMachines')
BEGIN
    ALTER TABLE [dbo].[CI_IECycleTime_Header] WITH NOCHECK
    ADD CONSTRAINT [FK_CI_IECycleTime_Header_MasterMachines] 
    FOREIGN KEY ([MachCode]) REFERENCES [dbo].[MasterMachines] ([MachCode]);
END

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_CI_IECycleTime_Header_CI_Product')
BEGIN
    ALTER TABLE [dbo].[CI_IECycleTime_Header] WITH NOCHECK
    ADD CONSTRAINT [FK_CI_IECycleTime_Header_CI_Product] 
    FOREIGN KEY ([DeviceCode]) REFERENCES [dbo].[CI_Product] ([DeviceCode]);
END

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_CI_IECycleTime_Header_CI_Process')
BEGIN
    ALTER TABLE [dbo].[CI_IECycleTime_Header] WITH NOCHECK
    ADD CONSTRAINT [FK_CI_IECycleTime_Header_CI_Process] 
    FOREIGN KEY ([ProcCode]) REFERENCES [dbo].[CI_Process] ([ProcCode]);
END
GO
