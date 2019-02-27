using System;
using System.Linq;
using DemiCode.Data.Scopes;
using FakeItEasy;

namespace DemiCode.Data.Fakes.FakeItEasy
{
    public abstract class FakeScopeBase : IScopeBase
    {
        protected readonly bool _fakeConcurrencyErrorOnCommit;

        public IContext FakeContext { get; set; }

        public bool EnableExternalDispose { get; set; }

        public IContext CurrentContext { get; private set; }

        public bool CommitWasCalled { get; protected set; }
        public bool NonCommittingWasCalled { get; protected set; }
        public bool ReadOnlyWasCalled { get; protected set; }
        public bool QueryWasCalled { get; protected set; }

        protected FakeScopeBase(bool fakeConcurrencyErrorOnCommit = false)
        {
            _fakeConcurrencyErrorOnCommit = fakeConcurrencyErrorOnCommit;
            FakeContext = A.Fake<IContext>();
        }

        protected TResult WorkWithContext<TResult>(Func<TResult> work)
        {
            CurrentContext = FakeContext;
            try
            {
                return work();
            }
            finally
            {
                CurrentContext = null;
            }
        }

        protected IQueryable<TData> DoQuery<TData>(Func<IQueryable<TData>> work)
        {
            QueryWasCalled = true;
            return WorkWithContext(work);
        }

        protected TData DoReadOnly<TData>(Func<TData> work)
        {
            ReadOnlyWasCalled = true;
            return WorkWithContext(work);
        }

        protected TData DoCommit<TData>(Func<TData> work, Func<TData> nonCommitingWork = null, Func<TData> onConcurrencyError = null)
        {
            NonCommittingWasCalled = false;
            CommitWasCalled = true;
            return WorkWithContext(() =>
            {
                if (_fakeConcurrencyErrorOnCommit)
                {
                    if (onConcurrencyError == null)
                        throw new ConcurrencyException();
                    return onConcurrencyError();
                }
                var commitResult = work();
                if (nonCommitingWork != null)
                {
                    NonCommittingWasCalled = true;
                    return nonCommitingWork();
                }
                return commitResult;
            });
        }
    }
}