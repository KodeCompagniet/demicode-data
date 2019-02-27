// ReSharper disable CheckNamespace

namespace DemiCode.Data.Repositories.Test
{
    public interface IFakeRepository
    {
        IFakeContext Context { get; set; }
    }
}