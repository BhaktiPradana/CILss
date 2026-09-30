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

        public List<SelectListItem> BuildingOptions { get; set; } = new();
        public List<SelectListItem> FloorOptions { get; set; } = new();
        public List<DistributionBoard> Boards { get; set; } = new();
        public Dictionary<string, int> MachineCounts { get; set; } = new();
        public Dictionary<string, List<DbMachineMapping>> MappingsPerBoard { get; set; } = new();
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

            Boards = allBoards
                .Where(b => string.IsNullOrEmpty(SelectedBuilding) || b.Building == SelectedBuilding)
                .Where(b => string.IsNullOrEmpty(SelectedFloor) || b.Floor == SelectedFloor)
                .ToList();

            MachineCounts = await _dbRepo.GetMachineCountsPerBoardAsync();
            TotalMappings = MachineCounts.Values.Sum();

            // Pre-load all mappings for each board
            foreach (var board in Boards)
            {
                var mappings = await _dbRepo.GetMappingsByBoardNameAsync(board.BoardName);
                MappingsPerBoard[board.BoardName] = mappings;
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
