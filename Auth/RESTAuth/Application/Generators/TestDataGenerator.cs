using Bogus;
using RESTAuth.Domain.Abstractions.Generators;
using RESTAuth.Domain.Entities;

namespace RESTAuth.Application.Generators;

public class TestDataGenerator: ITestDataGenerator
{
    public List<User> CreateFullDataAboutUsersAsync(int count)
    {
        var usersAddressesCount = 2;
        var userFaker = new Faker<User>()
            .RuleFor(u => u.Id, f => f.Random.Guid())
            .RuleFor(u => u.CreatedDate, _ => DateTime.UtcNow)
            .RuleFor(u => u.UpdatedDate, (f, u) => u.CreatedDate.AddDays(f.Random.Byte()).ToUniversalTime())
            .RuleFor(u => u.Email, f => f.Person.Email)
            .RuleFor(u => u.Name, f => f.Person.UserName)
            .RuleFor(u => u.Department, f => f.Commerce.Department())
            .RuleFor(u => u.Password, f => f.Random.Hash())
            .RuleFor(u => u.Role, f => f.PickRandom("Admin", "User"))
            .RuleFor(u => u.Salary, f => f.Random.Decimal(30000, 120000))
            .RuleFor(u => u.UserProfile, _ => GenerateUserProfile())
            .RuleFor(u => u.UserAddresses, _ => GenerateUserAddresses(usersAddressesCount));
        var users = userFaker.Generate(count);
        return users;
    }

    private List<UserAddress> GenerateUserAddresses(int usersAddressesCount)
    {
        var userProfileFaker = new Faker<UserAddress>()
            .RuleFor(u => u.City, f => f.Address.City())
            .RuleFor(u => u.Street, f => f.Address.StreetName())
            .RuleFor(u => u.Country, f => f.Address.Country());
        return userProfileFaker.Generate(usersAddressesCount);
    }

    private UserProfile GenerateUserProfile()
    {
        var userProfileFaker = new Faker<UserProfile>()
            .RuleFor(u => u.FirstName, f => f.Name.FirstName())
            .RuleFor(u => u.LastName, f => f.Name.LastName())
            .RuleFor(u => u.Phone, f => f.Phone.PhoneNumber())
            .RuleFor(u => u.BirthDate, f => f.Person.DateOfBirth.ToUniversalTime());
        return userProfileFaker.Generate();
    }
}