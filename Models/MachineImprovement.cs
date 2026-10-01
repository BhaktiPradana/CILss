namespace LssTraining.Web.Models
{
    public class MachineImprovement
    {
        public int Id { get; set; }
        public string MachCode { get; set; } = string.Empty;
        public string MachName { get; set; } = string.Empty;
        public string ImprovementTitle { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime ImplementationDate { get; set; }
        public decimal BaselineKwhPerDay { get; set; }
        public decimal TargetKwhPerDay { get; set; }
        public decimal? ActualKwhPerDay { get; set; }
        public string Status { get; set; } = "In Progress"; // "Planned", "In Progress", "Completed"
        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }

        public decimal EstimatedDailySavingsKwh => Math.Max(0, BaselineKwhPerDay - (ActualKwhPerDay ?? TargetKwhPerDay));
        public decimal SavingsPercentage => BaselineKwhPerDay > 0 ? Math.Round((EstimatedDailySavingsKwh / BaselineKwhPerDay) * 100, 1) : 0;
    }
}
