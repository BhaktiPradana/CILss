using LssTraining.Web.Data;
using LssTraining.Web.Models;
using LssTraining.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LssTraining.Web.Pages.Participants;

public class IndexModel(IParticipantRepository repository, ILogger<IndexModel> logger) : PageModel
{
    public IReadOnlyList<Participant> Items { get; private set; } = [];
    public string? Search { get; set; }
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; private set; }

    public async Task OnGetAsync(string? search, int page = 1, CancellationToken cancellationToken = default)
    {
        Search = search;
        CurrentPage = Math.Max(1, page);

        try
        {
            var result = await repository.SearchAsync(search, CurrentPage, 12, cancellationToken);
            Items = result.Items;
            TotalPages = (int)Math.Ceiling(result.Total / 12m);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Failed to load participants from database.");
            Items = new List<Participant>();
            TotalPages = 1;
            TempData["Error"] = "Unable to connect to the database. Please try again later.";
        }
    }

    public async Task<IActionResult> OnGetExportExcelAsync(
        [FromServices] IParticipantExportService exportService,
        CancellationToken cancellationToken)
    {
        try
        {
            var fileBytes = await exportService.GenerateExcelExportAsync(cancellationToken);
            var fileName = $"CILeanSixSigma_Participants_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";
            Response.Headers.ContentDisposition = $"attachment; filename=\"{fileName}\"";
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to generate Excel export.");
            TempData["Error"] = "Failed to export Excel: " + exception.Message;
            return RedirectToPage();
        }
    }
}
