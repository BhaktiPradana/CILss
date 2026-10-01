using LssTraining.Web.Data;
using LssTraining.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LssTraining.Web.Pages.Electricity
{
    public class ImprovementsModel : PageModel
    {
        private readonly ImprovementRepository _repo;
        private readonly DistributionBoardRepository _dbRepo;

        public ImprovementsModel(ImprovementRepository repo, DistributionBoardRepository dbRepo)
        {
            _repo = repo;
            _dbRepo = dbRepo;
        }

        [BindProperty(SupportsGet = true)]
        public string? SelectedMachCode { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? StatusFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SearchQuery { get; set; }

        [BindProperty]
        public ImprovementInput NewImprovement { get; set; } = new();

        public List<MachineImprovement> Improvements { get; set; } = new();
        public List<SelectListItem> MachineOptions { get; set; } = new();
        public List<SelectListItem> StatusOptions { get; set; } = new();

        public decimal TotalBaselineKwh { get; set; }
        public decimal TotalCurrentTargetKwh { get; set; }
        public decimal TotalDailySavingsKwh { get; set; }
        public decimal TotalSavingsPercentage { get; set; }

        public async Task OnGetAsync(CancellationToken cancellationToken)
        {
            await LoadDataAsync(cancellationToken);
        }

        public async Task<IActionResult> OnGetCalculateKwhAsync(string machCode, DateTime? implDate, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(machCode))
            {
                return new JsonResult(new { success = false, message = "Machine code required" });
            }

            var date = implDate ?? DateTime.Today;
            var calc = await _repo.CalculateMachineKwhDataAsync(machCode, date, cancellationToken);
            return new JsonResult(new { success = true, data = calc });
        }

        public async Task<IActionResult> OnPostAddAsync(CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(NewImprovement.MachCode))
            {
                ModelState.AddModelError("NewImprovement.MachCode", "Please select a machine.");
            }
            if (string.IsNullOrWhiteSpace(NewImprovement.ImprovementTitle))
            {
                ModelState.AddModelError("NewImprovement.ImprovementTitle", "Improvement title is required.");
            }

            // Auto-calculate baseline & target if not provided
            if (NewImprovement.BaselineKwhPerDay <= 0 && !string.IsNullOrWhiteSpace(NewImprovement.MachCode))
            {
                var autoCalc = await _repo.CalculateMachineKwhDataAsync(NewImprovement.MachCode, NewImprovement.ImplementationDate, cancellationToken);
                NewImprovement.BaselineKwhPerDay = autoCalc.BaselineKwhPerDay;
                if (NewImprovement.TargetKwhPerDay <= 0)
                {
                    NewImprovement.TargetKwhPerDay = autoCalc.TargetKwhPerDay;
                }
                if (!NewImprovement.ActualKwhPerDay.HasValue && autoCalc.ActualKwhPerDay.HasValue)
                {
                    NewImprovement.ActualKwhPerDay = autoCalc.ActualKwhPerDay;
                }
            }

            if (!ModelState.IsValid)
            {
                await LoadDataAsync(cancellationToken);
                return Page();
            }

            var item = new MachineImprovement
            {
                MachCode = NewImprovement.MachCode,
                ImprovementTitle = NewImprovement.ImprovementTitle.Trim(),
                Description = NewImprovement.Description?.Trim(),
                ImplementationDate = NewImprovement.ImplementationDate,
                BaselineKwhPerDay = NewImprovement.BaselineKwhPerDay,
                TargetKwhPerDay = NewImprovement.TargetKwhPerDay,
                ActualKwhPerDay = NewImprovement.ActualKwhPerDay,
                Status = NewImprovement.Status,
                CreatedBy = User.Identity?.Name ?? "Anonymous User",
                CreatedAt = DateTime.UtcNow
            };

            await _repo.CreateAsync(item, cancellationToken);
            TempData["SuccessMessage"] = "Machine energy improvement activity recorded successfully.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id, CancellationToken cancellationToken)
        {
            await _repo.DeleteAsync(id, cancellationToken);
            TempData["SuccessMessage"] = "Improvement record deleted.";
            return RedirectToPage();
        }

        private async Task LoadDataAsync(CancellationToken cancellationToken)
        {
            var machines = await _dbRepo.GetAllMachinesAsync();
            MachineOptions = new List<SelectListItem> { new("All Machines", "") };
            MachineOptions.AddRange(machines.Select(m => new SelectListItem($"{m.MachName} ({m.MachCode})", m.MachCode)));

            StatusOptions = new List<SelectListItem>
            {
                new("All Statuses", ""),
                new("Planned", "Planned"),
                new("In Progress", "In Progress"),
                new("Completed", "Completed")
            };

            var all = await _repo.GetAllAsync(cancellationToken);

            var query = all.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SelectedMachCode))
            {
                query = query.Where(x => x.MachCode.Equals(SelectedMachCode, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(StatusFilter))
            {
                query = query.Where(x => x.Status.Equals(StatusFilter, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                var q = SearchQuery.Trim();
                query = query.Where(x =>
                    x.ImprovementTitle.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    x.MachName.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    x.MachCode.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    (x.Description != null && x.Description.Contains(q, StringComparison.OrdinalIgnoreCase))
                );
            }

            Improvements = query.ToList();

            TotalBaselineKwh = Improvements.Sum(x => x.BaselineKwhPerDay);
            TotalCurrentTargetKwh = Improvements.Sum(x => x.ActualKwhPerDay ?? x.TargetKwhPerDay);
            TotalDailySavingsKwh = Improvements.Sum(x => x.EstimatedDailySavingsKwh);
            TotalSavingsPercentage = TotalBaselineKwh > 0 ? Math.Round((TotalDailySavingsKwh / TotalBaselineKwh) * 100, 1) : 0;
        }

        public class ImprovementInput
        {
            public string MachCode { get; set; } = string.Empty;
            public string ImprovementTitle { get; set; } = string.Empty;
            public string? Description { get; set; }
            public DateTime ImplementationDate { get; set; } = DateTime.Today;
            public decimal BaselineKwhPerDay { get; set; }
            public decimal TargetKwhPerDay { get; set; }
            public decimal? ActualKwhPerDay { get; set; }
            public string Status { get; set; } = "In Progress";
        }
    }
}
