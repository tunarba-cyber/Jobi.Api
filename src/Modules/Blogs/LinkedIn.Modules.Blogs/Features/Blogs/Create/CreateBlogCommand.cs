using FluentValidation;
using LinkedIn.Modules.Blogs.Domain.Entities;
using LinkedIn.Modules.Blogs.Infrastructure;
using LinkedIn.Modules.Blogs.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using LinkedIn.Shared.Infrastructure.Persistence;
namespace LinkedIn.Modules.Blogs.Features.Blogs.Create;

public sealed record CreateBlogCommand(
    string Title,
    string Summary,
    string Content,
    Guid CategoryId,
    string? ImageUrl,
    bool IsFeatured = false,
    bool PublishNow = true) : IRequest<Guid>;

public sealed class CreateBlogCommandValidator : AbstractValidator<CreateBlogCommand>
{
    public CreateBlogCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Summary).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Content).NotEmpty();
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.ImageUrl).MaximumLength(500);
    }
}

internal sealed class CreateBlogCommandHandler : IRequestHandler<CreateBlogCommand, Guid>
{
    private readonly BlogsDbContext _db;
    private readonly ICurrentUser _currentUser;

    public CreateBlogCommandHandler(BlogsDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(CreateBlogCommand request, CancellationToken ct)
    {
        var categoryExists = await _db.Categories.AnyAsync(c => c.Id == request.CategoryId, ct);
        if (!categoryExists)
            throw new InvalidOperationException($"Category '{request.CategoryId}' was not found.");

        var baseSlug = SlugGenerator.Generate(request.Title);
        var slug = SlugGenerator.EnsureUnique(baseSlug, candidate => _db.Blogs.Any(b => b.Slug == candidate));

        var blog = Blog.Create(
            request.Title,
            slug,
            request.Summary,
            request.Content,
            request.CategoryId,
            _currentUser.UserId!,
            request.ImageUrl,
            request.IsFeatured,
            request.PublishNow);

        _db.Blogs.Add(blog);
        await _db.SaveChangesAsync(ct);

        return blog.Id;
    }
}



