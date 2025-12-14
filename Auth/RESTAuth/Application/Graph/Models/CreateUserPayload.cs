namespace RESTAuth.Application.Graph.Models;

public class CreateUserPayload
{
    public IEnumerable<string>? Errors { get; set; }
    public Guid? Id { get; set; }
}