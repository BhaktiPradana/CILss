using Dapper;
using LssTraining.Web.Models;

namespace LssTraining.Web.Data;

public interface IDashboardRepository
{
    Task<DashboardMetrics> GetMetricsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProgramSummary>> GetProgramSummariesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ActivityItem>> GetRecentActivityAsync(int take = 8, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ParticipantRoadmap>> GetParticipantRoadmapAsync(string? search = null, CancellationToken cancellationToken = default);
}

public sealed class DashboardRepository(IDbConnectionFactory connectionFactory) : IDashboardRepository
{
    public async Task<DashboardMetrics> GetMetricsAsync(CancellationToken cancellationToken = default)
    {
        using var connection = connectionFactory.CreateConnection();
        var command = new CommandDefinition("""
            SELECT
                (SELECT COUNT(*) FROM Participants WHERE IsActive = 1) AS TotalParticipants,
                (SELECT COUNT(*) FROM Enrollments WHERE Status <> 'Completed') AS ActiveEnrollments,
                (SELECT COUNT(*) FROM Enrollments WHERE Status = 'Completed') AS CompletedEnrollments,
                (SELECT COUNT(*) FROM GreenBeltReviews WHERE Status = 'Completed') AS GreenBeltReviewsComplete,
                (SELECT COUNT(*) FROM GreenBeltReviews) AS GreenBeltReviewsTotal;
            """, cancellationToken: cancellationToken);

        var row = await connection.QuerySingleAsync<DashboardRow>(command);
        var total = row.ActiveEnrollments + row.CompletedEnrollments;
        var completionRate = total == 0 ? 0 : Math.Round(row.CompletedEnrollments * 100m / total, 1);

        return new DashboardMetrics(
            row.TotalParticipants,
            row.ActiveEnrollments,
            row.CompletedEnrollments,
            completionRate,
            row.GreenBeltReviewsComplete,
            row.GreenBeltReviewsTotal);
    }

    public async Task<IReadOnlyList<ProgramSummary>> GetProgramSummariesAsync(CancellationToken cancellationToken = default)
    {
        using var connection = connectionFactory.CreateConnection();
        var command = new CommandDefinition("""
            SELECT p.Id,
                   p.Name,
                   p.ShortName,
                   p.Description,
                   COUNT(e.Id) AS ParticipantCount,
                   SUM(CASE WHEN e.Status = 'Completed' THEN 1 ELSE 0 END) AS CompletedCount,
                   SUM(CASE WHEN e.Status <> 'Completed' THEN 1 ELSE 0 END) AS InProgressCount
            FROM TrainingPrograms p
            LEFT JOIN Enrollments e ON e.ProgramId = p.Id
            WHERE p.IsActive = 1
            GROUP BY p.Id, p.Name, p.ShortName, p.Description, p.SortOrder
            ORDER BY p.SortOrder;
            """, cancellationToken: cancellationToken);

        var rows = await connection.QueryAsync<ProgramRow>(command);
        return rows.Select(row =>
        {
            var rate = row.ParticipantCount == 0 ? 0 : Math.Round(row.CompletedCount * 100m / row.ParticipantCount, 1);
            var accent = row.ShortName switch
            {
                "White Belt" => "blue",
                "Yellow Belt" => "amber",
                "Green Belt" => "teal",
                _ => "navy"
            };

            return new ProgramSummary(
                row.Id,
                row.Name,
                row.ShortName,
                row.Description,
                row.ParticipantCount,
                row.CompletedCount,
                row.InProgressCount,
                rate,
                accent);
        }).ToList();
    }

