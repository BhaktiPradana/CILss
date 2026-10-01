using Dapper;
using LssTraining.Web.Models;
using Microsoft.Data.SqlClient;

namespace LssTraining.Web.Data;

public interface IEnrollmentRepository
{
    Task<IReadOnlyList<Enrollment>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<int> CreateAsync(int participantId, int programId, bool tocFlag, string? actorName = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ModuleProgress>> GetProgressAsync(int enrollmentId, CancellationToken cancellationToken = default);
    Task UpdateHoursAsync(int progressId, decimal actualHours, string? actorName = null, CancellationToken cancellationToken = default);
    Task OverrideAsync(int progressId, string status, string reason, string? actorName = null, CancellationToken cancellationToken = default);
    Task<int?> EnrollOrUpdateParticipantAsync(int participantId, int? programId, string? statusPhase, bool tocFlag, string? actorName = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ParticipantProgramProgress>> GetParticipantProgramsAsync(int participantId, CancellationToken cancellationToken = default);
    Task SaveModuleCompletionsAsync(int enrollmentId, IEnumerable<int> completedModuleIds, string? actorName = null, CancellationToken cancellationToken = default);
    Task SetCertifiedAsync(int enrollmentId, bool isCertified, string? actorName = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProgramGraduateItem>> GetProgramGraduatesAsync(int programId, CancellationToken cancellationToken = default);
    Task EnsureParticipantModulesAsync(CancellationToken cancellationToken = default);
}

public sealed class EnrollmentRepository(IDbConnectionFactory connectionFactory) : IEnrollmentRepository
{
    public async Task EnsureParticipantModulesAsync(CancellationToken cancellationToken = default)
    {
        using var connection = connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition("""
            INSERT INTO ParticipantModules (EnrollmentId, ModuleId, ActualHours, Status, IsOverridden, UpdatedAt)
            SELECT e.Id, tm.Id, 0, 'Not Started', 0, SYSUTCDATETIME()
            FROM Enrollments e
            JOIN TrainingModules tm ON tm.ProgramId = e.ProgramId
            WHERE NOT EXISTS (
                SELECT 1
                FROM ParticipantModules pm
                WHERE pm.EnrollmentId = e.Id AND pm.ModuleId = tm.Id
            );
            """, cancellationToken: cancellationToken));
    }

    public async Task<IReadOnlyList<Enrollment>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await EnsureParticipantModulesAsync(cancellationToken);

        using var connection = connectionFactory.CreateConnection();
        var command = new CommandDefinition("""
            SELECT e.*,
                   p.FullName AS ParticipantName,
                   p.EmployeeId,
                   tp.Name AS ProgramName,
                   (
                       SELECT COUNT(1)
                       FROM ParticipantModules pm
                       WHERE pm.EnrollmentId = e.Id
                         AND (pm.Status IN ('Completed', 'Overridden') OR pm.ActualHours >= (SELECT tm.TargetHours FROM TrainingModules tm WHERE tm.Id = pm.ModuleId))
                   ) AS CompletedModulesCount,
                   (
                       SELECT COUNT(1)
                       FROM TrainingModules tm
                       WHERE tm.ProgramId = e.ProgramId
                   ) AS TotalModulesCount
            FROM Enrollments e
            JOIN Participants p ON p.Id = e.ParticipantId
            JOIN TrainingPrograms tp ON tp.Id = e.ProgramId
            ORDER BY e.EnrolledAt DESC;
            """, cancellationToken: cancellationToken);

        var enrollments = await connection.QueryAsync<Enrollment>(command);
        return enrollments.ToList();
    }

    public async Task<int> CreateAsync(int participantId, int programId, bool tocFlag, string? actorName = null, CancellationToken cancellationToken = default)
    {
        await using var connection = (SqlConnection)connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            var enrollmentId = await connection.ExecuteScalarAsync<int>(new CommandDefinition("""
                INSERT INTO Enrollments (ParticipantId, ProgramId, TocFlag)
                OUTPUT INSERTED.Id
                VALUES (@ParticipantId, @ProgramId, @TocFlag);
                """,
                new { ParticipantId = participantId, ProgramId = programId, TocFlag = tocFlag },
                transaction,
                cancellationToken: cancellationToken));

            await connection.ExecuteAsync(new CommandDefinition("""
                INSERT INTO ParticipantModules (EnrollmentId, ModuleId)
                SELECT @EnrollmentId, Id
                FROM TrainingModules
                WHERE ProgramId = @ProgramId;
                """,
                new { EnrollmentId = enrollmentId, ProgramId = programId },
                transaction,
                cancellationToken: cancellationToken));

            var isGreenBelt = await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
                "SELECT IsGreenBelt FROM TrainingPrograms WHERE Id = @ProgramId;",
                new { ProgramId = programId },
                transaction,
                cancellationToken: cancellationToken));

