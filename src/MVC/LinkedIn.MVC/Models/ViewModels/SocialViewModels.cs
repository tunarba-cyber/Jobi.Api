using System.ComponentModel.DataAnnotations;
using LinkedIn.MVC.Models.Api;
using Microsoft.AspNetCore.Http;

namespace LinkedIn.MVC.Models.ViewModels;

public sealed class CompaniesListViewModel
{
    public required PagedResult<CompanyListItemDto> Companies { get; init; }
    public string? Search { get; init; }
}

public sealed class PostsFeedViewModel
{
    public required PagedResult<PostDto> Posts { get; init; }
    public string Scope { get; init; } = "all";
    public string? AuthorId { get; init; }
    public IReadOnlyList<SuggestionDto> Suggestions { get; init; } = Array.Empty<SuggestionDto>();
}

public sealed class PostDetailsViewModel
{
    public required PostDto Post { get; init; }
    public IReadOnlyList<CommentDto> Comments { get; init; } = Array.Empty<CommentDto>();
}

public sealed class PostFormModel
{
    [MaxLength(2000, ErrorMessage = "A post can be at most 2000 characters.")]
    public string? Caption { get; set; }
    public IFormFile? Image { get; set; }
}