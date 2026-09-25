namespace LinkedIn.MVC.Models.Api;

// Mirrors LinkedIn.Modules.Jobs.Domain.Enums. Kept as a copy rather than a
// project reference so the MVC app stays decoupled from the API internals -
// it only knows the HTTP contract.

public enum JobType
{
    FullTime,
    PartTime,
    Remote,
    Freelance,
    Internship,
    Contract
}

public enum ExperienceLevel
{
    EntryLevel,
    Intermediate,
    Senior,
    Lead
}

public enum JobStatus
{
    Draft,
    Active,
    Closed
}

public static class EnumDisplay
{
    public static string ToLabel(this JobType value) => value switch
    {
        JobType.FullTime => "Full time",
        JobType.PartTime => "Part time",
        JobType.Remote => "Remote",
        JobType.Freelance => "Freelance",
        JobType.Internship => "Internship",
        JobType.Contract => "Contract",
        _ => value.ToString()
    };

    public static string ToLabel(this ExperienceLevel value) => value switch
    {
        ExperienceLevel.EntryLevel => "Entry level",
        ExperienceLevel.Intermediate => "Intermediate",
        ExperienceLevel.Senior => "Senior",
        ExperienceLevel.Lead => "Lead",
        _ => value.ToString()
    };
}