            if (isGreenBelt)
            {
                await connection.ExecuteAsync(new CommandDefinition("""
                    INSERT INTO GreenBeltReviews (EnrollmentId, ReviewNumber)
                    SELECT @EnrollmentId, value
                    FROM (VALUES (0), (1), (2), (3), (4), (5)) AS v(value)
                    WHERE NOT EXISTS (
                        SELECT 1
                        FROM GreenBeltReviews
                        WHERE EnrollmentId = @EnrollmentId AND ReviewNumber = v.value
                    );
                    """,
                    new { EnrollmentId = enrollmentId },
                    transaction,
                    cancellationToken: cancellationToken));
            }

            var (participantName, programName) = await connection.QuerySingleAsync<(string, string)>(new CommandDefinition("""
                SELECT p.FullName, tp.Name
                FROM Participants p, TrainingPrograms tp
                WHERE p.Id = @ParticipantId AND tp.Id = @ProgramId;
                """,
                new { ParticipantId = participantId, ProgramId = programId },
                transaction,
                cancellationToken: cancellationToken));

            await connection.ExecuteAsync(new CommandDefinition("""
                INSERT INTO ActivityLogs (Description, ActorName, Type)
                VALUES (@Description, @ActorName, 'Enrollment');
                """,
                new
                {
                    Description = $"Enrolled {participantName} in {programName}",
                    ActorName = actorName ?? "System"
                },
                transaction,
                cancellationToken: cancellationToken));

