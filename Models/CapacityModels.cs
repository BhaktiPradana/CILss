namespace LssTraining.Web.Models
{
    public class CapacitySummary
    {
        public int TotalProducts { get; set; }
        public int TotalProcesses { get; set; }
        public int TotalEmployees { get; set; }
    }

    public class ManpowerBySection
    {
        public string SectionCode { get; set; }
        public int EmployeeCount { get; set; }
    }

    public class ProcessArea
    {
        public string Area { get; set; }
        public int ProcessCount { get; set; }
    }
}
