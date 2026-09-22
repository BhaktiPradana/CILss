using Dapper;
using DocumentFormat.OpenXml.Math;
using LssTraining.Web.Models;

namespace LssTraining.Web.Data;

public interface IAuthRepository
{
    Task<UserAccount?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<bool> CreateUserAsync(string username, string displayName, string rawPassword, CancellationToken cancellationToken = default);
}

public sealed class AuthRepository(IDbConnectionFactory connectionFactory) : IAuthRepository
{
    public async Task<UserAccount?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        using var connection = connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<UserAccount>(new CommandDefinition(
            "SELECT * FROM Users WHERE Username = @Username AND IsActive = 1;",
            new { Username = username.Trim() }, cancellationToken: cancellationToken));
    }
    public async Task<bool> CreateUserAsync(string username, string displayName, string rawPassword, CancellationToken cancellationToken = default)
    {
        using var connection = connectionFactory.CreateConnection();
        var exists = await connection.ExecuteScalarAsync<bool>(
            new CommandDefinition("SELECT CASE WHEN EXISTS (SELECT 1 FROM Users WHERE Username = @Username) THEN 1 ELSE 0 END",
            new { Username = username.Trim() }, cancellationToken: cancellationToken));

        if (exists) return false;
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(rawPassword);

        var query = @"
            INSERT INTO Users (Username, DisplayName, PasswordHash, Role, IsActive, CreatedAt)
            VALUES (@Username, @DisplayName, @PasswordHash, 'Trainer', 0, GETDATE())"; 

        var result = await connection.ExecuteAsync(new CommandDefinition(query,
            new
            {
                Username = username.Trim(),
                DisplayName = displayName.Trim(),
                PasswordHash = passwordHash
            },
            cancellationToken: cancellationToken));

        return result > 0;
    }
}