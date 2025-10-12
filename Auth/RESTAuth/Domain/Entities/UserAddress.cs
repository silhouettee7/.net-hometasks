namespace RESTAuth.Domain.Entities;

public class UserAddress: Entity<int>
{
    public User User { get; set; }
    public Guid UserId { get; set; }
    public string Country { get; set; }
    public string City { get; set; }
    public string Street { get; set; }
}