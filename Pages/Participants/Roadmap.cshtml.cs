using LssTraining.Web.Data;
using LssTraining.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LssTraining.Web.Pages.Participants;

public class RoadmapModel(IDashboardRepository repository, ILogger<RoadmapModel> logger) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    public IReadOnlyList<ParticipantRoadmap> Participants { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        try
        {
            Participants = await repository.GetParticipantRoadmapAsync(Search, cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Database unavailable; using preview roadmap data.");
            Participants = PreviewParticipants()
                .Where(x => string.IsNullOrWhiteSpace(Search)
                    || x.FullName.Contains(Search, StringComparison.OrdinalIgnoreCase)
                    || x.EmployeeId.Contains(Search, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
    }

    private static IReadOnlyList<ParticipantRoadmap> PreviewParticipants() =>
    [
        new(1, "LSS-024", "Ayu Lestari", "Quality Assurance", "Training & Certification Green Belt + TOC", "Define", 1, 0, 101, [
            new GreenBeltReview { Id = 1001, EnrollmentId = 101, ReviewNumber = 0, DmaicStage = "Define", Status = "Completed", TrainerComment = "Project Charter approved" },
            new GreenBeltReview { Id = 1002, EnrollmentId = 101, ReviewNumber = 1, DmaicStage = "Define", Status = "In Progress", TrainerComment = "Process Map & VOC in progress" },
            new GreenBeltReview { Id = 1003, EnrollmentId = 101, ReviewNumber = 2, DmaicStage = "Measure", Status = "Not Started" },
            new GreenBeltReview { Id = 1004, EnrollmentId = 101, ReviewNumber = 3, DmaicStage = "Analyze", Status = "Not Started" },
            new GreenBeltReview { Id = 1005, EnrollmentId = 101, ReviewNumber = 4, DmaicStage = "Improve", Status = "Not Started" },
            new GreenBeltReview { Id = 1006, EnrollmentId = 101, ReviewNumber = 5, DmaicStage = "Control", Status = "Not Started" }
        ]),
        new(2, "LSS-031", "Bima Prakoso", "Production", "Training & Certification Green Belt + TOC", "Measure", 2, 1, 102, [
            new GreenBeltReview { Id = 1007, EnrollmentId = 102, ReviewNumber = 0, DmaicStage = "Define", Status = "Completed", TrainerComment = "Charter OK" },
            new GreenBeltReview { Id = 1008, EnrollmentId = 102, ReviewNumber = 1, DmaicStage = "Define", Status = "Completed", TrainerComment = "SIPOC completed" },
            new GreenBeltReview { Id = 1009, EnrollmentId = 102, ReviewNumber = 2, DmaicStage = "Measure", Status = "In Progress", TrainerComment = "Baseline sigma data collection" },
            new GreenBeltReview { Id = 1010, EnrollmentId = 102, ReviewNumber = 3, DmaicStage = "Analyze", Status = "Not Started" },
            new GreenBeltReview { Id = 1011, EnrollmentId = 102, ReviewNumber = 4, DmaicStage = "Improve", Status = "Not Started" },
            new GreenBeltReview { Id = 1012, EnrollmentId = 102, ReviewNumber = 5, DmaicStage = "Control", Status = "Not Started" }
        ]),
        new(3, "LSS-018", "Citra Wulandari", "Supply Chain", "Training & Certification Green Belt + TOC", "Analyze", 3, 2, 103, [
            new GreenBeltReview { Id = 1013, EnrollmentId = 103, ReviewNumber = 0, DmaicStage = "Define", Status = "Completed" },
            new GreenBeltReview { Id = 1014, EnrollmentId = 103, ReviewNumber = 1, DmaicStage = "Define", Status = "Completed" },
            new GreenBeltReview { Id = 1015, EnrollmentId = 103, ReviewNumber = 2, DmaicStage = "Measure", Status = "Completed" },
            new GreenBeltReview { Id = 1016, EnrollmentId = 103, ReviewNumber = 3, DmaicStage = "Analyze", Status = "In Progress", TrainerComment = "Fishbone and 5-Why validating" },
            new GreenBeltReview { Id = 1017, EnrollmentId = 103, ReviewNumber = 4, DmaicStage = "Improve", Status = "Not Started" },
            new GreenBeltReview { Id = 1018, EnrollmentId = 103, ReviewNumber = 5, DmaicStage = "Control", Status = "Not Started" }
        ]),
        new(4, "LSS-042", "Dimas Pratama", "Engineering", "Training & Certification Green Belt + TOC", "Improve", 4, 3, 104, [
            new GreenBeltReview { Id = 1019, EnrollmentId = 104, ReviewNumber = 0, DmaicStage = "Define", Status = "Completed" },
            new GreenBeltReview { Id = 1020, EnrollmentId = 104, ReviewNumber = 1, DmaicStage = "Define", Status = "Completed" },
            new GreenBeltReview { Id = 1021, EnrollmentId = 104, ReviewNumber = 2, DmaicStage = "Measure", Status = "Completed" },
            new GreenBeltReview { Id = 1022, EnrollmentId = 104, ReviewNumber = 3, DmaicStage = "Analyze", Status = "Completed" },
            new GreenBeltReview { Id = 1023, EnrollmentId = 104, ReviewNumber = 4, DmaicStage = "Improve", Status = "In Progress", TrainerComment = "Packaging machine pilot project" },
            new GreenBeltReview { Id = 1024, EnrollmentId = 104, ReviewNumber = 5, DmaicStage = "Control", Status = "Not Started" }
        ]),
        new(5, "LSS-009", "Siti Rahma", "Finance", "Training & Certification Yellow Belt", null, 0, -1, null, []),
        new(6, "LSS-027", "Yoga Saputra", "Operations", "Training & Certification Green Belt + TOC", "Control", 5, 4, 105, [
            new GreenBeltReview { Id = 1025, EnrollmentId = 105, ReviewNumber = 0, DmaicStage = "Define", Status = "Completed" },
            new GreenBeltReview { Id = 1026, EnrollmentId = 105, ReviewNumber = 1, DmaicStage = "Define", Status = "Completed" },
            new GreenBeltReview { Id = 1027, EnrollmentId = 105, ReviewNumber = 2, DmaicStage = "Measure", Status = "Completed" },
            new GreenBeltReview { Id = 1028, EnrollmentId = 105, ReviewNumber = 3, DmaicStage = "Analyze", Status = "Completed" },
            new GreenBeltReview { Id = 1029, EnrollmentId = 105, ReviewNumber = 4, DmaicStage = "Improve", Status = "Completed" },
            new GreenBeltReview { Id = 1030, EnrollmentId = 105, ReviewNumber = 5, DmaicStage = "Control", Status = "In Progress", TrainerComment = "SOP and Control Chart finalization" }
        ]),
        new(7, "LSS-036", "Nadia Putri", "HR", "Training White Belt", null, 0, -1, null, [])
    ];
}
