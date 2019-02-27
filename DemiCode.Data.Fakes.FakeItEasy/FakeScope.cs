using System;
using System.Linq;
using DemiCode.Data.Scopes;
using FakeItEasy;

namespace DemiCode.Data.Fakes.FakeItEasy
{
    /// <summary>
    /// A one-repo fake scope.
    /// </summary>
    public class FakeScope<TRepository> : FakeScopeBase, IScope<TRepository> where TRepository : class
    {
        /// <summary>
        /// The current repo.
        /// </summary>
        public TRepository FakeRepository { get; private set; }

        /// <summary>
        /// Returns a scope factory that returns this scope.
        /// </summary>
        public ScopeFactory<TRepository> Factory { get { return () => this; } }

        public FakeScope(bool fakeConcurrencyErrorOnCommit = false)
            : base(fakeConcurrencyErrorOnCommit)
        {
            FakeRepository = A.Fake<TRepository>();
        }

        /// <summary>
        /// Default all calls with <see cref="IQueryable{TRepoEntity}"/> return values
        /// to return an empty queryable.
        /// </summary>
        /// <typeparamref name="TRepoEntity">The entity type supported by the repository</typeparamref>
        public FakeScope<TRepository> WithEmptyQueryableReturnValues<TRepoEntity>()
        {
            A.CallTo(FakeRepository).WithReturnType<IQueryable<TRepoEntity>>().ReturnsEmpty();
            return this;
        }

        public TData Commit<TData>(Func<TRepository, TData> work, Func<TRepository, TData> nonCommitingWork = null, Func<TRepository, TData> onConcurrencyError = null)
        {
            return DoCommit(
                () => work(FakeRepository),
                nonCommitingWork != null ? () => nonCommitingWork(FakeRepository) : (Func<TData>)null,
                onConcurrencyError != null ? () => onConcurrencyError(FakeRepository) : (Func<TData>)null);
        }

        public TData ReadOnly<TData>(Func<TRepository, TData> work)
        {
            return DoReadOnly(() => work(FakeRepository));
        }

        public IQueryable<TData> Query<TData>(Func<TRepository, IQueryable<TData>> query)
        {
            return DoQuery(() => query(FakeRepository));
        }
    }

    public class FakeScope<TRepository1, TRepository2> : FakeScopeBase, IScope<TRepository1, TRepository2>
        where TRepository1 : class
        where TRepository2 : class
    {
        public TRepository1 FakeRepository1 { get; private set; }
        public TRepository2 FakeRepository2 { get; private set; }

        /// <summary>
        /// Returns a scope factory that returns this scope.
        /// </summary>
        public ScopeFactory<TRepository1, TRepository2> Factory { get { return () => this; } }

        public FakeScope(bool fakeConcurrencyErrorOnCommit = false)
            : base(fakeConcurrencyErrorOnCommit)
        {
            FakeRepository1 = A.Fake<TRepository1>();
            FakeRepository2 = A.Fake<TRepository2>();
        }

        /// <summary>
        /// Default all calls with <see cref="IQueryable{TRepoEntity}"/> return values
        /// to return an empty queryable.
        /// </summary>
        /// <typeparamref name="TRepoEntity1">The entity type supported by the first repository</typeparamref>
        /// <typeparamref name="TRepoEntity2">The entity type supported by the second repository</typeparamref>
        public FakeScope<TRepository1, TRepository2> WithEmptyQueryableReturnValues<TRepoEntity1, TRepoEntity2>()
        {
            A.CallTo(FakeRepository1).WithReturnType<IQueryable<TRepoEntity1>>().ReturnsEmpty();
            A.CallTo(FakeRepository2).WithReturnType<IQueryable<TRepoEntity2>>().ReturnsEmpty();
            return this;
        }

        public TData Commit<TData>(Func<TRepository1, TRepository2, TData> work, Func<TRepository1, TRepository2, TData> nonCommitingWork = null, Func<TRepository1, TRepository2, TData> onConcurrencyError = null)
        {
            return DoCommit(
                () => work(FakeRepository1, FakeRepository2),
                nonCommitingWork != null ? () => nonCommitingWork(FakeRepository1, FakeRepository2) : (Func<TData>)null,
                onConcurrencyError != null ? () => onConcurrencyError(FakeRepository1, FakeRepository2) : (Func<TData>)null);
        }

        public TData ReadOnly<TData>(Func<TRepository1, TRepository2, TData> work)
        {
            return DoReadOnly(() => work(FakeRepository1, FakeRepository2));
        }

        public IQueryable<TData> Query<TData>(Func<TRepository1, TRepository2, IQueryable<TData>> query)
        {
            return DoQuery(() => query(FakeRepository1, FakeRepository2));
        }
    }

