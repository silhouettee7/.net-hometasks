namespace RESTAuth.Domain.Entities;

public class User: Entity<Guid>
{
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
    public string Name { get; set; }
    public string Email { get; set; } 
    public string Password { get; set; }
    public string Role { get; set; }
    public decimal Salary { get; set; }
    public string Department { get; set; }
}