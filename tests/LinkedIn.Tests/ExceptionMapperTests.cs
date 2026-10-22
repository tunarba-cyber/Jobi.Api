using LinkedIn.Shared.Infrastructure.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LinkedIn.Tests;

public class ExceptionMapperTests
{
    // Stands in for Microsoft.Data.SqlClient.SqlException, which can't be constructed in a test.
    private sealed class SqlException : Exception { public int Number { get; init; } }

    [Fact]
    public void Missing_item_is_404() =>
        Assert.Equal(404, ExceptionMapper.Map(new KeyNotFoundException("Blog 'x' was not found."))!.Status);

    [Theory]
    [InlineData(2601)]
    [InlineData(2627)]
    public void Unique_violation_is_409(int number) =>
        Assert.Equal(409, ExceptionMapper.Map(new DbUpdateException("x", new SqlException { Number = number }))!.Status);

    [Fact]
    public void Other_database_errors_stay_unexpected() =>
        Assert.Null(ExceptionMapper.Map(new DbUpdateException("x", new SqlException { Number = 547 })));

    [Fact]
    public void Concurrency_conflict_is_409() =>
        Assert.Equal(409, ExceptionMapper.Map(new DbUpdateConcurrencyException("x", new Exception()))!.Status);

    [Fact]
    public void Malformed_request_keeps_its_status() =>
        Assert.Equal(400, ExceptionMapper.Map(new BadHttpRequestException("bad", 400))!.Status);

    [Fact]
    public void Timeout_is_504() =>
        Assert.Equal(504, ExceptionMapper.Map(new TimeoutException())!.Status);

    [Fact]
    public void Unknown_exception_is_unexpected() =>
        Assert.Null(ExceptionMapper.Map(new InvalidOperationException("boom")));
}