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
        public int InputDemandUnits { get; set; }
        public string DemandPeriod { get; set; } = "Weekly";
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
        public string DemandPeriod { get; set; } = "Weekly"; // "Weekly" or "Yearly"
    }

    public class SimulationInput
    {
        public List<SimulationDemand> Demands { get; set; } = new List<SimulationDemand>();
        public decimal OvertimeLimitPercent { get; set; }
        public string ShiftSystem { get; set; } = "5-2"; // 5-2 or 6-1
        public int ShiftsPerDay { get; set; } = 3;
        public int WorkingWeeksPerYear { get; set; } = 52;
        public string YearlyBreakdownMode { get; set; } = "CapacityFill"; // "CapacityFill" or "Levelized"

        public int EffectiveWeeksPerYear => WorkingWeeksPerYear >= 1 && WorkingWeeksPerYear <= 53 ? WorkingWeeksPerYear : 52;
        public bool HasYearlyDemand => Demands != null && Demands.Any(d => d.DemandQuantity > 0 && string.Equals(d.DemandPeriod, "Yearly", System.StringComparison.OrdinalIgnoreCase));
        
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

    public class WeeklyDemandBreakdown
    {
        public int WeekNumber { get; set; }
        public long PlannedDemandUnits { get; set; }
        public long NormalCapacityUnits { get; set; }
        public long OvertimeCapacityUnits { get; set; }
        public long TotalScheduledUnits => NormalCapacityUnits + OvertimeCapacityUnits;
        public long ShortageUnits { get; set; }
        public long CumulativeUnits { get; set; }
        public long RemainingYearlyDemand { get; set; }
        public decimal BottleneckHoursUsed { get; set; }
        public decimal BottleneckNormalHours { get; set; }
        public decimal BottleneckTotalHours { get; set; }
        public decimal UtilizationPercent => BottleneckTotalHours > 0 ? (BottleneckHoursUsed / BottleneckTotalHours) * 100m : 0m;
        public int RequiredHeadcount { get; set; }
        public string Status { get; set; } = "Normal"; // "Normal", "Overtime", "Shortage", "Completed"
    }

    public class SimulationResult
    {
        // Global aggregate
        public decimal RequiredHours { get; set; }
        public decimal AvailableNormalHours { get; set; }
        public decimal AvailableOvertimeHours { get; set; }
        public decimal TotalAvailableHours => AvailableNormalHours + AvailableOvertimeHours;
        public decimal ShortageOrSurplus => TotalAvailableHours - RequiredHours;
        public bool IsCapacityMet => ShortageOrSurplus >= 0 && Bottlenecks.Count == 0 && TotalYearlyShortageUnits == 0;
        public string Recommendation { get; set; } = string.Empty;

        // Yearly-to-Weekly capacity breakdown
        public bool HasYearlyDemand { get; set; }
        public int WorkingWeeksPerYear { get; set; } = 52;
        public string YearlyBreakdownMode { get; set; } = "CapacityFill";
        public string BottleneckConstraintName { get; set; } = string.Empty;
        public long TotalYearlyDemandUnits { get; set; }
        public long WeeklyNormalCapacityUnits { get; set; }
        public long WeeklyOvertimeCapacityUnits { get; set; }
        public long WeeklyMaxCapacityUnits => WeeklyNormalCapacityUnits + WeeklyOvertimeCapacityUnits;
        public long TotalYearlyDeliverableUnits { get; set; }
        public long TotalYearlyShortageUnits { get; set; }
        public int ActiveProductionWeeks { get; set; }
        public int? CompletionWeek { get; set; }
        public decimal RequiredWeeksAtMaxCapacity { get; set; }
        public List<WeeklyDemandBreakdown> WeeklyBreakdowns { get; set; } = new List<WeeklyDemandBreakdown>();

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

    // =========================================================================
    // MULTI-DEVICE CAPACITY PLANNING & MACHINE MATRIX SIMULATOR MODELS
    // =========================================================================

    public class DeviceDemandInput
    {
        public string DeviceCode { get; set; } = string.Empty;
        public string DeviceName { get; set; } = string.Empty;
        public int StepCount { get; set; }
        public int DemandQuantity { get; set; } = 10000;
        public string DemandPeriod { get; set; } = "Weekly"; // "Weekly" or "Yearly"
    }

    public class DeviceCapacitySimulationInput
    {
        public List<DeviceDemandInput> Devices { get; set; } = new List<DeviceDemandInput>();
        public string ShiftSystem { get; set; } = "5-2"; // "5-2" or "6-1"
        public int ShiftsPerDay { get; set; } = 3; // 1, 2, 3
        public decimal OvertimeLimitPercent { get; set; } = 10m; // 0 - 50%
        public int WorkingWeeksPerYear { get; set; } = 52;

        public int EffectiveWeeksPerYear => WorkingWeeksPerYear >= 1 && WorkingWeeksPerYear <= 53 ? WorkingWeeksPerYear : 52;
        public int WorkingDays => ShiftSystem == "6-1" ? 6 : 5;
        public decimal NormalHoursPerShift => 7.25m;

        public decimal AvailableHoursPerPersonZeroOt => WorkingDays * NormalHoursPerShift;
        public decimal AvailableHoursPerPersonWithOt => AvailableHoursPerPersonZeroOt * (1m + (OvertimeLimitPercent / 100m));

        public decimal AvailableMachineHoursNormal => WorkingDays * ShiftsPerDay * NormalHoursPerShift;
        public decimal AvailableMachineHoursWithOt => AvailableMachineHoursNormal * (1m + (OvertimeLimitPercent / 100m));
    }

    public class DeviceMachineLoadItem
    {
        public string WorkCenter { get; set; } = string.Empty;
        public string MachineName { get; set; } = string.Empty;
        public string Area { get; set; } = "PROD";
        public decimal RequiredHours { get; set; }
        public decimal AvailableNormalHours { get; set; }
        public decimal AvailableWithOvertimeHours { get; set; }
        public decimal ShortageOrSurplus => AvailableWithOvertimeHours - RequiredHours;
        public decimal UtilizationPercent => AvailableWithOvertimeHours > 0 ? (RequiredHours / AvailableWithOvertimeHours) * 100m : 0m;
        public bool IsBottleneck => RequiredHours > AvailableWithOvertimeHours;
        public bool NeedsOvertime => RequiredHours > AvailableNormalHours && RequiredHours <= AvailableWithOvertimeHours;
        public int RecommendedMachines => AvailableWithOvertimeHours > 0 ? (int)Math.Ceiling(RequiredHours / AvailableWithOvertimeHours) : 1;
        public string Status => IsBottleneck ? "BOTTLENECK" : (NeedsOvertime ? "OT NEEDED" : "OK");
        public List<string> ContributingDevices { get; set; } = new List<string>();
        public Dictionary<string, decimal> DeviceHoursBreakdown { get; set; } = new Dictionary<string, decimal>();
    }

    public class DeviceLaborLoadItem
    {
        public string ProcessName { get; set; } = string.Empty;
        public string WorkCenter { get; set; } = string.Empty;
        public decimal TotalLaborHours { get; set; }
        public int HeadcountZeroOt { get; set; }
        public int HeadcountWithOt { get; set; }
        public decimal PercentOfTotalLabor { get; set; }
        public List<string> ContributingDevices { get; set; } = new List<string>();
    }

    public class DeviceRoutingStepItem
    {
        public string DeviceCode { get; set; } = string.Empty;
        public string DeviceName { get; set; } = string.Empty;
        public int Seq { get; set; }
        public string Description { get; set; } = string.Empty;
        public string WorkCenter { get; set; } = string.Empty;
        public decimal MachineHour { get; set; }
        public decimal LaborHour { get; set; }
        public decimal DemandUnits { get; set; }
        public int InputDemand { get; set; }
        public string Period { get; set; } = "Weekly";
        public decimal RequiredMachineHours { get; set; }
        public decimal RequiredLaborHours { get; set; }
    }

    public class DeviceCapacityAnalysisResult
    {
        // Parameters used
        public string ShiftSystem { get; set; } = "5-2";
        public int ShiftsPerDay { get; set; } = 3;
        public decimal OvertimeLimitPercent { get; set; } = 10m;
        public int WorkingWeeksPerYear { get; set; } = 52;
        public decimal AvailableHoursPerPersonZeroOt { get; set; }
        public decimal AvailableHoursPerPersonWithOt { get; set; }
        public decimal AvailableMachineHoursNormal { get; set; }
        public decimal AvailableMachineHoursWithOt { get; set; }

        // Total demands
        public int TotalDevicesCount { get; set; }
        public decimal TotalWeeklyUnits { get; set; }

        // Machine Capacity Section
        public decimal TotalRequiredMachineHours { get; set; }
        public decimal TotalAvailableMachineHours { get; set; }
        public bool IsMachineCapacityMet { get; set; }
        public int BottleneckMachineCount { get; set; }
        public List<DeviceMachineLoadItem> MachineLoads { get; set; } = new List<DeviceMachineLoadItem>();
        public List<DeviceMachineLoadItem> Bottlenecks { get; set; } = new List<DeviceMachineLoadItem>();

        // Labor Capacity Section
        public decimal TotalRequiredLaborHours { get; set; }
        public int TotalHeadcountZeroOt { get; set; }
        public int TotalHeadcountWithOt { get; set; }
        public int HeadcountSavedByOt => Math.Max(0, TotalHeadcountZeroOt - TotalHeadcountWithOt);
        public List<DeviceLaborLoadItem> LaborLoads { get; set; } = new List<DeviceLaborLoadItem>();

        // Detailed step routing
        public List<DeviceRoutingStepItem> RoutingSteps { get; set; } = new List<DeviceRoutingStepItem>();
    }

    // =========================================================================
    // LIVE CAPACITY PLANNING MODELS
    // =========================================================================

    public class LiveCapacityFilterInput
    {
        public string Timeframe { get; set; } = "Last7Days"; // "Today", "Yesterday", "Last7Days", "Last30Days", "All"
        public string Division { get; set; } = "All"; // "All", "MLS", "OPTIC", "AIS"
        public string ShiftSystem { get; set; } = "5-2"; // "5-2" (5 days) or "6-1" (6 days)
        public int ShiftsPerDay { get; set; } = 3; // 1, 2, 3
        public decimal OvertimeLimitPercent { get; set; } = 10m; // 0 - 50%
        public string MetricSource { get; set; } = "QtyIn"; // "QtyIn" or "QtyOut"

        public int WorkingDays => ShiftSystem == "6-1" ? 6 : 5;
        public decimal NormalHoursPerShift => 7.25m;
        public decimal AvailableHoursPerPersonZeroOt => WorkingDays * NormalHoursPerShift;
        public decimal AvailableHoursPerPersonWithOt => AvailableHoursPerPersonZeroOt * (1m + (OvertimeLimitPercent / 100m));
        public decimal AvailableMachineHoursNormal => WorkingDays * ShiftsPerDay * NormalHoursPerShift;
        public decimal AvailableMachineHoursWithOt => AvailableMachineHoursNormal * (1m + (OvertimeLimitPercent / 100m));
    }

    public class LiveDeviceProductionItem
    {
        public string DeviceCode { get; set; } = string.Empty;
        public string DeviceName { get; set; } = string.Empty;
        public string Division { get; set; } = string.Empty;
        public decimal TotalQtyIn { get; set; }
        public decimal TotalQtyOut { get; set; }
        public decimal FirstPassYield { get; set; }
        public decimal OverallYield { get; set; }
        public System.DateTime? LatestDate { get; set; }
        public int ProductionDays { get; set; }
        public decimal WeeklyRunRate { get; set; }
        public int StepCount { get; set; }
        public decimal RequiredMachineHours { get; set; }
        public decimal RequiredLaborHours { get; set; }
        public bool HasRouting { get; set; }
    }

    public class LiveCapacityResult
    {
        public LiveCapacityFilterInput Filter { get; set; } = new LiveCapacityFilterInput();
        public System.DateTime EvaluatedAt { get; set; } = System.DateTime.Now;

        // High Level Metrics
        public int TotalActiveDevices { get; set; }
        public decimal TotalActualUnits { get; set; }
        public decimal TotalWeeklyRunRateUnits { get; set; }
        public decimal AverageFirstPassYield { get; set; }
        public decimal AverageOverallYield { get; set; }

        // Live Machine Capacity
        public decimal TotalRequiredMachineHours { get; set; }
        public decimal TotalAvailableMachineHours { get; set; }
        public decimal MachineUtilizationPercent => TotalAvailableMachineHours > 0 ? (TotalRequiredMachineHours / TotalAvailableMachineHours) * 100m : 0m;
        public int TotalMachineWorkCenters { get; set; }
        public int BottleneckCount => BottleneckMachines != null ? BottleneckMachines.Count : 0;
        public bool IsMachineCapacityMet => BottleneckCount == 0;
        public List<DeviceMachineLoadItem> MachineLoads { get; set; } = new List<DeviceMachineLoadItem>();
        public List<DeviceMachineLoadItem> BottleneckMachines { get; set; } = new List<DeviceMachineLoadItem>();

        // Live Labor Capacity
        public decimal TotalRequiredLaborHours { get; set; }
        public int RequiredHeadcountZeroOt { get; set; }
        public int RequiredHeadcountWithOt { get; set; }
        public int ActiveEmployeesAvailable { get; set; }
        public int LaborSurplusOrDeficit => ActiveEmployeesAvailable - RequiredHeadcountWithOt;
        public bool IsLaborCapacityMet => LaborSurplusOrDeficit >= 0;
        public decimal LaborUtilizationPercent => ActiveEmployeesAvailable > 0 ? ((decimal)RequiredHeadcountWithOt / ActiveEmployeesAvailable) * 100m : 0m;
        public List<DeviceLaborLoadItem> LaborLoads { get; set; } = new List<DeviceLaborLoadItem>();

        // Overall Alert Status
        public bool IsOverallCapacitySufficient => IsMachineCapacityMet && IsLaborCapacityMet;
        private string? _customAlertSeverity;
        public string AlertSeverity
        {
            get => _customAlertSeverity ?? ((!IsMachineCapacityMet && !IsLaborCapacityMet) ? "CRITICAL_DEFICIT" :
                   (!IsMachineCapacityMet) ? "MACHINE_DEFICIT" :
                   (!IsLaborCapacityMet) ? "LABOR_DEFICIT" :
                   (MachineLoads.Any(m => m.NeedsOvertime) || RequiredHeadcountZeroOt > ActiveEmployeesAvailable) ? "WARNING_OVERTIME" :
                   "OPTIMAL");
            set => _customAlertSeverity = value;
        }

        public string AlertTitle { get; set; } = string.Empty;
        public string AlertMessage { get; set; } = string.Empty;
        public List<string> AlertPoints { get; set; } = new List<string>();

        // Active Devices List
        public List<LiveDeviceProductionItem> ActiveDevices { get; set; } = new List<LiveDeviceProductionItem>();
    }
}
