namespace Hypesoft.Infrastructure.Data;

public class MongoDBSettings
{
    public const string SectionName = "MongoDBSettings";
    
    public string DatabaseName { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
}
