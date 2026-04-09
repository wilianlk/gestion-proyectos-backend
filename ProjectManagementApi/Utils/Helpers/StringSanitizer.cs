namespace ProjectManagementApi.Utils.Helpers
{
    /// <summary>
    /// Utility class for sanitizing strings to ensure compatibility with Informix database
    /// </summary>
    public static class StringSanitizer
    {
        /// <summary>
        /// Sanitizes text by replacing Unicode special characters with ASCII equivalents.
        /// This prevents "Code-set conversion failed" errors in Informix when writing data.
        /// </summary>
        /// <param name="text">Text to sanitize</param>
        /// <returns>Sanitized text with Unicode characters replaced by ASCII equivalents</returns>
        public static string SanitizeForInformix(string? text)
        {
            if (string.IsNullOrEmpty(text))
                return text ?? string.Empty;

            // Replace common Unicode dashes with regular hyphen
            text = text.Replace("–", "-")   // en-dash (U+2013)
                       .Replace("—", "-")   // em-dash (U+2014)
                       .Replace("‐", "-")   // hyphen (U+2010)
                       .Replace("‑", "-")   // non-breaking hyphen (U+2011)
                       .Replace("−", "-")   // minus sign (U+2212)
                       .Replace("·", ".");  // middle dot to period

            return text;
        }
    }
}
