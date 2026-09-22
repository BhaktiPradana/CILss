using LssTraining.Web.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LssTraining.Web.Pages.Training;

public class IndexModel(ITrainingRepository repository, ILogger<IndexModel> logger) : PageModel
{
    public IReadOnlyList<ProgramCard> Programs { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        try
        {
            var programs = await repository.GetProgramsAsync(cancellationToken);
            var cards = new List<ProgramCard>();

            foreach (var program in programs)
            {
                var modules = await repository.GetModulesAsync(program.Id, cancellationToken);
                var cssClass = program.ShortName switch
                {
                    "White Belt" => "blue",
                    "Yellow Belt" => "amber",
                    "Green Belt" => "teal",
                    _ => "navy"
                };

                cards.Add(new ProgramCard(
                    program.Id,
                    program.Name,
                    program.ShortName,
                    program.Description,
                    modules.Count,
                    modules.Sum(x => x.TargetHours),
                    cssClass
                ));
            }

            Programs = cards;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to load training programs.");
        }
    }

    public sealed record ProgramCard(
        int Id,
        string Name,
        string ShortName,
        string Description,
        int ModuleCount,
        decimal TotalHours,
        string CssClass
    );
}
