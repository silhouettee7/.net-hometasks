namespace RESTAuth;

public interface IUserRepository: IRepository<User>
{
    List<User> GetUsersForPeriodFromRegistrationDate(DateTime startDate, DateTime endDate);
    List<User> GetUsersForPeriodFromUpdatedDate(DateTime startDate, DateTime endDate);
}