using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using Autofac;
using NUnit.Framework;

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

namespace DemiCode.Data.Scopes.Test
{

    [TestFixture]
    [Category("Integration")]
    public class ScopeTest_WithConcurrencyErrors
    {
        private long _threadDoneReadingCount;
        private ConcurrentBag<string> _errors;
        private EventWaitHandle _waitHandle;
        private IContainer _container;
        private const string TestUserExternalId = "userid555";
        private const string OtherTestUserExternalId = "ScopeTest_testuser#12345";

        [SetUp]
        public void SetUp()
        {
            var cb = new ContainerBuilder();
            cb.RegisterInstance<ConnectionStringSettingsCollectionProvider>(() => ConfigurationManager.ConnectionStrings);
            cb.RegisterModule<AssemblyModule>();
            _container = cb.Build();

            CheckTestDataExist();
            RemoveTestUser();
        }

        [TearDown]
        public void TearDown()
        {
            RemoveTestUser();

            if (_container != null)
                _container.Dispose();
        }

        [Test]
        public void Commit_WhenIndirectTableIsUpdatedConcurrently_ShouldNotFail()
        {
            Assert.Fail("NOTE: well intended, BUT, transaction retries does not work consistently. Nested transaction scopes will sometimes partially fail!");
            var scope = _container.Resolve<IScope<IPersonRepository, IRoleRepository>>();
            scope.Commit((repo, roles) =>
            {
                var odo = roles.GetRole((int)Roles.ODO);

                var person = new Person
                {
                    ExternalId = OtherTestUserExternalId,
                    Name = OtherTestUserExternalId,
                    Phone = "555-" + Guid.NewGuid(),
                    Roles = new[] { odo }
                };
                repo.InsertPerson(person);

                return 0;
            });

            var calls = new List<string>();
            WithHandle((beforeIndirectTableWasUpdated, beforeDirectTableUpdates, beforeIndirectTransactionCommits) =>
                Concurrent(
                    // Modify indirect table [Role]
                    () =>
                    {
                        using (var ts1 = TransactionScopes.Create())
                        {
                            WaitBeforeCommit();

                            scope.Commit((repo, roles) =>
                            {
                                var ctx = scope.CurrentContext.GetContext();
                                foreach (var role in roles.GetRoles())
                                {
                                    ctx.Entry(role).State = EntityState.Modified;
                                }

                                return 0;
                            });

                            beforeIndirectTableWasUpdated.Set();
                            Thread.Sleep(500);
                            ts1.Complete();
                        }

                    },

                    // Use indirect data [Role] in table [Person]
                    () =>
                    {
                        using (var ts2 = TransactionScopes.Create())
                        {
                            WaitBeforeCommit();

                            beforeIndirectTableWasUpdated.WaitOne();

                            scope.Commit((repo, roles) =>
                            {
                                calls.Add("dask");

                                var ar = roles.GetRole((int)Roles.AR);

                                var person =
                                    repo.GetPersonsWithRoles().First(p => p.ExternalId == OtherTestUserExternalId);
                                person.Roles.Add(ar);

                                return 0;
                            });

                            ts2.Complete();
                        }
                    })
            );

            Assert.That(calls.Count, Is.EqualTo(2));
        }

        [Test]
        public void Commit_WithTwoTransactionsUpdatingTheSameData_OneTransactionMustHandleConcurrencyError()
        {
            var scope = _container.Resolve<IScope<IPersonRepository>>();

            Concurrent(() =>
                scope.Commit(repo =>
                {
                    var person = repo.GetPersons().First(p => p.ExternalId == TestUserExternalId);
                    person.Phone = "555-" + Guid.NewGuid();

                    SignalDoneReading();

                    WaitBeforeCommit();

                    return 0;
                },
                null,
                repo =>
                {
                    _errors.Add(Thread.CurrentThread.Name);
                    return 0;
                })
            ,
            () => WaitForThreadsDoneReading());

            Assert.That(_errors.Count, Is.EqualTo(1));
        }

        [Test]
        public void Commit_WithTwoTransactionsInsertingEqualIdentities_OneTransactionMustHandleConcurrencyError()
        {
            var scope = _container.Resolve<IScope<IPersonRepository>>();

            Concurrent(() =>
                scope.Commit(repo =>
                {
                    var person = new Person
                    {
                        ExternalId = OtherTestUserExternalId,
                        Name = OtherTestUserExternalId,
                        Phone = "555-" + Guid.NewGuid()
                    };

                    repo.InsertPerson(person);

                    WaitBeforeCommit();

                    return 0;
                },
                null,
                repo =>
                {
                    _errors.Add(Thread.CurrentThread.Name);
                    return 0;
                })
            );

            Assert.That(_errors.Count, Is.EqualTo(1));
        }

