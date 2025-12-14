namespace RESTAuth.Domain.Dtos;

public abstract class AbstractLoginDto
{
    public string Email { get; set; }
    public string Password { get; set; }
}