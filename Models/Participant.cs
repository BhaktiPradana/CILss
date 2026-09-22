namespace LssTraining.Web.Models;

public enum DmaicStage
{
    Define,
    Measure,
    Analyze,
    Improve,
    Control
}

public sealed class Participant
{
    public int Id { get; set; }
    public string EmployeeId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Department { get; set; }
    public string? Position { get; set; }
    public string? StatusPhase { get; set; }
    public bool IsActive { get; set; } = true;
    public int? ProgramId { get; set; }
    public string? ProgramName { get; set; }
    public int? EnrollmentId { get; set; }
    public bool TocFlag { get; set; }
    public int CompletedModulesCount { get; set; }
    public int TotalModulesCount { get; set; }
    public string? EnrollmentStatus { get; set; }
    public int RemainingModulesCount => Math.Max(0, TotalModulesCount - CompletedModulesCount);

    public int StageIndex => StatusPhase switch
    {
        "Define" => 0,
        "Measure" => 1,
        "Analyze" => 2,
        "Improve" => 3,
        "Control" => 4,
        _ => -1
    };
}
