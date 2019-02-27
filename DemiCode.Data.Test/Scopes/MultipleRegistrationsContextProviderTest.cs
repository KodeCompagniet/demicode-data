using System;
using FakeItEasy;
using NUnit.Framework;

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming

namespace DemiCode.Data.Scopes.Test
{
    [TestFixture]
    public class MultipleRegistrationsContextProviderTest
    {
        [Test]
        public void GetContext_WhenNoFactoryYieldsContext_IsNull()
        {
            var provider = new MultipleRegistrationsContextProvider(new Func<Type, IContext>[] {repoType => null});

            Assert.That(provider.GetContext(typeof (string)), Is.Null);
        }

        [Test]
        public void GetContext_WhenFactoryYieldsContext()
        {
            var ctx = A.Fake<IContext>();
            var provider = new MultipleRegistrationsContextProvider(new Func<Type, IContext>[]
            {
                repoType => null,
                repoType => ctx
            });

            Assert.That(provider.GetContext(typeof(string)), Is.SameAs(ctx));
        }

        [Test]
        public void GetContext_WhenMultipleFactoriesYieldsContext_FirstContextIsReturned()
        {
            var ctx1 = A.Fake<IContext>();
            var ctx2 = A.Fake<IContext>();
            var ctx3 = A.Fake<IContext>();
            var provider = new MultipleRegistrationsContextProvider(new Func<Type, IContext>[]
            {
                repoType => ctx1,
                repoType => ctx2,
                repoType => ctx3
            });

            Assert.That(provider.GetContext(typeof(string)), Is.SameAs(ctx1));
        }
    }
}