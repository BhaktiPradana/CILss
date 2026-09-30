using System;

namespace LssTraining.Web.Models
{
    public class KwhRecord
    {
        public DateTime Timestamp { get; set; } 
        public string MeterName { get; set; } = string.Empty;
        public decimal Usage_kWh { get; set; }
        public string Floor { get; set; } = "Other Floor";
    }
}