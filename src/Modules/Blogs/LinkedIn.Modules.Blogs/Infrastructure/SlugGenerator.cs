using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace LinkedIn.Modules.Blogs.Infrastructure;

public static partial class SlugGenerator
{
    private static readonly Dictionary<char, string> AzMap = new()
    {
        ['ə'] = "e", ['ı'] = "i", ['ö'] = "o", ['ü'] = "u",
        ['ğ'] = "g", ['ş'] = "s", ['ç'] = "c"
    };

    public static string Generate(string title)
    {
        var lowered = title.ToLower(CultureInfo.GetCultureInfo("az-Latn-AZ"));

        var sb = new StringBuilder();
        foreach (var ch in lowered)
            sb.Append(AzMap.TryGetValue(ch, out var replacement) ? replacement : ch);

        var normalized = sb.ToString().Normalize(NormalizationForm.FormD);
        var withoutDiacritics = new string(normalized
            .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            .ToArray());

        var slug = NonAlphanumeric().Replace(withoutDiacritics, "-");
        slug = MultipleDashes().Replace(slug, "-").Trim('-');

        return slug.Length > 200 ? slug[..200].Trim('-') : slug;
    }

    /// <summary>Appends -2, -3, ... until the slug is free.</summary>
    public static string EnsureUnique(string slug, Func<string, bool> exists)
    {
        if (!exists(slug)) return slug;

        var i = 2;
        while (exists($"{slug}-{i}")) i++;
        return $"{slug}-{i}";
    }

    [GeneratedRegex("[^a-z0-9]+")]
    private static partial Regex NonAlphanumeric();

    [GeneratedRegex("-{2,}")]
    private static partial Regex MultipleDashes();
}
