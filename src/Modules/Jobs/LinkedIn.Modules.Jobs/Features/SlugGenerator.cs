using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace LinkedIn.Modules.Jobs.Features;

public static partial class SlugGenerator
{
    public static string Generate(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;

        // Letters that Unicode normalization can't decompose, in both cases.
        var mapped = new StringBuilder(input.Length);
        foreach (var ch in input.Trim())
        {
            mapped.Append(ch switch
            {
                'ə' or 'Ə' => "e",
                'ı' or 'İ' => "i",
                'ğ' or 'Ğ' => "g",
                'ş' or 'Ş' => "s",
                'ç' or 'Ç' => "c",
                'ö' or 'Ö' => "o",
                'ü' or 'Ü' => "u",
                'ß' => "ss",
                'ж' or 'Ж' => "zh",
                _ => ch.ToString()
            });
        }

        // Strip remaining accents (é -> e).
        var decomposed = mapped.ToString().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var plain = new StringBuilder(decomposed.Length);
        foreach (var ch in decomposed)
            if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
                plain.Append(ch);

        var slug = NonSlugCharacters().Replace(plain.ToString(), "-");
        slug = MultipleHyphens().Replace(slug, "-");
        return slug.Trim('-');
    }

    [GeneratedRegex(@"[^a-z0-9]+")]
    private static partial Regex NonSlugCharacters();

    [GeneratedRegex(@"-{2,}")]
    private static partial Regex MultipleHyphens();
}