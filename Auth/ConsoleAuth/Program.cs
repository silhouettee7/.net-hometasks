using ConsoleAuth;

var userService = new UserService();
var authService = new AuthService();
while (true)
{
    Console.WriteLine("Введите одну из следующих команд: /register, /login, /edit, /delete");
    var query = Console.ReadLine();
    switch (query)
    {
        case "/register":
            RegisterViaConsole();
            break;
        case "/login":
            LoginViaConsole();
            break;
        case "/edit":
            EditUserViaConsole();
            break;
        case "/delete":
            DeleteUserViaConsole();
            break;
    }
}

void RegisterViaConsole()
{
    var user = CreateUserFromConsole();
    userService.Register(user);
    Console.WriteLine("Вы успешно зарегались");
}

void LoginViaConsole()
{
    Console.WriteLine("Введите почту");
    var email = Console.ReadLine() ?? string.Empty;
    Console.WriteLine("Введите пароль");
    var password = Console.ReadLine() ?? string.Empty;
    var loginResult = authService.Login(email, password);
    if (loginResult.success)
    {
        Console.WriteLine("Ваш идентификатор сессии: "+loginResult.session);
        return;
    }
    Console.WriteLine("Не удалось войти");
}

void EditUserViaConsole()
{
    var authResult = AuthenticateViaConsole();
    if (!authResult.success) return;
    var user = CreateUserFromConsole();
    userService.EditUser(authResult.id, user);
    Console.WriteLine("Успешно изменили пользователя");
}

void DeleteUserViaConsole()
{
    var authResult = AuthenticateViaConsole();
    if (!authResult.success) return;
    userService.DeleteUser(authResult.id);
    Console.WriteLine("Успешно удалили пользователя");
}

User CreateUserFromConsole()
{
    Console.WriteLine("Введите имя");
    var name = Console.ReadLine() ?? string.Empty;
    Console.WriteLine("Введите возраст");
    int.TryParse(Console.ReadLine(), out int age);
    Console.WriteLine("Введите почту");
    var email = Console.ReadLine() ?? string.Empty;
    Console.WriteLine("Введите пароль");
    var password = Console.ReadLine() ?? string.Empty;
    var user = new User(name, age, email, password);
    return user;
}

(bool success, Guid id) AuthenticateViaConsole()
{
    Console.WriteLine("Введите идентификатор сессии");
    var session = Console.ReadLine() ?? string.Empty;
    var authResult = authService.Authenticate(session);
    if (!authResult.success)
    {
        Console.WriteLine("Ошибка аутентификации");
        return (false, Guid.Empty);
    }
    return (true, authResult!.session.Id);
}

