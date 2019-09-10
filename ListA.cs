
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace functors
{
    public class ListA<TInner> : IEnumerable<TInner>, IApplicative<ListA<TInner>, TInner>
    {
        private readonly IEnumerable<TInner> _wrappedImpl;

        public ListA()
        {
        }

        public ListA(IEnumerable<TInner> collection)
        {
            _wrappedImpl = collection;
        }

        public IEnumerable<TInner> Unbox() => _wrappedImpl;

        IFunctor<ListA<TInner>, TInner> IFunctor<ListA<TInner>, TInner>.WrapImpl(TInner value)
            => new ListA<TInner>(new[] { value });

        Task IApplicative<ListA<TInner>, TInner>.UnboxImpl(Func<TInner, Task> handler) { this.Select(handler).ToList(); return Task.CompletedTask; }

        IFunctor<T2, T3> IFunctor<ListA<TInner>, TInner>.FmapImpl<T2, T3>(Func<ListA<TInner>, T3> f)
            => new ListA<T3>(this.Select(x => f(new ListA<TInner>(new[] { x })))) as IFunctor<T2, T3>;

        IEnumerator<TInner> IEnumerable<TInner>.GetEnumerator() => _wrappedImpl.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_wrappedImpl).GetEnumerator();
    }

    public static class ListAunctorExtentions
    {
        public static ListA<TRes> Fmap<TInner, TRes>(this ListA<TInner> functor, Func<TInner, TRes> f)
            => new ListA<TRes>(functor.Select(f));
    }

    public static class ListA
    {
        public static ListA<T> Create<T>(IEnumerable<T> initial) => new ListA<T>(initial);
    }
}
