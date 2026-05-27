namespace Backend.Application.Common.Settings;

public class RedisSettings
{
    public string Configuration { get; set; } = string.Empty;
    public string InstanceName { get; set; } = "BackendCache:";
}
