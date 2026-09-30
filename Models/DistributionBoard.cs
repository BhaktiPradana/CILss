namespace LssTraining.Web.Models
{
    public class DistributionBoard
    {
        public int DBID { get; set; }
        public string DBName { get; set; } = string.Empty;

        // Display properties for UI cards
        public string BoardName => DBName;
        public string Location { get; set; } = string.Empty;
        public string Floor { get; set; } = string.Empty;
        public string Building { get; set; } = string.Empty;
        public string PanelType { get; set; } = "DB";
        public int? RatedCapacityAmps { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
