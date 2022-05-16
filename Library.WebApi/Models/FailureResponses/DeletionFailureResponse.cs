namespace Library.WebApi.Models.FailureResponses;

public class DeletionFailureResponse
{
    public DeletionFailureResponse() { }
    public DeletionFailureResponse(params string[] errors) { Errors = errors; }
    public IEnumerable<string> Errors { get; init; } = Enumerable.Empty<string>();
}