        private void WaitBeforeCommit()
        {
            _waitHandle.WaitOne();
        }

        private void SignalDoneReading()
        {
            Interlocked.Decrement(ref _threadDoneReadingCount);
        }

        private static void WithHandle(Action<EventWaitHandle> work)
        {
            using (var waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset))
            {
                work(waitHandle);
            }
        }
        private static void WithHandle(Action<EventWaitHandle, EventWaitHandle> work)
        {
            using (var waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset))
            using (var waitHandle2 = new EventWaitHandle(false, EventResetMode.ManualReset))
            {
                work(waitHandle, waitHandle2);
            }
        }
        private static void WithHandle(Action<EventWaitHandle, EventWaitHandle, EventWaitHandle> work)
        {
            using (var waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset))
            using (var waitHandle2 = new EventWaitHandle(false, EventResetMode.ManualReset))
            using (var waitHandle3 = new EventWaitHandle(false, EventResetMode.ManualReset))
            {
                work(waitHandle, waitHandle2, waitHandle3);
            }
        }
        private void Concurrent(Action work)
        {
            Concurrent(work, work, () => false);
        }
        private void Concurrent(Action work, Func<bool> waiting)
        {
            Concurrent(work, work, waiting);
        }
        private void Concurrent(Action work, Action work2)
        {
            Concurrent(work, work2, () => false);
        }
        private void Concurrent(Action work, Action work2, Func<bool> waiting)
        {
            bool bailOut;
            long threadCount = 2;
            _threadDoneReadingCount = threadCount;
            _errors = new ConcurrentBag<string>();
            var exceptions = new ConcurrentBag<Exception>();
            Action decrementThreadCount = () => Interlocked.Decrement(ref threadCount);

            using (_waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset))
            {
                var t1 = new Thread(() =>
                {
                    try
                    {
                        work();
                    }
                    catch (Exception e)
                    {
                        exceptions.Add(e);
                    }
                    finally
                    {
                        decrementThreadCount();
                    }
                }) { Name = "thread#1" };
                var t2 = new Thread(() =>
                {
                    try
                    {
                        work2();
                    }
                    catch (Exception e)
                    {
                        exceptions.Add(e);
                    }
                    finally
                    {
                        decrementThreadCount();
                    }
                }) { Name = "thread#2" };
                t1.Start();
                t2.Start();

                bailOut = waiting();

                // Signal Go! to both workers
                _waitHandle.Set();

                // Wait for both threads to finish executing
                var total = 0;
                while (Interlocked.Read(ref threadCount) > 0)
                {
                    Thread.Sleep(50);
                    if (total++ > 10)
                        return;
                }
            }

            if (bailOut)
                throw new Exception("Wait routine had to bail out!");

            if (exceptions.Count > 0)
                throw new AggregateException(exceptions.ToList());
        }



        private void CheckTestDataExist()
        {
            var scope = _container.Resolve<IScope<IPersonRepository>>();
            scope.ReadOnly(repo =>
            {
                var person = repo.GetPersons().FirstOrDefault(p => p.ExternalId == TestUserExternalId);
                if (person == null)
                    throw new Exception(
                        "Database is not initialized with expected dev data. Run Update-Database to seed the database");
                return 0;
            });

        }

        private void RemoveTestUser()
        {
            var scope = _container.Resolve<IScope<IPersonRepository>>();
            scope.Commit(repo =>
            {
                var person = repo.GetPersonsWithRoles().FirstOrDefault(p => p.ExternalId == OtherTestUserExternalId);
                if (person != null)
                {
                    foreach (var role in person.Roles.ToList())
                    {
                        person.Roles.Remove(role);
                    }
                    repo.DeletePerson(person);
                }

                return 0;
            });

        }

        private bool WaitForThreadsDoneReading()
        {
            var total = 0;
            while (Interlocked.Read(ref _threadDoneReadingCount) > 0)
            {
                Thread.Sleep(50);
                if (total++ > 10)
                {
                    // bail out!
                    return true;
                }
            }
            return false;
        }

    }

}
