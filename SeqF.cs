
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace functors
{
    public class SeqF<TInner> : IEnumerable<TInner>, IFunctor<SeqF<TInner>, TInner>
    {
        private readonly IEnumerable<TInner> _wrappedImpl;

        public SeqF()
        {
        }

        public SeqF(IEnumerable<TInner> collection)
        {
            _wrappedImpl = collection;
        }

        public IEnumerable<TInner> Unbox() => _wrappedImpl;

        IFunctor<SeqF<TInner>, TInner> IFunctor<SeqF<TInner>, TInner>.WrapImpl(TInner value)
            => new SeqF<TInner>(new[] { value });

        Task IFunctor<SeqF<TInner>, TInner>.UnboxImpl(Func<TInner, Task> handler) { this.Select(handler); return Task.CompletedTask; }

        IFunctor<T2, T3> IFunctor<SeqF<TInner>, TInner>.FmapImpl<T2, T3>(Func<SeqF<TInner>, T3> f)
            => new SeqF<T3>(this.Select(x => f(new SeqF<TInner>(new[] { x })))) as IFunctor<T2, T3>;

        IEnumerator<TInner> IEnumerable<TInner>.GetEnumerator() => _wrappedImpl.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_wrappedImpl).GetEnumerator();
    }

    public static class SeqFunctorExtentions
    {
        public static SeqF<TRes> Fmap<TInner, TRes>(this SeqF<TInner> functor, Func<TInner, TRes> f)
            => new SeqF<TRes>(functor.Select(f));
    }

    public static class SeqF
    {
        public static SeqF<T> Create<T>(IEnumerable<T> initial) => new SeqF<T>(initial);
    }
}
