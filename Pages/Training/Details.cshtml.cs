using LssTraining.Web.Data;
using LssTraining.Web.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LssTraining.Web.Pages.Training;

public class DetailsModel(ITrainingRepository repository, IEnrollmentRepository enrollmentRepository) : PageModel
{
    public TrainingProgram? Program { get; private set; }

    public IReadOnlyList<TrainingModule> Modules { get; private set; } = [];

    public IReadOnlyList<ProgramGraduateItem> Graduates { get; private set; } = [];

    public IReadOnlyList<ProgramGraduateItem> CertifiedParticipants { get; private set; } = [];

    public IReadOnlyList<ProgramGraduateItem> CompletedTrainingParticipants { get; private set; } = [];

    public IReadOnlyList<ProgramGraduateItem> InTrainingParticipants { get; private set; } = [];

    public bool IsCertificationProgram =>
        Program is not null && (
            Program.IsGreenBelt ||
            Program.ShortName.Contains("Yellow", StringComparison.OrdinalIgnoreCase) ||
            Program.Name.Contains("Yellow Belt", StringComparison.OrdinalIgnoreCase) ||
            Program.ShortName.Contains("BB", StringComparison.OrdinalIgnoreCase) ||
            Program.Name.Contains("Black Belt", StringComparison.OrdinalIgnoreCase) ||
            Program.Name.Contains("Green Belt", StringComparison.OrdinalIgnoreCase));

    public async Task OnGetAsync(int id, CancellationToken cancellationToken)
    {
        Program = await repository.GetProgramByIdAsync(id, cancellationToken);
        if (Program is not null)
        {
            Modules = await repository.GetModulesAsync(id, cancellationToken);
            var allGraduates = await enrollmentRepository.GetProgramGraduatesAsync(id, cancellationToken);
            Graduates = allGraduates;

            CertifiedParticipants = allGraduates.Where(g => g.IsCertified).ToList();
            CompletedTrainingParticipants = allGraduates.Where(g => !g.IsCertified && g.IsCompletedTraining).ToList();
            InTrainingParticipants = allGraduates.Where(g => !g.IsCertified && !g.IsCompletedTraining).ToList();
        }
    }
}
