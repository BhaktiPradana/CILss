using System.ComponentModel.DataAnnotations;
using LssTraining.Web.Data;
using LssTraining.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LssTraining.Web.Pages.Training;

[Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin,Trainer")]
public class ModuleEditModel(ITrainingRepository repository) : PageModel
{
    [BindProperty]
    public ModuleInput Input { get; set; } = new();

    public int ProgramId => Input.ProgramId;

    public bool IsNew => Input.Id == 0;

    public async Task<IActionResult> OnGetAsync(int programId, int? id, CancellationToken cancellationToken)
    {
        Input.ProgramId = programId;
        if (id is null)
        {
            Input.SortOrder = 1;
            return Page();
        }

        var module = await repository.GetModuleByIdAsync(id.Value, cancellationToken);
        if (module is null)
        {
            return NotFound();
        }

        Input = new ModuleInput
        {
            Id = module.Id,
            ProgramId = module.ProgramId,
            Name = module.Name,
            Description = module.Description,
            TargetHours = module.TargetHours,
            SortOrder = module.SortOrder,
            IsRequired = module.IsRequired
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var module = new TrainingModule
        {
            Id = Input.Id,
            ProgramId = Input.ProgramId,
            Name = Input.Name.Trim(),
            Description = Input.Description?.Trim(),
            TargetHours = Input.TargetHours,
            SortOrder = Input.SortOrder,
            IsRequired = Input.IsRequired
        };

        if (Input.Id == 0)
        {
            await repository.CreateModuleAsync(module, cancellationToken);
        }
        else
        {
            await repository.UpdateModuleAsync(module, cancellationToken);
        }

        TempData["Success"] = "Module successfully saved.";
        return RedirectToPage("Details", new { id = Input.ProgramId });
    }

    public sealed class ModuleInput
    {
        public int Id { get; set; }

        [Range(1, int.MaxValue)]
        public int ProgramId { get; set; }

        [Required, StringLength(160)]
        public string Name { get; set; } = "";

        [StringLength(500)]
        public string? Description { get; set; }

        [Range(0.5, 1000)]
        public decimal TargetHours { get; set; } = 8;

        [Range(1, 999)]
        public int SortOrder { get; set; } = 1;

        public bool IsRequired { get; set; } = true;
    }
}
