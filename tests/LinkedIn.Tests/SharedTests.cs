using FluentValidation;
using LinkedIn.Shared.Abstractions.Paging;
using LinkedIn.Shared.Abstractions.Primitives;
using Error = LinkedIn.Shared.Abstractions.Primitives.Error;
using LinkedIn.Shared.Infrastructure.Behaviors;
using MediatR;
using System.ComponentModel.DataAnnotations;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Xunit;
namespace LinkedIn.Tests;

public class SharedTests
{
    private sealed record Q : PagedQuery;
    private sealed record Ping(string Name) : IRequest<string>;
    private sealed class PingValidator : AbstractValidator<Ping> { public PingValidator() => RuleFor(p => p.Name).NotEmpty(); }

    [Fact]
    public void Result_success_cannot_carry_error() =>
        Assert.Throws<InvalidOperationException>(() => Result.Failure(Error.None));

    [Fact]
    public void Result_failure_value_access_throws() =>
        Assert.Throws<InvalidOperationException>(() => Result.Failure<int>(Error.NotFound("x", "y")).Value);

    [Theory]
    [InlineData(0, 1)]
    [InlineData(-5, 1)]
    [InlineData(3, 3)]
    public void PagedQuery_clamps_page(int input, int expected) =>
        Assert.Equal(expected, (new Q { Page = input }).Page);

    [Theory]
    [InlineData(0, 10)]
    [InlineData(1000, 100)]
    [InlineData(25, 25)]
    public void PagedQuery_clamps_page_size(int input, int expected) =>
        Assert.Equal(expected, (new Q { PageSize = input }).PageSize);

    [Fact]
    public void PagedResult_computes_pages()
    {
        var r = new PagedResult<int>(new[] { 1 }, page: 2, pageSize: 10, totalCount: 25);
        Assert.Equal(3, r.TotalPages);
        Assert.True(r.HasNextPage);
        Assert.True(r.HasPreviousPage);
    }

    [Fact]
    public async Task ValidationBehavior_rejects_invalid_request()
    {
        var behavior = new ValidationBehavior<Ping, string>(new[] { new PingValidator() });
        await Assert.ThrowsAsync<FluentValidation.ValidationException>(() =>
    behavior.Handle(new Ping(""), () => Task.FromResult("ok"), CancellationToken.None));
    }

    [Fact]
    public async Task ValidationBehavior_passes_valid_request()
    {
        var behavior = new ValidationBehavior<Ping, string>(new[] { new PingValidator() });
        var result = await behavior.Handle(new Ping("a"), () => Task.FromResult("ok"), CancellationToken.None);
        Assert.Equal("ok", result);
    }
}