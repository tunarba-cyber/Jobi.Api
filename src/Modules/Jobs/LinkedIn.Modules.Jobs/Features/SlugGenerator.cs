using System.Text.RegularExpressions;

namespace LinkedIn.Modules.Jobs.Features;

internal static partial class SlugGenerator
{
    public static string Generate(string input)
    {
        var slug = input.Trim().ToLowerInvariant();
        slug = NonSlugCharacters().Replace(slug, "-");
        slug = MultipleHyphens().Replace(slug, "-");
        return slug.Trim('-');
    }

    [GeneratedRegex(@"[^a-z0-9]+")]
    private static partial Regex NonSlugCharacters();

    [GeneratedRegex(@"-{2,}")]
    private static partial Regex MultipleHyphens();
}