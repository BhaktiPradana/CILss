IF DB_ID(N'LssTraining') IS NULL
    CREATE DATABASE [LssTraining];
GO
USE [LssTraining];
GO

IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Users PRIMARY KEY,
        Username nvarchar(80) NOT NULL CONSTRAINT UQ_Users_Username UNIQUE,
        DisplayName nvarchar(160) NOT NULL,
        PasswordHash nvarchar(500) NOT NULL,
        Role nvarchar(30) NOT NULL CONSTRAINT CK_Users_Role CHECK (Role IN ('Admin', 'Trainer')),
        IsActive bit NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT 1,
        CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT SYSUTCDATETIME()
    );
END
IF OBJECT_ID(N'dbo.TrainingPrograms', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TrainingPrograms (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_TrainingPrograms PRIMARY KEY,
        Name nvarchar(180) NOT NULL CONSTRAINT UQ_TrainingPrograms_Name UNIQUE,
        ShortName nvarchar(80) NOT NULL,
        Description nvarchar(500) NOT NULL CONSTRAINT DF_TrainingPrograms_Description DEFAULT '',
        SortOrder int NOT NULL CONSTRAINT DF_TrainingPrograms_SortOrder DEFAULT 0,
        IsGreenBelt bit NOT NULL CONSTRAINT DF_TrainingPrograms_IsGreenBelt DEFAULT 0,
        IsActive bit NOT NULL CONSTRAINT DF_TrainingPrograms_IsActive DEFAULT 1
    );
END
IF OBJECT_ID(N'dbo.Participants', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Participants (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Participants PRIMARY KEY,
        EmployeeId nvarchar(32) NOT NULL CONSTRAINT UQ_Participants_EmployeeId UNIQUE,
        FullName nvarchar(160) NOT NULL,
        Email nvarchar(160) NULL,
        Department nvarchar(120) NULL,
        Position nvarchar(120) NULL,
        StatusPhase nvarchar(30) NULL,
        IsActive bit NOT NULL CONSTRAINT DF_Participants_IsActive DEFAULT 1,
        CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_Participants_CreatedAt DEFAULT SYSUTCDATETIME(),
        UpdatedAt datetime2(0) NULL
    );
END
ELSE IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Participants') AND name = N'StatusPhase')
BEGIN
    ALTER TABLE dbo.Participants ADD StatusPhase nvarchar(30) NULL;
END
IF OBJECT_ID(N'dbo.TrainingModules', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TrainingModules (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_TrainingModules PRIMARY KEY,
        ProgramId int NOT NULL CONSTRAINT FK_TrainingModules_Program REFERENCES dbo.TrainingPrograms(Id),
        Name nvarchar(160) NOT NULL,
        Description nvarchar(500) NULL,
        TargetHours decimal(8,2) NOT NULL CONSTRAINT CK_TrainingModules_TargetHours CHECK (TargetHours > 0),
        SortOrder int NOT NULL CONSTRAINT DF_TrainingModules_SortOrder DEFAULT 1,
        IsRequired bit NOT NULL CONSTRAINT DF_TrainingModules_IsRequired DEFAULT 1,
        CONSTRAINT UQ_TrainingModules_Program_Name UNIQUE (ProgramId, Name)
    );
END
IF OBJECT_ID(N'dbo.Enrollments', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Enrollments (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Enrollments PRIMARY KEY,
        ParticipantId int NOT NULL CONSTRAINT FK_Enrollments_Participant REFERENCES dbo.Participants(Id),
        ProgramId int NOT NULL CONSTRAINT FK_Enrollments_Program REFERENCES dbo.TrainingPrograms(Id),
        EnrolledAt datetime2(0) NOT NULL CONSTRAINT DF_Enrollments_EnrolledAt DEFAULT SYSUTCDATETIME(),
        Status nvarchar(30) NOT NULL CONSTRAINT DF_Enrollments_Status DEFAULT 'In Progress' CONSTRAINT CK_Enrollments_Status CHECK (Status IN ('In Progress', 'Completed', 'Certified')),
        TocFlag bit NOT NULL CONSTRAINT DF_Enrollments_TocFlag DEFAULT 0,
        CertifiedAt datetime2(0) NULL,
        CONSTRAINT UQ_Enrollments_Participant_Program UNIQUE (ParticipantId, ProgramId)
    );
END
IF OBJECT_ID(N'dbo.ParticipantModules', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ParticipantModules (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_ParticipantModules PRIMARY KEY,
        EnrollmentId int NOT NULL CONSTRAINT FK_ParticipantModules_Enrollment REFERENCES dbo.Enrollments(Id),
        ModuleId int NOT NULL CONSTRAINT FK_ParticipantModules_Module REFERENCES dbo.TrainingModules(Id),
        ActualHours decimal(8,2) NOT NULL CONSTRAINT DF_ParticipantModules_ActualHours DEFAULT 0,
        Status nvarchar(30) NOT NULL CONSTRAINT DF_ParticipantModules_Status DEFAULT 'Not Started' CONSTRAINT CK_ParticipantModules_Status CHECK (Status IN ('Not Started', 'In Progress', 'Completed', 'Overridden')),
        IsOverridden bit NOT NULL CONSTRAINT DF_ParticipantModules_IsOverridden DEFAULT 0,
        OverrideReason nvarchar(500) NULL,
        UpdatedAt datetime2(0) NOT NULL CONSTRAINT DF_ParticipantModules_UpdatedAt DEFAULT SYSUTCDATETIME(),
        CONSTRAINT UQ_ParticipantModules_Enrollment_Module UNIQUE (EnrollmentId, ModuleId)
    );
END
IF OBJECT_ID(N'dbo.GreenBeltReviews', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.GreenBeltReviews (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_GreenBeltReviews PRIMARY KEY,
        EnrollmentId int NOT NULL CONSTRAINT FK_GreenBeltReviews_Enrollment REFERENCES dbo.Enrollments(Id),
        ReviewNumber tinyint NOT NULL CONSTRAINT CK_GreenBeltReviews_Number CHECK (ReviewNumber BETWEEN 0 AND 5),
        Status nvarchar(30) NOT NULL CONSTRAINT DF_GreenBeltReviews_Status DEFAULT 'Not Started' CONSTRAINT CK_GreenBeltReviews_Status CHECK (Status IN ('Not Started', 'In Progress', 'Completed')),
        DmaicStage nvarchar(20) NULL CONSTRAINT CK_GreenBeltReviews_Stage CHECK (DmaicStage IS NULL OR DmaicStage IN ('Define', 'Measure', 'Analyze', 'Improve', 'Control')),
        TrainerComment nvarchar(2000) NULL,
        ReviewedAt datetime2(0) NULL,
        CONSTRAINT UQ_GreenBeltReviews_Enrollment_Number UNIQUE (EnrollmentId, ReviewNumber)
    );
END
IF OBJECT_ID(N'dbo.ActivityLogs', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ActivityLogs (
        Id bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_ActivityLogs PRIMARY KEY,
        Description nvarchar(500) NOT NULL,
        ActorName nvarchar(160) NULL,
        Type nvarchar(40) NOT NULL,
        CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_ActivityLogs_CreatedAt DEFAULT SYSUTCDATETIME()
    );
END

IF NOT EXISTS (SELECT 1 FROM dbo.TrainingPrograms)
BEGIN
    INSERT INTO dbo.TrainingPrograms (Name, ShortName, Description, SortOrder, IsGreenBelt) VALUES
    (N'Training White Belt', N'White Belt', N'Lean Six Sigma fundamentals.', 1, 0),
    (N'Training & Certification Yellow Belt', N'Yellow Belt', N'Operational problem solving & DMAIC.', 2, 0),
    (N'Training & Certification Green Belt + TOC', N'Green Belt', N'DMAIC project leadership & TOC.', 3, 1),
    (N'Certified Black Belt', N'Black Belt', N'Advanced strategy & transformation.', 4, 0);
END
IF NOT EXISTS (SELECT 1 FROM dbo.TrainingModules)
BEGIN
    INSERT INTO dbo.TrainingModules (ProgramId, Name, Description, TargetHours, SortOrder) SELECT Id, N'Lean Six Sigma Foundations', N'Core concepts of Lean, Six Sigma, and continuous improvement.', 8, 1 FROM dbo.TrainingPrograms WHERE ShortName = N'White Belt';
    INSERT INTO dbo.TrainingModules (ProgramId, Name, Description, TargetHours, SortOrder) SELECT Id, N'Problem Solving Essentials', N'Problem-solving toolkits and DMAIC methodology.', 16, 1 FROM dbo.TrainingPrograms WHERE ShortName = N'Yellow Belt';
    INSERT INTO dbo.TrainingModules (ProgramId, Name, Description, TargetHours, SortOrder) SELECT Id, N'DMAIC Project Leadership', N'End-to-end DMAIC project leadership from Define to Control.', 24, 1 FROM dbo.TrainingPrograms WHERE ShortName = N'Green Belt';
    INSERT INTO dbo.TrainingModules (ProgramId, Name, Description, TargetHours, SortOrder) SELECT Id, N'TOC & Constraint Management', N'Theory of Constraints applied to project acceleration.', 8, 2 FROM dbo.TrainingPrograms WHERE ShortName = N'Green Belt';
    INSERT INTO dbo.TrainingModules (ProgramId, Name, Description, TargetHours, SortOrder) SELECT Id, N'Advanced Improvement Strategy', N'Enterprise transformation strategy and executive coaching.', 40, 1 FROM dbo.TrainingPrograms WHERE ShortName = N'Black Belt';
END
GO

IF OBJECT_ID(N'dbo.MasterDBs', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.MasterDBs (
        DBID int IDENTITY(1,1) NOT NULL CONSTRAINT PK_MasterDBs PRIMARY KEY,
        DBName nvarchar(250) NOT NULL CONSTRAINT UQ_MasterDBs_DBName UNIQUE
    );
END

IF OBJECT_ID(N'dbo.MasterMachines', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.MasterMachines (
        MachCode nvarchar(50) NOT NULL CONSTRAINT PK_MasterMachines PRIMARY KEY,
        MachName nvarchar(250) NOT NULL
    );
END

IF OBJECT_ID(N'dbo.MasterMachineConnections', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.MasterMachineConnections (
        ConnectionID int IDENTITY(1,1) NOT NULL CONSTRAINT PK_MasterMachineConnections PRIMARY KEY,
        MachCode nvarchar(50) NOT NULL CONSTRAINT FK_MasterMachineConnections_Machines REFERENCES dbo.MasterMachines(MachCode),
        SubDBName nvarchar(250) NULL,
        DBName nvarchar(250) NOT NULL,
        Remarks nvarchar(500) NULL
    );
END

IF OBJECT_ID(N'dbo.Daily_kWh_Data', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Daily_kWh_Data (
        ID int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Daily_kWh_Data PRIMARY KEY,
        LogDate date NOT NULL,
        MeterName nvarchar(250) NOT NULL,
        Total_kWh decimal(18,4) NOT NULL,
        SyncDate datetime2(3) NOT NULL CONSTRAINT DF_Daily_kWh_SyncDate DEFAULT SYSUTCDATETIME()
    );
END

IF OBJECT_ID(N'dbo.Hourly_kWh_Data', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Hourly_kWh_Data (
        ID int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Hourly_kWh_Data PRIMARY KEY,
        LogTime datetime2(3) NOT NULL,
        MeterName nvarchar(250) NOT NULL,
        kWh_Usage decimal(18,4) NOT NULL,
        SyncDate datetime2(3) NOT NULL CONSTRAINT DF_Hourly_kWh_SyncDate DEFAULT SYSUTCDATETIME()
    );
END

IF OBJECT_ID(N'dbo.MachineImprovements', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.MachineImprovements (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_MachineImprovements PRIMARY KEY,
        MachCode nvarchar(50) NOT NULL,
        ImprovementTitle nvarchar(250) NOT NULL,
        Description nvarchar(1000) NULL,
        ImplementationDate date NOT NULL,
        BaselineKwhPerDay decimal(18,2) NOT NULL DEFAULT 0,
        TargetKwhPerDay decimal(18,2) NOT NULL DEFAULT 0,
        ActualKwhPerDay decimal(18,2) NULL,
        Status nvarchar(30) NOT NULL DEFAULT 'In Progress',
        CreatedBy nvarchar(160) NULL,
        CreatedAt datetime2(0) NOT NULL DEFAULT SYSUTCDATETIME()
    );
END

IF NOT EXISTS (SELECT 1 FROM dbo.MasterDBs)
BEGIN
    INSERT INTO dbo.MasterDBs (DBName) VALUES
    -- Building 207: Overall
    (N'PM DB SSB1T'), (N'PM DB SSB2TA'), (N'PM DB SSB2TB'), (N'PM DB SSB3TA'), (N'PM DB SSB3TB'),
    -- Building 207: Detection 1st floor
    (N'DB Testing 1st Floor'), (N'DB AC Testing'), (N'DB SMT ( GL )'), (N'Reflow #1'), (N'Reflow #2'),
    (N'DB Reflow #3'), (N'Washing'), (N'AC SMT (GL)'), (N'Compressor SMT'), (N'DETECTION 1ST FLOOR Real Energy Into the Load'),
    (N'WAREHOUSE'), (N'AC_C10K'), (N'C10K_1'), (N'C10K_2'), (N'L-L250A'),
    -- Building 207: Detection 2nd floor
    (N'2Z4'), (N'2ZA'), (N'AC AHU 2-1'), (N'AC AHU 2-2'), (N'DB_FFU_LT2'), (N'DB JOGAN'), (N'DB_LVP'),
    (N'DB_NCR1'), (N'DB_NCR2'), (N'DB_PHASE 6'), (N'DB_NEW POLARIS'), (N'DB_POL 10 11'), (N'DB_POL 13-14'),
    (N'DB_POL 15-17'), (N'DB_POL18-20'), (N'DB_PYRO 1.6'), (N'DB_SAWING'), (N'DB_T1'), (N'DB_AC_OFFICE_DETECTION'),
    (N'PM_DB_OFFICE_DETECTION'), (N'DB_P1'), (N'DB_P2'), (N'DB4'), (N'DB_MIELE'), (N'AC IRD'),
    (N'CHILLER_1-4'), (N'DB L3'), (N'DB_REALIBILITY'), (N'VACUMP'),
    -- Building 207: Detection 3rd floor
    (N'3Z1 ( ex SMD )'), (N'3Z1A (ex-Testing)'), (N'AC 3rd Floor'), (N'LIGHTING FTC'), (N'WORKSHOP'),
    -- Building 207: Detection Special Lighting floor
    (N'AC SL'), (N'Lighting Hydra'), (N'DB SL200A'), (N'HYD #1'), (N'HYD #2'), (N'HYD #3'), (N'HYD #4'),
    (N'DB A'), (N'DB UV'), (N'DB_H'), (N'DB_PHASE 1'), (N'DB_PHASE 2'), (N'AC 250A LSM'), (N'CNC'),
    -- Building 209: Overall
    (N'PM SSB1TA'), (N'PM SSB1TB'), (N'PM SSB2TA'), (N'PM SSB2TB'), (N'PM SSB3TB'),
    -- Building 209: 1st floor
    (N'PM_LOT209_LT1.PM_AC_CL2 Real Energy Into the Load'),
    (N'PM_LOT209_LT1.PM_CHILLER Real Energy Into the Load'),
    (N'PM_LOT209_LT1.PM_DB_CHILLER Real Energy Into the Load'),
    (N'PM_LOT209_LT1.PM_DBCanteen Real Energy Into the Load'),
    (N'PM_LOT209_LT1.PM_DBRO Real Energy Into the Load'),
    (N'PM_LOT209_LT1.PM_MDB400A Real Energy Into the Load'),
    (N'PM_LOT209_LT1.PM_ULTRASONIC1 Real Energy Into the Load'),
    (N'PM_LOT209_LT1.PM_ULTRASONIC2 Real Energy Into the Load'),
    (N'PM_LOT209_LT1.PM_ULTRASONIC3 Real Energy Into the Load'),
    (N'PM_LOT209_LT1.PM_ULTRASONIC4 Real Energy Into the Load'),
    (N'PM_LOT209_LT1.PM_WWTP Real Energy Into the Load'),
    (N'PM_LOT209_LT1.PMDB_AC320A Real Energy Into the Load'),
    (N'PM_LOT209_LT1.PMDB_COATING Real Energy Into the Load'),
    (N'PM_LOT209_LT1.PMDB_COMPRESSOR Real Energy Into the Load'),
    (N'PM_LOT209_LT1.PMDB_LGHTNG Real Energy Into the Load'),
    (N'PM_LOT209_LT1.PMDB_LIGHTING_2 Real Energy Into the Load'),
    (N'PM_LOT209_LT1.PMDB_MC1 Real Energy Into the Load'),
    (N'PM_LOT209_LT1.PMDB_MC2 Real Energy Into the Load'),
    (N'PM_LOT209_LT1.PMDB_MC3 Real Energy Into the Load'),
    (N'PM_LOT209_LT1.PMDB_POWER_100A Real Energy Into the Load'),
    (N'PM_LOT209_LT1.PMDB_POWER_EXHAUST Real Energy Into the Load'),
    (N'PM_LOT209_LT1.PMMDB_320A Real Energy Into the Load'),
    (N'Total SSB Lt# 1 209 Real Energy Into the Load'),
    -- Building 209: 2nd floor
    (N'PM_LOT209_LT2.PM_ASTROFURNACE2 Real Energy Into the Load'),
    (N'PM_LOT209_LT2.PM_ASTROFURNACE4 Real Energy Into the Load'),
    (N'PM_LOT209_LT2.PM_CAMCOFURNACE7 Real Energy Into the Load'),
    (N'PM_LOT209_LT2.PM_CAMCOFURNACE8 Real Energy Into the Load'),
    (N'PM_LOT209_LT2.PM_CHILLER13 Real Energy Into the Load'),
    (N'PM_LOT209_LT2.PM_LOT209_EXHAUST2_LT2 Real Energy Into the Load'),
    (N'PM_LOT209_LT2.PM_NIPLATING Real Energy Into the Load'),
    (N'PM_LOT209_LT2.PMDB_AGING1 Real Energy Into the Load'),
    (N'PM_LOT209_LT2.PMDB_AGING2 Real Energy Into the Load'),
    (N'PM_LOT209_LT2.PMDB_ASTRO Real Energy Into the Load'),
    (N'PM_LOT209_LT2.PMDB1_250A Real Energy Into the Load'),
    (N'PM_LOT209_LT2.PMDB1_320A Real Energy Into the Load'),
    (N'PM_LOT209_LT2.PMDB2_320A Real Energy Into the Load'),
    (N'PM_LOT209_LT2.PMDB2_500A Real Energy Into the Load'),
    (N'PM_LOT209_LT2.PMDB3_320A Real Energy Into the Load'),
    (N'PM_LOT209_LT2.PMDB3_500A Real Energy Into the Load'),
    (N'PM_LOT209_LT2.PMDB4_LIGHTING Real Energy Into the Load'),
    (N'PM_LOT209_LT2.PMPANEL_EXHAUST Real Energy Into the Load'),
    (N'Total SSB Lt# 2 209 Real Energy Into the Load'),
    -- Building 209: 3rd floor
    (N'PM_LOT209_LT3.PM_CHILLER_ROOFTOP Real Energy Into the Load'),
    (N'PM_LOT209_LT3.PMDB_COMPRESSOR_LT3 Real Energy Into the Load'),
    (N'PM_LOT209_LT3.PMDB_EXHAUST_LT3 Real Energy Into the Load'),
    (N'PM_LOT209_LT3.PMDB_LIGHTING_LT3 Real Energy Into the Load'),
    (N'PM_LOT209_LT3.PMDB_MC_LT3 Real Energy Into the Load'),
    (N'PM_LOT209_LT3.PMDB_MC2_LT3 Real Energy Into the Load'),
    (N'PM_LOT209_LT3.PMDB_POWER_LT3 Real Energy Into the Load'),
    (N'PM_LOT209_LT3.PMMDB_400A_LT3 Real Energy Into the Load'),
    (N'PM_LOT209_LT3.PMSSB3TB Real Energy Into the Load'),
    (N'PM_LOT209_LT3.MDB_200A_AC_TC Real Energy Into the Load'),
    (N'PM_LOT209_LT3.L_P Real Energy Into the Load'),
    (N'PM_LOT209_LT3.MC_TC Real Energy Into the Load'),
    (N'PM_LOT209_LT3.P Real Energy Into the Load'),
    -- Building 238
    (N'PM_LOT238.PM_MAIN Voltage A-B (V)');
END

IF NOT EXISTS (SELECT 1 FROM dbo.MasterMachines)
BEGIN
    INSERT INTO dbo.MasterMachines (MachCode, MachName) VALUES
    (N'ACT1', N'CAP TRANSFER'),
    (N'AD01', N'AUTO DISPENSING #1'),
    (N'AD02', N'AUTO DISPENSING #2'),
    (N'AD03', N'AUTO DISPENSING #3'),
    (N'AD04', N'AUTO DISPENSING #4'),
    (N'AD05', N'AUTO DISPENSING #5'),
    (N'AD06', N'AUTO DISPENSING #6'),
    (N'AD07', N'AUTO DISPENSING #7'),
    (N'AD08', N'AUTO DISPENSING #8'),
    (N'AD09', N'AUTO DISPENSING #9'),
    (N'AD10', N'AUTO DISPENSING #10'),
    (N'AGV1', N'OASIS 300C (AGV) #1'),
    (N'ALB1', N'AUTO LOADING BASEPLATE#1'),
    (N'ALB2', N'AUTO LOADING BASEPLATE#2'),
    (N'ALB3', N'AUTO LOADING BASEPLATE#3'),
    (N'AQL0', N'AQL SMD'),
    (N'AQL1', N'AQL IRD'),
    (N'AQL2', N'AQL GD');
END
GO
