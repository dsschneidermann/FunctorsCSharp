using System;

namespace functors
{
    public interface IFunctor<TFunctor, TInner> where TFunctor : class
    {
        TFunctorResult FmapImpl<TFunctorResult, TInnerResult>(Func<TInner, TInnerResult> f)
            where TFunctorResult : class, IFunctor<TFunctorResult, TInnerResult>;
    }
}
