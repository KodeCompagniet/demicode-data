using System.Transactions;
using NUnit.Framework;

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

namespace DemiCode.Data.Transactions.Test
{

    [TestFixture]
    public class TransactionScopesTest
    {

        [Test]
        public void Create_ReturnsScope()
        {
            using (var ts = TransactionScopes.Create())
            {
                Assert.That(ts, Is.Not.Null);
            }
        }

        [Test]
        public void CreateRequiresNew_ReturnsScope()
        {
            using (var ts = TransactionScopes.CreateRequiresNew())
            {
                Assert.That(ts, Is.Not.Null);
            }
        }

        [Test]
        public void CreateRequiresNone_ReturnsScope()
        {
            using (var ts = TransactionScopes.CreateRequiresNone())
            {
                Assert.That(ts, Is.Not.Null);
            }
        }

        [Test]
        public void AmbientTransactionExist_WithoutTransaction_IsFalse()
        {
            Assert.That(TransactionScopes.AmbientTransactionExist, Is.False);
        }

        [Test]
        public void AmbientTransactionExist_WithTransaction_IsTrue()
        {
            using (new TransactionScope(TransactionScopeOption.Required))
            {
                Assert.That(TransactionScopes.AmbientTransactionExist, Is.True);
            }
        }

        [Test]
        public void Create_WithIsolationLevel_SetsIsolationLevel()
        {
            using (TransactionScopes.Create(TransactionScopeOption.Required, IsolationLevel.Snapshot))
            {
                Assert.That(Transaction.Current.IsolationLevel, Is.EqualTo(IsolationLevel.Snapshot));
            }
        }

        [Test]
        public void Create_WithNonDefaultIsolationLevelTransaction_SubScopeMustInheritIsolationLevel()
        {
            using (new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted }))
            {
                using (TransactionScopes.Create())
                {
                    Assert.That(Transaction.Current.IsolationLevel, Is.EqualTo(IsolationLevel.ReadCommitted));
                }
            }
        }

        [Test]
        public void CreateRequiresNew_WithNonDefaultIsolationLevelTransaction_SubScopeDoesNotInheritIsolationLevel()
        {
            TransactionScopes.DefaultIsolationLevel = IsolationLevel.ReadCommitted;
            
            using (new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }))
            {
                using (TransactionScopes.CreateRequiresNew())
                {
                    Assert.That(Transaction.Current.IsolationLevel, Is.EqualTo(IsolationLevel.ReadCommitted));
                }
            }
        }

        [Test]
        public void CreateRequiresNone_WithNonSnapshotIsolationLevelTransaction_SubScopeDoesNotCreateTransaction()
        {
            using (new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted }))
            {
                using (TransactionScopes.CreateRequiresNone())
                {
                    Assert.That(Transaction.Current, Is.Null);
                }
            }
        }

        [Test]
        public void Create_WithDefaultIsolationLevel()
        {
            TransactionScopes.DefaultIsolationLevel = IsolationLevel.Snapshot;
            using (TransactionScopes.Create())
            {
                Assert.That(Transaction.Current.IsolationLevel, Is.EqualTo(IsolationLevel.Snapshot));
            }
        }

        [Test]
        public void CreateRequiresNew_WithDefaultIsolationLevel()
        {
            TransactionScopes.DefaultIsolationLevel = IsolationLevel.Snapshot;
            using (TransactionScopes.CreateRequiresNew())
            {
                Assert.That(Transaction.Current.IsolationLevel, Is.EqualTo(IsolationLevel.Snapshot));
            }
        }

    }

}
