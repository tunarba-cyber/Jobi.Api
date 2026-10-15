namespace LinkedIn.MVC.Models.Api;

public sealed record AiJobDescriptionRequest(string Title, string? CompanyName, string? Location, string? JobType, string? ExperienceLevel, string? KeyPoints, string? Language);
public sealed record AiCoverLetterRequest(string JobTitle, string? CompanyName, string? JobDescription, string Background, string? Language);
public sealed record AiImproveBioRequest(string Bio, string? Language);
public sealed record AiTextDto(string Text);