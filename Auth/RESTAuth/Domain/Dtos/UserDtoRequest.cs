namespace RESTAuth.Domain.Dtos;

public class UserDtoRequest
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string Department { get; set; }
}