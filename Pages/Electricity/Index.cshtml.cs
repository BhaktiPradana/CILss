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
                    string upperName = rec.MeterName.ToUpper();
                    string lotName = "Unknown Lot";
                    string floorName = "Unknown Floor";
                    if (upperName.Contains("LOT207")) lotName = "Lot 207";
                    else if (upperName.Contains("LOT209")) lotName = "Lot 209";
                    else if (upperName.Contains("LOT238")) lotName = "Lot 238";
                    else if (upperName.Contains("LOT292")) lotName = "Lot 292";

                    // Ekstrak Lantai
                    if (upperName.Contains("LT1") || upperName.Contains("LT 1")) floorName = "LT 1";
                    else if (upperName.Contains("LT2") || upperName.Contains("LT 2")) floorName = "LT 2";
                    else if (upperName.Contains("LT3") || upperName.Contains("LT 3")) floorName = "LT 3";
                    else if (upperName.Contains("LT4") || upperName.Contains("LT 4")) floorName = "LT 4";
                    if (lotName != "Unknown Lot" && floorName != "Unknown Floor")
                    {
                        rec.Floor = $"{lotName} {floorName}";
                    }
                    else
                    {
                        rec.Floor = "Other Buildings / Floors";
                    }
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
    }
}