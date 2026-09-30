namespace LssTraining.Web.Models
{
    public class Machine
    {
        public string MachCode { get; set; } = string.Empty;
        public string MachName { get; set; } = string.Empty;
    }

    public class MasterMachineConnection
    {
        public int ConnectionID { get; set; }
        public string MachCode { get; set; } = string.Empty;
        public string? SubDBName { get; set; }
        public string DBName { get; set; } = string.Empty;
        public string? Remarks { get; set; }
    }

    public class DbMachineMapping
    {
        public int ConnectionID { get; set; }
        public string MachCode { get; set; } = string.Empty;
        public string MachName { get; set; } = string.Empty;
        public string DBName { get; set; } = string.Empty;
        public string? SubDBName { get; set; }
        public string? Remarks { get; set; }
        public string? DbPrimary { get; set; }
        public string? DbSecondary { get; set; }
        public string? DbTertiary { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
