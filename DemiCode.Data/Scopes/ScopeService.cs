using System;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Transactions;
using DemiCode.Data.Transactions;

namespace DemiCode.Data.Scopes
{
    /// <summary>
    /// The service responsible for creating IScope instances.
    /// </summary>
    public class ScopeService : IScopeService
    {
        private readonly Func<Type, IContext, object> _repositoryFactory;
        private readonly IContextProvider _contextProvider;

        /// <summary>
        /// Delegate used to resolve a context compatible with the specified repository type.
        /// </summary>

        [ThreadStatic]
        private static IContext _currentContext;

        /// <summary>
        /// Construct a scope service.
        /// </summary>
        public ScopeService(Func<Type, IContext, object> repositoryFactory, IContextProvider contextProvider)
        {
            _repositoryFactory = repositoryFactory;
            _contextProvider = contextProvider;
        }

        /// <summary>
        /// Override the transaction isolation level used when performing database operations.
        /// </summary>
        public static IsolationLevel TransactionIsolationLevel { get; set; }

        /// <summary>
        /// Provide a factory for the scope service in order to override the execution strategy used when performing database operations.
        /// </summary>
        public static Func<IDbExecutionStrategy> ExecutionStrategyFactory { get; set; }

        /// <summary>
        /// Create a scope that supports one repository.
        /// </summary>
        public IScope<TRepository> CreateScope<TRepository>() where TRepository : class
        {
            return new ScopeImpl<TRepository, IAmNoRepository, IAmNoRepository>(_contextProvider, _repositoryFactory, TransactionIsolationLevel);
        }

        /// <summary>
        /// Create a scope that supports two repositories.
        /// </summary>
        public IScope<TRepository1, TRepository2> CreateScope<TRepository1, TRepository2>()
            where TRepository1 : class
            where TRepository2 : class
        {
            return new ScopeImpl<TRepository1, TRepository2, IAmNoRepository>(_contextProvider, _repositoryFactory, TransactionIsolationLevel);
        }

        /// <summary>
        /// Create a scope that supports three repositories.
        /// </summary>
        public IScope<TRepository1, TRepository2, TRepository3> CreateScope<TRepository1, TRepository2, TRepository3>()
            where TRepository1 : class
            where TRepository2 : class
            where TRepository3 : class
        {
            return new ScopeImpl<TRepository1, TRepository2, TRepository3>(_contextProvider, _repositoryFactory, TransactionIsolationLevel);
        }

        /// <summary>
        /// Marker class indicating unused repository slot.
        /// </summary>
        internal interface IAmNoRepository
        {
        }

