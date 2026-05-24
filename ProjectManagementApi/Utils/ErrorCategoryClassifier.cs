namespace ProjectManagementApi.Utils
{
    public static class ErrorCategoryClassifier
    {
        public static string Classify(Exception? exception, int? statusCode)
        {
            if (statusCode is 401 or 403)
            {
                return "AUTH";
            }

            if (exception == null)
            {
                return "UNKNOWN";
            }

            var message = exception.GetBaseException().Message ?? string.Empty;
            var lowered = message.ToLowerInvariant();
            var exceptionName = exception.GetType().Name.ToLowerInvariant();

            if (lowered.Contains("ibm") ||
                lowered.Contains("informix") ||
                lowered.Contains("sql") ||
                lowered.Contains("database") ||
                exceptionName.Contains("sql") ||
                exceptionName.Contains("db"))
            {
                return "DB";
            }

            if (exceptionName.Contains("unauthorized") ||
                exceptionName.Contains("security") ||
                lowered.Contains("token") ||
                lowered.Contains("credencial") ||
                lowered.Contains("auth"))
            {
                return "AUTH";
            }

            return "APP";
        }
    }
}
