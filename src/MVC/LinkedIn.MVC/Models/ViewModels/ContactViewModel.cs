using System.ComponentModel.DataAnnotations;

namespace LinkedIn.MVC.Models.ViewModels;

public sealed class ContactViewModel
{
    [Required(ErrorMessage = "Name is required.")]
    [MaxLength(100, ErrorMessage = "Name must be 100 characters or fewer.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    [MaxLength(255, ErrorMessage = "Email must be 255 characters or fewer.")]
    public string Email { get; set; } = string.Empty;

    [MaxLength(200, ErrorMessage = "Subject must be 200 characters or fewer.")]
    public string? Subject { get; set; }

    [Required(ErrorMessage = "Message is required.")]
    [MaxLength(4000, ErrorMessage = "Message must be 4000 characters or fewer.")]
    public string Message { get; set; } = string.Empty;
}