    public async Task<IReadOnlyList<ParticipantRoadmap>> GetParticipantRoadmapAsync(string? search = null, CancellationToken cancellationToken = default)
    {
        using var connection = connectionFactory.CreateConnection();
        var command = new CommandDefinition("""
            INSERT INTO ParticipantModules (EnrollmentId, ModuleId, ActualHours, Status, IsOverridden, UpdatedAt)
            SELECT e.Id, tm.Id, 0, 'Not Started', 0, SYSUTCDATETIME()
            FROM Enrollments e
            JOIN TrainingModules tm ON tm.ProgramId = e.ProgramId
            WHERE NOT EXISTS (
                SELECT 1
                FROM ParticipantModules pm
                WHERE pm.EnrollmentId = e.Id AND pm.ModuleId = tm.Id
            );

            WITH LatestReview AS (
                SELECT e.ParticipantId,
                       e.Id AS GreenBeltEnrollmentId,
                       r.ReviewNumber,
                       r.DmaicStage,
                       ROW_NUMBER() OVER (
                           PARTITION BY e.ParticipantId
                           ORDER BY CASE WHEN r.Status = 'In Progress' THEN 1 WHEN r.Status = 'Completed' THEN 2 ELSE 3 END,
                                    r.ReviewNumber DESC
                       ) AS rn
                FROM Enrollments e
                JOIN TrainingPrograms tp ON tp.Id = e.ProgramId
                LEFT JOIN GreenBeltReviews r ON r.EnrollmentId = e.Id
                WHERE tp.IsGreenBelt = 1
            ),
            LatestEnrollment AS (
                SELECT e.ParticipantId,
                       e.Id AS EnrollmentId,
                       e.Status AS EnrollmentStatus,
                       e.CertifiedAt,
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
                       ) AS TotalModulesCount,
                       ROW_NUMBER() OVER (PARTITION BY e.ParticipantId ORDER BY e.EnrolledAt DESC) AS rn
                FROM Enrollments e
                JOIN TrainingPrograms tp ON tp.Id = e.ProgramId
            )
            SELECT p.Id AS ParticipantId,
                   p.EmployeeId,
                   p.FullName,
                   p.Department,
                   COALESCE(le.ProgramName, 'Not enrolled') AS ProgramName,
                   COALESCE(p.StatusPhase, lr.DmaicStage) AS DmaicStage,
                   COALESCE(lr.ReviewNumber, 0) AS ReviewNumber,
                   CASE COALESCE(p.StatusPhase, lr.DmaicStage)
                       WHEN 'Define' THEN 0
                       WHEN 'Measure' THEN 1
                       WHEN 'Analyze' THEN 2
                       WHEN 'Improve' THEN 3
                       WHEN 'Control' THEN 4
                       ELSE -1
                   END AS StageIndex,
                   lr.GreenBeltEnrollmentId,
                   COALESCE(le.CompletedModulesCount, 0) AS CompletedModulesCount,
                   COALESCE(le.TotalModulesCount, 0) AS TotalModulesCount,
                   COALESCE(le.EnrollmentStatus, 'In Progress') AS EnrollmentStatus,
                   CASE WHEN le.EnrollmentStatus = 'Certified' OR le.CertifiedAt IS NOT NULL THEN 1 ELSE 0 END AS IsCertified
            FROM Participants p
            OUTER APPLY (SELECT TOP 1 * FROM LatestReview x WHERE x.ParticipantId = p.Id AND x.rn = 1) lr
            OUTER APPLY (SELECT TOP 1 * FROM LatestEnrollment y WHERE y.ParticipantId = p.Id AND y.rn = 1) le
            WHERE p.IsActive = 1
              AND (@Search IS NULL
                   OR p.FullName LIKE @Search
                   OR p.EmployeeId LIKE @Search
                   OR p.Department LIKE @Search
                   OR ISNULL(p.StatusPhase, '') LIKE @Search)
            ORDER BY p.FullName;

            SELECT r.*
            FROM GreenBeltReviews r
            JOIN Enrollments e ON e.Id = r.EnrollmentId
            JOIN Participants p ON p.Id = e.ParticipantId
            WHERE p.IsActive = 1
            ORDER BY r.EnrollmentId, r.ReviewNumber;
            """,
            new { Search = string.IsNullOrWhiteSpace(search) ? null : $"%{search.Trim()}%" },
            cancellationToken: cancellationToken);

        using var multi = await connection.QueryMultipleAsync(command);
        var roadmaps = (await multi.ReadAsync<ParticipantRoadmapRow>()).ToList();
        var reviewsList = (await multi.ReadAsync<GreenBeltReview>()).ToList();
        var reviewsByEnrollment = reviewsList
            .GroupBy(r => r.EnrollmentId)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<GreenBeltReview>)g.ToList());

        return roadmaps.Select(r => new ParticipantRoadmap(
            r.ParticipantId,
            r.EmployeeId,
            r.FullName,
            r.Department,
            r.ProgramName,
            r.DmaicStage,
            r.ReviewNumber,
            r.StageIndex,
            r.GreenBeltEnrollmentId,
            r.GreenBeltEnrollmentId.HasValue && reviewsByEnrollment.TryGetValue(r.GreenBeltEnrollmentId.Value, out var revs) ? revs : [],
            r.CompletedModulesCount,
            r.TotalModulesCount,
            r.EnrollmentStatus,
            r.IsCertified
        )).ToList();
    }

    public async Task<IReadOnlyList<ActivityItem>> GetRecentActivityAsync(int take = 8, CancellationToken cancellationToken = default)
    {
        using var connection = connectionFactory.CreateConnection();
        var command = new CommandDefinition("""
            SELECT TOP (@Take) Description, ActorName, CreatedAt, Type
            FROM ActivityLogs
            ORDER BY CreatedAt DESC;
            """,
            new { Take = take },
            cancellationToken: cancellationToken);

        return (await connection.QueryAsync<ActivityItem>(command)).ToList();
    }

    private sealed class DashboardRow
    {
        public int TotalParticipants { get; init; }
        public int ActiveEnrollments { get; init; }
        public int CompletedEnrollments { get; init; }
        public int GreenBeltReviewsComplete { get; init; }
        public int GreenBeltReviewsTotal { get; init; }
    }

    private sealed class ProgramRow
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string ShortName { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public int ParticipantCount { get; init; }
        public int CompletedCount { get; init; }
        public int InProgressCount { get; init; }
    }

    private sealed class ParticipantRoadmapRow
    {
        public int ParticipantId { get; init; }
        public string EmployeeId { get; init; } = string.Empty;
        public string FullName { get; init; } = string.Empty;
        public string? Department { get; init; }
        public string ProgramName { get; init; } = string.Empty;
        public string? DmaicStage { get; init; }
        public int ReviewNumber { get; init; }
        public int StageIndex { get; init; }
        public int? GreenBeltEnrollmentId { get; init; }
        public int CompletedModulesCount { get; init; }
        public int TotalModulesCount { get; init; }
        public string EnrollmentStatus { get; init; } = "In Progress";
        public bool IsCertified { get; init; }
    }
}
