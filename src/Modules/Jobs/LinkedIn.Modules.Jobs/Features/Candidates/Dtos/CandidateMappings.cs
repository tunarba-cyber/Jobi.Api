using System.Linq.Expressions;
using LinkedIn.Modules.Jobs.Domain.Entities;
using LinkedIn.Modules.Jobs.Features.Candidates.Dtos;

namespace LinkedIn.Modules.Jobs.Features.Candidates.Dtos;

internal static class CandidateMappings
{
    public static readonly Expression<Func<CandidateProfile, CandidateProfileDto>> ToDto =
        c => new CandidateProfileDto(
            c.Id, c.Slug, c.FullName, c.Headline, c.Bio, c.Location,
            c.PhotoUrl, c.ResumeUrl, c.Skills, c.ExperienceLevel, c.IsAvailableForWork);
}