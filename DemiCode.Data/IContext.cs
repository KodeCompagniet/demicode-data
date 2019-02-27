using System;

namespace DemiCode.Data
{

    /// <summary>
    /// Wraps an underlying Entity Framework context.
    /// </summary>
    public interface IContext : IDisposable
    {
        /// <summary>
        /// Commit all changes made within the scope.
        /// </summary>
        void Commit();

        /// <summary>
        /// Seed the database with data from the internal migrations configuration.
        /// </summary>
        void Seed();
    }
}