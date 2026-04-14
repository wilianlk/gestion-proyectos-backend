

namespace ProjectManagementApi.Utils.Helpers;

public class AppSettings
{
    public Jwt Jwt { get; set; }
    public FileUpload FileUpload { get; set; }
}

public class Jwt
{
    public string Issuer { get; set; }
    public string Audience { get; set; }
    public string Key { get; set; }
    public double Expired { get; set; }
}

public class FileUpload
{
    public int MaxSizeMB { get; set; }
    public List<string> AllowedExtensions { get; set; }
    public List<string> AllowedContentTypes { get; set; }
}
