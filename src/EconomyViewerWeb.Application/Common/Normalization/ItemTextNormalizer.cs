using System.Text.RegularExpressions;
using EconomyViewerWeb.Application.Exceptions;

namespace EconomyViewerWeb.Application.Common.Normalization;


public static class ItemTextNormalizer
{
    private static readonly Regex MultipleWhitespaceRegex = new(
        @"\s+",
        RegexOptions.Compiled);

    public static string NormalizeRequired(string? value, string fieldName)
    {
        var normalizedValue = value?.Trim();

        if (string.IsNullOrWhiteSpace(normalizedValue))
        {
            throw new ValidationException(
                $"{fieldName} cannot be empty.");
        }

        return MultipleWhitespaceRegex.Replace(
            normalizedValue,
            " ");
    }
}
