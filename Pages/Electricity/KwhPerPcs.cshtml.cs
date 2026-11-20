using LssTraining.Web.Data;
using LssTraining.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace LssTraining.Web.Pages.Electricity
{
    public class KwhPerPcsModel : PageModel
    {
        private readonly KwhPerPcsRepository _kwhPerPcsRepo;

        public KwhPerPcsModel(KwhPerPcsRepository kwhPerPcsRepo)
        {
            _kwhPerPcsRepo = kwhPerPcsRepo;
        }

        [BindProperty(SupportsGet = true)]
        public DateTime StartDate { get; set; } = new DateTime(2026, 8, 1);

        [BindProperty(SupportsGet = true)]
        public DateTime EndDate { get; set; } = new DateTime(2026, 10, 9);

        [BindProperty(SupportsGet = true)]
        public string? SelectedDB { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public string AllocationMethod { get; set; } = "Equal";

        public List<SelectListItem> DbOptions { get; set; } = new();
        public List<MachineKwhPerPcsDto> Machines { get; set; } = new();
        public List<DbKwhSummaryDto> DbSummaries { get; set; } = new();
        public KwhPerPcsDashboardSummary Summary { get; set; } = new();

        public string TrendChartJson { get; set; } = "{}";
        public string MachineChartJson { get; set; } = "{}";
        public string DbChartJson { get; set; } = "{}";

        public async Task OnGetAsync()
        {
            // Normalize dates
            if (StartDate > EndDate)
            {
                var temp = StartDate;
                StartDate = EndDate;
                EndDate = temp;
            }

            // Populate DB dropdown
            var allDbs = await _kwhPerPcsRepo.GetAvailableDbsAsync();
            DbOptions = new List<SelectListItem> { new("All Distribution Boards", "") };
            foreach (var db in allDbs)
            {
                DbOptions.Add(new SelectListItem(db, db));
            }

            // Fetch calculation data
            var (machs, dbs, summ, trends) = await _kwhPerPcsRepo.GetKwhPerPcsDataAsync(
                StartDate, 
                EndDate, 
                SelectedDB, 
                SearchTerm, 
                AllocationMethod
            );

            Machines = machs;
            DbSummaries = dbs;
            Summary = summ;

            BuildChartsJson(trends, machs, dbs);
        }

        private void BuildChartsJson(List<KwhPerPcsDailyTrendPoint> trends, List<MachineKwhPerPcsDto> machs, List<DbKwhSummaryDto> dbs)
        {
            // 1. Daily Trend Chart
            if (trends.Any())
            {
                var trendPayload = new
                {
                    categories = trends.Select(t => t.DateLabel).ToList(),
                    kwhPerPcsSeries = trends.Select(t => Math.Round(t.KwhPerPcs, 4)).ToList(),
                    kwhSeries = trends.Select(t => Math.Round(t.TotalKwh, 1)).ToList(),
                    qtySeries = trends.Select(t => Math.Round(t.TotalPieces, 0)).ToList()
                };
                TrendChartJson = JsonSerializer.Serialize(trendPayload);
            }

            // 2. Machine Ranking Chart (Top 12 Machines by output or efficiency)
            var rankedMachines = machs
                .Where(m => m.QuantityIn > 0 && m.KwhPerPcs > 0)
                .OrderBy(m => m.KwhPerPcs)
                .Take(15)
                .ToList();

            if (rankedMachines.Any())
            {
                var machPayload = new
                {
                    categories = rankedMachines.Select(m => $"{m.MachCode} ({m.MachName})").ToList(),
                    kwhPerPcs = rankedMachines.Select(m => Math.Round(m.KwhPerPcs, 4)).ToList(),
                    allocKwh = rankedMachines.Select(m => Math.Round(m.AllocatedKwh, 1)).ToList(),
                    qtyIn = rankedMachines.Select(m => m.QuantityIn).ToList()
                };
                MachineChartJson = JsonSerializer.Serialize(machPayload);
            }

            // 3. DB Energy Allocation Chart
            if (dbs.Any())
            {
                var dbPayload = new
                {
                    categories = dbs.Select(d => d.DBName.Replace("PM_LOT207_LT", "L").Replace("PM_LOT209_LT", "L")).ToList(),
                    totalKwh = dbs.Select(d => Math.Round(d.TotalKwh, 1)).ToList(),
                    totalQty = dbs.Select(d => Math.Round(d.TotalQuantityIn, 0)).ToList(),
                    overallKwhPerPcs = dbs.Select(d => Math.Round(d.OverallKwhPerPcs, 4)).ToList()
                };
                DbChartJson = JsonSerializer.Serialize(dbPayload);
            }
        }

        public async Task<IActionResult> OnGetExportCsvAsync(
            DateTime? exportStartDate,
            DateTime? exportEndDate,
            string? exportDb,
            string? exportSearch,
            string? exportMethod)
        {
            var start = exportStartDate ?? StartDate;
            var end = exportEndDate ?? EndDate;
            if (start > end)
            {
                var temp = start;
                start = end;
                end = temp;
            }

            var method = !string.IsNullOrEmpty(exportMethod) ? exportMethod : AllocationMethod;

            var (machs, dbs, summ, trends) = await _kwhPerPcsRepo.GetKwhPerPcsDataAsync(
                start,
                end,
                exportDb,
                exportSearch,
                method
            );

            var sb = new StringBuilder();
            sb.AppendLine("MachineCode,MachineName,DistributionBoard,Area,DB_TotalKwh,SharingMachines,AllocatedKwh,QuantityInPcs,KwhPerPcs,AllocationMethod,StartDate,EndDate");

            foreach (var m in machs.OrderBy(x => x.MachCode))
            {
                sb.AppendLine(string.Format(
                    CultureInfo.InvariantCulture,
                    "\"{0}\",\"{1}\",\"{2}\",\"{3}\",{4:F2},{5},{6:F2},{7},{8:F6},\"{9}\",\"{10:yyyy-MM-dd}\",\"{11:yyyy-MM-dd}\"",
                    EscapeCsv(m.MachCode),
                    EscapeCsv(m.MachName),
                    EscapeCsv(m.DBName),
                    EscapeCsv(m.Area),
                    m.DbTotalKwh,
                    m.SharedCount,
                    m.AllocatedKwh,
                    m.QuantityIn,
                    m.KwhPerPcs,
                    EscapeCsv(m.AllocationMethod),
                    start,
                    end
                ));
            }

            var preamble = Encoding.UTF8.GetPreamble();
            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            var fileBytes = new byte[preamble.Length + bytes.Length];
            Buffer.BlockCopy(preamble, 0, fileBytes, 0, preamble.Length);
            Buffer.BlockCopy(bytes, 0, fileBytes, preamble.Length, bytes.Length);

            string fileName = $"Machine_Kwh_Per_Pcs_{start:yyyyMMdd}_to_{end:yyyyMMdd}.csv";
            return File(fileBytes, "text/csv", fileName);
        }

        private static string EscapeCsv(string? field)
        {
            if (string.IsNullOrEmpty(field)) return "";
            return field.Replace("\"", "\"\"");
        }
    }
}

