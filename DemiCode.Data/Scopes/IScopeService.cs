namespace DemiCode.Data.Scopes
{
    public delegate IScope<TRepository> ScopeFactory<out TRepository>() 
        where TRepository: class;

    public delegate IScope<TRepository1, TRepository2> ScopeFactory<out TRepository1, out TRepository2>()
        where TRepository1 : class
        where TRepository2 : class;

    public delegate IScope<TRepository1, TRepository2, TRepository3> ScopeFactory<out TRepository1, out TRepository2, out TRepository3>()
        where TRepository1 : class
        where TRepository2 : class
        where TRepository3 : class;

    public interface IScopeService
    {
        IScope<TRepository> CreateScope<TRepository>() 
            where TRepository:class;

        IScope<TRepository1, TRepository2> CreateScope<TRepository1, TRepository2>()
            where TRepository1 : class
            where TRepository2 : class;

        IScope<TRepository1, TRepository2, TRepository3> CreateScope<TRepository1, TRepository2, TRepository3>()
            where TRepository1 : class
            where TRepository2 : class
            where TRepository3 : class;
    }
}