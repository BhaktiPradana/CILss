using Dapper;
using LssTraining.Web.Models;
using Microsoft.Data.SqlClient;

namespace LssTraining.Web.Data;

public interface IReviewRepository
{
    Task<(Enrollment Enrollment, IReadOnlyList<GreenBeltReview> Reviews)?> GetRoadmapAsync(int enrollmentId, CancellationToken cancellationToken = default);
    Task UpdateAsync(GreenBeltReview review, string actorName, CancellationToken cancellationToken = default);
}

public sealed class ReviewRepository(IDbConnectionFactory connectionFactory) : IReviewRepository
{
    public async Task<(Enrollment Enrollment, IReadOnlyList<GreenBeltReview> Reviews)?> GetRoadmapAsync(int enrollmentId, CancellationToken cancellationToken = default)
    {
        using var connection = connectionFactory.CreateConnection();
        var command = new CommandDefinition("""
            SELECT e.*, p.FullName AS ParticipantName, p.EmployeeId, tp.Name AS ProgramName
            FROM Enrollments e JOIN Participants p ON p.Id = e.ParticipantId
            JOIN TrainingPrograms tp ON tp.Id = e.ProgramId WHERE e.Id = @Id;
            SELECT * FROM GreenBeltReviews WHERE EnrollmentId = @Id ORDER BY ReviewNumber;
            """, new { Id = enrollmentId }, cancellationToken: cancellationToken);
        using var multi = await connection.QueryMultipleAsync(command);
        var enrollment = await multi.ReadSingleOrDefaultAsync<Enrollment>();
        if (enrollment is null) return null;
        var reviews = (await multi.ReadAsync<GreenBeltReview>()).ToList();
        return (enrollment, reviews);
    }

    public async Task UpdateAsync(GreenBeltReview review, string actorName, CancellationToken cancellationToken = default)
    {
        await using var connection = (SqlConnection)connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(cancellationToken);
        var command = new CommandDefinition("""
            UPDATE GreenBeltReviews SET Status = @Status, DmaicStage = @DmaicStage,
                   TrainerComment = @TrainerComment, ReviewedAt = CASE WHEN @Status = 'Not Started' THEN NULL ELSE SYSUTCDATETIME() END
            WHERE Id = @Id;
            INSERT INTO ActivityLogs (Description, ActorName, Type) VALUES (@Description, @ActorName, 'Review');
            """, new { review.Id, review.Status, review.DmaicStage, review.TrainerComment,
            Description = $"Updated {review.ReviewLabel} Green Belt", ActorName = actorName }, transaction: transaction, cancellationToken: cancellationToken);
        await connection.ExecuteAsync(command);
        await transaction.CommitAsync(cancellationToken);
    }
}
