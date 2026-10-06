namespace LssTraining.Web.Models
{
    using System.Collections.Generic;

    public class CapacitySummary
    {
        public int TotalDevices { get; set; }
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

    public class DeviceGroup
    {
        public string DeviceGroupCode { get; set; }
        public string DeviceGroupName { get; set; }
    }

    // --- NEW ENTITIES FOR SMART ROUTING SIMULATOR ---

    public class Device
    {
        public string DeviceCode { get; set; }
        public string DeviceGroupCode { get; set; }
    }

    public class DeviceProcess
    {
        public string DeviceCode { get; set; }
        public int Seq { get; set; }
        public string ProcCode { get; set; }
        public decimal LaborHour { get; set; }
        public decimal LimitReject { get; set; } // Yield
    }


    public class CycleTimeData
    {
        public string DeviceCode { get; set; }
        public string MachCode { get; set; }
        public string ProcCode { get; set; }
        public decimal UPH { get; set; }
        public decimal LaborTime { get; set; }
        public decimal MachineTime { get; set; }
    }

    // --- SIMULATION MODELS ---

    public class SimulationDemand
    {
        public string DeviceGroupCode { get; set; }
        public int DemandQuantity { get; set; }
    }

    public class SimulationInput
    {
        public List<SimulationDemand> Demands { get; set; } = new List<SimulationDemand>();
        public decimal OvertimeLimitPercent { get; set; }
        public string ShiftSystem { get; set; } = "5-2"; // 5-2 or 6-1
        public int ShiftsPerDay { get; set; } = 3;
        
        public int WorkingDays => ShiftSystem == "6-1" ? 6 : 5;
        public decimal NormalHoursPerShift => 7.25m;
        public decimal AvailableHoursPerPerson => WorkingDays * NormalHoursPerShift;
        public decimal AvailableMachineHours => WorkingDays * ShiftsPerDay * NormalHoursPerShift;
    }

    public class ProcessManpowerResult
    {
        public string ProcCode { get; set; }
        public string ProcName { get; set; }
        public decimal TotalLaborHoursRequired { get; set; }
        public int HeadcountZeroOvertime { get; set; }
        public int HeadcountWithOvertime { get; set; }
    }

    public class MachineCapacityResult
    {
        public string MachCode { get; set; }
        public string MachName { get; set; }
        public string Area { get; set; }
        public decimal RequiredHours { get; set; }
        public decimal AvailableNormalHours { get; set; }
        public decimal AvailableOvertimeHours { get; set; }
        public decimal TotalAvailableHours => AvailableNormalHours + AvailableOvertimeHours;
        public decimal ShortageOrSurplus => TotalAvailableHours - RequiredHours;
        public bool IsCapacityMet => ShortageOrSurplus >= 0;
        public decimal UtilizationPercent => TotalAvailableHours > 0 ? (RequiredHours / TotalAvailableHours) * 100 : 0;
    }

    public class SimulationResult
    {
        // Global aggregate
        public decimal RequiredHours { get; set; }
        public decimal AvailableNormalHours { get; set; }
        public decimal AvailableOvertimeHours { get; set; }
        public decimal TotalAvailableHours => AvailableNormalHours + AvailableOvertimeHours;
        public decimal ShortageOrSurplus => TotalAvailableHours - RequiredHours;
        public bool IsCapacityMet => ShortageOrSurplus >= 0;
        public string Recommendation { get; set; }

        // Manpower Headcount Summary
        public List<ProcessManpowerResult> ManpowerResults { get; set; } = new List<ProcessManpowerResult>();
        public int TotalHeadcountZeroOvertime { get; set; }
        public int TotalHeadcountWithOvertime { get; set; }

        // Detailed breakdown per machine/bottleneck
        public List<MachineCapacityResult> MachineResults { get; set; } = new List<MachineCapacityResult>();
        
        // List of Bottleneck machines
        public List<MachineCapacityResult> Bottlenecks { get; set; } = new List<MachineCapacityResult>();
    }
}
