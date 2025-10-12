namespace RESTAuth.Domain.Entities;

public class UserProfile: Entity<int>
{
    public Guid UserId { get; set; }
    public User User { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime BirthDate { get; set; }
    public string Phone { get; set; }
}