    public class FakeScope<TRepository1, TRepository2, TRepository3> : FakeScopeBase, IScope<TRepository1, TRepository2, TRepository3>
        where TRepository1 : class
        where TRepository2 : class
        where TRepository3 : class
    {
        public TRepository1 FakeRepository1 { get; private set; }
        public TRepository2 FakeRepository2 { get; private set; }
        public TRepository3 FakeRepository3 { get; private set; }

        /// <summary>
        /// Returns a scope factory that returns this scope.
        /// </summary>
        public ScopeFactory<TRepository1, TRepository2, TRepository3> Factory { get { return () => this; } }

        public FakeScope(bool fakeConcurrencyErrorOnCommit = false)
            : base(fakeConcurrencyErrorOnCommit)
        {
            FakeRepository1 = A.Fake<TRepository1>();
            FakeRepository2 = A.Fake<TRepository2>();
            FakeRepository3 = A.Fake<TRepository3>();
        }

        /// <summary>
        /// Default all calls with <see cref="IQueryable{TRepoEntity}"/> return values
        /// to return an empty queryable.
        /// </summary>
        /// <typeparamref name="TRepoEntity1">The entity type supported by the first repository</typeparamref>
        /// <typeparamref name="TRepoEntity2">The entity type supported by the second repository</typeparamref>
        /// <typeparamref name="TRepoEntity3">The entity type supported by the third repository</typeparamref>
        public FakeScope<TRepository1, TRepository2, TRepository3> WithEmptyQueryableReturnValues<TRepoEntity1, TRepoEntity2, TRepoEntity3>()
        {
            A.CallTo(FakeRepository1).WithReturnType<IQueryable<TRepoEntity1>>().ReturnsEmpty();
            A.CallTo(FakeRepository2).WithReturnType<IQueryable<TRepoEntity2>>().ReturnsEmpty();
            A.CallTo(FakeRepository3).WithReturnType<IQueryable<TRepoEntity3>>().ReturnsEmpty();
            return this;
        }

        public TData Commit<TData>(Func<TRepository1, TRepository2, TRepository3, TData> work, Func<TRepository1, TRepository2, TRepository3, TData> nonCommitingWork = null, Func<TRepository1, TRepository2, TRepository3, TData> onConcurrencyError = null)
        {
            return DoCommit(
                () => work(FakeRepository1, FakeRepository2, FakeRepository3),
                nonCommitingWork != null ? () => nonCommitingWork(FakeRepository1, FakeRepository2, FakeRepository3) : (Func<TData>)null,
                onConcurrencyError != null ? () => onConcurrencyError(FakeRepository1, FakeRepository2, FakeRepository3) : (Func<TData>)null);
        }

        public TData ReadOnly<TData>(Func<TRepository1, TRepository2, TRepository3, TData> work)
        {
            return DoReadOnly(() => work(FakeRepository1, FakeRepository2, FakeRepository3));
        }

        public IQueryable<TData> Query<TData>(Func<TRepository1, TRepository2, TRepository3, IQueryable<TData>> query)
        {
            return DoQuery(() => query(FakeRepository1, FakeRepository2, FakeRepository3));
        }
    }
}