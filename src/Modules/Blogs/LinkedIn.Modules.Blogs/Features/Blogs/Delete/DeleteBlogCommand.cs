using LinkedIn.Modules.Blogs.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkedIn.Modules.Blogs.Features.Blogs.Delete;

public sealed record DeleteBlogCommand(Guid Id) : IRequest;

internal sealed class DeleteBlogCommandHandler : IRequestHandler<DeleteBlogCommand>
{
    private readonly BlogsDbContext _db;

    public DeleteBlogCommandHandler(BlogsDbContext db) => _db = db;

    public async Task Handle(DeleteBlogCommand request, CancellationToken ct)
    {
        var blog = await _db.Blogs.FirstOrDefaultAsync(b => b.Id == request.Id, ct)
            ?? throw new KeyNotFoundException($"Blog '{request.Id}' was not found.");

        blog.SoftDelete();
        await _db.SaveChangesAsync(ct);
    }
}
