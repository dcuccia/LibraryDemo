namespace Library.WebApi.Models;

public class DeletionFailureResponse
{
    public DeletionFailureResponse() { }
    public DeletionFailureResponse(params string[] errors) { Errors = errors; }
    public IEnumerable<string> Errors { get; init; } = Enumerable.Empty<string>();
}