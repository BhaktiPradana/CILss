using LssTraining.Web.Data;
using LssTraining.Web.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LssTraining.Web.Pages.Training;

public class EnrollmentsModel(IEnrollmentRepository repository, ILogger<EnrollmentsModel> logger) : PageModel
{
    [Microsoft.AspNetCore.Mvc.BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    public IReadOnlyList<Enrollment> Enrollments { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        IReadOnlyList<Enrollment> list;
        try
        {
            list = await repository.GetAllAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to load enrollments from database; using preview fallback.");
            list = GetPreviewEnrollments();
        }

        if (!string.IsNullOrWhiteSpace(Search))
        {
            var q = Search.Trim();
            Enrollments = list.Where(x =>
                x.ParticipantName.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                x.EmployeeId.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                x.ProgramName.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                x.Status.Contains(q, StringComparison.OrdinalIgnoreCase)).ToList();
        }
        else
        {
            Enrollments = list;
        }
    }

    private static IReadOnlyList<Enrollment> GetPreviewEnrollments() =>
    [
        new() { Id = 101, ParticipantId = 1, ProgramId = 2, ParticipantName = "Ayu Lestari", EmployeeId = "LSS-024", ProgramName = "Training & Certification Green Belt + TOC", EnrolledAt = DateTime.UtcNow.AddDays(-30), Status = "In Progress", TocFlag = true },
        new() { Id = 102, ParticipantId = 2, ProgramId = 2, ParticipantName = "Bima Prakoso", EmployeeId = "LSS-031", ProgramName = "Training & Certification Green Belt + TOC", EnrolledAt = DateTime.UtcNow.AddDays(-28), Status = "In Progress", TocFlag = true },
        new() { Id = 103, ParticipantId = 3, ProgramId = 2, ParticipantName = "Citra Wulandari", EmployeeId = "LSS-018", ProgramName = "Training & Certification Green Belt + TOC", EnrolledAt = DateTime.UtcNow.AddDays(-25), Status = "In Progress", TocFlag = true },
        new() { Id = 104, ParticipantId = 4, ProgramId = 2, ParticipantName = "Dimas Pratama", EmployeeId = "LSS-042", ProgramName = "Training & Certification Green Belt + TOC", EnrolledAt = DateTime.UtcNow.AddDays(-20), Status = "In Progress", TocFlag = true },
        new() { Id = 105, ParticipantId = 6, ProgramId = 2, ParticipantName = "Yoga Saputra", EmployeeId = "LSS-027", ProgramName = "Training & Certification Green Belt + TOC", EnrolledAt = DateTime.UtcNow.AddDays(-15), Status = "In Progress", TocFlag = true },
        new() { Id = 106, ParticipantId = 5, ProgramId = 3, ParticipantName = "Siti Rahma", EmployeeId = "LSS-009", ProgramName = "Training & Certification Yellow Belt", EnrolledAt = DateTime.UtcNow.AddDays(-10), Status = "In Progress", TocFlag = false }
    ];
}
