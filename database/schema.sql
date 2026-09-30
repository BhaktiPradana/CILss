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

IF NOT EXISTS (SELECT 1 FROM dbo.MasterDBs)
BEGIN
    INSERT INTO dbo.MasterDBs (DBName) VALUES
    (N'PM_LOT207_LT1.DB_AC_C10K'),
    (N'PM_LOT207_LT1.DB_AC_GL'),
    (N'PM_LOT207_LT1.DB_AC_OFFICE'),
    (N'PM_LOT207_LT1.DB_AC_TESTING'),
    (N'PM_LOT207_LT1.DB_C10K_1'),
    (N'PM_LOT207_LT1.DB_C10K_2'),
    (N'PM_LOT207_LT1.DB_CANTEEN'),
    (N'PM_LOT207_LT1.DB_FAC_LT1'),
    (N'ELM_LOT207_LT2.CBS4_DB_MIELE200A'),
    (N'ELM_LOT207_LT2.DB_FFU_LT2'),
    (N'ELM_LOT207_LT2.PMDB_L3'),
    (N'ELM_LOT207_LT2.RGU_DB_4'),
    (N'ELM_LOT207_LT2.RGU_DB_PYRO'),
    (N'ELM_LOT207_LT3.DB_H'),
    (N'ELM_LOT207_LT3.DB_REALIBILITY');
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
