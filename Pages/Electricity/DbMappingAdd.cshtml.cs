using LssTraining.Web.Data;
using LssTraining.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json;

namespace LssTraining.Web.Pages.Electricity
{
    public class DbMappingAddModel : PageModel
    {
        private readonly DistributionBoardRepository _dbRepo;

        public DbMappingAddModel(DistributionBoardRepository dbRepo)
        {
            _dbRepo = dbRepo;
        }

        [BindProperty]
        public string? SelectedMachCode { get; set; }

        [BindProperty]
        public string? DbPrimary { get; set; }

        [BindProperty]
        public string? DbSecondary { get; set; }

        [BindProperty]
        public string? DbTertiary { get; set; }

        [BindProperty]
        public string? SubDBName { get; set; }

        [BindProperty]
        public string? Remarks { get; set; }

        public List<Machine> Machines { get; set; } = new();
        public List<SelectListItem> MachineNameOptions { get; set; } = new();
        public List<SelectListItem> MachineCodeOptions { get; set; } = new();
        public List<SelectListItem> DbOptions { get; set; } = new();
        public string MachinesJson { get; set; } = "[]";

        public async Task OnGetAsync(string? machCode = null)
        {
            if (!string.IsNullOrWhiteSpace(machCode))
            {
                SelectedMachCode = machCode;
            }
            await LoadFormData();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(SelectedMachCode))
            {
                ModelState.AddModelError("", "Please select a machine.");
                await LoadFormData();
                return Page();
            }

            if (string.IsNullOrWhiteSpace(DbPrimary))
            {
                ModelState.AddModelError("", "DB Connection (Primary) is required.");
                await LoadFormData();
                return Page();
            }

            await _dbRepo.SaveMappingAsync(SelectedMachCode, DbPrimary, SubDBName, Remarks, DbPrimary, DbSecondary, DbTertiary);
            return RedirectToPage("/Electricity/DbMapping");
        }

        private async Task LoadFormData()
        {
            Machines = await _dbRepo.GetAllMachinesAsync();

            MachineNameOptions = new List<SelectListItem> { new("-- Select Machine Name --", "") };
            MachineNameOptions.AddRange(Machines.Select(m => new SelectListItem(m.MachName, m.MachCode)));

            MachineCodeOptions = new List<SelectListItem> { new("-- Select Machine Code --", "") };
            MachineCodeOptions.AddRange(Machines.Select(m => new SelectListItem(m.MachCode, m.MachCode)));

            var boardNames = await _dbRepo.GetBoardNamesAsync();
            DbOptions = new List<SelectListItem> { new("-- Select DB --", "") };
            DbOptions.AddRange(boardNames.Select(b => new SelectListItem(b, b)));

            // JSON for JS auto-sync between MachName and MachCode dropdowns
            MachinesJson = JsonSerializer.Serialize(
                Machines.Select(m => new { code = m.MachCode, name = m.MachName })
            );
        }
    }
}
