// ReSharper disable CheckNamespace

namespace DemiCode.Data.Repositories.Test
{
    public class FakeRepository : IFakeRepository
    {
        public FakeRepository(IFakeContext context)
        {
            Context = context;
        }

        public IFakeContext Context { get; set; }
    }
}