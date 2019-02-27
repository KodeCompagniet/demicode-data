using System;
using System.Linq;

namespace DemiCode.Data.Scopes
{
    /// <summary>
    /// A scope defines the boundary for a "unit of work" against the underlying database.
    /// </summary>
    public interface IScope<out TRepository> : IScopeBase
        where TRepository : class
    {
        /// <summary>
        /// Perform <paramref name="work"/> and commit all changes made to the repository (<typeparamref name="TRepository"/>).
        /// If not null, performs <paramref name="nonCommitingWork"/> on the commited data, to get updated data that are not included in the original query 
        /// </summary>
        TData Commit<TData>(Func<TRepository, TData> work, Func<TRepository, TData> nonCommitingWork = null, Func<TRepository, TData> onConcurrencyError = null);

        /// <summary>
        /// Perform <paramref name="work"/> and without committing any changes made to the repository (<typeparamref name="TRepository"/>).
        /// </summary>
        TData ReadOnly<TData>(Func<TRepository, TData> work);

        /// <summary>
        /// Perform (or just return) a re-queryable <paramref name="query"/>.
        /// </summary>
        /// <remarks>The query method is called within the scope, though the underlying data context is not kept nor released when the method returns.
        /// This Enables the returned queryable to be re-queried.</remarks>
        IQueryable<TData> Query<TData>(Func<TRepository, IQueryable<TData>> query);
    }

    /// <summary>
    /// A scope defines the boundary for a "unit of work" against the underlying database.
    /// </summary>
    public interface IScope<out TRepository1, out TRepository2> : IScopeBase
        where TRepository1 : class
        where TRepository2 : class
    {
        /// <summary>
        /// Perform <paramref name="work"/> and commit all changes made to any repository (<typeparamref name="TRepository1"/> or <typeparamref name="TRepository2"/>).
        /// If not null, performs <paramref name="nonCommitingWork"/> on the commited data, to get updated data that are not included in the original query 
        /// </summary>
        TData Commit<TData>(Func<TRepository1, TRepository2, TData> work, Func<TRepository1, TRepository2, TData> nonCommitingWork = null, Func<TRepository1, TRepository2, TData> onConcurrencyError = null);

        /// <summary>
        /// Perform <paramref name="work"/> and without committing any changes made to the repositories.
        /// </summary>
        TData ReadOnly<TData>(Func<TRepository1, TRepository2, TData> work);

        /// <summary>
        /// Perform (or just return) a re-queryable <paramref name="query"/>.
        /// </summary>
        /// <remarks>The query method is called within the scope, though the underlying data context is not kept nor released when the method returns.
        /// This Enables the returned queryable to be re-queried.</remarks>
        IQueryable<TData> Query<TData>(Func<TRepository1, TRepository2, IQueryable<TData>> query);
    }

    /// <summary>
    /// A scope defines the boundary for a "unit of work" against the underlying database.
    /// </summary>
    public interface IScope<out TRepository1, out TRepository2, out TRepository3> : IScopeBase
        where TRepository1 : class
        where TRepository2 : class
        where TRepository3 : class
    {
        /// <summary>
        /// Perform <paramref name="work"/> and commit all changes made to any repository (<typeparamref name="TRepository1"/>, <typeparamref name="TRepository2"/> or <typeparamref name="TRepository3"/>).
        /// If not null, performs <paramref name="nonCommitingWork"/> on the commited data, to get updated data that are not included in the original query 
        /// </summary>
        TData Commit<TData>(Func<TRepository1, TRepository2, TRepository3, TData> work, Func<TRepository1, TRepository2, TRepository3, TData> nonCommitingWork = null, Func<TRepository1, TRepository2, TRepository3, TData> onConcurrencyError = null);

        /// <summary>
        /// Perform <paramref name="work"/> and without committing any changes made to the repositories.
        /// </summary>
        TData ReadOnly<TData>(Func<TRepository1, TRepository2, TRepository3, TData> work);

        /// <summary>
        /// Perform (or just return) a re-queryable <paramref name="query"/>.
        /// </summary>
        /// <remarks>The query method is called within the scope, though the underlying data context is not kept nor released when the method returns.
        /// This Enables the returned queryable to be re-queried.</remarks>
        IQueryable<TData> Query<TData>(Func<TRepository1, TRepository2, TRepository3, IQueryable<TData>> query);
    }
}