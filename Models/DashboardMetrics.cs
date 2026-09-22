namespace LssTraining.Web.Models;

public sealed record DashboardMetrics(
    int TotalParticipants,
    int ActiveEnrollments,
    int CompletedEnrollments,
    decimal OverallCompletionRate,
    int GreenBeltReviewsComplete,
    int GreenBeltReviewsTotal);

public sealed record ProgramSummary(
    int Id,
    string Name,
    string ShortName,
    string Description,
    int ParticipantCount,
    int CompletedCount,
    int InProgressCount,
    decimal CompletionRate,
    string AccentClass);

public sealed record ActivityItem(
    string Description,
    string? ActorName,
    DateTime CreatedAt,
    string Type);

public sealed record ParticipantRoadmap(
    int ParticipantId,
    string EmployeeId,
    string FullName,
    string? Department,
    string ProgramName,
    string? DmaicStage,
    int ReviewNumber,
    int StageIndex,
    int? GreenBeltEnrollmentId,
    IReadOnlyList<GreenBeltReview>? Reviews = null,
    int CompletedModulesCount = 0,
    int TotalModulesCount = 0,
    string EnrollmentStatus = "In Progress",
    bool IsCertified = false)
{
    public int RemainingModulesCount => Math.Max(0, TotalModulesCount - CompletedModulesCount);
    public bool IsCompletedTraining => (TotalModulesCount > 0 && CompletedModulesCount >= TotalModulesCount) || EnrollmentStatus == "Completed";
    public bool IsInTraining => !IsCertified && !IsCompletedTraining;

    public string ModuleProgressSummary => TotalModulesCount == 0
        ? "0 Modul"
        : CompletedModulesCount == 0
            ? $"Modul 0/{TotalModulesCount} (Kurang {RemainingModulesCount} modul)"
            : IsCompletedTraining
                ? $"Lengkap ({CompletedModulesCount}/{TotalModulesCount} Modul · Kurang 0 modul)"
                : $"Sampai Modul {CompletedModulesCount} dari {TotalModulesCount} (Kurang {RemainingModulesCount} modul)";
}
