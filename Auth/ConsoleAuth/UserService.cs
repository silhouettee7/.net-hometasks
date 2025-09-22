namespace ConsoleAuth;

public class UserService
{
    public void Register(User user)
    {
        var userId = Guid.NewGuid();
        user.Id = userId;
        LocalCache.Users.Add(user);
    }
    public void EditUser(Guid id, User user)
    {
        var actualUser = LocalCache.Users.FirstOrDefault(u => u.Id == id);
        if (actualUser == null) return;
        actualUser.Email = user.Email;
        actualUser.Password = user.Password;
        actualUser.Name = user.Name;
        actualUser.Age = user.Age;
        
    }

    public void DeleteUser(Guid id)
    {
        var user = LocalCache.Users.FirstOrDefault(u => u.Id == id);
        LocalCache.Users.Remove(user);
    }
}