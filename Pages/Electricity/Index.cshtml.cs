using LssTraining.Web.Data;
using LssTraining.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace LssTraining.Web.Pages.Electricity
{
    public class IndexModel : PageModel
    {
        private readonly KwhRepository _kwhRepo;

        public IndexModel(KwhRepository kwhRepo)
        {
            _kwhRepo = kwhRepo;
        }

        [BindProperty(SupportsGet = true)]
        public DateTime StartDate { get; set; } = DateTime.Today.AddDays(-7);

        [BindProperty(SupportsGet = true)]
        public DateTime EndDate { get; set; } = DateTime.Today;

        [BindProperty(SupportsGet = true)]
        public string? SelectedMeter { get; set; }

        [BindProperty(SupportsGet = true)]
        public string ViewMode { get; set; } = "Daily";

        public List<SelectListItem> MeterOptions { get; set; } = new List<SelectListItem>();
        public List<KwhRecord> Records { get; set; } = new List<KwhRecord>();
        public string ChartDataJson { get; set; } = "{}";

        public async Task OnGetAsync()
        {
            var meters = await _kwhRepo.GetMeterNamesAsync();
            MeterOptions = new List<SelectListItem> { new SelectListItem("All Distribution Boards", "") };
            foreach (var m in meters)
            {
                MeterOptions.Add(new SelectListItem(m, m));
            }

            var actualEndDate = ViewMode == "Hourly" ? EndDate.Date.AddDays(1).AddSeconds(-1) : EndDate;
            if (ViewMode == "Hourly")
            {
                Records = await _kwhRepo.GetHourlyDataAsync(StartDate, actualEndDate, SelectedMeter);
            }
            else
            {
                Records = await _kwhRepo.GetDailyDataAsync(StartDate, actualEndDate, SelectedMeter);
            }
            if (Records != null && Records.Any())
            {
                foreach (var rec in Records)
                {
                    rec.Floor = DetermineFloorGroup(rec.MeterName);
                }
                var floorGroups = Records.GroupBy(r => r.Floor).OrderBy(g => g.Key).ToList();
                var chartDataDict = new Dictionary<string, object>();

                foreach (var floorGroup in floorGroups)
                {
                    var timestamps = floorGroup.Select(r => r.Timestamp).Distinct().OrderBy(t => t).ToList();
                    var categories = timestamps.Select(t => ViewMode == "Hourly" ? t.ToString("dd MMM HH:mm") : t.ToString("dd MMM yyyy")).ToList();

                    var seriesList = new List<object>();
                    var dbGroups = floorGroup.GroupBy(r => r.MeterName).ToList();

                    foreach (var db in dbGroups)
                    {
                        var dataPoints = new List<decimal>();
                        var dbDict = db.ToDictionary(r => r.Timestamp, r => r.Usage_kWh);
                        foreach (var ts in timestamps)
                        {
                            dataPoints.Add(dbDict.ContainsKey(ts) ? dbDict[ts] : 0);
                        }
                        var cleanName = db.Key.Contains(".") ? db.Key.Substring(db.Key.IndexOf('.') + 1) : db.Key;
                        seriesList.Add(new { name = cleanName, data = dataPoints });
                    }

                    chartDataDict.Add(floorGroup.Key, new { categories = categories, series = seriesList });
                }
                ChartDataJson = JsonSerializer.Serialize(chartDataDict);
            }
        }

        private string DetermineFloorGroup(string meterName)
        {
            if (string.IsNullOrWhiteSpace(meterName)) return "Other Buildings / Floors";
            string m = meterName.Trim();

            var b207Overall = new[] { "PM DB SSB1T", "PM DB SSB2TA", "PM DB SSB2TB", "PM DB SSB3TA", "PM DB SSB3TB" };
            if (b207Overall.Any(x => m.Equals(x, StringComparison.OrdinalIgnoreCase))) return "Overall Building 207";

            var b209Overall = new[] { "PM SSB1TA", "PM SSB1TB", "PM SSB2TA", "PM SSB2TB", "PM SSB3TB" };
            if (b209Overall.Any(x => m.Equals(x, StringComparison.OrdinalIgnoreCase))) return "Overall Building 209";

            var b207Det1 = new[] { "DB Testing 1st Floor", "DB AC Testing", "DB SMT ( GL )", "Reflow #1", "Reflow #2", "DB Reflow #3", "Washing", "AC SMT (GL)", "Compressor SMT", "DETECTION 1ST FLOOR Real Energy Into the Load", "WAREHOUSE", "AC_C10K", "C10K_1", "C10K_2", "L-L250A" };
            if (b207Det1.Any(x => m.Equals(x, StringComparison.OrdinalIgnoreCase))) return "Building 207, Detection 1st floor";

            var b207Det2 = new[] { "2Z4", "2ZA", "AC AHU 2-1", "AC AHU 2-2", "DB_FFU_LT2", "DB JOGAN", "DB_LVP", "DB_NCR1", "DB_NCR2", "DB_PHASE 6", "DB_NEW POLARIS", "DB_POL 10 11", "DB_POL 13-14", "DB_POL 15-17", "DB_POL18-20", "DB_PYRO 1.6", "DB_SAWING", "DB_T1", "DB_AC_OFFICE_DETECTION", "PM_DB_OFFICE_DETECTION", "DB_P1", "DB_P2", "DB4", "DB_MIELE", "AC IRD", "CHILLER_1-4", "DB L3", "DB_REALIBILITY", "VACUMP" };
            if (b207Det2.Any(x => m.Equals(x, StringComparison.OrdinalIgnoreCase))) return "Building 207, Detection 2nd floor";

            var b207Det3 = new[] { "3Z1 ( ex SMD )", "3Z1A (ex-Testing)", "AC 3rd Floor", "LIGHTING FTC", "WORKSHOP", "DB_REALIBILITY" };
            if (b207Det3.Any(x => m.Equals(x, StringComparison.OrdinalIgnoreCase))) return "Building 207, Detection 3rd floor";

            var b207DetSL = new[] { "AC SL", "Lighting Hydra", "DB SL200A", "HYD #1", "HYD #2", "HYD #3", "HYD #4", "DB A", "DB UV", "DB_H", "DB_PHASE 1", "DB_PHASE 2", "AC 250A LSM", "CNC" };
            if (b207DetSL.Any(x => m.Equals(x, StringComparison.OrdinalIgnoreCase))) return "Building 207, Detection Special Lighting floor";

            var b209Det1 = new[] { 
                "PM_LOT209_LT1.PM_AC_CL2 Real Energy Into the Load", "PM_LOT209_LT1.PM_CHILLER Real Energy Into the Load", "PM_LOT209_LT1.PM_DB_CHILLER Real Energy Into the Load", 
                "PM_LOT209_LT1.PM_DBCanteen Real Energy Into the Load", "PM_LOT209_LT1.PM_DBRO Real Energy Into the Load", "PM_LOT209_LT1.PM_MDB400A Real Energy Into the Load", 
                "PM_LOT209_LT1.PM_ULTRASONIC1 Real Energy Into the Load", "PM_LOT209_LT1.PM_ULTRASONIC2 Real Energy Into the Load", "PM_LOT209_LT1.PM_ULTRASONIC3 Real Energy Into the Load", 
                "PM_LOT209_LT1.PM_ULTRASONIC4 Real Energy Into the Load", "PM_LOT209_LT1.PM_WWTP Real Energy Into the Load", "PM_LOT209_LT1.PMDB_AC320A Real Energy Into the Load", 
                "PM_LOT209_LT1.PMDB_COATING Real Energy Into the Load", "PM_LOT209_LT1.PMDB_COMPRESSOR Real Energy Into the Load", "PM_LOT209_LT1.PMDB_LGHTNG Real Energy Into the Load", 
                "PM_LOT209_LT1.PMDB_LIGHTING_2 Real Energy Into the Load", "PM_LOT209_LT1.PMDB_MC1 Real Energy Into the Load", "PM_LOT209_LT1.PMDB_MC2 Real Energy Into the Load", 
                "PM_LOT209_LT1.PMDB_MC3 Real Energy Into the Load", "PM_LOT209_LT1.PMDB_POWER_100A Real Energy Into the Load", "PM_LOT209_LT1.PMDB_POWER_EXHAUST Real Energy Into the Load", 
                "PM_LOT209_LT1.PMMDB_320A Real Energy Into the Load", "Total SSB Lt# 1 209 Real Energy Into the Load" 
            };
            if (b209Det1.Any(x => m.Equals(x, StringComparison.OrdinalIgnoreCase))) return "Building 209 1st floor";

            var b209Det2 = new[] {
                "PM_LOT209_LT2.PM_ASTROFURNACE2 Real Energy Into the Load", "PM_LOT209_LT2.PM_ASTROFURNACE4 Real Energy Into the Load", "PM_LOT209_LT2.PM_CAMCOFURNACE7 Real Energy Into the Load", 
                "PM_LOT209_LT2.PM_CAMCOFURNACE8 Real Energy Into the Load", "PM_LOT209_LT2.PM_CHILLER13 Real Energy Into the Load", "PM_LOT209_LT2.PM_LOT209_EXHAUST2_LT2 Real Energy Into the Load", 
                "PM_LOT209_LT2.PM_NIPLATING Real Energy Into the Load", "PM_LOT209_LT2.PMDB_AGING1 Real Energy Into the Load", "PM_LOT209_LT2.PMDB_AGING2 Real Energy Into the Load", 
                "PM_LOT209_LT2.PMDB_ASTRO Real Energy Into the Load", "PM_LOT209_LT2.PMDB1_250A Real Energy Into the Load", "PM_LOT209_LT2.PMDB1_320A Real Energy Into the Load", 
                "PM_LOT209_LT2.PMDB2_320A Real Energy Into the Load", "PM_LOT209_LT2.PMDB2_500A Real Energy Into the Load", "PM_LOT209_LT2.PMDB3_320A Real Energy Into the Load", 
                "PM_LOT209_LT2.PMDB3_500A Real Energy Into the Load", "PM_LOT209_LT2.PMDB4_LIGHTING Real Energy Into the Load", "PM_LOT209_LT2.PMPANEL_EXHAUST Real Energy Into the Load", 
                "Total SSB Lt# 2 209 Real Energy Into the Load"
            };
            if (b209Det2.Any(x => m.Equals(x, StringComparison.OrdinalIgnoreCase))) return "Building 209 2nd floor";

            var b209Det3 = new[] {
                "PM_LOT209_LT3.PM_CHILLER_ROOFTOP Real Energy Into the Load", "PM_LOT209_LT3.PMDB_COMPRESSOR_LT3 Real Energy Into the Load", "PM_LOT209_LT3.PMDB_EXHAUST_LT3 Real Energy Into the Load", 
                "PM_LOT209_LT3.PMDB_LIGHTING_LT3 Real Energy Into the Load", "PM_LOT209_LT3.PMDB_MC_LT3 Real Energy Into the Load", "PM_LOT209_LT3.PMDB_MC2_LT3 Real Energy Into the Load", 
                "PM_LOT209_LT3.PMDB_POWER_LT3 Real Energy Into the Load", "PM_LOT209_LT3.PMMDB_400A_LT3 Real Energy Into the Load", "PM_LOT209_LT3.PMSSB3TB Real Energy Into the Load", 
                "PM_LOT209_LT3.MDB_200A_AC_TC Real Energy Into the Load", "PM_LOT209_LT3.L_P Real Energy Into the Load", "PM_LOT209_LT3.MC_TC Real Energy Into the Load", 
                "PM_LOT209_LT3.P Real Energy Into the Load"
            };
            if (b209Det3.Any(x => m.Equals(x, StringComparison.OrdinalIgnoreCase))) return "Building 209 3rd floor";

            var b238 = new[] { "PM_LOT238.PM_MAIN Voltage A-B (V)" };
            if (b238.Any(x => m.Equals(x, StringComparison.OrdinalIgnoreCase))) return "Building 238";

            // Fallback for unlisted meters to still classify them generally if possible
            string upperName = m.ToUpper();
            string lotName = "Unknown Lot";
            string floorName = "Unknown Floor";

            if (upperName.Contains("LOT207")) lotName = "Lot 207";
            else if (upperName.Contains("LOT209")) lotName = "Lot 209";
            else if (upperName.Contains("LOT238")) lotName = "Lot 238";
            else if (upperName.Contains("LOT292")) lotName = "Lot 292";

            if (upperName.Contains("LT1") || upperName.Contains("LT 1")) floorName = "LT 1";
            else if (upperName.Contains("LT2") || upperName.Contains("LT 2")) floorName = "LT 2";
            else if (upperName.Contains("LT3") || upperName.Contains("LT 3")) floorName = "LT 3";
            else if (upperName.Contains("LT4") || upperName.Contains("LT 4")) floorName = "LT 4";

            if (lotName != "Unknown Lot" && floorName != "Unknown Floor")
                return $"{lotName} {floorName}";

            return "Other Buildings / Floors";
        }
    }
}