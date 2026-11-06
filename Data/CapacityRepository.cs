using Dapper;
using LssTraining.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LssTraining.Web.Data
{
    public class CapacityRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public CapacityRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<CapacitySummary> GetSummaryAsync()
        {
            using var connection = _connectionFactory.CreateConnection();
            var summary = new CapacitySummary();
            
            try
            {
                // Fetch totals from each master table (prioritizing CI_SQ01)
                summary.TotalDevices = await connection.ExecuteScalarAsync<int>("SELECT COUNT(DISTINCT DeviceCode) FROM CI_SQ01");
                summary.TotalProcesses = await connection.ExecuteScalarAsync<int>("SELECT COUNT(1) FROM CI_SQ01");
                summary.TotalEmployees = await connection.ExecuteScalarAsync<int>("SELECT COUNT(1) FROM CI_Employee");
            }
            catch 
            {
                // Fallback if CI_SQ01 table has not been created yet
                try
                {
                    summary.TotalDevices = await connection.ExecuteScalarAsync<int>("SELECT COUNT(1) FROM CI_Product");
                    summary.TotalProcesses = await connection.ExecuteScalarAsync<int>("SELECT COUNT(1) FROM CI_Process");
                    summary.TotalEmployees = await connection.ExecuteScalarAsync<int>("SELECT COUNT(1) FROM CI_Employee");
                }
                catch { }
            }
            return summary;
        }

        public async Task<IEnumerable<ManpowerBySection>> GetManpowerBySectionAsync()
        {
            using var connection = _connectionFactory.CreateConnection();
            try
            {
                var sql = @"
                    SELECT 
                        ISNULL(NULLIF(LTRIM(RTRIM(SECTION_CODE)), ''), 'UNASSIGNED') AS SectionCode, 
                        COUNT(1) AS EmployeeCount 
                    FROM CI_Employee 
                    GROUP BY SECTION_CODE 
                    ORDER BY EmployeeCount DESC";
                return await connection.QueryAsync<ManpowerBySection>(sql);
            }
            catch 
            {
                return new List<ManpowerBySection>();
            }
        }

        public async Task<IEnumerable<ProcessArea>> GetProcessAreasAsync()
        {
            using var connection = _connectionFactory.CreateConnection();
            try
            {
                var sql = @"
                    SELECT 
                        ISNULL(NULLIF(LTRIM(RTRIM(Area)), ''), 'UNASSIGNED') AS Area, 
                        COUNT(1) AS ProcessCount 
                    FROM CI_Process 
                    GROUP BY Area 
                    ORDER BY ProcessCount DESC";
                return await connection.QueryAsync<ProcessArea>(sql);
            }
            catch
            {
                return new List<ProcessArea>();
            }
        }

        public async Task<IEnumerable<DeviceGroup>> GetDeviceGroupsAsync()
        {
            using var connection = _connectionFactory.CreateConnection();
            try
            {
                var sql = @"
                    SELECT pg.DeviceGroupCode, pg.DeviceGroupName
                    FROM CI_ProductGroup pg
                    ORDER BY pg.DeviceGroupName";
                return await connection.QueryAsync<DeviceGroup>(sql);
            }
            catch
            {
                return new List<DeviceGroup> { new DeviceGroup { DeviceGroupCode = "DEMO", DeviceGroupName = "Demo Product" } };
            }
        }

        public async Task<IEnumerable<ProductDeviceItem>> GetDevicesAsync(string? deviceGroupCode = null)
        {
            using var connection = _connectionFactory.CreateConnection();
            try
            {
                var sql = @"
                    SELECT 
                        p.DeviceCode,
                        ISNULL(p.DeviceName, p.DeviceCode) AS DeviceName,
                        p.DeviceGroupCode,
                        ISNULL(pg.DeviceGroupName, p.DeviceGroupCode) AS DeviceGroupName,
                        COUNT(sq.Seq) AS StepCount
                    FROM CI_Product p
                    LEFT JOIN CI_ProductGroup pg ON p.DeviceGroupCode = pg.DeviceGroupCode
                    LEFT JOIN CI_SQ01 sq ON p.DeviceCode = sq.DeviceCode
                    WHERE (@DeviceGroupCode IS NULL OR @DeviceGroupCode = '' OR p.DeviceGroupCode = @DeviceGroupCode)
                    GROUP BY p.DeviceCode, p.DeviceName, p.DeviceGroupCode, pg.DeviceGroupName
                    HAVING COUNT(sq.Seq) > 0
                    ORDER BY p.DeviceGroupCode, p.DeviceCode";
                return await connection.QueryAsync<ProductDeviceItem>(sql, new { DeviceGroupCode = deviceGroupCode });
            }
            catch
            {
                return new List<ProductDeviceItem>();
            }
        }

        public async Task<SimulationResult> RunSimulationAsync(SimulationInput input, int totalEmployees)
        {
            using var connection = _connectionFactory.CreateConnection();
            var result = new SimulationResult();

            int weeksPerYear = input.EffectiveWeeksPerYear;
            string breakdownMode = string.Equals(input.YearlyBreakdownMode, "Levelized", StringComparison.OrdinalIgnoreCase)
                ? "Levelized"
                : "CapacityFill";
            result.WorkingWeeksPerYear = weeksPerYear;
            result.YearlyBreakdownMode = breakdownMode;
            
            var machineRequiredHours = new Dictionary<string, decimal>();
            var processLaborHours = new Dictionary<string, decimal>();
            var processNames = new Dictionary<string, string>();

            // Tracking for Yearly -> Weekly capacity breakdown
            long totalYearlyUnits = 0L;
            var fixedWeeklyMachineHours = new Dictionary<string, decimal>();
            var yearlyDemandMachineHoursTotal = new Dictionary<string, decimal>();
            var fixedWeeklyProcessLabor = new Dictionary<string, decimal>();
            var yearlyProcessLaborTotal = new Dictionary<string, decimal>();

            // 1. Preload master cycle times & machines mapping
            var avgProcessUph = new Dictionary<string, decimal>();
            var processDefaultMachine = new Dictionary<string, string>();
            var machineNames = new Dictionary<string, (string Name, string Area)>();

            try
            {
                var uphRows = await connection.QueryAsync<dynamic>(@"
                    SELECT ProcCode, AVG(UPH) AS AvgUPH, MAX(MachCode) AS SampleMach 
                    FROM CI_IECycleTime_Header 
                    WHERE UPH > 0 
                    GROUP BY ProcCode");
                foreach (var r in uphRows)
                {
                    string p = (string)r.ProcCode;
                    if (!string.IsNullOrEmpty(p))
                    {
                        avgProcessUph[p] = (decimal)r.AvgUPH;
                        if (r.SampleMach != null)
                            processDefaultMachine[p] = (string)r.SampleMach;
                    }
                }

                var machRows = await connection.QueryAsync<dynamic>("SELECT MachCode, MachName, ISNULL(Area, 'PROD') AS Area FROM MasterMachines");
                foreach (var m in machRows)
                {
                    string mc = (string)m.MachCode;
                    if (!string.IsNullOrEmpty(mc))
                    {
                        machineNames[mc] = ((string)(m.MachName ?? mc), (string)m.Area);
                    }
                }
            }
            catch { }

            // 2. Process each demand input
            foreach (var demandInput in input.Demands)
            {
                if (demandInput.DemandQuantity <= 0)
                    continue;

                // Determine target device
                string targetDevice = demandInput.DeviceCode ?? string.Empty;
                string targetGroup = demandInput.DeviceGroupCode ?? string.Empty;

                if (string.IsNullOrWhiteSpace(targetDevice))
                {
                    // If no specific device selected, find the primary master sequence device for the product family
                    try
                    {
                        var sqlFindMaster = @"
                            SELECT TOP 1 p.DeviceCode
                            FROM CI_Product p
                            JOIN CI_SQ01 sq ON p.DeviceCode = sq.DeviceCode
                            WHERE (@DeviceGroupCode IS NULL OR @DeviceGroupCode = '' OR p.DeviceGroupCode = @DeviceGroupCode)
                            GROUP BY p.DeviceCode
                            ORDER BY COUNT(sq.Seq) DESC, p.DeviceCode";
                        targetDevice = await connection.ExecuteScalarAsync<string>(sqlFindMaster, new { DeviceGroupCode = targetGroup }) ?? string.Empty;
                    }
                    catch { }
                }

                if (string.IsNullOrWhiteSpace(targetDevice))
                {
                    // Fallback to demo device
                    targetDevice = !string.IsNullOrWhiteSpace(targetGroup) ? targetGroup + "-01" : "DEMO-DEVICE";
                }

                // Query the exact process sequence ordered by Seq ASC from dbo.CI_SQ01
                List<dynamic> routingSteps = new List<dynamic>();
                try
                {
                    var sqlRouting = @"
                        SELECT 
                            sq.DeviceCode,
                            ISNULL(p.DeviceName, sq.DeviceCode) AS DeviceName,
                            ISNULL(p.DeviceGroupCode, @FallbackGroup) AS DeviceGroupCode,
                            sq.Seq,
                            CAST(sq.Seq AS NVARCHAR(50)) AS ProcCode,
                            ISNULL(sq.Description, 'Seq ' + CAST(sq.Seq AS NVARCHAR(10))) AS ProcName,
                            'PROD' AS ProcessArea,
                            ISNULL(sq.LaborHour, 0) AS LaborHour,
                            ISNULL(sq.MachineHour, 0) AS MachineHour
                        FROM CI_SQ01 sq
                        LEFT JOIN CI_Product p ON sq.DeviceCode = p.DeviceCode
                        WHERE sq.DeviceCode = @DeviceCode
                        ORDER BY sq.Seq ASC";

                    routingSteps = (await connection.QueryAsync<dynamic>(sqlRouting, new { DeviceCode = targetDevice, FallbackGroup = targetGroup })).AsList();

                    // If still no routing, inherit from the primary master sequence device in the same family
                    if (routingSteps.Count == 0 && !string.IsNullOrWhiteSpace(targetGroup))
                    {
                        var sqlGroupRouting = @"
                            SELECT 
                                sq.DeviceCode,
                                ISNULL(p.DeviceName, sq.DeviceCode) AS DeviceName,
                                ISNULL(p.DeviceGroupCode, @DeviceGroupCode) AS DeviceGroupCode,
                                sq.Seq,
                                CAST(sq.Seq AS NVARCHAR(50)) AS ProcCode,
                                ISNULL(sq.Description, 'Seq ' + CAST(sq.Seq AS NVARCHAR(10))) AS ProcName,
                                'PROD' AS ProcessArea,
                                ISNULL(sq.LaborHour, 0) AS LaborHour,
                                ISNULL(sq.MachineHour, 0) AS MachineHour
                            FROM CI_SQ01 sq
                            LEFT JOIN CI_Product p ON sq.DeviceCode = p.DeviceCode
                            WHERE sq.DeviceCode = (
                                SELECT TOP 1 p2.DeviceCode
                                FROM CI_Product p2
                                JOIN CI_SQ01 sq2 ON p2.DeviceCode = sq2.DeviceCode
                                WHERE p2.DeviceGroupCode = @DeviceGroupCode
                                GROUP BY p2.DeviceCode
                                ORDER BY COUNT(sq2.Seq) DESC, p2.DeviceCode
                            )
                            ORDER BY sq.Seq ASC";
                        routingSteps = (await connection.QueryAsync<dynamic>(sqlGroupRouting, new { DeviceGroupCode = targetGroup })).AsList();
                    }
                }
                catch { }

                if (routingSteps.Count == 0)
                {
                    // Fallback mock routing if database sequence is completely empty
                    routingSteps = new List<dynamic>
                    {
                        new { DeviceCode = targetDevice, DeviceName = "Demo " + targetDevice, DeviceGroupCode = targetGroup, Seq = 5, ProcCode = "LOAD", ProcName = "LOADING PO", ProcessArea = "FL-PREP", LaborHour = 1.0, MachineHour = 0.0 },
                        new { DeviceCode = targetDevice, DeviceName = "Demo " + targetDevice, DeviceGroupCode = targetGroup, Seq = 10, ProcCode = "AA", ProcName = "ASIC ATTACH", ProcessArea = "FL-TPS", LaborHour = 1.0, MachineHour = 0.00048 },
                        new { DeviceCode = targetDevice, DeviceName = "Demo " + targetDevice, DeviceGroupCode = targetGroup, Seq = 30, ProcCode = "WBT", ProcName = "WIRE BOND TPS", ProcessArea = "FL-TPS", LaborHour = 1.0, MachineHour = 0.00067 },
                        new { DeviceCode = targetDevice, DeviceName = "Demo " + targetDevice, DeviceGroupCode = targetGroup, Seq = 50, ProcCode = "PWT", ProcName = "PREWELD TPS", ProcessArea = "FL-TPS", LaborHour = 1.0, MachineHour = 0.00028 },
                        new { DeviceCode = targetDevice, DeviceName = "Demo " + targetDevice, DeviceGroupCode = targetGroup, Seq = 60, ProcCode = "WDT", ProcName = "WELDING TPS", ProcessArea = "FL-TPS", LaborHour = 1.0, MachineHour = 0.00057 },
                        new { DeviceCode = targetDevice, DeviceName = "Demo " + targetDevice, DeviceGroupCode = targetGroup, Seq = 80, ProcCode = "LTT", ProcName = "LEAK TEST TPS", ProcessArea = "BL-TPS", LaborHour = 1.0, MachineHour = 0.00022 },
                        new { DeviceCode = targetDevice, DeviceName = "Demo " + targetDevice, DeviceGroupCode = targetGroup, Seq = 120, ProcCode = "PKS", ProcName = "PACKING TPS SENSOR", ProcessArea = "BL-TPS", LaborHour = 1.0, MachineHour = 0.0 }
                    };
                }

                result.TotalDevicesSimulated++;

                int demandQty = demandInput.DemandQuantity;
                bool isYearlyDemand = string.Equals(demandInput.DemandPeriod, "Yearly", StringComparison.OrdinalIgnoreCase);
                string normalizedPeriod = isYearlyDemand ? "Yearly" : "Weekly";
                decimal weeklyEquivalentUnits = isYearlyDemand
                    ? Math.Round((decimal)demandQty / weeksPerYear, 2)
                    : demandQty;
                decimal exactWeeklyUnits = isYearlyDemand
                    ? (decimal)demandQty / weeksPerYear
                    : demandQty;

                if (isYearlyDemand)
                {
                    totalYearlyUnits += demandQty;
                }

                foreach (var step in routingSteps)
                {
                    int seq = (int)step.Seq;
                    string procCode = "SEQ-" + seq;
                    string procName = (string)(step.ProcName ?? ("Process Step " + seq));
                    string procArea = (string)(step.ProcessArea ?? "PROD");

                    decimal machineHourPerPcs = step.MachineHour != null ? Convert.ToDecimal(step.MachineHour) : 0m;
                    decimal laborHourPerPcs = step.LaborHour != null ? Convert.ToDecimal(step.LaborHour) : 0m;

                    bool isMachineStep = machineHourPerPcs > 0m;
                    decimal reqMachHours = machineHourPerPcs * exactWeeklyUnits;
                    decimal reqLaborHours = laborHourPerPcs * exactWeeklyUnits;

                    decimal stepUph = machineHourPerPcs > 0m 
                        ? Math.Round(1.0m / machineHourPerPcs, 1) 
                        : (laborHourPerPcs > 0m ? Math.Round(1.0m / laborHourPerPcs, 1) : 0m);

                    string machCode;
                    string machName;

                    if (isMachineStep)
                    {
                        machCode = "MC-SEQ" + seq;
                        machName = procName + " (Machine)";

                        if (!machineNames.ContainsKey(machCode))
                            machineNames[machCode] = (machName, procArea);

                        if (!machineRequiredHours.ContainsKey(machCode))
                            machineRequiredHours[machCode] = 0m;
                        machineRequiredHours[machCode] += reqMachHours;

                        if (isYearlyDemand)
                        {
                            yearlyDemandMachineHoursTotal[machCode] = yearlyDemandMachineHoursTotal.GetValueOrDefault(machCode, 0m) + (machineHourPerPcs * demandQty);
                        }
                        else
                        {
                            fixedWeeklyMachineHours[machCode] = fixedWeeklyMachineHours.GetValueOrDefault(machCode, 0m) + (machineHourPerPcs * demandQty);
                        }
                    }
                    else
                    {
                        machCode = "MANUAL-SEQ" + seq;
                        machName = "Manual Station: " + procName;
                    }

                    // Accumulate process labor hours
                    string processKey = procName;
                    if (!processLaborHours.ContainsKey(processKey))
                    {
                        processLaborHours[processKey] = 0m;
                        processNames[processKey] = procName;
                    }
                    processLaborHours[processKey] += reqLaborHours;

                    if (isYearlyDemand)
                    {
                        yearlyProcessLaborTotal[processKey] = yearlyProcessLaborTotal.GetValueOrDefault(processKey, 0m) + (laborHourPerPcs * demandQty);
                    }
                    else
                    {
                        fixedWeeklyProcessLabor[processKey] = fixedWeeklyProcessLabor.GetValueOrDefault(processKey, 0m) + (laborHourPerPcs * demandQty);
                    }

                    // Add to detailed routing step breakdown
                    result.RoutingSteps.Add(new ProcessRoutingStep
                    {
                        DeviceCode = (string)(step.DeviceCode ?? targetDevice),
                        DeviceName = (string)(step.DeviceName ?? targetDevice),
                        DeviceGroupCode = (string)(step.DeviceGroupCode ?? targetGroup ?? string.Empty),
                        Seq = seq,
                        ProcCode = procCode,
                        ProcName = procName,
                        Area = procArea,
                        MachCode = machCode,
                        MachName = machName,
                        UPH = stepUph,
                        LaborHour = laborHourPerPcs,
                        LimitReject = 1.0m,
                        DemandUnits = weeklyEquivalentUnits,
                        InputDemandUnits = demandQty,
                        DemandPeriod = normalizedPeriod,
                        MachineHoursRequired = reqMachHours,
                        LaborHoursRequired = reqLaborHours,
                        IsMachineStep = isMachineStep
                    });
                }
            }

            // 3. Compute Machine Capacity Results
            foreach (var kvp in machineRequiredHours)
            {
                string machCode = kvp.Key;
                decimal required = kvp.Value;

                var machResult = new MachineCapacityResult
                {
                    MachCode = machCode,
                    RequiredHours = required
                };

                if (machineNames.ContainsKey(machCode))
                {
                    machResult.MachName = machineNames[machCode].Name;
                    machResult.Area = machineNames[machCode].Area;
                }
                else
                {
                    machResult.MachName = machCode;
                    machResult.Area = machCode.Contains("-") ? machCode.Split('-')[1] : "PROD";
                }

                // Available normal hours based on shifts & working days
                machResult.AvailableNormalHours = input.NormalHoursPerShift * input.WorkingDays * input.ShiftsPerDay;
                machResult.AvailableOvertimeHours = machResult.AvailableNormalHours * (input.OvertimeLimitPercent / 100m);

                result.MachineResults.Add(machResult);

                result.RequiredHours += machResult.RequiredHours;
                result.AvailableNormalHours += machResult.AvailableNormalHours;
                result.AvailableOvertimeHours += machResult.AvailableOvertimeHours;

                if (!machResult.IsCapacityMet)
                {
                    result.Bottlenecks.Add(machResult);
                }
            }

            // 4. Compute Manpower Headcount Results (Standard 7.25 hours/shift per operator)
            result.OperatorHoursPerShift = input.NormalHoursPerShift;
            foreach (var kvp in processLaborHours)
            {
                var mp = new ProcessManpowerResult
                {
                    ProcCode = kvp.Key,
                    ProcName = processNames.ContainsKey(kvp.Key) ? processNames[kvp.Key] : kvp.Key,
                    TotalLaborHoursRequired = kvp.Value,
                    AvailableHoursPerPerson = input.AvailableHoursPerPerson
                };

                decimal availablePerPersonZeroOT = input.AvailableHoursPerPerson;
                decimal availablePerPersonWithOT = input.AvailableHoursPerPerson * (1m + input.OvertimeLimitPercent / 100m);

                mp.HeadcountZeroOvertime = mp.TotalLaborHoursRequired > 0 
                    ? (int)System.Math.Ceiling(mp.TotalLaborHoursRequired / availablePerPersonZeroOT) 
                    : 0;

                mp.HeadcountWithOvertime = mp.TotalLaborHoursRequired > 0 
                    ? (int)System.Math.Ceiling(mp.TotalLaborHoursRequired / availablePerPersonWithOT) 
                    : 0;

                result.ManpowerResults.Add(mp);
            }

            result.TotalHeadcountZeroOvertime = result.ManpowerResults.Sum(x => x.HeadcountZeroOvertime);
            result.TotalHeadcountWithOvertime = result.ManpowerResults.Sum(x => x.HeadcountWithOvertime);

            // 5. Compute Weekly Capacity Breakdown when Yearly Demand is present
            if (totalYearlyUnits > 0)
            {
                result.HasYearlyDemand = true;
                result.TotalYearlyDemandUnits = totalYearlyUnits;

                decimal weeklyNormalAvailPerMachine = input.NormalHoursPerShift * input.WorkingDays * input.ShiftsPerDay;
                decimal weeklyOvertimeAvailPerMachine = weeklyNormalAvailPerMachine * (input.OvertimeLimitPercent / 100m);
                decimal weeklyTotalAvailPerMachine = weeklyNormalAvailPerMachine + weeklyOvertimeAvailPerMachine;

                string? constraintMach = null;
                decimal constraintUnitHours = 0m;
                decimal constraintFixedWeeklyHours = 0m;
                long minMaxUnits = long.MaxValue;
                long minNormalUnits = long.MaxValue;

                foreach (var kvp in yearlyDemandMachineHoursTotal)
                {
                    if (kvp.Value <= 0m) continue;
                    decimal unitHours = kvp.Value / totalYearlyUnits;
                    decimal fixedHrs = fixedWeeklyMachineHours.GetValueOrDefault(kvp.Key, 0m);
                    decimal remNormHrs = Math.Max(0m, weeklyNormalAvailPerMachine - fixedHrs);
                    decimal remTotalHrs = Math.Max(0m, weeklyTotalAvailPerMachine - fixedHrs);

                    long normCapUnits = (long)Math.Floor(remNormHrs / unitHours);
                    long maxCapUnits = (long)Math.Floor(remTotalHrs / unitHours);

                    if (maxCapUnits < minMaxUnits || (maxCapUnits == minMaxUnits && normCapUnits < minNormalUnits))
                    {
                        minMaxUnits = maxCapUnits;
                        minNormalUnits = normCapUnits;
                        constraintMach = kvp.Key;
                        constraintUnitHours = unitHours;
                        constraintFixedWeeklyHours = fixedHrs;
                    }
                }

                decimal bottleneckNormalHours = weeklyNormalAvailPerMachine;
                decimal bottleneckTotalHours = weeklyTotalAvailPerMachine;

                if (constraintMach != null)
                {
                    string mName = machineNames.ContainsKey(constraintMach) ? machineNames[constraintMach].Name : constraintMach;
                    result.BottleneckConstraintName = $"{constraintMach} - {mName}";
                    result.WeeklyNormalCapacityUnits = minNormalUnits;
                    result.WeeklyOvertimeCapacityUnits = Math.Max(0L, minMaxUnits - minNormalUnits);
                }
                else
                {
                    decimal totalYearlyLabor = yearlyProcessLaborTotal.Values.Sum();
                    decimal laborPerUnit = totalYearlyLabor / totalYearlyUnits;
                    if (totalEmployees > 0 && laborPerUnit > 0m)
                    {
                        decimal fixedLabor = fixedWeeklyProcessLabor.Values.Sum();
                        bottleneckNormalHours = totalEmployees * input.AvailableHoursPerPerson;
                        bottleneckTotalHours = bottleneckNormalHours * (1m + input.OvertimeLimitPercent / 100m);
                        decimal remNormLabor = Math.Max(0m, bottleneckNormalHours - fixedLabor);
                        decimal remTotalLabor = Math.Max(0m, bottleneckTotalHours - fixedLabor);

                        long normUnits = (long)Math.Floor(remNormLabor / laborPerUnit);
                        long maxUnits = (long)Math.Floor(remTotalLabor / laborPerUnit);
                        result.BottleneckConstraintName = $"Manual Labor ({totalEmployees} Active Operators)";
                        result.WeeklyNormalCapacityUnits = normUnits;
                        result.WeeklyOvertimeCapacityUnits = Math.Max(0L, maxUnits - normUnits);
                        constraintUnitHours = laborPerUnit;
                        constraintFixedWeeklyHours = fixedLabor;
                    }
                    else
                    {
                        long avgWeekly = (totalYearlyUnits + weeksPerYear - 1) / weeksPerYear;
                        result.BottleneckConstraintName = "Manual Station (Unconstrained)";
                        result.WeeklyNormalCapacityUnits = avgWeekly;
                        result.WeeklyOvertimeCapacityUnits = (long)Math.Floor(avgWeekly * (input.OvertimeLimitPercent / 100m));
                        bottleneckNormalHours = input.AvailableHoursPerPerson;
                        bottleneckTotalHours = bottleneckNormalHours * (1m + input.OvertimeLimitPercent / 100m);
                    }
                }

                long cNorm = result.WeeklyNormalCapacityUnits;
                long cOt = result.WeeklyOvertimeCapacityUnits;
                long cMax = cNorm + cOt;

                result.RequiredWeeksAtMaxCapacity = cMax > 0
                    ? Math.Round((decimal)totalYearlyUnits / cMax, 1)
                    : 0m;

                long remaining = totalYearlyUnits;
                long cumulative = 0L;
                long baseLevelTarget = totalYearlyUnits / weeksPerYear;
                long levelRemainder = totalYearlyUnits % weeksPerYear;

                decimal availPersonZeroOT = input.AvailableHoursPerPerson;
                decimal availPersonWithOT = input.AvailableHoursPerPerson * (1m + input.OvertimeLimitPercent / 100m);
                var allProcKeys = fixedWeeklyProcessLabor.Keys.Union(yearlyProcessLaborTotal.Keys).ToList();

                for (int w = 1; w <= weeksPerYear; w++)
                {
                    long plannedTarget;
                    long normalUnits;
                    long otUnits;
                    long shortageUnits = 0L;

                    if (breakdownMode == "Levelized")
                    {
                        plannedTarget = baseLevelTarget + (w <= levelRemainder ? 1L : 0L);
                        normalUnits = Math.Min(plannedTarget, cNorm);
                        otUnits = Math.Min(Math.Max(0L, plannedTarget - normalUnits), cOt);
                        long actual = normalUnits + otUnits;
                        shortageUnits = Math.Max(0L, plannedTarget - actual);
                        cumulative += actual;
                        remaining = Math.Max(0L, totalYearlyUnits - cumulative);
                    }
                    else
                    {
                        long weeksAfter = weeksPerYear - w;
                        long futureNormalCap = weeksAfter * cNorm;
                        normalUnits = Math.Min(remaining, cNorm);
                        long remAfterNorm = remaining - normalUnits;
                        long otNeededNow = Math.Max(0L, remAfterNorm - futureNormalCap);
                        otUnits = Math.Min(otNeededNow, cOt);
                        long actual = normalUnits + otUnits;
                        plannedTarget = remaining > 0 ? Math.Min(remaining, Math.Max(actual, cNorm)) : 0L;
                        remaining -= actual;
                        cumulative += actual;
                        if (w == weeksPerYear && remaining > 0)
                        {
                            shortageUnits = remaining;
                            plannedTarget = actual + remaining;
                        }
                    }

                    long scheduled = normalUnits + otUnits;
                    if (scheduled > 0)
                        result.ActiveProductionWeeks++;
                    if (result.CompletionWeek == null && cumulative >= totalYearlyUnits)
                        result.CompletionWeek = w;

                    decimal hoursUsed = constraintUnitHours > 0m
                        ? constraintFixedWeeklyHours + (scheduled * constraintUnitHours)
                        : 0m;

                    int weekHeadcount = 0;
                    if (scheduled > 0 || fixedWeeklyProcessLabor.Count > 0)
                    {
                        decimal personCap = otUnits > 0 ? availPersonWithOT : availPersonZeroOT;
                        if (personCap > 0m)
                        {
                            foreach (var pk in allProcKeys)
                            {
                                decimal pHrs = fixedWeeklyProcessLabor.GetValueOrDefault(pk, 0m)
                                    + (yearlyProcessLaborTotal.TryGetValue(pk, out var yTot) ? scheduled * (yTot / totalYearlyUnits) : 0m);
                                if (pHrs > 0m)
                                    weekHeadcount += (int)Math.Ceiling(pHrs / personCap);
                            }
                        }
                    }

                    string status = shortageUnits > 0 || (scheduled == 0 && remaining > 0)
                        ? "Shortage"
                        : scheduled == 0
                            ? "Completed"
                            : otUnits > 0
                                ? "Overtime"
                                : "Normal";

                    result.WeeklyBreakdowns.Add(new WeeklyDemandBreakdown
                    {
                        WeekNumber = w,
                        PlannedDemandUnits = plannedTarget,
                        NormalCapacityUnits = normalUnits,
                        OvertimeCapacityUnits = otUnits,
                        ShortageUnits = shortageUnits,
                        CumulativeUnits = cumulative,
                        RemainingYearlyDemand = remaining,
                        BottleneckHoursUsed = hoursUsed,
                        BottleneckNormalHours = bottleneckNormalHours,
                        BottleneckTotalHours = bottleneckTotalHours,
                        RequiredHeadcount = weekHeadcount,
                        Status = status
                    });
                }

                result.TotalYearlyDeliverableUnits = cumulative;
                result.TotalYearlyShortageUnits = Math.Max(0L, totalYearlyUnits - cumulative);
            }

            // 6. Formulation of Actionable Simulation Recommendation
            if (result.IsCapacityMet)
            {
                string headcountInfo = (result.TotalHeadcountZeroOvertime == result.TotalHeadcountWithOvertime || input.OvertimeLimitPercent == 0)
                    ? $"{result.TotalHeadcountZeroOvertime} operators (Standard 7.25h/shift, No Overtime)"
                    : $"{result.TotalHeadcountWithOvertime} operators (with {input.OvertimeLimitPercent}% Overtime) or {result.TotalHeadcountZeroOvertime} operators (Zero OT)";

                if (result.HasYearlyDemand)
                {
                    string finishNote = breakdownMode == "CapacityFill" && result.CompletionWeek.HasValue
                        ? $"Target completed by Week {result.CompletionWeek} of {weeksPerYear} working weeks."
                        : $"Evenly distributed across {weeksPerYear} working weeks.";
                    result.Recommendation = $"✅ Capacity is SUFFICIENT for Yearly Demand ({result.TotalYearlyDemandUnits:N0} units). Weekly Capacity Owned: {result.WeeklyNormalCapacityUnits:N0} units/wk Normal ({result.WeeklyMaxCapacityUnits:N0} units/wk inc. OT). {finishNote} Avg Headcount: {headcountInfo}.";
                }
                else
                {
                    result.Recommendation = $"✅ Capacity is SUFFICIENT across all {result.RoutingSteps.Count} sequence steps. Required Headcount: {headcountInfo}.";
                }
            }
            else
            {
                var topBottlenecks = result.Bottlenecks.Count > 0
                    ? string.Join(", ", result.Bottlenecks.OrderByDescending(b => b.UtilizationPercent).Take(3).Select(b => $"{b.MachCode} ({b.UtilizationPercent:N0}%)"))
                    : result.BottleneckConstraintName;

                if (result.HasYearlyDemand && result.TotalYearlyShortageUnits > 0)
                {
                    string weeksNeeded = result.RequiredWeeksAtMaxCapacity > 0
                        ? $" (requires ~{result.RequiredWeeksAtMaxCapacity:N1} weeks at max capacity)"
                        : "";
                    result.Recommendation = $"⚠️ CAPACITY SHORTAGE for Yearly Demand ({result.TotalYearlyDemandUnits:N0} units): Max deliverable in {weeksPerYear} weeks is {result.TotalYearlyDeliverableUnits:N0} units — Shortage of {result.TotalYearlyShortageUnits:N0} units{weeksNeeded}. Bottleneck: {topBottlenecks}. Avg Headcount: {result.TotalHeadcountZeroOvertime} (No OT) / {result.TotalHeadcountWithOvertime} ({input.OvertimeLimitPercent}% OT).";
                }
                else
                {
                    result.Recommendation = $"⚠️ CAPACITY SHORTAGE detected on {result.Bottlenecks.Count} bottleneck machines: {topBottlenecks}. Required Headcount: {result.TotalHeadcountZeroOvertime} operators (No OT) or {result.TotalHeadcountWithOvertime} operators (with {input.OvertimeLimitPercent}% OT).";
                }
            }

            return result;
        }

        public async Task<List<ProductDeviceItem>> GetAllDevicesQuickListAsync()
        {
            using var connection = _connectionFactory.CreateConnection();
            try
            {
                var sql = @"
                    SELECT 
                        sq.DeviceCode,
                        ISNULL(p.DeviceName, sq.DeviceCode) AS DeviceName,
                        ISNULL(p.DeviceGroupCode, '') AS DeviceGroupCode,
                        COUNT(sq.Seq) AS StepCount
                    FROM CI_SQ01 sq
                    LEFT JOIN CI_Product p ON sq.DeviceCode = p.DeviceCode
                    GROUP BY sq.DeviceCode, p.DeviceName, p.DeviceGroupCode
                    ORDER BY sq.DeviceCode";
                return (await connection.QueryAsync<ProductDeviceItem>(sql)).AsList();
            }
            catch
            {
                return new List<ProductDeviceItem>();
            }
        }

        public async Task<DeviceCapacityAnalysisResult> AnalyzeDeviceCapacityAsync(DeviceCapacitySimulationInput input)
        {
            using var connection = _connectionFactory.CreateConnection();
            var result = new DeviceCapacityAnalysisResult
            {
                ShiftSystem = input.ShiftSystem,
                ShiftsPerDay = input.ShiftsPerDay,
                OvertimeLimitPercent = input.OvertimeLimitPercent,
                WorkingWeeksPerYear = input.EffectiveWeeksPerYear,
                AvailableHoursPerPersonZeroOt = input.AvailableHoursPerPersonZeroOt,
                AvailableHoursPerPersonWithOt = input.AvailableHoursPerPersonWithOt,
                AvailableMachineHoursNormal = input.AvailableMachineHoursNormal,
                AvailableMachineHoursWithOt = input.AvailableMachineHoursWithOt
            };

            var validDevices = input.Devices?
                .Where(d => !string.IsNullOrWhiteSpace(d.DeviceCode) && d.DemandQuantity > 0)
                .ToList() ?? new List<DeviceDemandInput>();

            if (validDevices.Count == 0)
                return result;

            result.TotalDevicesCount = validDevices.Count;

            // Check if WorkCenter column exists in CI_SQ01
            bool hasWorkCenterCol = false;
            try
            {
                hasWorkCenterCol = await connection.ExecuteScalarAsync<int>(
                    "SELECT COUNT(1) FROM sys.columns WHERE object_id = OBJECT_ID('dbo.CI_SQ01') AND name = 'WorkCenter'") > 0;
            }
            catch { }

            var deviceCodes = validDevices.Select(d => d.DeviceCode.Trim()).Distinct().ToList();

            string sql = hasWorkCenterCol
                ? @"SELECT sq.DeviceCode, ISNULL(p.DeviceName, sq.DeviceCode) AS DeviceName, sq.Seq, 
                           sq.Description, sq.MachineHour, sq.LaborHour, sq.WorkCenter
                    FROM CI_SQ01 sq
                    LEFT JOIN CI_Product p ON sq.DeviceCode = p.DeviceCode
                    WHERE sq.DeviceCode IN @DeviceCodes
                    ORDER BY sq.DeviceCode, sq.Seq ASC"
                : @"SELECT sq.DeviceCode, ISNULL(p.DeviceName, sq.DeviceCode) AS DeviceName, sq.Seq, 
                           sq.Description, sq.MachineHour, sq.LaborHour, NULL AS WorkCenter
                    FROM CI_SQ01 sq
                    LEFT JOIN CI_Product p ON sq.DeviceCode = p.DeviceCode
                    WHERE sq.DeviceCode IN @DeviceCodes
                    ORDER BY sq.DeviceCode, sq.Seq ASC";

            List<dynamic> allSteps = new List<dynamic>();
            try
            {
                allSteps = (await connection.QueryAsync<dynamic>(sql, new { DeviceCodes = deviceCodes })).AsList();
            }
            catch { }

            var stepsByDevice = allSteps
                .GroupBy(s => (string)s.DeviceCode)
                .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);

            var machineLoadsDict = new Dictionary<string, DeviceMachineLoadItem>(StringComparer.OrdinalIgnoreCase);
            var laborLoadsDict = new Dictionary<string, DeviceLaborLoadItem>(StringComparer.OrdinalIgnoreCase);

            foreach (var devInput in validDevices)
            {
                string code = devInput.DeviceCode.Trim();
                int demandQty = devInput.DemandQuantity;
                bool isYearly = string.Equals(devInput.DemandPeriod, "Yearly", StringComparison.OrdinalIgnoreCase);
                decimal exactWeeklyUnits = isYearly
                    ? (decimal)demandQty / input.EffectiveWeeksPerYear
                    : demandQty;

                result.TotalWeeklyUnits += exactWeeklyUnits;

                if (!stepsByDevice.TryGetValue(code, out var devSteps) || devSteps.Count == 0)
                {
                    continue;
                }

                foreach (var step in devSteps)
                {
                    int seq = (int)step.Seq;
                    string desc = (string)(step.Description ?? ("Process " + seq));
                    string? rawWc = step.WorkCenter != null ? (string)step.WorkCenter : null;
                    string workCenter = ResolveWorkCenter(rawWc, desc);
                    string machName = ResolveMachineName(workCenter, desc);

                    decimal machHour = step.MachineHour != null ? Convert.ToDecimal(step.MachineHour) : 0m;
                    decimal laborHour = step.LaborHour != null ? Convert.ToDecimal(step.LaborHour) : 0m;

                    decimal stepReqMachHrs = machHour * exactWeeklyUnits;
                    decimal stepReqLaborHrs = laborHour * exactWeeklyUnits;

                    // Record Routing Step
                    result.RoutingSteps.Add(new DeviceRoutingStepItem
                    {
                        DeviceCode = code,
                        DeviceName = (string)(step.DeviceName ?? code),
                        Seq = seq,
                        Description = desc,
                        WorkCenter = workCenter,
                        MachineHour = machHour,
                        LaborHour = laborHour,
                        DemandUnits = exactWeeklyUnits,
                        InputDemand = demandQty,
                        Period = isYearly ? "Yearly" : "Weekly",
                        RequiredMachineHours = stepReqMachHrs,
                        RequiredLaborHours = stepReqLaborHrs
                    });

                    // Machine Matrix aggregation
                    if (machHour > 0m)
                    {
                        if (!machineLoadsDict.TryGetValue(workCenter, out var machItem))
                        {
                            machItem = new DeviceMachineLoadItem
                            {
                                WorkCenter = workCenter,
                                MachineName = machName,
                                AvailableNormalHours = input.AvailableMachineHoursNormal,
                                AvailableWithOvertimeHours = input.AvailableMachineHoursWithOt,
                                Area = "PROD"
                            };
                            machineLoadsDict[workCenter] = machItem;
                        }

                        machItem.RequiredHours += stepReqMachHrs;
                        if (!machItem.ContributingDevices.Contains(code))
                            machItem.ContributingDevices.Add(code);

                        if (!machItem.DeviceHoursBreakdown.ContainsKey(code))
                            machItem.DeviceHoursBreakdown[code] = 0m;
                        machItem.DeviceHoursBreakdown[code] += stepReqMachHrs;
                    }

                    // Labor aggregation
                    if (laborHour > 0m)
                    {
                        string laborKey = workCenter;
                        if (!laborLoadsDict.TryGetValue(laborKey, out var laborItem))
                        {
                            laborItem = new DeviceLaborLoadItem
                            {
                                WorkCenter = workCenter,
                                ProcessName = desc
                            };
                            laborLoadsDict[laborKey] = laborItem;
                        }

                        laborItem.TotalLaborHours += stepReqLaborHrs;
                        if (!laborItem.ContributingDevices.Contains(code))
                            laborItem.ContributingDevices.Add(code);
                    }
                }
            }

            // Summarize Machine Loads
            result.MachineLoads = machineLoadsDict.Values
                .OrderByDescending(m => m.UtilizationPercent)
                .ThenByDescending(m => m.RequiredHours)
                .ToList();

            result.Bottlenecks = result.MachineLoads.Where(m => m.IsBottleneck).ToList();
            result.BottleneckMachineCount = result.Bottlenecks.Count;
            result.IsMachineCapacityMet = result.BottleneckMachineCount == 0;
            result.TotalRequiredMachineHours = result.MachineLoads.Sum(m => m.RequiredHours);
            result.TotalAvailableMachineHours = result.MachineLoads.Count * input.AvailableMachineHoursWithOt;

            // Summarize Labor Loads
            decimal totalLabor = laborLoadsDict.Values.Sum(l => l.TotalLaborHours);
            result.TotalRequiredLaborHours = totalLabor;

            decimal availLabor0Ot = input.AvailableHoursPerPersonZeroOt > 0 ? input.AvailableHoursPerPersonZeroOt : 36.25m;
            decimal availLaborWithOt = input.AvailableHoursPerPersonWithOt > 0 ? input.AvailableHoursPerPersonWithOt : 39.875m;

            result.TotalHeadcountZeroOt = availLabor0Ot > 0 ? (int)Math.Ceiling(totalLabor / availLabor0Ot) : 0;
            result.TotalHeadcountWithOt = availLaborWithOt > 0 ? (int)Math.Ceiling(totalLabor / availLaborWithOt) : 0;

            foreach (var l in laborLoadsDict.Values)
            {
                l.HeadcountZeroOt = availLabor0Ot > 0 ? (int)Math.Ceiling(l.TotalLaborHours / availLabor0Ot) : 0;
                l.HeadcountWithOt = availLaborWithOt > 0 ? (int)Math.Ceiling(l.TotalLaborHours / availLaborWithOt) : 0;
                l.PercentOfTotalLabor = totalLabor > 0 ? (l.TotalLaborHours / totalLabor) * 100m : 0m;
            }

            result.LaborLoads = laborLoadsDict.Values
                .OrderByDescending(l => l.TotalLaborHours)
                .ToList();

            return result;
        }

        private static string ResolveWorkCenter(string? rawWorkCenter, string description)
        {
            if (!string.IsNullOrWhiteSpace(rawWorkCenter))
                return rawWorkCenter.Trim();

            string d = (description ?? string.Empty).ToUpperInvariant();
            if (d.Contains("WIRE BOND")) return "P_WB";
            if (d.Contains("DIE ATTACH") || d.Contains("CHIP ATTACH") || d.Contains("ASIC")) return "P_ASMDA";
            if (d.Contains("PREWELD")) return "P_PWELD";
            if (d.Contains("WELD")) return "P_WELD";
            if (d.Contains("GLASS SAW") || d.Contains("SAWING GLASS")) return "Q_GLSAW";
            if (d.Contains("SAW")) return "R_WFSAW";
            if (d.Contains("LASER") || d.Contains("MARK")) return "T_MARK";
            if (d.Contains("PIN CUT") || d.Contains("CUT PIN")) return "Q_PIN";
            if (d.Contains("PRESSURE CHAMBER")) return "R_PCHAM";
            if (d.Contains("BUBBLE") || d.Contains("LEAK TEST")) return "R_BTEST";
            if (d.Contains("TRIM")) return "T_LTRIM";
            if (d.Contains("FINAL") && d.Contains("TEST")) return "T_FTEST";
            if (d.Contains("TEST")) return "Q_TEST";
            if (d.Contains("CVD")) return "PF_CVD";
            if (d.Contains("SANDBLAST")) return "PF_SBM";
            if (d.Contains("TIN")) return "PF_TINM";
            if (d.Contains("WASH") || d.Contains("CLEAN")) return "PF_PCWSH";
            if (d.Contains("MOUNT")) return "UV_MOUNT";
            if (d.Contains("GLAZE")) return "Q_GLAZE";
            if (d.Contains("INSPECT") || d.Contains("VISUAL") || d.Contains("QA")) return "P_QA";
            if (d.Contains("PACK")) return "Q_PACK";

            return "WC_" + (description?.Length > 15 ? description.Substring(0, 15).Trim() : description?.Trim() ?? "GEN");
        }

        private static string ResolveMachineName(string workCenter, string description)
        {
            string wc = (workCenter ?? string.Empty).ToUpperInvariant();
            if (wc.Contains("WB")) return "Wire Bonder (" + workCenter + ")";
            if (wc.Contains("ASMDA") || wc.Contains("ADA") || wc.Contains("DA")) return "Die Attach Machine (" + workCenter + ")";
            if (wc.Contains("PWELD")) return "Prewelding Station (" + workCenter + ")";
            if (wc.Contains("WELD")) return "Welding Machine (" + workCenter + ")";
            if (wc.Contains("GLSAW") || wc.Contains("WFSAW") || wc.Contains("SAW")) return "Precision Sawing System (" + workCenter + ")";
            if (wc.Contains("MARK")) return "Laser Marker (" + workCenter + ")";
            if (wc.Contains("PIN")) return "Pin Cutting / Polish Machine (" + workCenter + ")";
            if (wc.Contains("PCHAM")) return "Pressure Chamber (" + workCenter + ")";
            if (wc.Contains("BTEST")) return "Bubble / Leak Tester (" + workCenter + ")";
            if (wc.Contains("LTRIM")) return "Laser Trimming System (" + workCenter + ")";
            if (wc.Contains("FTEST") || wc.Contains("TEST")) return "Electrical Test Station (" + workCenter + ")";
            if (wc.Contains("CVD")) return "CVD Coating Chamber (" + workCenter + ")";
            if (wc.Contains("SBM")) return "Sandblasting Machine (" + workCenter + ")";
            if (wc.Contains("TIN")) return "Tinning Station (" + workCenter + ")";
            if (wc.Contains("WSH") || wc.Contains("WASH")) return "Ultrasonic Washing Unit (" + workCenter + ")";
            if (wc.Contains("MOUNT")) return "Mounting Station (" + workCenter + ")";
            
            return description + " (" + workCenter + ")";
        }
    }
}
