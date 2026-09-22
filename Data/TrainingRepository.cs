using Dapper;
using LssTraining.Web.Models;

namespace LssTraining.Web.Data;

public interface ITrainingRepository
{
    Task<IReadOnlyList<TrainingProgram>> GetProgramsAsync(CancellationToken cancellationToken = default);
    Task<TrainingProgram?> GetProgramByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TrainingModule>> GetModulesAsync(int programId, CancellationToken cancellationToken = default);
    Task<TrainingModule?> GetModuleByIdAsync(int moduleId, CancellationToken cancellationToken = default);
    Task<int> CreateModuleAsync(TrainingModule module, CancellationToken cancellationToken = default);
    Task UpdateModuleAsync(TrainingModule module, CancellationToken cancellationToken = default);
    Task DeleteModuleAsync(int moduleId, CancellationToken cancellationToken = default);
    Task<bool> ModuleHasProgressAsync(int moduleId, CancellationToken cancellationToken = default);
}

public sealed class TrainingRepository(IDbConnectionFactory connectionFactory) : ITrainingRepository
{
    public async Task<IReadOnlyList<TrainingProgram>> GetProgramsAsync(CancellationToken cancellationToken = default)
    {
        using var connection = connectionFactory.CreateConnection();
        var programs = await connection.QueryAsync<TrainingProgram>(new CommandDefinition(
            "SELECT * FROM TrainingPrograms WHERE IsActive = 1 ORDER BY SortOrder;",
            cancellationToken: cancellationToken));
        return programs.ToList();
    }

    public async Task<TrainingProgram?> GetProgramByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        using var connection = connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<TrainingProgram>(new CommandDefinition(
            "SELECT * FROM TrainingPrograms WHERE Id = @Id;",
            new { Id = id },
            cancellationToken: cancellationToken));
    }

    public async Task<IReadOnlyList<TrainingModule>> GetModulesAsync(int programId, CancellationToken cancellationToken = default)
    {
        using var connection = connectionFactory.CreateConnection();
        var modules = await connection.QueryAsync<TrainingModule>(new CommandDefinition(
            "SELECT * FROM TrainingModules WHERE ProgramId = @ProgramId ORDER BY SortOrder, Id;",
            new { ProgramId = programId },
            cancellationToken: cancellationToken));
        return modules.ToList();
    }

    public async Task<TrainingModule?> GetModuleByIdAsync(int moduleId, CancellationToken cancellationToken = default)
    {
        using var connection = connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<TrainingModule>(new CommandDefinition(
            "SELECT * FROM TrainingModules WHERE Id = @Id;",
            new { Id = moduleId },
            cancellationToken: cancellationToken));
    }

    public async Task<int> CreateModuleAsync(TrainingModule module, CancellationToken cancellationToken = default)
    {
        using var connection = connectionFactory.CreateConnection();
        return await connection.ExecuteScalarAsync<int>(new CommandDefinition("""
            INSERT INTO TrainingModules (ProgramId, Name, Description, TargetHours, SortOrder, IsRequired)
            OUTPUT INSERTED.Id
            VALUES (@ProgramId, @Name, @Description, @TargetHours, @SortOrder, @IsRequired);
            """,
            module,
            cancellationToken: cancellationToken));
    }

    public async Task UpdateModuleAsync(TrainingModule module, CancellationToken cancellationToken = default)
    {
        using var connection = connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition("""
            UPDATE TrainingModules
            SET Name = @Name,
                Description = @Description,
                TargetHours = @TargetHours,
                SortOrder = @SortOrder,
                IsRequired = @IsRequired
            WHERE Id = @Id;
            """,
            module,
            cancellationToken: cancellationToken));
    }

    public async Task DeleteModuleAsync(int moduleId, CancellationToken cancellationToken = default)
    {
        using var connection = connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(
            "DELETE FROM TrainingModules WHERE Id = @Id;",
            new { Id = moduleId },
            cancellationToken: cancellationToken));
    }

    public async Task<bool> ModuleHasProgressAsync(int moduleId, CancellationToken cancellationToken = default)
    {
        using var connection = connectionFactory.CreateConnection();
        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            "SELECT COUNT(1) FROM ParticipantModules WHERE ModuleId = @Id AND (ActualHours > 0 OR Status <> 'Not Started');",
            new { Id = moduleId },
            cancellationToken: cancellationToken));
    }
}
