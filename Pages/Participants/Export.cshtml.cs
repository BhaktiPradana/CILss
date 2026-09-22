using LssTraining.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LssTraining.Web.Pages.Participants;

public class ExportModel(IParticipantExportService exportService, ILogger<ExportModel> logger) : PageModel
{
    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        try
        {
            var fileBytes = await exportService.GenerateExcelExportAsync(cancellationToken);
            var fileName = $"CILeanSixSigma_Participants_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";
            Response.Headers.ContentDisposition = $"attachment; filename=\"{fileName}\"";
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to download export directly.");
            TempData["Error"] = "Failed to export Excel: " + ex.Message;
            return RedirectToPage("/Participants/Index");
        }
    }
}
