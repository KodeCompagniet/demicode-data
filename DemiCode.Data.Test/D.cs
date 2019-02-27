using System.Collections.Generic;

// ReSharper disable CheckNamespace

namespace DemiCode.Data.Test
{
    /// <summary>
    /// Helper class for building "fake" Func's and lambdas by wrapping real funcs.
    /// Currently, it enables tracking call count and parameter values passed to the func for later assertions during test.
    /// </summary>
    internal static class D
    {
        public abstract class DeBase
        {
            public int CallCount { get; set; }
            public bool WasExecuted { get { return CallCount > 0; } }
        }

        public class Func<TIn1, TIn2, TOut> : DeBase
        {
            private readonly System.Func<TIn1, TIn2, TOut> _func;

            private readonly List<TIn1> _in1Values = new List<TIn1>();
            private readonly List<TIn2> _in2Values = new List<TIn2>();
            private readonly List<TOut> _outValues = new List<TOut>();

            public Func(System.Func<TIn1, TIn2, TOut> func)
            {
                _func = func;
                TheFunc = (in1, in2) =>
                           {
                               CallCount++;
                               _in1Values.Add(in1);
                               _in2Values.Add(in2);

                               var result = _func(in1, in2);
                               
                               _outValues.Add(result);
                               
                               return result;
                           };
            }


            public System.Func<TIn1, TIn2, TOut> TheFunc { get; private set; }

            public TIn1[] In1 { get { return _in1Values.ToArray(); } }
            public TIn2[] In2 { get { return _in2Values.ToArray(); } }
            public TOut[] Out { get { return _outValues.ToArray(); } }
        }

        public class Func<TIn1, TOut> : DeBase
        {
            private readonly System.Func<TIn1, TOut> _func;

            private readonly List<TIn1> _in1Values = new List<TIn1>();
            private readonly List<TOut> _outValues = new List<TOut>();

            public Func(System.Func<TIn1, TOut> func)
            {
                _func = func;
                TheFunc = in1 =>
                           {
                               CallCount++;
                               _in1Values.Add(in1);

                               var result = _func(in1);
                               
                               _outValues.Add(result);
                               
                               return result;
                           };
            }

            public System.Func<TIn1, TOut> TheFunc { get; private set; }

            public TIn1[] In1 { get { return _in1Values.ToArray(); } }
            public TOut[] Out { get { return _outValues.ToArray(); } }
        }

        public class Func<TOut> : DeBase
        {
            private readonly System.Func<TOut> _func;

            private readonly List<TOut> _outValues = new List<TOut>();

            public Func(System.Func<TOut> func)
            {
                _func = func;
                TheFunc = () =>
                           {
                               CallCount++;

                               var result = _func();
                               
                               _outValues.Add(result);
                               
                               return result;
                           };
            }

            public System.Func<TOut> TheFunc { get; private set; }

            public TOut[] Out { get { return _outValues.ToArray(); } }
        }

        public static Func<TOut> FakeFunc<TOut>(System.Func<TOut> func)
        {
            return new Func<TOut>(func);
        }

        public static Func<TIn1, TOut> FakeFunc<TIn1, TOut>(System.Func<TIn1, TOut> func)
        {
            return new Func<TIn1, TOut>(func);
        }

        public static Func<TIn1, TIn2, TOut> FakeFunc<TIn1, TIn2, TOut>(System.Func<TIn1, TIn2, TOut> func)
        {
            return new Func<TIn1, TIn2, TOut>(func);
        }
    }
}