using Dapper;
using LssTraining.Web.Models;
using Microsoft.Data.SqlClient;

namespace LssTraining.Web.Data;

public interface IParticipantRepository
{
    Task<(IReadOnlyList<Participant> Items, int Total)> SearchAsync(string? search, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<Participant?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Participant?> GetByEmployeeIdAsync(string employeeId, CancellationToken cancellationToken = default);
    Task<int> CreateAsync(Participant participant, string? actorName = null, CancellationToken cancellationToken = default);
    Task UpdateAsync(Participant participant, string? actorName = null, CancellationToken cancellationToken = default);
    Task SetActiveAsync(int id, bool isActive, string? actorName = null, CancellationToken cancellationToken = default);
}

public sealed class ParticipantRepository(IDbConnectionFactory connectionFactory) : IParticipantRepository
{
    public async Task<(IReadOnlyList<Participant> Items, int Total)> SearchAsync(string? search, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        using var connection = connectionFactory.CreateConnection();
        var hasSearch = !string.IsNullOrWhiteSpace(search);
        var filter = hasSearch
            ? "(p.FullName LIKE @Search OR p.EmployeeId LIKE @Search OR ISNULL(p.Department, '') LIKE @Search OR ISNULL(p.Position, '') LIKE @Search OR ISNULL(p.StatusPhase, '') LIKE @Search)"
            : "1 = 1";

        var command = new CommandDefinition($"""
            SELECT COUNT(*) FROM Participants p WHERE {filter};
            SELECT p.*, e.Id AS EnrollmentId, e.ProgramId, tp.Name AS ProgramName, ISNULL(e.TocFlag, 0) AS TocFlag,
                   ISNULL(e.CompletedModulesCount, 0) AS CompletedModulesCount,
                   ISNULL(e.TotalModulesCount, 0) AS TotalModulesCount,
                   e.EnrollmentStatus
            FROM Participants p
            OUTER APPLY (
                SELECT TOP 1 e2.Id, e2.ProgramId, e2.TocFlag, e2.Status AS EnrollmentStatus,
                       (
                           SELECT COUNT(1)
                           FROM ParticipantModules pm
                           WHERE pm.EnrollmentId = e2.Id
                             AND (pm.Status IN ('Completed', 'Overridden') OR pm.ActualHours >= (SELECT tm.TargetHours FROM TrainingModules tm WHERE tm.Id = pm.ModuleId))
                       ) AS CompletedModulesCount,
                       (
                           SELECT COUNT(1)
                           FROM TrainingModules tm
                           WHERE tm.ProgramId = e2.ProgramId
                       ) AS TotalModulesCount
                FROM Enrollments e2
                WHERE e2.ParticipantId = p.Id
                ORDER BY e2.EnrolledAt DESC
            ) e
            LEFT JOIN TrainingPrograms tp ON tp.Id = e.ProgramId
            WHERE {filter}
            ORDER BY p.FullName, p.Id OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
            """,
            new
            {
                Search = hasSearch ? $"%{search!.Trim()}%" : null,
                Offset = Math.Max(0, page - 1) * pageSize,
                PageSize = pageSize
            },
            cancellationToken: cancellationToken);

        using var multi = await connection.QueryMultipleAsync(command);
        var total = await multi.ReadSingleAsync<int>();
        var items = (await multi.ReadAsync<Participant>()).ToList();
        return (items, total);
    }

    public async Task<Participant?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        using var connection = connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<Participant>(new CommandDefinition("""
            SELECT p.*, e.Id AS EnrollmentId, e.ProgramId, tp.Name AS ProgramName, ISNULL(e.TocFlag, 0) AS TocFlag,
                   ISNULL(e.CompletedModulesCount, 0) AS CompletedModulesCount,
                   ISNULL(e.TotalModulesCount, 0) AS TotalModulesCount,
                   e.EnrollmentStatus
            FROM Participants p
            OUTER APPLY (
                SELECT TOP 1 e2.Id, e2.ProgramId, e2.TocFlag, e2.Status AS EnrollmentStatus,
                       (
                           SELECT COUNT(1)
                           FROM ParticipantModules pm
                           WHERE pm.EnrollmentId = e2.Id
                             AND (pm.Status IN ('Completed', 'Overridden') OR pm.ActualHours >= (SELECT tm.TargetHours FROM TrainingModules tm WHERE tm.Id = pm.ModuleId))
                       ) AS CompletedModulesCount,
                       (
                           SELECT COUNT(1)
                           FROM TrainingModules tm
                           WHERE tm.ProgramId = e2.ProgramId
                       ) AS TotalModulesCount
                FROM Enrollments e2
                WHERE e2.ParticipantId = p.Id
                ORDER BY e2.EnrolledAt DESC
            ) e
            LEFT JOIN TrainingPrograms tp ON tp.Id = e.ProgramId
            WHERE p.Id = @Id;
            """,
            new { Id = id },
            cancellationToken: cancellationToken));
    }

