using System;
using System.Collections.Generic;

namespace LssTraining.Web.Models
{
    public class ElectricityCardDto
    {
        public string MeterName { get; set; } = string.Empty;
        public string CardTitle { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;
        public string Building { get; set; } = string.Empty;
        public string Floor { get; set; } = string.Empty;
        public string PanelType { get; set; } = "DB";
        public string Unit { get; set; } = "kWh";
        public decimal TotalUsage { get; set; }
        public decimal AverageDaily { get; set; }
        public decimal LatestUsage { get; set; }
        public decimal PeakUsage { get; set; }
        public DateTime? LatestTimestamp { get; set; }
        public bool HasData { get; set; }
        public int DataPointsCount { get; set; }
        public List<decimal> Sparkline { get; set; } = new();
    }

    public class ElectricityDashboardSummary
    {
        public decimal TotalKwh { get; set; }
        public int TotalCards { get; set; }
        public int ActiveCards { get; set; }
        public decimal AverageDailyKwh { get; set; }
        public string PeakMeterName { get; set; } = "-";
        public decimal PeakUsage { get; set; }
    }

    public class MeterTimeDataPoint
    {
        public DateTime Timestamp { get; set; }
        public decimal Value { get; set; }
    }
}