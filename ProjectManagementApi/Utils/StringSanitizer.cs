using System.Globalization;
using System.Text;

namespace ProjectManagementApi.Utils
{
    /// <summary>
    /// Utility class for sanitizing strings to ensure compatibility with Informix database.
    /// </summary>
    public static class StringSanitizer
    {
        /// <summary>
        /// Sanitizes text by transliterating common Unicode characters into ASCII-safe values.
        /// This avoids Informix code-set conversion errors when users send accented characters,
        /// smart quotes, arrows, bullets, or similar symbols.
        /// </summary>
        /// <param name="text">Text to sanitize.</param>
        /// <returns>ASCII-safe text that preserves line breaks and tabs.</returns>
        public static string SanitizeForInformix(string? text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text ?? string.Empty;
            }

            text = text.Replace("â€“", "-")
                       .Replace("â€”", "-")
                       .Replace("â€", "-")
                       .Replace("â€‘", "-")
                       .Replace("âˆ’", "-")
                       .Replace("Â·", ".")
                       .Replace("–", "-")
                       .Replace("—", "-")
                       .Replace("‐", "-")
                       .Replace("‑", "-")
                       .Replace("−", "-")
                       .Replace("•", "-")
                       .Replace("·", ".")
                       .Replace("“", "\"")
                       .Replace("”", "\"")
                       .Replace("„", "\"")
                       .Replace("’", "'")
                       .Replace("‘", "'")
                       .Replace("´", "'")
                       .Replace("`", "'")
                       .Replace("→", "->")
                       .Replace("←", "<-")
                       .Replace("↔", "<->")
                       .Replace("º", "o")
                       .Replace("ª", "a");

            var normalized = text.Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder(normalized.Length);

            foreach (var character in normalized)
            {
                var category = CharUnicodeInfo.GetUnicodeCategory(character);

                if (category == UnicodeCategory.NonSpacingMark)
                {
                    continue;
                }

                if (character == '\r' || character == '\n' || character == '\t')
                {
                    builder.Append(character);
                    continue;
                }

                if (character >= 32 && character <= 126)
                {
                    builder.Append(character);
                    continue;
                }

                if (char.IsWhiteSpace(character))
                {
                    builder.Append(' ');
                }
            }

            return builder.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}
