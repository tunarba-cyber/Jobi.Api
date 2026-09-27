using System.ComponentModel.DataAnnotations;

namespace LinkedIn.MVC.Models.ViewModels;

public sealed class RegisterViewModel
{
    [Required] public string FirstName { get; set; } = string.Empty;
    [Required] public string LastName { get; set; } = string.Empty;
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required, DataType(DataType.Password)] public string Password { get; set; } = string.Empty;

    /// <summary>"Candidate" or "Employer" - matches RegisterCommand.Role exactly, parsed server-side.</summary>
    public string Role { get; set; } = "Candidate";
}

public sealed class LoginViewModel
{
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required, DataType(DataType.Password)] public string Password { get; set; } = string.Empty;
    public bool RememberMe { get; set; }
    public string? ReturnUrl { get; set; }
}
public sealed record ConfirmEmailResultViewModel(bool Success, string? ErrorMessage);