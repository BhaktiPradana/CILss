-- ============================================================================
-- Script: Create_CI_Yield.sql
-- Description: DDL script untuk membuat tabel [dbo].[CI_Yield]
-- Target Database: [CI_LSS] on BTMSQL01\DIGITMONT_DEV
-- Created for: Daily Device FPY and Overall Yield Synchronization
-- ============================================================================

USE [CI_LSS];
GO

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

    -- Unique composite index untuk mencegah duplikasi data per Device, Tanggal, dan Divisi
    CREATE UNIQUE NONCLUSTERED INDEX [UQ_CI_Yield_Device_Date_Div] 
        ON [dbo].[CI_Yield] ([DeviceCode] ASC, [Date] ASC, [Division] ASC);

    -- Indeks performa untuk filter tanggal dan device
    CREATE NONCLUSTERED INDEX [IX_CI_Yield_Date] 
        ON [dbo].[CI_Yield] ([Date] ASC);

    CREATE NONCLUSTERED INDEX [IX_CI_Yield_DeviceCode] 
        ON [dbo].[CI_Yield] ([DeviceCode] ASC);

    PRINT 'Tabel [dbo].[CI_Yield] dan indeks terkait berhasil dibuat.';
END
ELSE
BEGIN
    PRINT 'Tabel [dbo].[CI_Yield] sudah ada di database.';
END
GO

