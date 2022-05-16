namespace Library.WebApi.Models.FailureResponses;

public class CreationFailureResponse
{
    public CreationFailureResponse() { }
    public CreationFailureResponse(params string[] errors) { Errors = errors; }
    public IEnumerable<string> Errors { get; init; } = Enumerable.Empty<string>();
}