using System.Security.Claims;
using LssTraining.Web.Data;
using LssTraining.Web.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LssTraining.Web.Pages;

public class IndexModel(IDashboardRepository repository, ILogger<IndexModel> logger) : PageModel
{
    public DashboardMetrics Metrics { get; private set; } = new(0, 0, 0, 0, 0, 0);
    public IReadOnlyList<ProgramSummary> Programs { get; private set; } = [];
    public bool HasDatabaseError { get; private set; }
    public string UserRole => User.FindFirst(ClaimTypes.Role)?.Value ?? "User";
    public string UserInitials => string.Join("", (User.Identity?.Name ?? "U")
        .Split(' ', StringSplitOptions.RemoveEmptyEntries)
        .Take(2)
        .Select(x => x[0]))
        .ToUpperInvariant();

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        try
        {
            Metrics = await repository.GetMetricsAsync(cancellationToken);
            Programs = await repository.GetProgramSummariesAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            HasDatabaseError = true;
            logger.LogWarning(exception, "Database unavailable; using preview dashboard data.");
            LoadPreviewData();
        }
    }

    private void LoadPreviewData()
    {
        Metrics = new DashboardMetrics(48, 12, 27, 69.2m, 18, 30);
        Programs =
        [
            new(1, TrainingNames.WhiteBelt, "White Belt", "Lean Six Sigma fundamentals.", 18, 15, 3, 83.3m, "blue"),
            new(2, TrainingNames.YellowBelt, "Yellow Belt", "Operational problem solving & DMAIC.", 14, 8, 6, 57.1m, "amber"),
            new(3, TrainingNames.GreenBelt, "Green Belt", "DMAIC project leadership & TOC.", 10, 3, 7, 30m, "teal"),
            new(4, TrainingNames.BlackBelt, "Black Belt", "Advanced strategy & transformation.", 6, 1, 5, 16.7m, "navy")
        ];
    }
}