            await transaction.CommitAsync(cancellationToken);
            return enrollmentId;
        }
        catch (SqlException exception) when (exception.Number is 2601 or 2627)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw new InvalidOperationException("Participant is already enrolled in this program.", exception);
        }
    }

    public async Task<IReadOnlyList<ModuleProgress>> GetProgressAsync(int enrollmentId, CancellationToken cancellationToken = default)
    {
        await EnsureParticipantModulesAsync(cancellationToken);

        using var connection = connectionFactory.CreateConnection();
        var command = new CommandDefinition("""
            SELECT pm.Id,
                   pm.EnrollmentId,
                   pm.ModuleId,
                   m.Name AS ModuleName,
                   m.TargetHours,
                   pm.ActualHours,
                   pm.Status,
                   pm.IsOverridden,
                   pm.OverrideReason
            FROM ParticipantModules pm
            JOIN TrainingModules m ON m.Id = pm.ModuleId
            WHERE pm.EnrollmentId = @EnrollmentId
            ORDER BY m.SortOrder, m.Id;
            """,
            new { EnrollmentId = enrollmentId },
            cancellationToken: cancellationToken);

        var progressList = await connection.QueryAsync<ModuleProgress>(command);
        return progressList.ToList();
    }

    public async Task UpdateHoursAsync(int progressId, decimal actualHours, string? actorName = null, CancellationToken cancellationToken = default)
    {
        await using var connection = (SqlConnection)connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(cancellationToken);

        var row = await connection.QuerySingleAsync<(int EnrollmentId, string ModuleName, decimal TargetHours, bool IsOverridden)>(new CommandDefinition("""
            SELECT pm.EnrollmentId, m.Name AS ModuleName, m.TargetHours, pm.IsOverridden
            FROM ParticipantModules pm
            JOIN TrainingModules m ON m.Id = pm.ModuleId
            WHERE pm.Id = @Id;
            """,
            new { Id = progressId },
            transaction,
            cancellationToken: cancellationToken));

        var status = row.IsOverridden
            ? "Overridden"
            : actualHours <= 0
                ? "Not Started"
                : actualHours >= row.TargetHours
                    ? "Completed"
                    : "In Progress";

        await connection.ExecuteAsync(new CommandDefinition("""
            UPDATE ParticipantModules
            SET ActualHours = @ActualHours,
                Status = @Status,
                UpdatedAt = SYSUTCDATETIME()
            WHERE Id = @Id;
            """,
            new { Id = progressId, ActualHours = actualHours, Status = status },
            transaction,
            cancellationToken: cancellationToken));

        await RecalculateEnrollmentAsync(connection, transaction, row.EnrollmentId, cancellationToken);

        await connection.ExecuteAsync(new CommandDefinition("""
            INSERT INTO ActivityLogs (Description, ActorName, Type)
            VALUES (@Description, @ActorName, 'Progress');
            """,
            new { Description = $"Updated hours for module {row.ModuleName}", ActorName = actorName ?? "System" },
            transaction,
            cancellationToken: cancellationToken));

        await transaction.CommitAsync(cancellationToken);
    }

    public async Task OverrideAsync(int progressId, string status, string reason, string? actorName = null, CancellationToken cancellationToken = default)
    {
        await using var connection = (SqlConnection)connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(cancellationToken);

        await connection.ExecuteAsync(new CommandDefinition("""
            UPDATE ParticipantModules
            SET Status = @Status,
                IsOverridden = 1,
                OverrideReason = @Reason,
                UpdatedAt = SYSUTCDATETIME()
            WHERE Id = @Id;
            """,
            new { Id = progressId, Status = status, Reason = reason },
            transaction,
            cancellationToken: cancellationToken));

        var enrollmentId = await connection.ExecuteScalarAsync<int>(new CommandDefinition(
            "SELECT EnrollmentId FROM ParticipantModules WHERE Id = @Id;",
            new { Id = progressId },
            transaction,
            cancellationToken: cancellationToken));

        await RecalculateEnrollmentAsync(connection, transaction, enrollmentId, cancellationToken);

        await connection.ExecuteAsync(new CommandDefinition("""
            INSERT INTO ActivityLogs (Description, ActorName, Type)
            VALUES (@Description, @ActorName, 'Progress');
            """,
            new { Description = $"Overrode module status: {reason}", ActorName = actorName ?? "System" },
            transaction,
            cancellationToken: cancellationToken));

        await transaction.CommitAsync(cancellationToken);
    }

    private static async Task RecalculateEnrollmentAsync(SqlConnection connection, SqlTransaction transaction, int enrollmentId, CancellationToken cancellationToken)
    {
        var currentStatus = await connection.ExecuteScalarAsync<string>(new CommandDefinition(
            "SELECT Status FROM Enrollments WHERE Id = @EnrollmentId;",
            new { EnrollmentId = enrollmentId },
            transaction,
            cancellationToken: cancellationToken));

        if (currentStatus == "Certified")
        {
            return;
        }

        await connection.ExecuteAsync(new CommandDefinition("""
            UPDATE Enrollments
            SET Status = CASE
                WHEN EXISTS (
                    SELECT 1
                    FROM ParticipantModules pm
                    JOIN TrainingModules m ON m.Id = pm.ModuleId
                    WHERE pm.EnrollmentId = @EnrollmentId
                      AND m.IsRequired = 1
                      AND pm.Status <> 'Completed'
                      AND pm.Status <> 'Overridden'
                ) THEN 'In Progress'
                ELSE 'Completed'
            END
            WHERE Id = @EnrollmentId;
            """,
            new { EnrollmentId = enrollmentId },
            transaction,
            cancellationToken: cancellationToken));
    }

    public async Task<int?> EnrollOrUpdateParticipantAsync(
        int participantId,
        int? programId,
        string? statusPhase,
        bool tocFlag,
        string? actorName = null,
        CancellationToken cancellationToken = default)
    {
        await using var connection = (SqlConnection)connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            await connection.ExecuteAsync(new CommandDefinition("""
                UPDATE Participants
                SET StatusPhase = @StatusPhase,
                    UpdatedAt = SYSUTCDATETIME()
                WHERE Id = @ParticipantId;
                """,
                new
                {
                    ParticipantId = participantId,
                    StatusPhase = string.IsNullOrWhiteSpace(statusPhase) ? null : statusPhase.Trim()
                },
                transaction,
                cancellationToken: cancellationToken));

            int? enrollmentId = null;
            if (programId.HasValue && programId.Value > 0)
            {
                var existingEnrollment = await connection.QuerySingleOrDefaultAsync<Enrollment>(new CommandDefinition("""
                    SELECT TOP 1 *
                    FROM Enrollments
                    WHERE ParticipantId = @ParticipantId AND ProgramId = @ProgramId;
                    """,
                    new { ParticipantId = participantId, ProgramId = programId.Value },
                    transaction,
                    cancellationToken: cancellationToken));

                if (existingEnrollment is null)
                {
                    enrollmentId = await connection.ExecuteScalarAsync<int>(new CommandDefinition("""
                        INSERT INTO Enrollments (ParticipantId, ProgramId, TocFlag)
                        OUTPUT INSERTED.Id
                        VALUES (@ParticipantId, @ProgramId, @TocFlag);
                        """,
                        new { ParticipantId = participantId, ProgramId = programId.Value, TocFlag = tocFlag },
                        transaction,
                        cancellationToken: cancellationToken));

                    await connection.ExecuteAsync(new CommandDefinition("""
                        INSERT INTO ParticipantModules (EnrollmentId, ModuleId)
                        SELECT @EnrollmentId, Id
                        FROM TrainingModules
                        WHERE ProgramId = @ProgramId;
                        """,
                        new { EnrollmentId = enrollmentId.Value, ProgramId = programId.Value },
                        transaction,
                        cancellationToken: cancellationToken));

                    var isGreenBelt = await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
                        "SELECT IsGreenBelt FROM TrainingPrograms WHERE Id = @ProgramId;",
                        new { ProgramId = programId.Value },
                        transaction,
                        cancellationToken: cancellationToken));

                    if (isGreenBelt)
                    {
                        // This part previously triggered a GENERATE_SERIES error during Add New Participant
                        await connection.ExecuteAsync(new CommandDefinition("""
                            INSERT INTO GreenBeltReviews (EnrollmentId, ReviewNumber)
                            SELECT @EnrollmentId, value
                            FROM (VALUES (0), (1), (2), (3), (4), (5)) AS v(value)
                            WHERE NOT EXISTS (
                                SELECT 1
                                FROM GreenBeltReviews
                                WHERE EnrollmentId = @EnrollmentId AND ReviewNumber = v.value
                            );
                            """,
                            new { EnrollmentId = enrollmentId.Value },
                            transaction,
                            cancellationToken: cancellationToken));
                    }

                    var (pName, progName) = await connection.QuerySingleAsync<(string, string)>(new CommandDefinition("""
                        SELECT p.FullName, tp.Name
                        FROM Participants p, TrainingPrograms tp
                        WHERE p.Id = @ParticipantId AND tp.Id = @ProgramId;
                        """,
                        new { ParticipantId = participantId, ProgramId = programId.Value },
                        transaction,
                        cancellationToken: cancellationToken));

                    await connection.ExecuteAsync(new CommandDefinition("""
                        INSERT INTO ActivityLogs (Description, ActorName, Type)
                        VALUES (@Description, @ActorName, 'Enrollment');
                        """,
                        new
                        {
                            Description = $"Enrolled {pName} in {progName}",
                            ActorName = actorName ?? "System"
                        },
                        transaction,
                        cancellationToken: cancellationToken));
                }
                else
                {
                    enrollmentId = existingEnrollment.Id;
                    await connection.ExecuteAsync(new CommandDefinition("""
                        UPDATE Enrollments
                        SET TocFlag = @TocFlag
                        WHERE Id = @EnrollmentId;
                        """,
                        new { TocFlag = tocFlag, EnrollmentId = enrollmentId.Value },
                        transaction,
                        cancellationToken: cancellationToken));
                }

                var checkGb = await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
                    "SELECT IsGreenBelt FROM TrainingPrograms WHERE Id = @ProgramId;",
                    new { ProgramId = programId.Value },
                    transaction,
                    cancellationToken: cancellationToken));

                if (checkGb && enrollmentId.HasValue)
                {
                    await SyncReviewsAsync(connection, transaction, enrollmentId.Value, statusPhase, cancellationToken);
                }
            }

            var greenBeltEnrollments = await connection.QueryAsync<int>(new CommandDefinition("""
                SELECT e.Id
                FROM Enrollments e
                JOIN TrainingPrograms tp ON tp.Id = e.ProgramId
                WHERE e.ParticipantId = @ParticipantId AND tp.IsGreenBelt = 1;
                """,
                new { ParticipantId = participantId },
                transaction,
                cancellationToken: cancellationToken));

            foreach (var gbEnrollmentId in greenBeltEnrollments)
            {
                await SyncReviewsAsync(connection, transaction, gbEnrollmentId, statusPhase, cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
            return enrollmentId;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<IReadOnlyList<ParticipantProgramProgress>> GetParticipantProgramsAsync(int participantId, CancellationToken cancellationToken = default)
    {
        await EnsureParticipantModulesAsync(cancellationToken);

        using var connection = connectionFactory.CreateConnection();
        var enrollments = (await connection.QueryAsync<ParticipantProgramProgress>(new CommandDefinition("""
            SELECT e.Id AS EnrollmentId,
                   e.ParticipantId,
                   e.ProgramId,
                   e.Status,
                   e.TocFlag,
                   e.EnrolledAt,
                   e.CertifiedAt,
                   tp.Name AS ProgramName,
                   tp.ShortName,
                   tp.IsGreenBelt
            FROM Enrollments e
            JOIN TrainingPrograms tp ON tp.Id = e.ProgramId
            WHERE e.ParticipantId = @ParticipantId
            ORDER BY e.EnrolledAt ASC, e.Id ASC;
            """,
            new { ParticipantId = participantId },
            cancellationToken: cancellationToken))).ToList();

        foreach (var enrollment in enrollments)
        {
            var modules = await connection.QueryAsync<ModuleProgress>(new CommandDefinition("""
                SELECT pm.Id,
                       pm.EnrollmentId,
                       pm.ModuleId,
                       m.Name AS ModuleName,
                       m.TargetHours,
                       pm.ActualHours,
                       pm.Status,
                       pm.IsOverridden,
                       pm.OverrideReason
                FROM ParticipantModules pm
                JOIN TrainingModules m ON m.Id = pm.ModuleId
                WHERE pm.EnrollmentId = @EnrollmentId
                ORDER BY m.SortOrder, m.Id;
                """,
                new { EnrollmentId = enrollment.EnrollmentId },
                cancellationToken: cancellationToken));

            enrollment.Modules = modules.ToList();
        }

        return enrollments;
    }

    public async Task SaveModuleCompletionsAsync(int enrollmentId, IEnumerable<int> completedModuleIds, string? actorName = null, CancellationToken cancellationToken = default)
    {
        await using var connection = (SqlConnection)connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(cancellationToken);

        var completedSet = completedModuleIds.ToHashSet();

        var modules = (await connection.QueryAsync<(int ModuleId, decimal TargetHours)>(new CommandDefinition("""
            SELECT pm.ModuleId, m.TargetHours
            FROM ParticipantModules pm
            JOIN TrainingModules m ON m.Id = pm.ModuleId
            WHERE pm.EnrollmentId = @EnrollmentId;
            """,
            new { EnrollmentId = enrollmentId },
            transaction,
            cancellationToken: cancellationToken))).ToList();

        foreach (var mod in modules)
        {
            var isCompleted = completedSet.Contains(mod.ModuleId);
            var status = isCompleted ? "Completed" : "Not Started";
            var hours = isCompleted ? mod.TargetHours : 0m;

            await connection.ExecuteAsync(new CommandDefinition("""
                UPDATE ParticipantModules
                SET Status = @Status,
                    ActualHours = @Hours,
                    IsOverridden = 0,
                    OverrideReason = NULL,
                    UpdatedAt = SYSUTCDATETIME()
                WHERE EnrollmentId = @EnrollmentId AND ModuleId = @ModuleId;
                """,
                new { EnrollmentId = enrollmentId, ModuleId = mod.ModuleId, Status = status, Hours = hours },
                transaction,
                cancellationToken: cancellationToken));
        }

        var currentStatus = await connection.ExecuteScalarAsync<string>(new CommandDefinition(
            "SELECT Status FROM Enrollments WHERE Id = @EnrollmentId;",
            new { EnrollmentId = enrollmentId },
            transaction,
            cancellationToken: cancellationToken));

        var allRequiredCompleted = modules.Count > 0 && modules.All(m => completedSet.Contains(m.ModuleId));

        if (currentStatus != "Certified")
        {
            var nextStatus = allRequiredCompleted ? "Completed" : "In Progress";
            await connection.ExecuteAsync(new CommandDefinition("""
                UPDATE Enrollments
                SET Status = @Status
                WHERE Id = @EnrollmentId;
                """,
                new { EnrollmentId = enrollmentId, Status = nextStatus },
                transaction,
                cancellationToken: cancellationToken));
        }
        else if (!allRequiredCompleted)
        {
            await connection.ExecuteAsync(new CommandDefinition("""
                UPDATE Enrollments
                SET Status = 'In Progress',
                    CertifiedAt = NULL
                WHERE Id = @EnrollmentId;
                """,
                new { EnrollmentId = enrollmentId },
                transaction,
                cancellationToken: cancellationToken));
        }

        await connection.ExecuteAsync(new CommandDefinition("""
            INSERT INTO ActivityLogs (Description, ActorName, Type)
            VALUES (@Description, @ActorName, 'Progress');
            """,
            new { Description = $"Updated module progress for enrollment #{enrollmentId}", ActorName = actorName ?? "System" },
            transaction,
            cancellationToken: cancellationToken));

        await transaction.CommitAsync(cancellationToken);
    }

    public async Task SetCertifiedAsync(int enrollmentId, bool isCertified, string? actorName = null, CancellationToken cancellationToken = default)
    {
        await using var connection = (SqlConnection)connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(cancellationToken);

        if (isCertified)
        {
            await connection.ExecuteAsync(new CommandDefinition("""
                UPDATE Enrollments
                SET Status = 'Certified',
                    CertifiedAt = SYSUTCDATETIME()
                WHERE Id = @EnrollmentId;
                """,
                new { EnrollmentId = enrollmentId },
                transaction,
                cancellationToken: cancellationToken));

            var info = await connection.QuerySingleOrDefaultAsync<(string FullName, string ProgramName)>(new CommandDefinition("""
                SELECT p.FullName, tp.Name
                FROM Enrollments e
                JOIN Participants p ON p.Id = e.ParticipantId
                JOIN TrainingPrograms tp ON tp.Id = e.ProgramId
                WHERE e.Id = @EnrollmentId;
                """,
                new { EnrollmentId = enrollmentId },
                transaction,
                cancellationToken: cancellationToken));

            await connection.ExecuteAsync(new CommandDefinition("""
                INSERT INTO ActivityLogs (Description, ActorName, Type)
                VALUES (@Description, @ActorName, 'Certification');
                """,
                new
                {
                    Description = $"Granted certification in {info.ProgramName} to {info.FullName}",
                    ActorName = actorName ?? "System"
                },
                transaction,
                cancellationToken: cancellationToken));
        }
        else
        {
            await connection.ExecuteAsync(new CommandDefinition("""
                UPDATE Enrollments
                SET Status = 'Completed',
                    CertifiedAt = NULL
                WHERE Id = @EnrollmentId;
                """,
                new { EnrollmentId = enrollmentId },
                transaction,
                cancellationToken: cancellationToken));

            await connection.ExecuteAsync(new CommandDefinition("""
                INSERT INTO ActivityLogs (Description, ActorName, Type)
                VALUES (@Description, @ActorName, 'Certification');
                """,
                new
                {
                    Description = $"Revoked certification for enrollment #{enrollmentId}",
                    ActorName = actorName ?? "System"
                },
                transaction,
                cancellationToken: cancellationToken));
        }

        await transaction.CommitAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ProgramGraduateItem>> GetProgramGraduatesAsync(int programId, CancellationToken cancellationToken = default)
    {
        await EnsureParticipantModulesAsync(cancellationToken);

        using var connection = connectionFactory.CreateConnection();
        var command = new CommandDefinition("""
            SELECT e.Id AS EnrollmentId,
                   e.ParticipantId,
                   p.EmployeeId,
                   p.FullName,
                   p.Department,
                   p.Position,
                   p.StatusPhase,
                   e.Status,
                   e.EnrolledAt,
                   e.CertifiedAt,
                   e.TocFlag,
                   (
                       SELECT COUNT(1)
                       FROM ParticipantModules pm
                       WHERE pm.EnrollmentId = e.Id
                         AND (pm.Status IN ('Completed', 'Overridden') OR pm.ActualHours >= (SELECT tm.TargetHours FROM TrainingModules tm WHERE tm.Id = pm.ModuleId))
                   ) AS CompletedModulesCount,
                   (
                       SELECT COUNT(1)
                       FROM TrainingModules tm
                       WHERE tm.ProgramId = e.ProgramId
                   ) AS TotalModulesCount
            FROM Enrollments e
            JOIN Participants p ON p.Id = e.ParticipantId
            WHERE e.ProgramId = @ProgramId
              AND p.IsActive = 1
            ORDER BY
                CASE WHEN e.Status = 'Certified' THEN 0 WHEN e.Status = 'Completed' THEN 1 ELSE 2 END,
                e.CertifiedAt DESC,
                e.EnrolledAt DESC,
                p.FullName ASC;
            """,
            new { ProgramId = programId },
            cancellationToken: cancellationToken);

        var list = await connection.QueryAsync<ProgramGraduateItem>(command);
        return list.ToList();
    }

    private static async Task SyncReviewsAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        int enrollmentId,
        string? statusPhase,
        CancellationToken cancellationToken)
    {
        await connection.ExecuteAsync(new CommandDefinition("""
            INSERT INTO GreenBeltReviews (EnrollmentId, ReviewNumber)
            SELECT @EnrollmentId, value
            FROM (VALUES (0), (1), (2), (3), (4), (5)) AS v(value)
            WHERE NOT EXISTS (
                SELECT 1
                FROM GreenBeltReviews
                WHERE EnrollmentId = @EnrollmentId AND ReviewNumber = v.value
            );
            """,
            new { EnrollmentId = enrollmentId },
            transaction,
            cancellationToken: cancellationToken));

        var stages = new[] { "Define", "Measure", "Analyze", "Improve", "Control" };
        var targetIndex = string.IsNullOrWhiteSpace(statusPhase) ? -1 : Array.IndexOf(stages, statusPhase.Trim());

        if (targetIndex >= 0)
        {
            await connection.ExecuteAsync(new CommandDefinition("""
                UPDATE GreenBeltReviews
                SET Status = 'Completed',
                    ReviewedAt = SYSUTCDATETIME()
                WHERE EnrollmentId = @EnrollmentId AND ReviewNumber = 0;
                """,
                new { EnrollmentId = enrollmentId },
                transaction,
                cancellationToken: cancellationToken));

            for (var i = 0; i < stages.Length; i++)
            {
                var revNum = i + 1;
                var stageName = stages[i];

                if (i < targetIndex)
                {
                    await connection.ExecuteAsync(new CommandDefinition("""
                        UPDATE GreenBeltReviews
                        SET Status = 'Completed',
                            DmaicStage = @Stage,
                            ReviewedAt = SYSUTCDATETIME()
                        WHERE EnrollmentId = @EnrollmentId AND ReviewNumber = @RevNum;
                        """,
                        new { EnrollmentId = enrollmentId, RevNum = revNum, Stage = stageName },
                        transaction,
                        cancellationToken: cancellationToken));
                }
                else if (i == targetIndex)
                {
                    await connection.ExecuteAsync(new CommandDefinition("""
                        UPDATE GreenBeltReviews
                        SET Status = 'In Progress',
                            DmaicStage = @Stage,
                            ReviewedAt = SYSUTCDATETIME()
                        WHERE EnrollmentId = @EnrollmentId AND ReviewNumber = @RevNum;
                        """,
                        new { EnrollmentId = enrollmentId, RevNum = revNum, Stage = stageName },
                        transaction,
                        cancellationToken: cancellationToken));
                }
                else
                {
                    await connection.ExecuteAsync(new CommandDefinition("""
                        UPDATE GreenBeltReviews
                        SET Status = 'Not Started',
                            DmaicStage = NULL,
                            ReviewedAt = NULL
                        WHERE EnrollmentId = @EnrollmentId AND ReviewNumber = @RevNum;
                        """,
                        new { EnrollmentId = enrollmentId, RevNum = revNum },
                        transaction,
                        cancellationToken: cancellationToken));
                }
            }
        }
    }
}