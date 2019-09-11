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

        IFunctor<LazyF<TInner>, TInner> IFunctor<LazyF<TInner>, TInner>.WrapImpl(TInner value)
            => new LazyF<TInner>(() => value);

        IFunctor<T2, T3> IFunctor<LazyF<TInner>, TInner>.FmapImpl<T2, T3>(Func<LazyF<TInner>, T3> f)
            => new LazyF<T3>(() => f(this)) as IFunctor<T2, T3>;
    }

    public static class LazyFunctorExtentions
    {
        public static LazyF<TRes> Fmap<TInner, TRes>(this LazyF<TInner> functor, Func<TInner, TRes> f)
            => new LazyF<TRes>(() => f(functor.Value));
    }

    public static class LazyF
    {
        public static LazyF<T> Create<T>(Func<T> initial) => new LazyF<T>(initial);
    }
}
