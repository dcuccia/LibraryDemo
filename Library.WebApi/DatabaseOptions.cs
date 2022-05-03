namespace Library.WebApi;

// public record DatabaseOptions(string ServiceType, string ConnectionString);
public record DatabaseOptions
{
    public string ServiceType { get; init; } = "";
    public string ConnectionString { get; init; } = "";
}