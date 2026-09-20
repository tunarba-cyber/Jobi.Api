using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace LinkedIn.Modules.Jobs.Features.Categories;

/// <summary>
/// Turns "UI/UX Design" into "ui-ux-design". Handles the Azerbaijani and Turkish
/// characters (ə, ç, ğ, ı, ö, ş, ü) that a job board in Baku will absolutely hit.
/// </summary>
public static partial class SlugGenerator
{
    public static string Generate(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;

        var normalized = input.Trim().ToLowerInvariant();

        // Map characters that Unicode normalization does not decompose.
        var mapped = new StringBuilder(normalized.Length);
        foreach (var ch in normalized)
        {
            mapped.Append(ch switch
            {
                'ə' => "e",
                'ı' => "i",
                'ğ' => "g",
                'ş' => "s",
                'ç' => "c",
                'ö' => "o",
                'ü' => "u",
                'ß' => "ss",
                _ => ch.ToString()
            });
        }

        // Strip remaining diacritics (é -> e, ñ -> n).
        var decomposed = mapped.ToString().Normalize(NormalizationForm.FormD);
        var stripped = new StringBuilder(decomposed.Length);
        foreach (var ch in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
                stripped.Append(ch);
        }

        var slug = stripped.ToString().Normalize(NormalizationForm.FormC);
        slug = NonSlugCharacters().Replace(slug, "-");
        slug = MultipleHyphens().Replace(slug, "-");

        return slug.Trim('-');
    }

    [GeneratedRegex(@"[^a-z0-9]+")]
    private static partial Regex NonSlugCharacters();

    [GeneratedRegex(@"-{2,}")]
    private static partial Regex MultipleHyphens();
}
