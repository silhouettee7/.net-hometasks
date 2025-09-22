namespace ConsoleAuth;

public class AuthService
{
    public (bool success, string? session) Login(string email, string password)
    {
        try
        {
            var user = LocalCache.Users.SingleOrDefault(u => u.Email == email);
            if (user == null)
            {
                return (false, null);
            }
            if (user.Password != password)
            {
                return (false, null);
            }
            var prevSessionKey = LocalCache.Sessions
                .FirstOrDefault(pair => pair.Value.Id == user.Id).Key;
            if (prevSessionKey != null)
            {
                LocalCache.Sessions.Remove(prevSessionKey);
            }
            var cacheItem = new CacheItem { Id = user.Id };
            var cacheKey = Guid.NewGuid().ToString();
            LocalCache.Sessions.Add(cacheKey, cacheItem);
            return (true, cacheKey);
        }
        catch (Exception ex)
        {
            return (false, null);
        }
    }

    public (bool success, CacheItem? session) Authenticate(string session)
    {
        if (!LocalCache.Sessions.TryGetValue(session, out var userSession))
        {
            return (false, null);
        }
        return (true, userSession);
        
    }
}