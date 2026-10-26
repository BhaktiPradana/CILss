namespace LssTraining.Web.Models
{
    using System.Collections.Generic;
    using System.Linq;

    public class CapacitySummary
    {
        public int TotalDevices { get; set; }
        public int TotalProcesses { get; set; }
        public int TotalEmployees { get; set; }
    }

    public class ManpowerBySection
    {
        public string SectionCode { get; set; } = string.Empty;
        public int EmployeeCount { get; set; }
    }

    public class ProcessArea
    {
        public string Area { get; set; } = string.Empty;
        public int ProcessCount { get; set; }
    }

    public class DeviceGroup
    {
        public string DeviceGroupCode { get; set; } = string.Empty;
        public string DeviceGroupName { get; set; } = string.Empty;
    }

    // --- NEW ENTITIES FOR SMART ROUTING SIMULATOR ---

    public class Device
    {
        public string DeviceCode { get; set; } = string.Empty;
        public string DeviceGroupCode { get; set; } = string.Empty;
    }

    public class DeviceProcess
    {
        public string DeviceCode { get; set; } = string.Empty;
        public int Seq { get; set; }
        public string ProcCode { get; set; } = string.Empty;
        public decimal LaborHour { get; set; }
        public decimal LimitReject { get; set; } // Yield
    }


    public class CycleTimeData
    {
        public string DeviceCode { get; set; } = string.Empty;
        public string MachCode { get; set; } = string.Empty;
        public string ProcCode { get; set; } = string.Empty;
        public decimal UPH { get; set; }
        public decimal LaborTime { get; set; }
        public decimal MachineTime { get; set; }
    }

    public class ProductDeviceItem
    {
        public string DeviceCode { get; set; } = string.Empty;
        public string DeviceName { get; set; } = string.Empty;
        public string DeviceGroupCode { get; set; } = string.Empty;
        public string DeviceGroupName { get; set; } = string.Empty;
        public int StepCount { get; set; }
        public string DisplayText => $"{DeviceCode} - {DeviceName} ({DeviceGroupCode}) [{StepCount} steps]";
    }

    public class ProcessRoutingStep
    {
        public string DeviceCode { get; set; } = string.Empty;
        public string DeviceName { get; set; } = string.Empty;
        public string DeviceGroupCode { get; set; } = string.Empty;
        public int Seq { get; set; }
        public string ProcCode { get; set; } = string.Empty;
        public string ProcName { get; set; } = string.Empty;
        public string Area { get; set; } = string.Empty;
        public string MachCode { get; set; } = string.Empty;
        public string MachName { get; set; } = string.Empty;
        public decimal UPH { get; set; }
        public decimal LaborHour { get; set; }
        public decimal LimitReject { get; set; } // Yield
        public decimal DemandUnits { get; set; }
        public decimal MachineHoursRequired { get; set; }
        public decimal LaborHoursRequired { get; set; }
        public bool IsMachineStep { get; set; }
    }

    // --- SIMULATION MODELS ---

    public class SimulationDemand
    {
        public string SelectionType { get; set; } = "Family"; // "Family" or "Device"
        public string? DeviceGroupCode { get; set; }
        public string? DeviceCode { get; set; }
        public int DemandQuantity { get; set; }
    }

    public class SimulationInput
    {
        public List<SimulationDemand> Demands { get; set; } = new List<SimulationDemand>();
        public decimal OvertimeLimitPercent { get; set; }
        public string ShiftSystem { get; set; } = "5-2"; // 5-2 or 6-1
        public int ShiftsPerDay { get; set; } = 3;
        
        public int WorkingDays => ShiftSystem == "6-1" ? 6 : 5;

        /// <summary>
        /// Standard working hours for each operator per shift: exactly 7.25 hours per shift.
        /// </summary>
        public decimal NormalHoursPerShift => 7.25m;

        /// <summary>
        /// Available working hours per operator: WorkingDays * NormalHoursPerShift (e.g. 5 * 7.25 = 36.25 hrs/week).
        /// </summary>
        public decimal AvailableHoursPerPerson => WorkingDays * NormalHoursPerShift;

        /// <summary>
        /// Available machine working hours: WorkingDays * ShiftsPerDay * NormalHoursPerShift (e.g. 5 * 3 * 7.25 = 108.75 hrs/week).
        /// </summary>
        public decimal AvailableMachineHours => WorkingDays * ShiftsPerDay * NormalHoursPerShift;
    }

    public class ProcessManpowerResult
    {
        public string ProcCode { get; set; } = string.Empty;
        public string ProcName { get; set; } = string.Empty;
        public decimal TotalLaborHoursRequired { get; set; }
        public int HeadcountZeroOvertime { get; set; }
        public int HeadcountWithOvertime { get; set; }
        public decimal AvailableHoursPerPerson { get; set; }
    }

    public class MachineCapacityResult
    {
        public string MachCode { get; set; } = string.Empty;
        public string MachName { get; set; } = string.Empty;
        public string Area { get; set; } = string.Empty;
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
        public string Recommendation { get; set; } = string.Empty;

        // Operator shift configuration & labor hours
        public decimal OperatorHoursPerShift { get; set; } = 7.25m;
        public decimal TotalLaborHoursRequired => ManpowerResults != null ? ManpowerResults.Sum(x => x.TotalLaborHoursRequired) : 0;

        // Manpower Headcount Summary
        public List<ProcessManpowerResult> ManpowerResults { get; set; } = new List<ProcessManpowerResult>();
        public int TotalHeadcountZeroOvertime { get; set; }
        public int TotalHeadcountWithOvertime { get; set; }

        // Detailed breakdown per machine/bottleneck
        public List<MachineCapacityResult> MachineResults { get; set; } = new List<MachineCapacityResult>();
        
        // List of Bottleneck machines
        public List<MachineCapacityResult> Bottlenecks { get; set; } = new List<MachineCapacityResult>();

        // Detailed process routing sequence from latest database
        public List<ProcessRoutingStep> RoutingSteps { get; set; } = new List<ProcessRoutingStep>();
        public int TotalDevicesSimulated { get; set; }
        public int TotalStepsSimulated => RoutingSteps != null ? RoutingSteps.Count : 0;
    }
}
