namespace Library.WebApi.Models.FailureResponses;

public class UpdateFailureResponse
{
    public UpdateFailureResponse() { }
    public UpdateFailureResponse(params string[] errors) { Errors = errors; }
    public IEnumerable<string> Errors { get; init; } = Enumerable.Empty<string>();
}