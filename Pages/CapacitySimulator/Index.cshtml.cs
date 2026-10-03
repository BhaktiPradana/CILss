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

        public async Task OnGetAsync()
        {
            Summary = await _repository.GetSummaryAsync();
            ManpowerList = await _repository.GetManpowerBySectionAsync();
            ProcessAreaList = await _repository.GetProcessAreasAsync();
        }
    }
}
