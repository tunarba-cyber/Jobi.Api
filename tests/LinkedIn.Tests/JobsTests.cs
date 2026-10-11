using LinkedIn.Modules.Jobs.Domain.Entities;
using LinkedIn.Modules.Jobs.Domain.Enums;
using LinkedIn.Modules.Jobs.Features;                       // SlugGenerator (adjust if yours lives elsewhere)
using LinkedIn.Modules.Jobs.Features.Applications.ApplyToJob;
using LinkedIn.Modules.Jobs.Features.Categories.CreateCategory;
using LinkedIn.Modules.Jobs.Infrastructure.Persistence;
using LinkedIn.Shared.Abstractions.Primitives;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LinkedIn.Tests;

public class JobsTests
{
    // InMemory does not enforce unique indexes or SQL filters, so those DB-level
    // guards are not covered here - only handler logic and validators.
    private static JobsDbContext NewDb() =>
        new(new DbContextOptionsBuilder<JobsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    [Theory]
    [InlineData("UI/UX Design", "ui-ux-design")]
    [InlineData("  Marketing  ", "marketing")]
    [InlineData("Şəkər Çay", "seker-cay")]
    public void SlugGenerator_produces_url_safe_slugs(string input, string expected) =>
        Assert.Equal(expected, SlugGenerator.Generate(input));

    [Fact]
    public void CreateCategory_validator_rejects_empty_name_and_bad_slug()
    {
        var v = new CreateCategoryCommandValidator();
        Assert.False(v.Validate(new CreateCategoryCommand("", null, null, null, 0, false)).IsValid);
        Assert.False(v.Validate(new CreateCategoryCommand("A", "Bad Slug!", null, null, 0, false)).IsValid);
        Assert.True(v.Validate(new CreateCategoryCommand("A", "good-slug", null, null, 0, false)).IsValid);
    }

    [Fact]
    public async Task CreateCategory_rejects_duplicate()
    {
        await using var db = NewDb();
        var handler = new CreateCategoryHandler(db);
        var cmd = new CreateCategoryCommand("Development", null, null, null, 1, true);

        var first = await handler.Handle(cmd, default);
        var second = await handler.Handle(cmd, default);

        Assert.True(first.IsSuccess);
        Assert.Equal("development", first.Value.Slug);
        Assert.True(second.IsFailure);
        Assert.Equal(ErrorType.Conflict, second.Error.Type);
    }

    private static async Task<long> SeedJobAsync(JobsDbContext db, JobStatus status)
    {
        var category = new Category { Name = "Dev", Slug = "dev" };
        var company = new Company { Name = "Acme", Slug = "acme", OwnerUserId = "owner" };
        db.AddRange(category, company);
        await db.SaveChangesAsync();

        var job = new Job
        {
            Title = "Dev",
            Slug = "dev-job",
            Description = "d",
            Location = "Baku",
            CategoryId = category.Id,
            CompanyId = company.Id,
            Status = status
        };
        db.Jobs.Add(job);
        await db.SaveChangesAsync();
        return job.Id;
    }

    private static ApplyToJobCommand Apply(long jobId, string user = "c1") =>
        new(jobId, user, "Cand", "c@x.test", null, null);

    [Fact]
    public async Task Apply_succeeds_for_active_job()
    {
        await using var db = NewDb();
        var jobId = await SeedJobAsync(db, JobStatus.Active);

        var result = await new ApplyToJobHandler(db).Handle(Apply(jobId), default);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, await db.Applications.CountAsync());
    }

    [Fact]
    public async Task Apply_rejects_closed_job()
    {
        await using var db = NewDb();
        var jobId = await SeedJobAsync(db, JobStatus.Closed);

        var result = await new ApplyToJobHandler(db).Handle(Apply(jobId), default);

        Assert.True(result.IsFailure);
        Assert.Equal("Application.JobNotOpen", result.Error.Code);
    }

    [Fact]
    public async Task Apply_rejects_duplicate_application()
    {
        await using var db = NewDb();
        var jobId = await SeedJobAsync(db, JobStatus.Active);
        var handler = new ApplyToJobHandler(db);

        await handler.Handle(Apply(jobId), default);
        var again = await handler.Handle(Apply(jobId), default);

        Assert.True(again.IsFailure);
        Assert.Equal("Application.AlreadyApplied", again.Error.Code);
    }

    [Fact]
    public async Task Apply_rejects_unknown_job()
    {
        await using var db = NewDb();
        var result = await new ApplyToJobHandler(db).Handle(Apply(999), default);
        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }
}