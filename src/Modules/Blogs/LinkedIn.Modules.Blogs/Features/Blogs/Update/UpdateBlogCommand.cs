using FluentValidation;
using LinkedIn.Modules.Blogs.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkedIn.Modules.Blogs.Features.Blogs.Update;

public sealed record UpdateBlogCommand(
    Guid Id,
    string Title,
    string Summary,
    string Content,
    Guid CategoryId,
    string? ImageUrl,
    bool IsFeatured) : IRequest;

public sealed class UpdateBlogCommandValidator : AbstractValidator<UpdateBlogCommand>
{
    public UpdateBlogCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Summary).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Content).NotEmpty();
        RuleFor(x => x.CategoryId).NotEmpty();
    }
}

internal sealed class UpdateBlogCommandHandler : IRequestHandler<UpdateBlogCommand>
{
    private readonly BlogsDbContext _db;

    public UpdateBlogCommandHandler(BlogsDbContext db) => _db = db;

    public async Task Handle(UpdateBlogCommand request, CancellationToken ct)
    {
        var blog = await _db.Blogs.FirstOrDefaultAsync(b => b.Id == request.Id, ct)
            ?? throw new KeyNotFoundException($"Blog '{request.Id}' was not found.");

        blog.Update(request.Title, request.Summary, request.Content, request.CategoryId, request.ImageUrl, request.IsFeatured);
        await _db.SaveChangesAsync(ct);
    }
}
