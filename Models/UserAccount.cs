namespace LssTraining.Web.Models;

public sealed class UserAccount
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "Trainer";
    public bool IsActive { get; set; } = true;
}
