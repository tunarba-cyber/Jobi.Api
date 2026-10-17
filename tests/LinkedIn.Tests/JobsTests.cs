using LinkedIn.Modules.Jobs.Domain.Entities;
using LinkedIn.Modules.Jobs.Domain.Enums;
using LinkedIn.Modules.Jobs.Features;                       // SlugGenerator (adjust if yours lives elsewhere)
using LinkedIn.Modules.Jobs.Features.Applications.ApplyToJob;
using LinkedIn.Modules.Jobs.Features.Categories.CreateCategory;
using LinkedIn.Modules.Jobs.Infrastructure.Persistence;
using LinkedIn.Shared.Abstractions.Primitives;
using Microsoft.EntityFrameworkCore;
using A = LinkedIn.Shared.Abstractions.Contracts;
using Xunit;

namespace LinkedIn.Tests;

internal sealed class FakeNotifier : A.INotificationPublisher
{
    public List<(string UserId, string Type)> Sent { get; } = new();

    public Task PublishAsync(string userId, string type, string title, string? body, string? link,
        string? groupKey = null, CancellationToken ct = default)
    {
        Sent.Add((userId, type));
        return Task.CompletedTask;
    }
}
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
    [InlineData("İstanbul Ğ Ö Ü", "istanbul-g-o-u")]
    [InlineData("Café  Menu!!", "cafe-menu")]

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
    public async Task Apply_succeeds_and_notifies_the_company_owner()
    {
        await using var db = NewDb();
        var jobId = await SeedJobAsync(db, JobStatus.Active);
        var notifier = new FakeNotifier();

        var result = await new ApplyToJobHandler(db, notifier).Handle(Apply(jobId), default);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, await db.Applications.CountAsync());
        Assert.Contains(("owner", A.NotificationTypes.NewApplication), notifier.Sent);
    }

    [Fact]
    public async Task Apply_rejection_does_not_notify()
    {
        await using var db = NewDb();
        var jobId = await SeedJobAsync(db, JobStatus.Closed);
        var notifier = new FakeNotifier();

        await new ApplyToJobHandler(db, notifier).Handle(Apply(jobId), default);

        Assert.Empty(notifier.Sent);
    }

    [Fact]
    public async Task Apply_rejects_closed_job()
    {
        await using var db = NewDb();
        var jobId = await SeedJobAsync(db, JobStatus.Closed);

        var handler = new ApplyToJobHandler(db, new FakeNotifier());
        var result = await handler.Handle(Apply(jobId), default);

        Assert.True(result.IsFailure);
        Assert.Equal("Application.JobNotOpen", result.Error.Code);
    }

    [Fact]
    public async Task Apply_rejects_duplicate_application()
    {
        await using var db = NewDb();
        var jobId = await SeedJobAsync(db, JobStatus.Active);
        var handler = new ApplyToJobHandler(db, new FakeNotifier());

        await handler.Handle(Apply(jobId), default);
        var again = await handler.Handle(Apply(jobId), default);

        Assert.True(again.IsFailure);
        Assert.Equal("Application.AlreadyApplied", again.Error.Code);
    }

    [Fact]
    public async Task Apply_rejects_unknown_job()
    {
        await using var db = NewDb();
        var handler = new ApplyToJobHandler(db, new FakeNotifier());
        var result = await handler.Handle(Apply(999), default);
        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }
}