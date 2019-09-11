
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace functors
{
    public interface IUnboxable
    {
        Task<object> Unbox();
    }

    public class ListA<TInner> : IEnumerable<TInner>, IApplicative<ListA<TInner>, ListA<object>, TInner>, IUnboxable
    {
        private readonly List<TInner> _wrappedImpl;

        public ListA()
        {
            _wrappedImpl = new List<TInner>();
        }

        public ListA(IEnumerable<TInner> collection)
        {
            _wrappedImpl = collection.ToList();
        }

        public ListA(TInner value)
        {
            _wrappedImpl = new List<TInner> { value };
        }

        public IEnumerator<TInner> GetEnumerator()
        {
            return _wrappedImpl.GetEnumerator();
        }

        object IApplicative<ListA<TInner>, ListA<object>, TInner>.ApplyFromImpl<TApplicativeFunc, TInnerFunc, TApplicativeOut, TOut>(TApplicativeFunc applyFrom)
        {
            return new ListA<TOut>(
                (applyFrom as ListA<TInnerFunc>).Cast<FuncA<TInner, TOut>>().SelectMany(@fun => 
                    this.Select(x => @fun.Invoke(x))
                ));
        }

        public async Task<object> Unbox()
        {
            if (this.All(x => x is IUnboxable))
            {
                var results = await Task.WhenAll(this.Select(x => ((IUnboxable)x).Unbox()));
                return results.ToList();
            }
            return this.ToList();
        }

        TFunctorResult IFunctor<ListA<TInner>, TInner>.FmapImpl<TFunctorResult, TInnerResult>(Func<TInner, TInnerResult> f)
        {
            return new ListA<TInnerResult>(_wrappedImpl.Select(f)) as TFunctorResult;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _wrappedImpl.GetEnumerator();
        }

        ListA<TInner> IApplicative<ListA<TInner>, ListA<object>, TInner>.PureImpl(TInner value)
        {
            return new ListA<TInner>(value);
        }

        public void Add(TInner item)
        {
            _wrappedImpl.Add(item);
        }
    }

    public static class ListA
    {
        public static ListA<T> Create<T>(IEnumerable<T> collection) => new ListA<T>(collection);
        public static ListA<T> Create<T>(params T[] collection) => new ListA<T>(collection);

        public static ListA<TRes> Fmap<TInner, TRes>(this ListA<TInner> app, Func<TInner, TRes> f)
            => ((IFunctor<ListA<TInner>, TInner>)app).FmapImpl<ListA<TRes>, TRes>(f);

        public static ListA<TOut> AppliedTo<TIn, TOut>(
            this ListA<FuncA<TIn, TOut>> app, ListA<TIn> other)
            => ((IApplicative<ListA<TIn>, ListA<object>, TIn>)other)
                .ApplyFromImpl<ListA<FuncA<TIn, TOut>>, FuncA<TIn, TOut>, ListA<TOut>, TOut>(app)
                as ListA<TOut>;
    }
}