    public async Task<Participant?> GetByEmployeeIdAsync(string employeeId, CancellationToken cancellationToken = default)
    {
        using var connection = connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<Participant>(new CommandDefinition("""
            SELECT p.*, e.Id AS EnrollmentId, e.ProgramId, tp.Name AS ProgramName, ISNULL(e.TocFlag, 0) AS TocFlag,
                   ISNULL(e.CompletedModulesCount, 0) AS CompletedModulesCount,
                   ISNULL(e.TotalModulesCount, 0) AS TotalModulesCount,
                   e.EnrollmentStatus
            FROM Participants p
            OUTER APPLY (
                SELECT TOP 1 e2.Id, e2.ProgramId, e2.TocFlag, e2.Status AS EnrollmentStatus,
                       (
                           SELECT COUNT(1)
                           FROM ParticipantModules pm
                           WHERE pm.EnrollmentId = e2.Id
                             AND (pm.Status IN ('Completed', 'Overridden') OR pm.ActualHours >= (SELECT tm.TargetHours FROM TrainingModules tm WHERE tm.Id = pm.ModuleId))
                       ) AS CompletedModulesCount,
                       (
                           SELECT COUNT(1)
                           FROM TrainingModules tm
                           WHERE tm.ProgramId = e2.ProgramId
                       ) AS TotalModulesCount
                FROM Enrollments e2
                WHERE e2.ParticipantId = p.Id
                ORDER BY e2.EnrolledAt DESC
            ) e
            LEFT JOIN TrainingPrograms tp ON tp.Id = e.ProgramId
            WHERE p.EmployeeId = @EmployeeId;
            """,
            new { EmployeeId = employeeId.Trim() },
            cancellationToken: cancellationToken));
    }

    public async Task<int> CreateAsync(Participant participant, string? actorName = null, CancellationToken cancellationToken = default)
    {
        await using var connection = (SqlConnection)connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            // Kolom Email dan @Email telah dihilangkan di sini
            var id = await connection.ExecuteScalarAsync<int>(new CommandDefinition("""
                INSERT INTO Participants (EmployeeId, FullName, Department, Position, StatusPhase, IsActive)
                OUTPUT INSERTED.Id
                VALUES (@EmployeeId, @FullName, @Department, @Position, @StatusPhase, @IsActive);
                """,
                participant,
                transaction,
                cancellationToken: cancellationToken));

            await AddActivityAsync(connection, transaction, $"Added participant {participant.FullName}", actorName, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return id;
        }
        catch (SqlException exception) when (exception.Number is 2601 or 2627)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw new InvalidOperationException("Employee ID is already in use by another participant.", exception);
        }
    }

    public async Task UpdateAsync(Participant participant, string? actorName = null, CancellationToken cancellationToken = default)
    {
        await using var connection = (SqlConnection)connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            // Kolom Email = @Email telah dihilangkan di sini
            await connection.ExecuteAsync(new CommandDefinition("""
                UPDATE Participants
                SET EmployeeId = @EmployeeId,
                    FullName = @FullName,
                    Department = @Department,
                    Position = @Position,
                    StatusPhase = @StatusPhase,
                    IsActive = @IsActive,
                    UpdatedAt = SYSUTCDATETIME()
                WHERE Id = @Id;
                """,
                participant,
                transaction,
                cancellationToken: cancellationToken));

            await AddActivityAsync(connection, transaction, $"Updated participant {participant.FullName}", actorName, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (SqlException exception) when (exception.Number is 2601 or 2627)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw new InvalidOperationException("Employee ID is already in use by another participant.", exception);
        }
    }

    public async Task SetActiveAsync(int id, bool isActive, string? actorName = null, CancellationToken cancellationToken = default)
    {
        await using var connection = (SqlConnection)connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(cancellationToken);

        await connection.ExecuteAsync(new CommandDefinition("""
            UPDATE Participants
            SET IsActive = @IsActive,
                UpdatedAt = SYSUTCDATETIME()
            WHERE Id = @Id;
            """,
            new { Id = id, IsActive = isActive },
            transaction,
            cancellationToken: cancellationToken));

        var actionText = isActive ? "Activated" : "Deactivated";
        await AddActivityAsync(connection, transaction, $"{actionText} participant", actorName, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private static Task AddActivityAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        string description,
        string? actorName,
        CancellationToken cancellationToken)
    {
        return connection.ExecuteAsync(new CommandDefinition("""
            INSERT INTO ActivityLogs (Description, ActorName, Type)
            VALUES (@Description, @ActorName, 'Participant');
            """,
            new { Description = description, ActorName = actorName ?? "System" },
            transaction,
            cancellationToken: cancellationToken));
    }
}