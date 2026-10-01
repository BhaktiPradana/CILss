using LssTraining.Web.Data;
using LssTraining.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json;

namespace LssTraining.Web.Pages.Electricity
{
    public class DbMappingModel : PageModel
    {
        private readonly DistributionBoardRepository _dbRepo;

        public DbMappingModel(DistributionBoardRepository dbRepo)
        {
            _dbRepo = dbRepo;
        }

        [BindProperty(SupportsGet = true)]
        public string? SelectedBuilding { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SelectedFloor { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        public List<SelectListItem> BuildingOptions { get; set; } = new();
        public List<SelectListItem> FloorOptions { get; set; } = new();
        public List<DistributionBoard> Boards { get; set; } = new();
        public Dictionary<string, int> MachineCounts { get; set; } = new();
        public Dictionary<string, List<DbMachineMapping>> MappingsPerBoard { get; set; } = new();
        public List<Machine> UnmappedMachines { get; set; } = new();
        public int TotalMappings { get; set; }

        public async Task OnGetAsync()
        {
            var buildings = await _dbRepo.GetBuildingsAsync();
            BuildingOptions = new List<SelectListItem> { new("All Buildings", "") };
            BuildingOptions.AddRange(buildings.Select(b => new SelectListItem(b, b)));

            var floors = await _dbRepo.GetFloorsAsync();
            FloorOptions = new List<SelectListItem> { new("All Floors", "") };
            FloorOptions.AddRange(floors.Select(f => new SelectListItem(f, f)));

            var allBoards = await _dbRepo.GetAllAsync();
            MachineCounts = await _dbRepo.GetMachineCountsPerBoardAsync();
            TotalMappings = MachineCounts.Values.Sum();
            UnmappedMachines = await _dbRepo.GetUnmappedMachinesAsync();

            var search = SearchTerm?.Trim();

            // Pre-load mappings for each board and apply machine-level filtering if search term is active
            foreach (var board in allBoards)
            {
                var mappings = await _dbRepo.GetMappingsByBoardNameAsync(board.BoardName);
                if (!string.IsNullOrWhiteSpace(search))
                {
                    mappings = mappings.Where(m =>
                        m.MachName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        m.MachCode.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        (m.DBName != null && m.DBName.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                        (m.SubDBName != null && m.SubDBName.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                        (m.Remarks != null && m.Remarks.Contains(search, StringComparison.OrdinalIgnoreCase))
                    ).ToList();
                }
                MappingsPerBoard[board.BoardName] = mappings;
            }

            // Filter Boards list by Building, Floor, and SearchTerm (matching DB Name or Machine inside DB)
            Boards = allBoards
                .Where(b => string.IsNullOrEmpty(SelectedBuilding) || b.Building == SelectedBuilding)
                .Where(b => string.IsNullOrEmpty(SelectedFloor) || b.Floor == SelectedFloor)
                .Where(b => string.IsNullOrEmpty(search) ||
                            b.BoardName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                            b.Building.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                            b.Floor.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                            b.PanelType.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                            (MappingsPerBoard.TryGetValue(b.BoardName, out var boardMappings) && boardMappings.Any()))
                .ToList();

            // Filter unmapped machines if search term is active
            if (!string.IsNullOrWhiteSpace(search))
            {
                UnmappedMachines = UnmappedMachines.Where(u =>
                    u.MachName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    u.MachCode.Contains(search, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(int connectionId = 0, string? machCode = null)
        {
            if (connectionId > 0)
            {
                await _dbRepo.DeleteMappingAsync(connectionId);
            }
            else if (!string.IsNullOrEmpty(machCode))
            {
                await _dbRepo.DeleteMappingByMachCodeAsync(machCode);
            }
            return RedirectToPage();
        }
    }
}
