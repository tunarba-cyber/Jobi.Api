using LinkedIn.Modules.Jobs.Domain.Entities;
using LinkedIn.Modules.Jobs.Features.Candidates.Dtos;
using LinkedIn.Modules.Jobs.Infrastructure.Persistence;
using LinkedIn.Shared.Abstractions.Primitives;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkedIn.Modules.Jobs.Features.Candidates.UpsertMyProfile;

internal sealed class UpsertMyProfileHandler : IRequestHandler<UpsertMyProfileCommand, Result<CandidateProfileDto>>
{
    private readonly JobsDbContext _db;

    public UpsertMyProfileHandler(JobsDbContext db) => _db = db;

    public async Task<Result<CandidateProfileDto>> Handle(UpsertMyProfileCommand request, CancellationToken cancellationToken)
    {
        var profile = await _db.CandidateProfiles
            .FirstOrDefaultAsync(c => c.OwnerUserId == request.RequestingUserId, cancellationToken);

        var isNew = profile is null;
        if (isNew)
        {
            profile = new CandidateProfile { OwnerUserId = request.RequestingUserId };

            var baseSlug = SlugGenerator.Generate(request.FullName);
            var slug = baseSlug;
            var suffix = 1;
            while (await _db.CandidateProfiles.AnyAsync(c => c.Slug == slug, cancellationToken))
                slug = $"{baseSlug}-{++suffix}";

            profile.Slug = slug;
            _db.CandidateProfiles.Add(profile);
        }

        profile!.FullName = request.FullName.Trim();
        profile.Headline = request.Headline.Trim();
        profile.Bio = request.Bio?.Trim();
        profile.Location = request.Location?.Trim();
        profile.PhotoUrl = request.PhotoUrl?.Trim();
        profile.ResumeUrl = request.ResumeUrl?.Trim();
        profile.Skills = request.Skills?.Trim();
        profile.ExperienceLevel = request.ExperienceLevel;
        profile.IsAvailableForWork = request.IsAvailableForWork;

        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success(new CandidateProfileDto(
            profile.Id, profile.Slug, profile.FullName, profile.Headline, profile.Bio,
            profile.Location, profile.PhotoUrl, profile.ResumeUrl, profile.Skills,
            profile.ExperienceLevel, profile.IsAvailableForWork));
    }
}