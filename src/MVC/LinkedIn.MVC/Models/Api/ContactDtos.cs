namespace LinkedIn.MVC.Models.Api;

public sealed record ContactRequest(string Name, string Email, string? Subject, string Message);
