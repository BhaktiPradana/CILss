using LssTraining.Web.Data;
using LssTraining.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LssTraining.Web.Pages.Training;

public class ProgressModel(IEnrollmentRepository enrollmentRepository, ILogger<ProgressModel> logger) : PageModel
{
    public Enrollment? Enrollment { get; private set; }

    public IReadOnlyList<ModuleProgress> Progress { get; private set; } = [];

    public async Task OnGetAsync(int enrollmentId, CancellationToken cancellationToken)
    {
        try
        {
            var enrollments = await enrollmentRepository.GetAllAsync(cancellationToken);
            Enrollment = enrollments.FirstOrDefault(x => x.Id == enrollmentId);
            if (Enrollment is not null)
            {
                Progress = await enrollmentRepository.GetProgressAsync(enrollmentId, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to load progress from database; using preview fallback.");
            LoadPreviewData(enrollmentId);
        }
    }

    public async Task<IActionResult> OnPostUpdateHoursAsync(int enrollmentId, int progressId, decimal actualHours, CancellationToken cancellationToken)
    {
        if (!User.IsInRole("Admin") && !User.IsInRole("Trainer"))
        {
            return Forbid();
        }

        if (actualHours < 0)
        {
            TempData["Success"] = null;
            ModelState.AddModelError(string.Empty, "Hours cannot be negative.");
            return await ReloadAsync(enrollmentId, cancellationToken);
        }

        try
        {
            await enrollmentRepository.UpdateHoursAsync(progressId, actualHours, User.Identity?.Name, cancellationToken);
            TempData["Success"] = "Module hours updated.";
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to update module hours.");
            ModelState.AddModelError(string.Empty, "Failed to update hours.");
        }

        return await ReloadAsync(enrollmentId, cancellationToken);
    }

    public async Task<IActionResult> OnPostOverrideAsync(int enrollmentId, int progressId, string status, string reason, CancellationToken cancellationToken)
    {
        if (!User.IsInRole("Admin") && !User.IsInRole("Trainer"))
        {
            return Forbid();
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            ModelState.AddModelError(string.Empty, "Override reason is required.");
            return await ReloadAsync(enrollmentId, cancellationToken);
        }

        await enrollmentRepository.OverrideAsync(progressId, status, reason.Trim(), User.Identity?.Name, cancellationToken);
        TempData["Success"] = "Module status overridden.";
        return await ReloadAsync(enrollmentId, cancellationToken);
    }

    private void LoadPreviewData(int enrollmentId)
    {
        Enrollment = new Enrollment
        {
            Id = enrollmentId,
            ParticipantId = 1,
            ProgramId = 2,
            ParticipantName = "Ayu Lestari",
            EmployeeId = "LSS-024",
            ProgramName = "Training & Certification Green Belt + TOC",
            Status = "In Progress",
            TocFlag = true
        };

        Progress =
        [
            new ModuleProgress { Id = 1, ModuleName = "Introduction to Lean Six Sigma", TargetHours = 8, ActualHours = 8, Status = "Completed" },
            new ModuleProgress { Id = 2, ModuleName = "Define Phase & Project Charter", TargetHours = 12, ActualHours = 12, Status = "Completed" },
            new ModuleProgress { Id = 3, ModuleName = "Measure Phase & Process Capability", TargetHours = 16, ActualHours = 10, Status = "In Progress" },
            new ModuleProgress { Id = 4, ModuleName = "Analyze Phase & Root Cause Analysis", TargetHours = 16, ActualHours = 0, Status = "Not Started" },
            new ModuleProgress { Id = 5, ModuleName = "Improve Phase & Solution Design", TargetHours = 14, ActualHours = 0, Status = "Not Started" },
            new ModuleProgress { Id = 6, ModuleName = "Control Phase & Statistical Process Control", TargetHours = 14, ActualHours = 0, Status = "Not Started" }
        ];
    }

    public string Percent(ModuleProgress module)
    {
        if (module.TargetHours <= 0) return "0";
        return Math.Min(100, (int)Math.Round(module.ActualHours * 100 / module.TargetHours)).ToString();
    }

    private async Task<IActionResult> ReloadAsync(int enrollmentId, CancellationToken cancellationToken)
    {
        var enrollments = await enrollmentRepository.GetAllAsync(cancellationToken);
        Enrollment = enrollments.FirstOrDefault(x => x.Id == enrollmentId);
        if (Enrollment is not null)
        {
            Progress = await enrollmentRepository.GetProgressAsync(enrollmentId, cancellationToken);
        }
        return Page();
    }
}
