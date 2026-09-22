namespace LssTraining.Web.Services
{
    public interface IParticipantExportService
    {
        Task<byte[]> GenerateExcelExportAsync(CancellationToken ct = default);
    }
}