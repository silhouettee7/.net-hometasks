namespace RESTAuth.Domain.Models;

public class UserReport
{
    public string Name { get; set; }
    public string Email { get; set; } 
    public decimal Salary { get; set; }
    public string Department { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
}