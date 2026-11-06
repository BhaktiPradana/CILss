using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LssTraining.Web.Data;
using LssTraining.Web.Models;
using System;
using System.Threading.Tasks;

namespace LssTraining.Web.Pages.CapacitySimulator
{
    public class LiveModel : PageModel
    {
        private readonly CapacityRepository _repository;

        public LiveModel(CapacityRepository repository)
        {
            _repository = repository;
        }

        [BindProperty(SupportsGet = true)]
        public LiveCapacityFilterInput Filter { get; set; } = new LiveCapacityFilterInput();

        public LiveCapacityResult Result { get; set; } = new LiveCapacityResult();

        public async Task OnGetAsync()
        {
            Result = await _repository.GetLiveCapacityAnalysisAsync(Filter);
        }

        public async Task<JsonResult> OnGetApiAsync()
        {
            var res = await _repository.GetLiveCapacityAnalysisAsync(Filter);
            return new JsonResult(res);
        }
    }
}

