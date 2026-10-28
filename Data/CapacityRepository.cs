using Dapper;
using LssTraining.Web.Models;
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
                // Mengambil jumlah total dari masing-masing tabel master (prioritas CI_SQ01)
                summary.TotalDevices = await connection.ExecuteScalarAsync<int>("SELECT COUNT(DISTINCT DeviceCode) FROM CI_SQ01");
                summary.TotalProcesses = await connection.ExecuteScalarAsync<int>("SELECT COUNT(1) FROM CI_SQ01");
                summary.TotalEmployees = await connection.ExecuteScalarAsync<int>("SELECT COUNT(1) FROM CI_Employee");
            }
            catch 
            {
                // Fallback jika tabel CI_SQ01 belum di-create
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
            
            var machineRequiredHours = new Dictionary<string, decimal>();
            var processLaborHours = new Dictionary<string, decimal>();
            var processNames = new Dictionary<string, string>();

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

                    // If still no routing, inherit from another device in the same family
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
                            FROM CI_Product p
                            JOIN CI_SQ01 sq ON p.DeviceCode = sq.DeviceCode
                            WHERE p.DeviceGroupCode = @DeviceGroupCode
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

                foreach (var step in routingSteps)
                {
                    int seq = (int)step.Seq;
                    string procCode = "SEQ-" + seq;
                    string procName = (string)(step.ProcName ?? ("Process Step " + seq));
                    string procArea = (string)(step.ProcessArea ?? "PROD");

                    decimal machineHourPerPcs = step.MachineHour != null ? (decimal)(double)step.MachineHour : 0m;
                    decimal laborHourPerPcs = step.LaborHour != null ? (decimal)(double)step.LaborHour : 0m;

                    bool isMachineStep = machineHourPerPcs > 0m;
                    decimal reqMachHours = machineHourPerPcs * demandQty;
                    decimal reqLaborHours = laborHourPerPcs * demandQty;

                    decimal stepUph = machineHourPerPcs > 0m 
                        ? Math.Round(1.0m / machineHourPerPcs, 1) 
                        : (laborHourPerPcs > 0m ? Math.Round(1.0m / laborHourPerPcs, 1) : 0m);

                    string machCode;
                    string machName;

                    if (isMachineStep)
                    {
                        machCode = "MC-SEQ" + seq;
                        machName = procName + " (Machine)";

                        if (!machineRequiredHours.ContainsKey(machCode))
                            machineRequiredHours[machCode] = 0m;
                        machineRequiredHours[machCode] += reqMachHours;
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
                        DemandUnits = demandQty,
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

            // 5. Formulation of Actionable Simulation Recommendation
            if (result.IsCapacityMet && result.Bottlenecks.Count == 0)
            {
                if (result.TotalHeadcountZeroOvertime == result.TotalHeadcountWithOvertime || input.OvertimeLimitPercent == 0)
                {
                    result.Recommendation = $"✅ Capacity is SUFFICIENT across all {result.RoutingSteps.Count} sequence steps. Required Headcount: {result.TotalHeadcountZeroOvertime} operators (Standard 7.25h/shift, No Overtime).";
                }
                else
                {
                    result.Recommendation = $"✅ Capacity is SUFFICIENT. Required Headcount: {result.TotalHeadcountWithOvertime} operators (with {input.OvertimeLimitPercent}% Overtime) or {result.TotalHeadcountZeroOvertime} operators (Zero OT).";
                }
            }
            else
            {
                var topBottlenecks = string.Join(", ", result.Bottlenecks.OrderByDescending(b => b.UtilizationPercent).Take(3).Select(b => $"{b.MachCode} ({b.UtilizationPercent:N0}%)"));
                result.Recommendation = $"⚠️ CAPACITY SHORTAGE detected on {result.Bottlenecks.Count} bottleneck machines: {topBottlenecks}. Required Headcount: {result.TotalHeadcountZeroOvertime} operators (No OT) or {result.TotalHeadcountWithOvertime} operators (with {input.OvertimeLimitPercent}% OT).";
            }

            return result;
        }
    }
}