        internal class ScopeImpl<TRepository1, TRepository2, TRepository3> : 
                                                               IScope<TRepository1>,
                                                               IScope<TRepository1, TRepository2>,
                                                               IScope<TRepository1, TRepository2, TRepository3>
            where TRepository1 : class
            where TRepository2 : class
            where TRepository3 : class
        {
            private readonly Type[] _repositoryTypes;

            private readonly IContextProvider _contextProvider;
            private readonly Func<Type, IContext, object> _repositoryFactory;
            private readonly IsolationLevel _transactionIsolationLevel;

            public ScopeImpl(IContextProvider contextProvider, Func<Type, IContext, object> repositoryFactory, IsolationLevel transactionIsolationLevel)
            {
                _contextProvider = contextProvider;
                _repositoryFactory = repositoryFactory;
                _transactionIsolationLevel = transactionIsolationLevel;
                _repositoryTypes = GetRepositoryTypes();
            }

            public TData Commit<TData>(Func<TRepository1, TData> work, Func<TRepository1, TData> nonCommitingWork = null, Func<TRepository1, TData> onConcurrencyError = null)
            {
                return CommitWork<TData>(work, nonCommitingWork, onConcurrencyError);
            }

            public TData ReadOnly<TData>(Func<TRepository1, TData> work)
            {
                return ReadOnlyWork<TData>(work);
            }

            public TData Commit<TData>(Func<TRepository1, TRepository2, TData> work, Func<TRepository1, TRepository2, TData> nonCommitingWork = null, Func<TRepository1, TRepository2, TData> onConcurrencyError = null)
            {
                return CommitWork<TData>(work, nonCommitingWork, onConcurrencyError);
            }

            public TData ReadOnly<TData>(Func<TRepository1, TRepository2, TData> work)
            {
                return ReadOnlyWork<TData>(work);
            }

            public TData Commit<TData>(Func<TRepository1, TRepository2, TRepository3, TData> work, Func<TRepository1, TRepository2, TRepository3, TData> nonCommitingWork = null, Func<TRepository1, TRepository2, TRepository3, TData> onConcurrencyError = null)
            {
                return CommitWork<TData>(work, nonCommitingWork, onConcurrencyError);
            }

            public TData ReadOnly<TData>(Func<TRepository1, TRepository2, TRepository3, TData> work)
            {
                return ReadOnlyWork<TData>(work);
            }

            public IQueryable<TData> Query<TData>(Func<TRepository1, IQueryable<TData>> query)
            {
                return QueryWork<TData>(query);
            }

            public IQueryable<TData> Query<TData>(Func<TRepository1, TRepository2, IQueryable<TData>> query)
            {
                return QueryWork<TData>(query);
            }

            public IQueryable<TData> Query<TData>(Func<TRepository1, TRepository2, TRepository3, IQueryable<TData>> query)
            {
                return QueryWork<TData>(query);
            }

            private TData CommitWork<TData>(Delegate work, Delegate nonCommitingWork, Delegate onConcurrencyError)
            {
                return InvokeWithSharedContext((context, repositories) =>
                {
                    try
                    {
                        var result = InvokeWithSharedTransactionCommit(() =>
                        {
                            var r = (TData)work.DynamicInvoke(repositories);
                            context.Commit();
                            return r;
                        });

                        if (nonCommitingWork != null)
                        {
                            result = (TData)nonCommitingWork.DynamicInvoke(repositories);
                        }
                        return result;
                    }
                    catch (ConcurrencyException)
                    {
                        if (onConcurrencyError == null)
                            throw;

                        return (TData)onConcurrencyError.DynamicInvoke(repositories);
                    }
                });
            }

            private TData ReadOnlyWork<TData>(Delegate work)
            {
                return InvokeWithSharedContext((context, repositories) => InvokeWithSharedTransaction(() => (TData)work.DynamicInvoke(repositories)));
            }

            private IQueryable<TData> QueryWork<TData>(Delegate work)
            {
                return InvokeWithReQueryableContext((context, repositories) => (IQueryable<TData>)work.DynamicInvoke(repositories));
            }

            private TData InvokeWithSharedTransaction<TData>(Func<TData> work)
            {
                if (TransactionScopes.AmbientTransactionExist)
                    return work();

                var wrappedWork = WrapWithExecutionStrategyAndTransactionScope(work);
                return wrappedWork();
            }

            /// <summary>
            /// If no ambient transaction exist, a new transaction scope will be created and tried committed.
            /// Any concurrency errors will be translated into a <see cref="ConcurrencyException"/> for later handling.
            /// </summary>
            private TData InvokeWithSharedTransactionCommit<TData>(Func<TData> work)
            {
                if (TransactionScopes.AmbientTransactionExist)
                    return work();

                var wrappedWork = WrapWithExecutionStrategyAndTransactionScope(work);
                try
                {
                    return wrappedWork();
                }
                catch (DbUpdateConcurrencyException updateConcurrencyException)
                {
                    // Wrap the EF specific concurrency exception into ours
                    throw new ConcurrencyException(updateConcurrencyException);
                }
                catch (DbUpdateException updateException)
                {
                    // If we get a snapshot update conflict exception from Sql Server, translate into our concurrency exception
                    if (IsSnapshotConcurrencyException(updateException))
                        throw new ConcurrencyException("Snapshot (update or delete) concurrency exception occured", updateException);
                    if (IsInsertDuplicateUniqueKeyException(updateException))
                        throw new ConcurrencyException("Insert duplicate unique key exception occured", updateException);
                    throw;
                }
            }

            private Func<TData> WrapWithExecutionStrategyAndTransactionScope<TData>(Func<TData> work)
            {
                if (ExecutionStrategyFactory == null)
                    return () => InvokeWithNewTransactionScope(work);
                
                var executionStrategy = ExecutionStrategyFactory();
                return () => executionStrategy.Execute(() => InvokeWithNewTransactionScope(work));
            }

            private TransactionScope CreateTransactionScope()
            {
                return TransactionScopes.Create(TransactionScopeOption.Required, _transactionIsolationLevel);
            }

            private TData InvokeWithNewTransactionScope<TData>(Func<TData> work)
            {
                using (var ts = CreateTransactionScope())
                {
                    var result = work();
                    ts.Complete();
                    return result;
                }
            }

            private TData InvokeWithSharedContext<TData>(Func<IContext, object[],TData> work)
            {
                IContext newCurrentContext;
                var repositories = CreateRepositories(_currentContext, out newCurrentContext);

                if (_currentContext != null)
                {
                    return work(_currentContext, repositories);
                }

                try
                {
                    _currentContext = newCurrentContext;
                    return work(_currentContext, repositories);
                }
                finally
                {
                    if (_currentContext != null)
                        _currentContext.Dispose();
                    _currentContext = null;
                }
            }

            private TData InvokeWithReQueryableContext<TData>(Func<IContext, object[],TData> work)
            {
                IContext context;
                var repositories = CreateRepositories(null, out context);

                return work(context, repositories);
            }

            private object[] CreateRepositories(IContext currentContext, out IContext newCurrentContext)
            {
                newCurrentContext = null;
                
                var currentContextType = currentContext != null ? currentContext.GetType() : null;
                var repoCount = _repositoryTypes.Length;
                var repos = new object[repoCount];

                for (var i = 0; i < repoCount; i++)
                {
                    var repositoryType = _repositoryTypes[i];
                    var repoCompatibleContext = _contextProvider.GetContext(repositoryType);
                    var repoCompatibleContextType = repoCompatibleContext.GetType();

                    if (currentContext == null)
                    {
                        currentContext = newCurrentContext = repoCompatibleContext;
                        currentContextType = repoCompatibleContextType;
                    }
                    else
                    {
                        // Dump the superfluous context we just created
                        repoCompatibleContext.Dispose();
                    }

                    if (currentContextType != repoCompatibleContextType)
                        throw new IncompatibleRepositoriesException(repositoryType, currentContextType);
                 
                    repos[i] = _repositoryFactory(repositoryType, currentContext);
                }
                return repos;
            }

            private static Type[] GetRepositoryTypes()
            {
                var ot = typeof(IAmNoRepository);
                return new[] {typeof (TRepository1), typeof (TRepository2), typeof (TRepository3)}
                    .Where(rt => rt != ot)
                    .ToArray();
            }

            private static bool IsSnapshotConcurrencyException(DbUpdateException exception)
            {
                return IsSpecificSqlException(exception, 3960);
            }

            private static bool IsInsertDuplicateUniqueKeyException(DbUpdateException exception)
            {
                return IsSpecificSqlException(exception, 2601);
            }

            private static bool IsSpecificSqlException(DbUpdateException exception, int sqlErrorCode)
            {
                var inner = exception.InnerException as UpdateException;
                if (inner != null)
                {
                    var iinner = inner.InnerException as SqlException;
                    if (iinner != null)
                    {
                        return iinner.Errors.OfType<SqlError>().Any(e => e.Number == sqlErrorCode);
                    }
                }

                return false;
            }

            public IContext CurrentContext { get { return _currentContext; } }
        }
    }
}