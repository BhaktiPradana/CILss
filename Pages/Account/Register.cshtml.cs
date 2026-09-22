using System.ComponentModel.DataAnnotations;
using LssTraining.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LssTraining.Web.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly IAuthRepository _authRepository;

        public RegisterModel(IAuthRepository authRepository)
        {
            _authRepository = authRepository;
        }

        [BindProperty]
        public RegisterInputModel Input { get; set; } = new();

        public string? ErrorMessage { get; set; }
        public bool IsSuccess { get; set; }

        public class RegisterInputModel
        {
            [Required(ErrorMessage = "Full Name is required.")]
            public string DisplayName { get; set; } = string.Empty;

            [Required(ErrorMessage = "Username is required.")]
            public string Username { get; set; } = string.Empty;

            [Required(ErrorMessage = "Password is required.")]
            [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
            public string Password { get; set; } = string.Empty;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var success = await _authRepository.CreateUserAsync(Input.Username, Input.DisplayName, Input.Password, cancellationToken);

            if (!success)
            {
                ErrorMessage = "Username is already taken. Please choose another one.";
                return Page();
            }

            IsSuccess = true;
            return Page();
        }
    }
}