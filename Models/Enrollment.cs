namespace LssTraining.Web.Models;

public enum ModuleStatus
{
    NotStarted,
    InProgress,
    Completed,
    Overridden
}

public sealed class Enrollment
{
    public int Id { get; set; }
    public int ParticipantId { get; set; }
    public int ProgramId { get; set; }
    public DateTime EnrolledAt { get; set; }
    public string Status { get; set; } = "In Progress";
    public bool TocFlag { get; set; }
    public DateTime? CertifiedAt { get; set; }
    public string ParticipantName { get; set; } = string.Empty;
    public string EmployeeId { get; set; } = string.Empty;
    public string ProgramName { get; set; } = string.Empty;
    public int CompletedModulesCount { get; set; }
    public int TotalModulesCount { get; set; }

    public int RemainingModulesCount => Math.Max(0, TotalModulesCount - CompletedModulesCount);
    public bool IsCertified => Status == "Certified" || CertifiedAt.HasValue;
    public bool IsCompletedTraining => (TotalModulesCount > 0 && CompletedModulesCount >= TotalModulesCount) || Status == "Completed";
    public bool IsInTraining => !IsCertified && !IsCompletedTraining;

    public string ModuleProgressSummary => TotalModulesCount == 0
        ? "0 Modules"
        : CompletedModulesCount == 0
            ? $"Module 0/{TotalModulesCount} ({RemainingModulesCount} module(s) remaining)"
            : IsCompletedTraining
                ? $"Complete ({CompletedModulesCount}/{TotalModulesCount} Modules · 0 remaining)"
                : $"Module {CompletedModulesCount} of {TotalModulesCount} ({RemainingModulesCount} module(s) remaining)";
}

public sealed class ModuleProgress
{
    public int Id { get; set; }
    public int EnrollmentId { get; set; }
    public int ModuleId { get; set; }
    public string ModuleName { get; set; } = string.Empty;
    public decimal TargetHours { get; set; }
    public decimal ActualHours { get; set; }
    public string Status { get; set; } = "Not Started";
    public bool IsOverridden { get; set; }
    public string? OverrideReason { get; set; }

    public bool IsCompleted => Status == "Completed" || Status == "Overridden" || ActualHours >= TargetHours;
}

public sealed class ParticipantProgramProgress
{
    public int EnrollmentId { get; set; }
    public int ParticipantId { get; set; }
    public int ProgramId { get; set; }
    public string ProgramName { get; set; } = string.Empty;
    public string ShortName { get; set; } = string.Empty;
    public bool IsGreenBelt { get; set; }
    public string Status { get; set; } = "In Progress";
    public bool TocFlag { get; set; }
    public DateTime EnrolledAt { get; set; }
    public DateTime? CertifiedAt { get; set; }
    public List<ModuleProgress> Modules { get; set; } = [];

    public int TotalModulesCount => Modules.Count;
    public List<ModuleProgress> CompletedModules => Modules.Where(m => m.IsCompleted).ToList();
    public List<ModuleProgress> RemainingModules => Modules.Where(m => !m.IsCompleted).ToList();
    public int CompletedModulesCount => CompletedModules.Count;
    public int RemainingModulesCount => Math.Max(0, TotalModulesCount - CompletedModulesCount);
    public bool IsAllModulesCompleted => TotalModulesCount > 0 && CompletedModulesCount == TotalModulesCount;
    public bool IsCertified => Status == "Certified" || CertifiedAt.HasValue;

    public string ModuleProgressSummary => TotalModulesCount == 0
        ? "0 Modules"
        : CompletedModulesCount == 0
            ? $"Module 0/{TotalModulesCount} ({RemainingModulesCount} module(s) remaining)"
            : IsAllModulesCompleted
                ? $"Complete ({CompletedModulesCount}/{TotalModulesCount} Modules · 0 remaining)"
                : $"Module {CompletedModulesCount} of {TotalModulesCount} ({RemainingModulesCount} module(s) remaining)";

    public bool CanCertify(string? statusPhase)
    {
        var isGreenOrBlackBelt = IsGreenBelt || ShortName.Contains("BB", StringComparison.OrdinalIgnoreCase) || ProgramName.Contains("Black Belt", StringComparison.OrdinalIgnoreCase);
        var isYellowBelt = ShortName.Contains("Yellow", StringComparison.OrdinalIgnoreCase) || ProgramName.Contains("Yellow", StringComparison.OrdinalIgnoreCase);

        if (isGreenOrBlackBelt)
        {
            return IsAllModulesCompleted && string.Equals(statusPhase?.Trim(), "Control", StringComparison.OrdinalIgnoreCase);
        }

        if (isYellowBelt)
        {
            return IsAllModulesCompleted;
        }

        return false;
    }
}

public sealed class ProgramGraduateItem
{
    public int EnrollmentId { get; set; }
    public int ParticipantId { get; set; }
    public string EmployeeId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Department { get; set; }
    public string? Position { get; set; }
    public string? StatusPhase { get; set; }
    public string Status { get; set; } = "In Progress";
    public DateTime EnrolledAt { get; set; }
    public DateTime? CertifiedAt { get; set; }
    public int CompletedModulesCount { get; set; }
    public int TotalModulesCount { get; set; }
    public bool TocFlag { get; set; }

    public int RemainingModulesCount => Math.Max(0, TotalModulesCount - CompletedModulesCount);
    public bool IsCertified => Status == "Certified" || CertifiedAt.HasValue;
    public bool IsCompletedTraining => (TotalModulesCount > 0 && CompletedModulesCount >= TotalModulesCount) || Status == "Completed";
    public bool IsInTraining => !IsCertified && !IsCompletedTraining;

    public string ModuleProgressSummary => TotalModulesCount == 0
        ? "0 Modules"
        : CompletedModulesCount == 0
            ? $"Module 0/{TotalModulesCount} ({RemainingModulesCount} module(s) remaining)"
            : IsCompletedTraining
                ? $"Complete ({CompletedModulesCount}/{TotalModulesCount} Modules · 0 remaining)"
                : $"Module {CompletedModulesCount} of {TotalModulesCount} ({RemainingModulesCount} module(s) remaining)";
}
