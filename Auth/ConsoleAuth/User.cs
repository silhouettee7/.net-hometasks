namespace ConsoleAuth;

public class User(string name, int age, string email, string password)
{
    public Guid Id { get; set; }
    public string Name { get; set; } = name;
    public byte Age { get; set; }
    public string Email { get; set; } = email;
    public string Password { get; set; } = password;
}