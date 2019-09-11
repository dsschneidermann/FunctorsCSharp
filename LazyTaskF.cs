
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace functors
{
    // Attempt 1
    // public class TaskF<TInner> : Task<TInner>, IFunctor<Task<TInner>, TInner>
    // {
    //     public TaskF(Func<TInner> valueFactory) : base(valueFactory)
    //     {
    //     }

    //     Task<TInner> IFunctor<Task<TInner>, TInner>.Box => this;

    //     public TInner Unbox() => this.GetAwaiter().GetResult(); // Whoops, forced sync

    //     IFunctor<T2, T3> IFunctor<Task<TInner>, TInner>.FmapImpl<T2, T3>(Func<Task<TInner>, T3> f)
    //         => new TaskF<T3>(() => f(this)) as IFunctor<T2, T3>;
    // }

    // public static class TaskFunctorExtentions
    // {
    //     // Because C# forces async-await capable methods to return Task<>, there is no way to implement this method
    //     // without needing to await it! So the behaviour of awaiting "all the way down" will hit our Fmaps - eg.
    //     // await (await (await myTask.Fmap(x => x + 5)).Fmap(x => x.ToString())).Unbox() // ugh!!!
    //     public static TaskF<TRes> Fmap<TInner, TRes>(this TaskF<TInner> functor, Func<TInner, TRes> f)
    //         => new TaskF<TRes>(() => f(functor.Unbox()));
    // }

    // We can use Lazy<> to simplify chaining and make it a LazyTask<T>, which is nice to have
    // for other reasons too.
    public class LazyTaskF<TInner> : Lazy<Task<TInner>>, IFunctor<LazyTaskF<TInner>, TInner>, IUnboxable
    {
        public LazyTaskF(Func<Task<TInner>> func) : base(func)
        {
        }

        public async Task<object> Unbox() {
            var result = await Value as object;
            if (result is IUnboxable box)
            {
                result = await box.Unbox();
            }
            return result;
        }

        TFunctorResult IFunctor<LazyTaskF<TInner>, TInner>.FmapImpl<TFunctorResult, TInnerResult>(Func<TInner, TInnerResult> f)
            => new LazyTaskF<TInnerResult>(async () => f(await this.Value)) as TFunctorResult;
    }

    public static class LazyTaskFunctorExtentions
    {
        // This handles when we have a LazyTaskF<T> (first step).
        public static LazyTaskF<TRes> Fmap<TInner, TRes>(this LazyTaskF<TInner> app, Func<TInner, TRes> f)
            => ((IFunctor<LazyTaskF<TInner>, TInner>)app).FmapImpl<LazyTaskF<TRes>, TRes>(f);
        
        // This handles when we chain and have a LazyTask<Task<T>>.
        public static LazyTaskF<TRes> Fmap<TInner, TRes>(this LazyTaskF<Task<TInner>> functor, Func<TInner, TRes> f)
            => new LazyTaskF<TRes>(async () => f(await (await functor.Value)));

        // And while we're at it, lets also allow void returning methods.
        // public static LazyTaskF<Unit> Fmap<TInner>(this LazyTaskF<TInner> functor, Action<TInner> f)
        //     => new LazyTaskF<Unit>(async () => { f(await functor.Value); return Unit.Instance; });

        // // And void when chaining.
        // public static LazyTaskF<Unit> Fmap<TInner>(this LazyTaskF<Task<TInner>> functor, Action<TInner> f)
        //     => new LazyTaskF<Unit>(async () => { f(await (await functor.Value)); return Unit.Instance; });

        // // For final simplicity of use, lets catch Task returns and make the non-result to Unit also.
        // public static LazyTaskF<Unit> Fmap<TInner>(this LazyTaskF<TInner> functor, Func<TInner, Task> f)
        //     => new LazyTaskF<Unit>(async () => { await f(await functor.Value); return Unit.Instance; });

        // // And Task support when chaining (yep, triple).
        // public static LazyTaskF<Unit> Fmap<TInner>(this LazyTaskF<Task<TInner>> functor, Func<TInner, Task> f)
        //     => new LazyTaskF<Unit>(async () => { await f(await (await functor.Value)); return Unit.Instance; });
    }

    public static class LazyTaskF
    {
        // Lets create a few helpers so we don't need to write out the initial type
        public static LazyTaskF<T> Create<T>(Func<Task<T>> initial) => new LazyTaskF<T>(initial);

        // Lets also support something that is not a task initially
        public static LazyTaskF<T> Create<T>(Func<T> initial) => new LazyTaskF<T>(() => Task.FromResult(initial()));
    }
}
