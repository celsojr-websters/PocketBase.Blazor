namespace PocketBase.Blazor.UnitTests.TestHelpers.Extensions;

using System.Text;
using System.Text.RegularExpressions;

public static class StringExtensions
{
    public static string ToSlug(this string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

            string slug = text.ToLowerInvariant();
        
        // Replace invalid characters with hyphens
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        
        // Replace multiple spaces with single space
        slug = Regex.Replace(slug, @"\s+", " ").Trim();
        
        // Replace spaces with hyphens
        slug = slug.Replace(" ", "-");
        
        return slug;
    }

    public static string ToBase64(this string text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        byte[] bytes = Encoding.UTF8.GetBytes(text);
        return Convert.ToBase64String(bytes);
    }

    public static string FromBase64(this string base64Text)
    {
        if (string.IsNullOrEmpty(base64Text))
            return string.Empty;

        byte[] bytes = Convert.FromBase64String(base64Text);
        return Encoding.UTF8.GetString(bytes);
    }
}
