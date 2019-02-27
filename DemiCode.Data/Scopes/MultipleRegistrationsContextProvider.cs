using System;
using System.Collections.Generic;
using System.Linq;

namespace DemiCode.Data.Scopes
{
    /// <summary>
    /// A context provider implementation that accepts a list of context factories. Upon context resolution, each factory will be queried with a given repository type.
    /// </summary>
    public class MultipleRegistrationsContextProvider : IContextProvider
    {
        private readonly IEnumerable<Func<Type, IContext>> _contextFactories;

        /// <summary>
        /// Construct the provider.
        /// </summary>
        public MultipleRegistrationsContextProvider(IEnumerable<Func<Type, IContext>> contextFactories)
        {
            _contextFactories = contextFactories;
        }

        /// <summary>
        /// Query each context factory and return the context from the first factory returning a non-null result.
        /// If no factory yields a context, null is returned.
        /// </summary>
        public IContext GetContext(Type forRepositoryType)
        {
            return _contextFactories
                .Select(cf => cf(forRepositoryType))
                .FirstOrDefault(c => c != null);
        }
    }
}