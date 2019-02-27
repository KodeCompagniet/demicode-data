using System;

namespace DemiCode.Data.Scopes
{
    /// <summary>
    /// Implement to privde <see cref="IContext"/> implementations given a repository type.
    /// </summary>
    public interface IContextProvider
    {
        /// <summary>
        /// Return a context that is compatible with <paramref name="forRepositoryType"/> instances.
        /// </summary>
        IContext GetContext(Type forRepositoryType);
    }
}