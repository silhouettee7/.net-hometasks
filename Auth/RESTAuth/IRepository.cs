namespace RESTAuth;

public interface IRepository<in T> where T : class
{
    void Add(T entity);
    void Update(T entity);
    void Delete(T entity);
}