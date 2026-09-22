using System.ComponentModel.DataAnnotations;
using LssTraining.Web.Data;
using LssTraining.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LssTraining.Web.Pages.Training;

[Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin,Trainer")]
public class EnrollModel(
    IParticipantRepository participantRepository,
    ITrainingRepository trainingRepository,
    IEnrollmentRepository enrollmentRepository) : PageModel
{
    [BindProperty]
    public EnrollInput Input { get; set; } = new();

    public IReadOnlyList<Participant> Participants { get; private set; } = [];

    public IReadOnlyList<TrainingProgram> Programs { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        await LoadDataAsync(cancellationToken);
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await LoadDataAsync(cancellationToken);
            return Page();
        }

        try
        {
            await enrollmentRepository.CreateAsync(
                Input.ParticipantId!.Value,
                Input.ProgramId!.Value,
                Input.TocFlag,
                User.Identity?.Name,
                cancellationToken
            );

            TempData["Success"] = "Participant enrolled successfully.";
            return RedirectToPage("Enrollments");
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await LoadDataAsync(cancellationToken);
            return Page();
        }
    }

    private async Task LoadDataAsync(CancellationToken cancellationToken)
    {
        Participants = (await participantRepository.SearchAsync(null, 1, 500, cancellationToken)).Items;
        Programs = await trainingRepository.GetProgramsAsync(cancellationToken);
    }

    public sealed class EnrollInput
    {
        [Required(ErrorMessage = "Please select a participant.")]
        public int? ParticipantId { get; set; }

        [Required(ErrorMessage = "Please select a program.")]
        public int? ProgramId { get; set; }

        public bool TocFlag { get; set; }
    }
}
