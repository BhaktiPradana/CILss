using LssTraining.Web.Data;
using LssTraining.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LssTraining.Web.Pages.Reviews;

[Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin,Trainer")]
public class EditModel(IReviewRepository repository) : PageModel
{
    [BindProperty]
    public ReviewInput Input { get; set; } = new();

    public GreenBeltReview? Review { get; private set; }

    public int EnrollmentId => Input.EnrollmentId;

    public async Task<IActionResult> OnGetAsync(int enrollmentId, int id, CancellationToken cancellationToken)
    {
        var result = await repository.GetRoadmapAsync(enrollmentId, cancellationToken);
        Review = result?.Reviews.FirstOrDefault(x => x.Id == id);
        if (Review is null)
        {
            return NotFound();
        }

        Input = new ReviewInput
        {
            Id = Review.Id,
            EnrollmentId = Review.EnrollmentId,
            ReviewNumber = Review.ReviewNumber,
            Status = Review.Status,
            DmaicStage = Review.DmaicStage,
            TrainerComment = Review.TrainerComment
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        await repository.UpdateAsync(new GreenBeltReview
        {
            Id = Input.Id,
            EnrollmentId = Input.EnrollmentId,
            ReviewNumber = Input.ReviewNumber,
            Status = Input.Status,
            DmaicStage = Input.DmaicStage,
            TrainerComment = Input.TrainerComment
        }, User.Identity?.Name ?? "System", cancellationToken);

        TempData["Success"] = "Review updated successfully.";
        return RedirectToPage("Index", new { enrollmentId = Input.EnrollmentId });
    }

    public sealed class ReviewInput
    {
        public int Id { get; set; }
        public int EnrollmentId { get; set; }
        public int ReviewNumber { get; set; }
        public string Status { get; set; } = "Not Started";
        public string? DmaicStage { get; set; }
        public string? TrainerComment { get; set; }
    }
}
