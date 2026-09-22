using System.ComponentModel.DataAnnotations;
using LssTraining.Web.Data;
using LssTraining.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LssTraining.Web.Pages.Participants;

[Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin,Trainer")]
public class EditModel(
    IParticipantRepository repository,
    ITrainingRepository trainingRepository,
    IEnrollmentRepository enrollmentRepository) : PageModel
{
    [BindProperty]
    public ParticipantInput Input { get; set; } = new();

    public IReadOnlyList<TrainingProgram> Programs { get; private set; } = [];
    public IReadOnlyList<TrainingModule> AllModules { get; private set; } = [];
    public IReadOnlyList<ParticipantProgramProgress> EnrolledPrograms { get; private set; } = [];
    public bool IsNew => Input.Id == 0;

    [BindProperty]
    public List<int> CompletedModuleIds { get; set; } = [];

    [BindProperty]
    public List<int> InitialCompletedModuleIds { get; set; } = [];

    [BindProperty]
    public int? NewProgramId { get; set; }

    [BindProperty]
    public bool NewProgramToc { get; set; }

    public async Task<IActionResult> OnGetAsync(int? id, CancellationToken cancellationToken)
    {
        await LoadMetadataAsync(cancellationToken);

        if (id is null)
        {
            var defaultGb = Programs.FirstOrDefault(p => p.IsGreenBelt) ?? Programs.FirstOrDefault();
            if (defaultGb is not null)
            {
                Input.ProgramId = defaultGb.Id;
            }
            return Page();
        }

        var participant = await repository.GetByIdAsync(id.Value, cancellationToken);
        if (participant is null)
        {
            return NotFound();
        }

        Input = new ParticipantInput
        {
            Id = participant.Id,
            EmployeeId = participant.EmployeeId,
            FullName = participant.FullName,
            Department = participant.Department,
            Position = participant.Position,
            ProgramId = participant.ProgramId,
            StatusPhase = participant.StatusPhase,
            TocFlag = participant.TocFlag,
            IsActive = participant.IsActive
        };

        EnrolledPrograms = await enrollmentRepository.GetParticipantProgramsAsync(id.Value, cancellationToken);
        CompletedModuleIds = EnrolledPrograms
            .SelectMany(p => p.Modules)
            .Where(m => m.IsCompleted)
            .Select(m => m.ModuleId)
            .ToList();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        await LoadMetadataAsync(cancellationToken);

        if (!ModelState.IsValid)
        {
            if (Input.Id > 0)
            {
                EnrolledPrograms = await enrollmentRepository.GetParticipantProgramsAsync(Input.Id, cancellationToken);
            }
            return Page();
        }

        if (await repository.GetByEmployeeIdAsync(Input.EmployeeId, cancellationToken) is { } existing && existing.Id != Input.Id)
        {
            ModelState.AddModelError("Input.EmployeeId", "Employee ID is already in use.");
        }

        if (!ModelState.IsValid)
        {
            if (Input.Id > 0)
            {
                EnrolledPrograms = await enrollmentRepository.GetParticipantProgramsAsync(Input.Id, cancellationToken);
            }
            return Page();
        }

        var participant = new Participant
        {
            Id = Input.Id,
            EmployeeId = Input.EmployeeId.Trim(),
            FullName = Input.FullName.Trim(),
            Department = string.IsNullOrWhiteSpace(Input.Department) ? null : Input.Department.Trim(),
            Position = string.IsNullOrWhiteSpace(Input.Position) ? null : Input.Position.Trim(),
            StatusPhase = string.IsNullOrWhiteSpace(Input.StatusPhase) ? null : Input.StatusPhase.Trim(),
            IsActive = Input.IsActive
        };

        try
        {
            var participantId = Input.Id;
            if (Input.Id == 0)
            {
                participantId = await repository.CreateAsync(participant, User.Identity?.Name, cancellationToken);

                if (Input.ProgramId.HasValue && Input.ProgramId.Value > 0)
                {
                    var enrollmentId = await enrollmentRepository.EnrollOrUpdateParticipantAsync(
                        participantId,
                        Input.ProgramId.Value,
                        Input.StatusPhase,
                        Input.TocFlag,
                        User.Identity?.Name,
                        cancellationToken);

                    if (enrollmentId.HasValue && InitialCompletedModuleIds.Count > 0)
                    {
                        await enrollmentRepository.SaveModuleCompletionsAsync(
                            enrollmentId.Value,
                            InitialCompletedModuleIds,
                            User.Identity?.Name,
                            cancellationToken);
                    }
                }
            }
            else
            {
                await repository.UpdateAsync(participant, User.Identity?.Name, cancellationToken);

                await enrollmentRepository.EnrollOrUpdateParticipantAsync(
                    participantId,
                    null,
                    Input.StatusPhase,
                    Input.TocFlag,
                    User.Identity?.Name,
                    cancellationToken);

                var existingEnrollments = await enrollmentRepository.GetParticipantProgramsAsync(participantId, cancellationToken);
                foreach (var enrollment in existingEnrollments)
                {
                    var enrollmentCompleted = enrollment.Modules
                        .Where(m => CompletedModuleIds.Contains(m.ModuleId))
                        .Select(m => m.ModuleId);

                    await enrollmentRepository.SaveModuleCompletionsAsync(
                        enrollment.EnrollmentId,
                        enrollmentCompleted,
                        User.Identity?.Name,
                        cancellationToken);
                }
            }

            TempData["Success"] = IsNew ? "New participant successfully registered." : "Participant profile and module progress saved successfully.";
            return RedirectToPage("Edit", new { id = participantId });
        }
        catch (InvalidOperationException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            if (Input.Id > 0)
            {
                EnrolledPrograms = await enrollmentRepository.GetParticipantProgramsAsync(Input.Id, cancellationToken);
            }
            return Page();
        }
    }

    public async Task<IActionResult> OnPostAddProgramAsync(int id, CancellationToken cancellationToken)
    {
        if (NewProgramId is null or <= 0)
        {
            TempData["Error"] = "Please select a training program to add.";
            return RedirectToPage(new { id });
        }

        try
        {
            await enrollmentRepository.CreateAsync(id, NewProgramId.Value, NewProgramToc, User.Identity?.Name, cancellationToken);
            TempData["Success"] = "New training program added for participant.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostCertifyAsync(int enrollmentId, int participantId, CancellationToken cancellationToken)
    {
        if (!User.IsInRole("Admin") && !User.IsInRole("Trainer"))
        {
            return Forbid();
        }

        await enrollmentRepository.SetCertifiedAsync(enrollmentId, true, User.Identity?.Name, cancellationToken);
        TempData["Success"] = "Participant granted certification.";
        return RedirectToPage(new { id = participantId });
    }

    public async Task<IActionResult> OnPostRevokeCertificationAsync(int enrollmentId, int participantId, CancellationToken cancellationToken)
    {
        if (!User.IsInRole("Admin") && !User.IsInRole("Trainer"))
        {
            return Forbid();
        }

        await enrollmentRepository.SetCertifiedAsync(enrollmentId, false, User.Identity?.Name, cancellationToken);
        TempData["Success"] = "Participant certification revoked.";
        return RedirectToPage(new { id = participantId });
    }

    private async Task LoadMetadataAsync(CancellationToken cancellationToken)
    {
        Programs = await trainingRepository.GetProgramsAsync(cancellationToken);
        var modulesList = new List<TrainingModule>();
        foreach (var program in Programs)
        {
            var pModules = await trainingRepository.GetModulesAsync(program.Id, cancellationToken);
            modulesList.AddRange(pModules);
        }
        AllModules = modulesList;
    }

    public sealed class ParticipantInput
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Employee ID is required.")]
        [StringLength(32)]
        public string EmployeeId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Full Name is required.")]
        [StringLength(160)]
        public string FullName { get; set; } = string.Empty;

        [StringLength(120)]
        public string? Department { get; set; }

        [StringLength(120)]
        public string? Position { get; set; }

        public int? ProgramId { get; set; }

        public string? StatusPhase { get; set; }

        public bool TocFlag { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
