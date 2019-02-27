using System.Linq;
using FakeItEasy;
using FakeItEasy.Configuration;

namespace DemiCode.Data.Fakes.FakeItEasy
{
    internal static class FakeExtensions
    {
        internal static IAfterCallSpecifiedWithOutAndRefParametersConfiguration ReturnsEmpty<T>(this IReturnValueConfiguration<IQueryable<T>> configuration)
        {
            return configuration.Returns(Enumerable.Empty<T>().AsQueryable());
        }
    }
}