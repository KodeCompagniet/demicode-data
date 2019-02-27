using System;

namespace DemiCode.Data
{
    /// <summary>
    /// Thrown when repositories are used together that require different <see cref="IContext"/> implementations, either via nested scopes or in multi-repo scopes.
    /// </summary>
    public class IncompatibleRepositoriesException : Exception
    {
        /// <summary>
        /// The repository type that is not compatible with the current context.
        /// </summary>
        public Type RepositoryType { get; private set; }

        /// <summary>
        /// The type of the current context.
        /// </summary>
        public Type ContextType { get; private set; }

        /// <summary>
        /// Construct the exception.
        /// </summary>
        public IncompatibleRepositoriesException(Type repositoryType, Type contextType) 
            : base("Repository type '" + repositoryType.FullName + "' is not compatible with context '" + contextType.FullName + "'")
        {
            RepositoryType = repositoryType;
            ContextType = contextType;
        }
    }
}