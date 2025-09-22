namespace ConsoleAuth;
public static class LocalCache
{
    public static readonly Dictionary<string, CacheItem> Sessions = new();
    public static readonly List<User> Users = new();
}


