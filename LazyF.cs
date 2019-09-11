using System;
using System.Threading.Tasks;

namespace functors
{
    public class LazyF<TInner> : Lazy<TInner>, IFunctor<LazyF<TInner>, TInner>, IUnboxable
    {
        public LazyF()
        {
        }

        public LazyF(Func<TInner> valueFactory) : base(valueFactory)
        {
        }

        public Task<object> Unbox() => Task.FromResult(this.Value as object);

        TFunctorResult IFunctor<LazyF<TInner>, TInner>.FmapImpl<TFunctorResult, TInnerResult>(Func<TInner, TInnerResult> f)
            => new LazyF<TInnerResult>(() => f(this.Value)) as TFunctorResult;
    }

    public static class LazyFunctorExtentions
    {
        public static LazyF<TRes> Fmap<TInner, TRes>(this LazyF<TInner> app, Func<TInner, TRes> f)
            => ((IFunctor<LazyF<TInner>, TInner>)app).FmapImpl<LazyF<TRes>, TRes>(f);
    }

    public static class LazyF
    {
        public static LazyF<T> Create<T>(Func<T> initial) => new LazyF<T>(initial);
    }
}
