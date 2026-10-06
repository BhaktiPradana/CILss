using LssTraining.Web.Data;
using LssTraining.Web.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LssTraining.Web.Pages.CapacitySimulator
{
    public class IndexModel : PageModel
    {
        private readonly CapacityRepository _repository;

        public CapacitySummary Summary { get; set; } = new CapacitySummary();
        public IEnumerable<ManpowerBySection> ManpowerList { get; set; } = new List<ManpowerBySection>();
        public IEnumerable<ProcessArea> ProcessAreaList { get; set; } = new List<ProcessArea>();

        public IndexModel(CapacityRepository repository)
        {
            _repository = repository;
        }

        [Microsoft.AspNetCore.Mvc.BindProperty]
        public SimulationInput Input { get; set; } = new SimulationInput 
        { 
            Demands = new List<SimulationDemand> { new SimulationDemand { DemandQuantity = 10000 } },
            OvertimeLimitPercent = 10 
        };

        public SimulationResult Result { get; set; }
        public IEnumerable<DeviceGroup> DeviceGroups { get; set; } = new List<DeviceGroup>();

        public async Task OnGetAsync()
        {
            await LoadDataAsync();
        }

        public async Task<Microsoft.AspNetCore.Mvc.IActionResult> OnPostSimulateAsync()
        {
            await LoadDataAsync();
            if (ModelState.IsValid)
            {
                Result = await _repository.RunSimulationAsync(Input, Summary.TotalEmployees);
            }
            return Page();
        }

        private async Task LoadDataAsync()
        {
            Summary = await _repository.GetSummaryAsync();
            ManpowerList = await _repository.GetManpowerBySectionAsync();
            ProcessAreaList = await _repository.GetProcessAreasAsync();
            DeviceGroups = await _repository.GetDeviceGroupsAsync();
        }
    }
}
