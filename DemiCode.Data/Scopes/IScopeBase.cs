namespace DemiCode.Data.Scopes
{
    /// <summary>
    /// Common interface for all IScope interfaces, e.g. <see cref="IScope{TRepository}"/>.
    /// </summary>
    public interface IScopeBase 
    {
        /// <summary>
        /// Return the context used by the current thread.
        /// </summary>
        /// <returns>Only valid from within a Commit or ReadOnly call</returns>
        IContext CurrentContext { get; }
    }
}