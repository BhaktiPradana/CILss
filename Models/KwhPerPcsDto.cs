using System;
using System.Collections.Generic;

namespace LssTraining.Web.Models
{
    public class MachineKwhPerPcsDto
    {
        public string MachCode { get; set; } = string.Empty;
        public string MachName { get; set; } = string.Empty;
        public string Area { get; set; } = string.Empty;
        public string DBName { get; set; } = string.Empty;
        public string? SubDBName { get; set; }
        public string Remarks { get; set; } = string.Empty;
        
        public decimal DBTotalKwh { get; set; }
        public int MachinesConnectedCount { get; set; }
        public decimal AllocatedKwh { get; set; }
        
        public double QuantityIn { get; set; }
        public decimal KwhPerPcs { get; set; }
        public bool IsMetered { get; set; }
        public string AllocationMethod { get; set; } = "Equal";
        public bool HasProduction => QuantityIn > 0;
    }

    public class DbKwhSummaryDto
    {
        public string DBName { get; set; } = string.Empty;
        public string Building { get; set; } = string.Empty;
        public string Floor { get; set; } = string.Empty;
        public decimal TotalKwh { get; set; }
        public double TotalQuantityIn { get; set; }
        public decimal OverallKwhPerPcs { get; set; }
        public int MachineCount { get; set; }
        public int ActiveMachineCount { get; set; }
        public bool IsMetered { get; set; }
    }

    public class KwhPerPcsDashboardSummary
    {
        public decimal TotalKwhAllocated { get; set; }
        public double TotalPiecesProduced { get; set; }
        public decimal AverageKwhPerPcs { get; set; }
        public int MappedMachinesCount { get; set; }
        public int ActiveMachinesCount { get; set; }
        public int MonitoredDbsCount { get; set; }
        public string BestMachineCode { get; set; } = "-";
        public string BestMachineName { get; set; } = "-";
        public decimal BestMachineKwhPerPcs { get; set; }
        public string HighestKwhMachineCode { get; set; } = "-";
        public decimal HighestKwhPerPcs { get; set; }
    }

    public class KwhPerPcsDailyTrendPoint
    {
        public DateTime Date { get; set; }
        public string DateLabel { get; set; } = string.Empty;
        public decimal TotalKwh { get; set; }
        public double TotalPieces { get; set; }
        public decimal KwhPerPcs { get; set; }
    }
}

