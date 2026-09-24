using System.Linq.Expressions;
using LinkedIn.Modules.Jobs.Domain.Entities;
using LinkedIn.Modules.Jobs.Features.Applications.Dtos;

namespace LinkedIn.Modules.Jobs.Features.Applications.Dtos;

internal static class ApplicationMappings
{
    // Since JobId is a real FK here, the DTO reads Job/Company live via Include()
    // rather than a denormalized snapshot - always shows the current job title.
    public static readonly Expression<Func<Application, ApplicationDto>> ToDto =
        a => new ApplicationDto(
            a.Id,
            a.JobId,
            a.Job!.Title,
            a.Job.CompanyId,
            a.Job.Company!.Name,
            a.CandidateUserId,
            a.CandidateName,
            a.CandidateEmail,
            a.ResumeUrl,
            a.CoverLetter,
            a.Status,
            a.CreatedAtUtc);
}