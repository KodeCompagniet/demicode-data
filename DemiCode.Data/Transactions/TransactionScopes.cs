using System.Transactions;

namespace DemiCode.Data.Transactions
{
    /// <summary>
    /// Helper methods for creating <see cref="TransactionScope"/> instances.
    /// </summary>
    public static class TransactionScopes
    {
        private static IsolationLevel _defaultIsolationLevel = IsolationLevel.ReadCommitted;

        /// <summary>
        /// Return true if the current thread is running in an existing transaction.
        /// </summary>
        public static bool AmbientTransactionExist
        {
            get { return Transaction.Current != null; }
        }

        /// <summary>
        /// Get or set the default isolation level used by <see cref="Create"/> and <see cref="CreateRequiresNew"/>.
        /// </summary>
        /// <value>Default is <see cref="IsolationLevel.ReadCommitted"/></value>
        public static IsolationLevel DefaultIsolationLevel
        {
            get { return _defaultIsolationLevel; }
            set { _defaultIsolationLevel = value; }
        }

        public static TransactionScope Create()
        {
            return Create(TransactionScopeOption.Required);
        }

        public static TransactionScope CreateRequiresNew()
        {
            return Create(TransactionScopeOption.RequiresNew);
        }

        public static TransactionScope CreateRequiresNone()
        {
            return Create(TransactionScopeOption.Suppress);
        }

        /// <summary>
        /// Create a new transaction scope with the specified option and isolation level.
        /// </summary>
        public static TransactionScope Create(TransactionScopeOption transactionScopeOption)
        {
            return Create(transactionScopeOption, _defaultIsolationLevel);
        }

        /// <summary>
        /// Create a new transaction scope with the specified option and isolation level.
        /// </summary>
        public static TransactionScope Create(TransactionScopeOption transactionScopeOption, IsolationLevel isolationLevel)
        {
            if (AmbientTransactionExist && transactionScopeOption == TransactionScopeOption.Required)
            {
                isolationLevel = Transaction.Current.IsolationLevel;
            }

            var transactionOptions = new TransactionOptions
            {
                IsolationLevel = isolationLevel,
                Timeout = TransactionManager.DefaultTimeout
            };
            return new TransactionScope(transactionScopeOption, transactionOptions);
        }
    }
}