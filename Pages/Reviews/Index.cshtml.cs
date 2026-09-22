using LssTraining.Web.Data;
using LssTraining.Web.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LssTraining.Web.Pages.Reviews;

public class IndexModel(IReviewRepository repository, ILogger<IndexModel> logger) : PageModel
{
    public Enrollment? Enrollment { get; private set; }

    public IReadOnlyList<GreenBeltReview> Reviews { get; private set; } = [];

    public async Task OnGetAsync(int enrollmentId, CancellationToken cancellationToken)
    {
        try
        {
            var result = await repository.GetRoadmapAsync(enrollmentId, cancellationToken);
            if (result is not null)
            {
                Enrollment = result.Value.Enrollment;
                Reviews = result.Value.Reviews;
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Database unavailable; attempting fallback preview review data for enrollment {EnrollmentId}", enrollmentId);
            LoadPreviewData(enrollmentId);
        }
    }

    private void LoadPreviewData(int enrollmentId)
    {
        var preview = GetPreviewRoadmaps().FirstOrDefault(p => p.Enrollment.Id == enrollmentId);
        if (preview.Enrollment != null)
        {
            Enrollment = preview.Enrollment;
            Reviews = preview.Reviews;
        }
        else
        {
            // Default sample preview
            Enrollment = new Enrollment
            {
                Id = enrollmentId,
                ParticipantId = 1,
                ProgramId = 2,
                ParticipantName = "Sample Participant",
                EmployeeId = "LSS-SAMPLE",
                ProgramName = "Training & Certification Green Belt + TOC",
                Status = "In Progress",
                TocFlag = true
            };
            Reviews =
            [
                new GreenBeltReview { Id = 1001, EnrollmentId = enrollmentId, ReviewNumber = 0, DmaicStage = "Define", Status = "Completed", TrainerComment = "Project Charter approved" },
                new GreenBeltReview { Id = 1002, EnrollmentId = enrollmentId, ReviewNumber = 1, DmaicStage = "Define", Status = "In Progress", TrainerComment = "SIPOC & VOC in progress" },
                new GreenBeltReview { Id = 1003, EnrollmentId = enrollmentId, ReviewNumber = 2, DmaicStage = "Measure", Status = "Not Started" },
                new GreenBeltReview { Id = 1004, EnrollmentId = enrollmentId, ReviewNumber = 3, DmaicStage = "Analyze", Status = "Not Started" },
                new GreenBeltReview { Id = 1005, EnrollmentId = enrollmentId, ReviewNumber = 4, DmaicStage = "Improve", Status = "Not Started" },
                new GreenBeltReview { Id = 1006, EnrollmentId = enrollmentId, ReviewNumber = 5, DmaicStage = "Control", Status = "Not Started" }
            ];
        }
    }

    private static (Enrollment Enrollment, IReadOnlyList<GreenBeltReview> Reviews)[] GetPreviewRoadmaps() =>
    [
        (new Enrollment { Id = 101, ParticipantId = 1, ProgramId = 2, ParticipantName = "Ayu Lestari", EmployeeId = "LSS-024", ProgramName = "Training & Certification Green Belt + TOC", Status = "In Progress", TocFlag = true },
        [
            new GreenBeltReview { Id = 1001, EnrollmentId = 101, ReviewNumber = 0, DmaicStage = "Define", Status = "Completed", TrainerComment = "Project Charter approved" },
            new GreenBeltReview { Id = 1002, EnrollmentId = 101, ReviewNumber = 1, DmaicStage = "Define", Status = "In Progress", TrainerComment = "Process Map & VOC in progress" },
            new GreenBeltReview { Id = 1003, EnrollmentId = 101, ReviewNumber = 2, DmaicStage = "Measure", Status = "Not Started" },
            new GreenBeltReview { Id = 1004, EnrollmentId = 101, ReviewNumber = 3, DmaicStage = "Analyze", Status = "Not Started" },
            new GreenBeltReview { Id = 1005, EnrollmentId = 101, ReviewNumber = 4, DmaicStage = "Improve", Status = "Not Started" },
            new GreenBeltReview { Id = 1006, EnrollmentId = 101, ReviewNumber = 5, DmaicStage = "Control", Status = "Not Started" }
        ]),
        (new Enrollment { Id = 102, ParticipantId = 2, ProgramId = 2, ParticipantName = "Bima Prakoso", EmployeeId = "LSS-031", ProgramName = "Training & Certification Green Belt + TOC", Status = "In Progress", TocFlag = true },
        [
            new GreenBeltReview { Id = 1007, EnrollmentId = 102, ReviewNumber = 0, DmaicStage = "Define", Status = "Completed", TrainerComment = "Charter OK" },
            new GreenBeltReview { Id = 1008, EnrollmentId = 102, ReviewNumber = 1, DmaicStage = "Define", Status = "Completed", TrainerComment = "SIPOC completed" },
            new GreenBeltReview { Id = 1009, EnrollmentId = 102, ReviewNumber = 2, DmaicStage = "Measure", Status = "In Progress", TrainerComment = "Baseline sigma data collection" },
            new GreenBeltReview { Id = 1010, EnrollmentId = 102, ReviewNumber = 3, DmaicStage = "Analyze", Status = "Not Started" },
            new GreenBeltReview { Id = 1011, EnrollmentId = 102, ReviewNumber = 4, DmaicStage = "Improve", Status = "Not Started" },
            new GreenBeltReview { Id = 1012, EnrollmentId = 102, ReviewNumber = 5, DmaicStage = "Control", Status = "Not Started" }
        ]),
        (new Enrollment { Id = 103, ParticipantId = 3, ProgramId = 2, ParticipantName = "Citra Wulandari", EmployeeId = "LSS-018", ProgramName = "Training & Certification Green Belt + TOC", Status = "In Progress", TocFlag = true },
        [
            new GreenBeltReview { Id = 1013, EnrollmentId = 103, ReviewNumber = 0, DmaicStage = "Define", Status = "Completed" },
            new GreenBeltReview { Id = 1014, EnrollmentId = 103, ReviewNumber = 1, DmaicStage = "Define", Status = "Completed" },
            new GreenBeltReview { Id = 1015, EnrollmentId = 103, ReviewNumber = 2, DmaicStage = "Measure", Status = "Completed" },
            new GreenBeltReview { Id = 1016, EnrollmentId = 103, ReviewNumber = 3, DmaicStage = "Analyze", Status = "In Progress", TrainerComment = "Fishbone and 5-Why validating" },
            new GreenBeltReview { Id = 1017, EnrollmentId = 103, ReviewNumber = 4, DmaicStage = "Improve", Status = "Not Started" },
            new GreenBeltReview { Id = 1018, EnrollmentId = 103, ReviewNumber = 5, DmaicStage = "Control", Status = "Not Started" }
        ]),
        (new Enrollment { Id = 104, ParticipantId = 4, ProgramId = 2, ParticipantName = "Dimas Pratama", EmployeeId = "LSS-042", ProgramName = "Training & Certification Green Belt + TOC", Status = "In Progress", TocFlag = true },
        [
            new GreenBeltReview { Id = 1019, EnrollmentId = 104, ReviewNumber = 0, DmaicStage = "Define", Status = "Completed" },
            new GreenBeltReview { Id = 1020, EnrollmentId = 104, ReviewNumber = 1, DmaicStage = "Define", Status = "Completed" },
            new GreenBeltReview { Id = 1021, EnrollmentId = 104, ReviewNumber = 2, DmaicStage = "Measure", Status = "Completed" },
            new GreenBeltReview { Id = 1022, EnrollmentId = 104, ReviewNumber = 3, DmaicStage = "Analyze", Status = "Completed" },
            new GreenBeltReview { Id = 1023, EnrollmentId = 104, ReviewNumber = 4, DmaicStage = "Improve", Status = "In Progress", TrainerComment = "Packaging machine pilot project" },
            new GreenBeltReview { Id = 1024, EnrollmentId = 104, ReviewNumber = 5, DmaicStage = "Control", Status = "Not Started" }
        ]),
        (new Enrollment { Id = 105, ParticipantId = 6, ProgramId = 2, ParticipantName = "Yoga Saputra", EmployeeId = "LSS-027", ProgramName = "Training & Certification Green Belt + TOC", Status = "In Progress", TocFlag = true },
        [
            new GreenBeltReview { Id = 1025, EnrollmentId = 105, ReviewNumber = 0, DmaicStage = "Define", Status = "Completed" },
            new GreenBeltReview { Id = 1026, EnrollmentId = 105, ReviewNumber = 1, DmaicStage = "Define", Status = "Completed" },
            new GreenBeltReview { Id = 1027, EnrollmentId = 105, ReviewNumber = 2, DmaicStage = "Measure", Status = "Completed" },
            new GreenBeltReview { Id = 1028, EnrollmentId = 105, ReviewNumber = 3, DmaicStage = "Analyze", Status = "Completed" },
            new GreenBeltReview { Id = 1029, EnrollmentId = 105, ReviewNumber = 4, DmaicStage = "Improve", Status = "Completed" },
            new GreenBeltReview { Id = 1030, EnrollmentId = 105, ReviewNumber = 5, DmaicStage = "Control", Status = "In Progress", TrainerComment = "SOP and Control Chart finalization" }
        ])
    ];
}
