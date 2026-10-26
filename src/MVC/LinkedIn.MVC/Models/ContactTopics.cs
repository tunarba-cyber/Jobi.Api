namespace LinkedIn.MVC.Models;

public static class ContactTopics
{
    public static readonly IReadOnlyList<(string Code, string Label)> All = new[]
    {
        ("General", "General question"),
        ("Account", "Problem with my account"),
        ("Bug", "Report a bug"),
        ("ReportUser", "Report a user"),
        ("ReportJob", "Report a job posting"),
        ("ReportPost", "Report a post"),
        ("ReportCompany", "Report a company"),
        ("Other", "Something else")
    };

    public static string Label(string code) => All.FirstOrDefault(t => t.Code == code).Label ?? code;
}