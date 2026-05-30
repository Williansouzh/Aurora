namespace Aurora.Infrastructure.Storage;

public class StorageSettings
{
    public string Endpoint { get; set; } = "localhost:9000";
    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string BucketName { get; set; } = "aurora-uploads";
    public bool UseSSL { get; set; }
    public string PublicUrl { get; set; } = "http://localhost:9000";
}
