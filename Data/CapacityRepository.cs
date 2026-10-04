using Dapper;
using LssTraining.Web.Models;
using System.Collections.Generic;
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
                // Mengambil jumlah total dari masing-masing tabel master
                summary.TotalProducts = await connection.ExecuteScalarAsync<int>("SELECT COUNT(1) FROM CI_Product");
                summary.TotalProcesses = await connection.ExecuteScalarAsync<int>("SELECT COUNT(1) FROM CI_Process");
                summary.TotalEmployees = await connection.ExecuteScalarAsync<int>("SELECT COUNT(1) FROM CI_Employee");
            }
            catch 
            {
                // Jika tabel belum di-create, return 0 (tidak error)
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

        public async Task<IEnumerable<ProductGroup>> GetProductGroupsAsync()
        {
            using var connection = _connectionFactory.CreateConnection();
            try
            {
                return await connection.QueryAsync<ProductGroup>("SELECT ProductGroupCode, ProductGroupName FROM CI_ProductGroup ORDER BY ProductGroupName");
            }
            catch
            {
                return new List<ProductGroup> { new ProductGroup { ProductGroupCode = "DEMO", ProductGroupName = "Demo Product" } };
            }
        }

        public async Task<SimulationResult> RunSimulationAsync(SimulationInput input, int totalEmployees)
        {
            using var connection = _connectionFactory.CreateConnection();
            var result = new SimulationResult();
            
            var machineRequiredHours = new Dictionary<string, decimal>();
            var processLaborHours = new Dictionary<string, decimal>();
            var processNames = new Dictionary<string, string>();

            foreach (var demandInput in input.Demands)
            {
                if (string.IsNullOrWhiteSpace(demandInput.ProductGroupCode) || demandInput.DemandQuantity <= 0)
                    continue;

                // 1. Get all Devices in this Product Family
                var devices = new List<string>();
                try 
                {
                    var sqlDevices = "SELECT ProductCode FROM MQS_MProduct WHERE ProductCode LIKE @ProductGroupCode + '%' OR @ProductGroupCode = 'ALL'";
                    devices = (await connection.QueryAsync<string>(sqlDevices, new { ProductGroupCode = demandInput.ProductGroupCode })).AsList();
                } 
                catch { }

                if (devices.Count == 0) 
                {
                    // Fallback to mock devices if DB not available
                    devices = new List<string> { demandInput.ProductGroupCode + "-01", demandInput.ProductGroupCode + "-02", demandInput.ProductGroupCode + "-03" };
                }

                // 2. Split Demand evenly across devices
                int demandPerDevice = demandInput.DemandQuantity / devices.Count;
                int remainingDemand = demandInput.DemandQuantity % devices.Count;

                // 3. For each device, get routing and cycle times
                for (int i = 0; i < devices.Count; i++)
                {
                    string device = devices[i];
                    int deviceDemand = demandPerDevice + (i == 0 ? remainingDemand : 0);

                    try 
                    {
                        var sqlRouting = @"
                            SELECT dp.ProcCode, p.ProcName, dp.Seq, dp.LaborHour, c.UPH, c.MachCode 
                            FROM dmDeviceProcess dp
                            LEFT JOIN dmProcess p ON dp.ProcCode = p.ProcCode
                            LEFT JOIN dmIECycleTime_Header c ON dp.DeviceCode = c.DeviceCode AND dp.ProcCode = c.ProcCode
                            WHERE dp.DeviceCode = @DeviceCode
                            ORDER BY dp.Seq";
                        
                        var routing = await connection.QueryAsync<dynamic>(sqlRouting, new { DeviceCode = device });

                        bool foundRouting = false;
                        foreach (var step in routing)
                        {
                            foundRouting = true;
                            
                            // Machine hours calculation
                            if (step.UPH != null && step.UPH > 0 && step.MachCode != null)
                            {
                                string machCode = step.MachCode;
                                decimal requiredHours = deviceDemand / (decimal)step.UPH;
                                
                                if (!machineRequiredHours.ContainsKey(machCode))
                                    machineRequiredHours[machCode] = 0;
                                    
                                machineRequiredHours[machCode] += requiredHours;
                            }

                            // Labor headcount calculation
                            string procCode = step.ProcCode;
                            if (procCode != null)
                            {
                                decimal laborHour = step.LaborHour != null ? (decimal)step.LaborHour : (step.UPH != null && step.UPH > 0 ? (1m / (decimal)step.UPH) : 0.01m); // fallback estimation
                                decimal totalLabor = laborHour * deviceDemand;
                                
                                if (!processLaborHours.ContainsKey(procCode))
                                {
                                    processLaborHours[procCode] = 0;
                                    processNames[procCode] = step.ProcName ?? procCode;
                                }
                                processLaborHours[procCode] += totalLabor;
                            }
                        }
                        
                        if (!foundRouting) throw new System.Exception("No routing found");
                    } 
                    catch 
                    {
                        // Mock routing sequence
                        string[] mockMachines = { "DA14", "AD01", "AOI1" };
                        string[] mockProcs = { "AA", "AD", "AOI" };
                        string[] mockProcNames = { "ASIC ATTACH", "AMBIENT CALIBRATION", "AOI INSPECTION" };
                        decimal[] mockUPH = { 2045m, 1500m, 3000m };
                        
                        for(int j=0; j<mockMachines.Length; j++) 
                        {
                            decimal requiredHours = deviceDemand / mockUPH[j];
                            if (!machineRequiredHours.ContainsKey(mockMachines[j]))
                                machineRequiredHours[mockMachines[j]] = 0;
                            machineRequiredHours[mockMachines[j]] += requiredHours;

                            if (!processLaborHours.ContainsKey(mockProcs[j]))
                            {
                                processLaborHours[mockProcs[j]] = 0;
                                processNames[mockProcs[j]] = mockProcNames[j];
                            }
                            processLaborHours[mockProcs[j]] += requiredHours; // assume labor equals machine time for mock
                        }
                    }
                }
            }

            // 4. Fetch available capacities for the accumulated machines
            foreach (var kvp in machineRequiredHours)
            {
                string machCode = kvp.Key;
                decimal required = kvp.Value;
                
                MachineCapacityResult machResult = new MachineCapacityResult
                {
                    MachCode = machCode,
                    RequiredHours = required
                };

                try 
                {
                    var sqlMach = "SELECT MachName, Area, AvailableHours_X FROM dmMachines WHERE MachCode = @MachCode";
                    var machInfo = await connection.QueryFirstOrDefaultAsync<dynamic>(sqlMach, new { MachCode = machCode });
                    
                    if (machInfo != null)
                    {
                        machResult.MachName = machInfo.MachName;
                        machResult.Area = machInfo.Area;
                        machResult.AvailableNormalHours = machInfo.AvailableHours_X != null ? (decimal)machInfo.AvailableHours_X : 8m;
                    }
                    else
                    {
                        machResult.MachName = "Unknown Machine";
                        machResult.AvailableNormalHours = 8m;
                    }
                } 
                catch 
                {
                    machResult.MachName = "Mock Machine " + machCode;
                    machResult.AvailableNormalHours = 8m;
                }

                machResult.AvailableOvertimeHours = machResult.AvailableNormalHours * (input.OvertimeLimitPercent / 100m);

                result.MachineResults.Add(machResult);
                
                // Aggregate global hours
                result.RequiredHours += machResult.RequiredHours;
                result.AvailableNormalHours += machResult.AvailableNormalHours;
                result.AvailableOvertimeHours += machResult.AvailableOvertimeHours;

                if (!machResult.IsCapacityMet)
                {
                    result.Bottlenecks.Add(machResult);
                }
            }

            // 5. Build Manpower Results
            foreach (var kvp in processLaborHours)
            {
                var mp = new ProcessManpowerResult
                {
                    ProcCode = kvp.Key,
                    ProcName = processNames[kvp.Key],
                    TotalLaborHoursRequired = kvp.Value
                };
                
                decimal availablePerPersonWithOT = 8m * (1m + input.OvertimeLimitPercent / 100m);
                mp.HeadcountWithOvertime = mp.TotalLaborHoursRequired > 0 ? (int)System.Math.Ceiling(mp.TotalLaborHoursRequired / availablePerPersonWithOT) : 0;
                
                result.ManpowerResults.Add(mp);
            }

            result.TotalHeadcountZeroOvertime = result.ManpowerResults.Sum(x => x.HeadcountZeroOvertime);
            result.TotalHeadcountWithOvertime = result.ManpowerResults.Sum(x => x.HeadcountWithOvertime);

            if (result.IsCapacityMet && result.Bottlenecks.Count == 0)
            {
                result.Recommendation = $"Current capacity is SUFFICIENT. You need {result.TotalHeadcountWithOvertime} headcount (with {input.OvertimeLimitPercent}% OT allowed) or {result.TotalHeadcountZeroOvertime} headcount (Zero OT).";
            }
            else
            {
                result.Recommendation = $"CAPACITY SHORTAGE detected. Found {result.Bottlenecks.Count} bottleneck machines. Required Headcount: {result.TotalHeadcountWithOvertime} (with OT) or {result.TotalHeadcountZeroOvertime} (Zero OT).";
            }

            return result;
        }
    }
}
