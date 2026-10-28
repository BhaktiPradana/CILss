USE [CI_LSS];
GO

-- ============================================================================
-- Table: dbo.CI_SQ01
-- Description: Data routing sequence, labor hours, dan machine hours 
--              dari file 'ScrapCost and TAKTTime Data.xlsx' (Sheet SQ01)
--              Terkoneksi ke dbo.CI_Product -> dbo.CI_ProductGroup
-- ============================================================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CI_SQ01' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE [dbo].[CI_SQ01] (
        [DeviceCode]       NVARCHAR(50)  NOT NULL,
        [Seq]              INT           NOT NULL,
        [Description]      NVARCHAR(250) NULL,
        [MachineHour]      FLOAT         NULL,
        [LaborHour]        FLOAT         NULL,
        [MachineHour_pcs]  AS [MachineHour],
        [LaborHour_pcs]    AS [LaborHour],
        [MachineHour/pcs]  AS [MachineHour],
        [LaborHour/pcs]    AS [LaborHour],
        CONSTRAINT [FK_CI_SQ01_CI_Product] FOREIGN KEY ([DeviceCode]) 
            REFERENCES [dbo].[CI_Product] ([DeviceCode])
    );

    CREATE CLUSTERED INDEX [CIX_CI_SQ01_DeviceCode_Seq] 
        ON [dbo].[CI_SQ01] ([DeviceCode] ASC, [Seq] ASC);

    CREATE NONCLUSTERED INDEX [IX_CI_SQ01_DeviceCode] 
        ON [dbo].[CI_SQ01] ([DeviceCode] ASC);
END
GO
