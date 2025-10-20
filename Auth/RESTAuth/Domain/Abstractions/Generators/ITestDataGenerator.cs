using RESTAuth.Domain.Entities;

namespace RESTAuth.Domain.Abstractions.Generators;

public interface ITestDataGenerator
{
    List<User> CreateFullDataAboutUsersAsync(int count);
}