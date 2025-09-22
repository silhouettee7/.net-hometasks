namespace RESTAuth.Domain.Dtos;

public class UserDtoResponse
{
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
}