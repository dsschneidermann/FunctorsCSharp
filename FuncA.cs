
using System;

namespace functors
{
    public class FuncA<TIn, TOut> : IApplicative<FuncA<TIn, TOut>, FuncA<object, object>, Func<TIn, TOut>>
    {
        private readonly Func<TIn, TOut> _wrappedFunc;

        public FuncA()
        {
        }

        public FuncA(TOut wrappedValue)
        {
            _wrappedFunc = (_) => wrappedValue;
        }

        public FuncA(Func<TIn, TOut> wrappedFunc) 
        {
            _wrappedFunc = wrappedFunc;
        }

        public TOut Invoke(TIn arg) {
            return _wrappedFunc.Invoke(arg);
        }

        object IApplicative<FuncA<TIn, TOut>, FuncA<object, object>, Func<TIn, TOut>>.ApplyFromImpl<TApplicativeFunc, TInnerFunc, TApplicativeOut, TOut1>(TApplicativeFunc applyFrom)
        {
            throw new NotImplementedException();
        }

        TFunctorResult IFunctor<FuncA<TIn, TOut>, Func<TIn, TOut>>.FmapImpl<TFunctorResult, TInnerResult>(Func<Func<TIn, TOut>, TInnerResult> f)
        {
            throw new NotImplementedException();
        }

        FuncA<TIn, TOut> IApplicative<FuncA<TIn, TOut>, FuncA<object, object>, Func<TIn, TOut>>.PureImpl(Func<TIn, TOut> value)
        {
            throw new NotImplementedException();
        }
    }

    public static class FuncA
    {
        public static FuncA<T1, TOut> Create<T1, TOut>(Func<T1, TOut> f)
            => new FuncA<T1, TOut>(x => f(x));

        public static FuncA<T1, FuncA<T2, TOut>> Create<T1, T2, TOut>(Func<T1, T2, TOut> f)
            => new FuncA<T1, FuncA<T2, TOut>>(x => new FuncA<T2, TOut>(y => f(x,y)));

        // etc...
    }
}
