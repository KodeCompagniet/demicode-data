using System;
using System.Threading;

namespace DemiCode.Data.Fakes.FakeItEasy.Utils
{
    public class Disposable : IDisposable
    {
        private const int DisposedFlag = 1;
        private int _isDisposed;

        protected bool IsDisposed
        {
            get
            {
                Thread.MemoryBarrier();
                return _isDisposed == DisposedFlag;
            }
        }

        public void Dispose()
        {
            var comparand = _isDisposed;
            Interlocked.CompareExchange(ref _isDisposed, DisposedFlag, comparand);
            if (comparand != 0)
                return;
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }
    }
}