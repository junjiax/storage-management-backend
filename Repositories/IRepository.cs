namespace dotnet_backend.Repositories
{
    public interface IRepository<T>
    {
        void Add(T item);
    }
}
