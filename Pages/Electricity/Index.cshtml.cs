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
        public string? Building { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Floor { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SelectedMeter { get; set; }

        [BindProperty(SupportsGet = true)]
        public string ViewMode { get; set; } = "Daily";

        [BindProperty(SupportsGet = true)]
        public DateTime StartDate { get; set; } = DateTime.Today.AddDays(-7);

        [BindProperty(SupportsGet = true)]
        public DateTime EndDate { get; set; } = DateTime.Today;

        public List<SelectListItem> BuildingOptions { get; set; } = new();
        public List<SelectListItem> FloorOptions { get; set; } = new();
        public List<SelectListItem> MeterOptions { get; set; } = new();

        public List<ElectricityCardDto> Cards { get; set; } = new();
        public Dictionary<string, List<ElectricityCardDto>> CardsByFloor { get; set; } = new();
        public ElectricityDashboardSummary Summary { get; set; } = new();
        public List<KwhRecord> Records { get; set; } = new();
        public string ChartDataJson { get; set; } = "{}";
        public bool HasDatabaseError { get; set; }

        public async Task OnGetAsync()
        {
            // Normalize dates
            if (StartDate > EndDate)
            {
                var temp = StartDate;
                StartDate = EndDate;
                EndDate = temp;
            }

            // 1. Build Filter Options
            BuildingOptions = new List<SelectListItem>
            {
                new("All Buildings", ""),
                new(ElectricityCatalog.Building207, ElectricityCatalog.Building207),
                new(ElectricityCatalog.Building209, ElectricityCatalog.Building209),
                new(ElectricityCatalog.Building238, ElectricityCatalog.Building238)
            };

            var validFloors = ElectricityCardCatalog.GetFloors(Building);
            FloorOptions = new List<SelectListItem> { new("All Floors & Sections", "") };
            foreach (var f in validFloors)
            {
                FloorOptions.Add(new SelectListItem(f, f));
            }

            // Reset Floor if not valid for selected building
            if (!string.IsNullOrEmpty(Floor) && !validFloors.Contains(Floor, StringComparer.OrdinalIgnoreCase))
            {
                Floor = null;
            }

            // Distribution Board Options
            MeterOptions = new List<SelectListItem> { new("All Distribution Boards", "") };
            var allItemsForDropdown = ElectricityCardCatalog.GetAllItems(Building, Floor);
            foreach (var m in allItemsForDropdown)
            {
                MeterOptions.Add(new SelectListItem($"{m.DisplayName} ({m.Floor})", m.MeterName));
            }

            // 2. Fetch Data from Repository
            var actualEndDate = ViewMode == "Hourly" ? EndDate.Date.AddDays(1).AddSeconds(-1) : EndDate;
            try
            {
                if (ViewMode == "Hourly")
                {
                    Records = await _kwhRepo.GetHourlyDataAsync(StartDate, actualEndDate, SelectedMeter);
                }
                else
                {
                    Records = await _kwhRepo.GetDailyDataAsync(StartDate, actualEndDate, SelectedMeter);
                }
            }
            catch
            {
                HasDatabaseError = true;
                Records = new List<KwhRecord>();
            }

            // If database returns empty or is unreachable, generate preview data
            if (Records == null || Records.Count == 0)
            {
                HasDatabaseError = true;
                Records = KwhRepository.GeneratePreviewData(StartDate, actualEndDate, ViewMode);
            }

            // Ensure every record has proper Building & Floor classification
            foreach (var rec in Records)
            {
                var cardItem = ElectricityCardCatalog.FindItem(rec.MeterName);
                if (cardItem != null)
                {
                    rec.Floor = cardItem.Floor;
                }
                else
                {
                    var resolved = ElectricityCatalog.ResolveMeter(rec.MeterName);
                    rec.Floor = resolved.Floor;
                }
            }

            // 3. Filter Target Catalog for Cards directly from ElectricityCardCatalog (strictly 130 cards)
            var catalogItems = ElectricityCardCatalog.GetAllItems(Building, Floor);

            if (!string.IsNullOrEmpty(SelectedMeter))
            {
                catalogItems = catalogItems.Where(m =>
                    m.MeterName.Equals(SelectedMeter, StringComparison.OrdinalIgnoreCase) ||
                    m.DisplayName.Equals(SelectedMeter, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                string term = SearchTerm.Trim();
                catalogItems = catalogItems.Where(m =>
                    m.DisplayName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    m.MeterName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    m.Subtitle.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    m.Floor.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    m.Building.Contains(term, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            // Calculate day span for daily average
            int daysCount = Math.Max(1, (int)(EndDate.Date - StartDate.Date).TotalDays + 1);

            // Group records by MeterName (trimmed, case-insensitive)
            var recordsByMeter = Records
                .GroupBy(r => r.MeterName.Trim(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);

            Cards = new List<ElectricityCardDto>();

            foreach (var def in catalogItems)
            {
                string cleanMeter = def.MeterName.Trim();
                string cleanDisplay = def.DisplayName.Trim();

                recordsByMeter.TryGetValue(cleanMeter, out var matching);

                // Fallback search by DisplayName or partial match if exact meter key wasn't found
                if (matching == null || !matching.Any())
                {
                    recordsByMeter.TryGetValue(cleanDisplay, out matching);
                }

                if (matching == null || !matching.Any())
                {
                    matching = Records.Where(r =>
                        r.MeterName.Trim().Equals(cleanMeter, StringComparison.OrdinalIgnoreCase) ||
                        r.MeterName.Trim().Equals(cleanDisplay, StringComparison.OrdinalIgnoreCase) ||
                        r.MeterName.Contains(cleanDisplay, StringComparison.OrdinalIgnoreCase) ||
                        cleanDisplay.Contains(r.MeterName.Trim(), StringComparison.OrdinalIgnoreCase)
                    ).ToList();
                }

                decimal totalUsage = matching != null ? matching.Sum(r => r.Usage_kWh) : 0m;
                decimal avgDaily = daysCount > 0 ? totalUsage / daysCount : 0m;
                var latestRec = matching?.OrderByDescending(r => r.Timestamp).FirstOrDefault();
                decimal latestVal = latestRec?.Usage_kWh ?? 0m;
                decimal peakVal = matching != null && matching.Any() ? matching.Max(r => r.Usage_kWh) : 0m;
                var sparkline = matching != null
                    ? matching.OrderBy(r => r.Timestamp).Select(r => r.Usage_kWh).TakeLast(8).ToList()
                    : new List<decimal>();

                Cards.Add(new ElectricityCardDto
                {
                    MeterName = def.MeterName,
                    CardTitle = def.DisplayName,
                    Subtitle = def.Subtitle,
                    Building = def.Building,
                    Floor = def.Floor,
                    PanelType = def.PanelType,
                    Unit = def.Unit,
                    TotalUsage = Math.Round(totalUsage, 2),
                    AverageDaily = Math.Round(avgDaily, 2),
                    LatestUsage = Math.Round(latestVal, 2),
                    PeakUsage = Math.Round(peakVal, 2),
                    LatestTimestamp = latestRec?.Timestamp,
                    HasData = matching != null && matching.Any(),
                    DataPointsCount = matching?.Count ?? 0,
                    Sparkline = sparkline
                });
            }

            // Group cards by Floor/Section, preserving catalog order
            CardsByFloor = Cards
                .GroupBy(c => c.Floor)
                .ToDictionary(g => g.Key, g => g.ToList());

            // 4. Overall Summary Calculations
            var kwhCards = Cards.Where(c => c.Unit == "kWh").ToList();
            var totalKwh = kwhCards.Sum(c => c.TotalUsage);
            var activeCount = Cards.Count(c => c.HasData && c.TotalUsage > 0);
            var peakCard = kwhCards.OrderByDescending(c => c.TotalUsage).FirstOrDefault();

            Summary = new ElectricityDashboardSummary
            {
                TotalKwh = Math.Round(totalKwh, 1),
                TotalCards = Cards.Count,
                ActiveCards = activeCount,
                AverageDailyKwh = Math.Round(totalKwh / daysCount, 1),
                PeakMeterName = peakCard?.CardTitle ?? "-",
                PeakUsage = peakCard?.TotalUsage ?? 0m
            };

            // 5. Build ApexCharts Series Data
            BuildChartJson();
        }

        private void BuildChartJson()
        {
            if (Records == null || Records.Count == 0)
            {
                ChartDataJson = "{}";
                return;
            }

            // Filter records to relevant active cards
            var activeMeterNames = new HashSet<string>(Cards.Select(c => c.MeterName.Trim()), StringComparer.OrdinalIgnoreCase);
            var activeDisplayNames = new HashSet<string>(Cards.Select(c => c.CardTitle.Trim()), StringComparer.OrdinalIgnoreCase);

            var filteredRecs = Records.Where(r => 
                activeMeterNames.Contains(r.MeterName.Trim()) || 
                activeDisplayNames.Contains(r.MeterName.Trim())
            ).ToList();

            if (!filteredRecs.Any())
            {
                filteredRecs = Records;
            }

            var timestamps = filteredRecs.Select(r => r.Timestamp).Distinct().OrderBy(t => t).ToList();
            var categories = timestamps.Select(t => ViewMode == "Hourly" ? t.ToString("dd MMM HH:mm") : t.ToString("dd MMM yyyy")).ToList();

            // If a single meter is selected or filtered, display only that meter
            // Otherwise, pick up to top 8 highest consumers for clean readability
            var meterGroups = filteredRecs.GroupBy(r => r.MeterName.Trim(), StringComparer.OrdinalIgnoreCase).ToList();

            if (meterGroups.Count > 8 && string.IsNullOrEmpty(SelectedMeter))
            {
                meterGroups = meterGroups
                    .OrderByDescending(g => g.Sum(r => r.Usage_kWh))
                    .Take(8)
                    .ToList();
            }

            var seriesList = new List<object>();

            foreach (var mg in meterGroups)
            {
                var dict = mg.GroupBy(r => r.Timestamp).ToDictionary(g => g.Key, g => g.Sum(r => r.Usage_kWh));
                var data = new List<decimal>();
                foreach (var ts in timestamps)
                {
                    data.Add(dict.TryGetValue(ts, out var val) ? val : 0m);
                }

                var cardItem = ElectricityCardCatalog.FindItem(mg.Key);
                string seriesName = cardItem?.DisplayName ?? ElectricityCatalog.ResolveMeter(mg.Key).CardTitle;
                if (!string.IsNullOrEmpty(cardItem?.Floor))
                {
                    seriesName += $" ({cardItem.Floor})";
                }

                seriesList.Add(new
                {
                    name = seriesName,
                    data = data
                });
            }

            var chartPayload = new
            {
                categories = categories,
                series = seriesList,
                unit = Cards.FirstOrDefault()?.Unit ?? "kWh"
            };

            ChartDataJson = JsonSerializer.Serialize(chartPayload);
        }
    }
}