
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
    public class LazyTaskF<TInner> : IFunctor<LazyTaskF<TInner>, Task<TInner>>
    {
        private readonly Lazy<Task<TInner>> _wrappedImpl;

        public LazyTaskF()
        {
        }

        public LazyTaskF(Func<Task<TInner>> wrappedImpl)
        {
            // Take a Func that is async-await capable and make it into a Lazy.
            _wrappedImpl = new Lazy<Task<TInner>>(wrappedImpl);
        }

        public LazyTaskF(Lazy<Task<TInner>> wrappedImpl)
        {
            // Or just take the Lazy that is given.
            _wrappedImpl = wrappedImpl;
        }

        public Task<TInner> Unbox() => _wrappedImpl.Value;

        IFunctor<LazyTaskF<TInner>, Task<TInner>> IFunctor<LazyTaskF<TInner>, Task<TInner>>.WrapImpl(Task<TInner> value)
            => new LazyTaskF<TInner>(() => value);

        IFunctor<T2, T3> IFunctor<LazyTaskF<TInner>, Task<TInner>>.FmapImpl<T2, T3>(Func<LazyTaskF<TInner>, T3> f)
            => new LazyTaskF<T3>(() => new Task<T3>(() => f(this))) as IFunctor<T2, T3>;
    }

    public static class LazyTaskFunctorExtentions
    {
        // This handles when we have a LazyTaskF<T> (first step).
        public static LazyTaskF<TRes> Fmap<TInner, TRes>(this LazyTaskF<TInner> functor, Func<TInner, TRes> f)
            => new LazyTaskF<TRes>(async () => f(await functor.NullCheck("functor").Unbox().NullCheck("unboxed")));

        // This handles when we chain and have a LazyTask<Task<T>>.
        public static LazyTaskF<TRes> Fmap<TInner, TRes>(this LazyTaskF<Task<TInner>> functor, Func<TInner, TRes> f)
            => new LazyTaskF<TRes>(async () => f(await (await functor.NullCheck("functor").Unbox().NullCheck("unboxed"))));

        // And while we're at it, lets also allow void returning methods.
        public static LazyTaskF<Unit> Fmap<TInner>(this LazyTaskF<TInner> functor, Action<TInner> f)
            => new LazyTaskF<Unit>(async () => { f(await functor.NullCheck("functor").Unbox().NullCheck("unboxed")); return Unit.Instance; });

        // And void when chaining.
        public static LazyTaskF<Unit> Fmap<TInner>(this LazyTaskF<Task<TInner>> functor, Action<TInner> f)
            => new LazyTaskF<Unit>(async () => { f(await (await functor.NullCheck("functor").Unbox().NullCheck("unboxed"))); return Unit.Instance; });

        // For final simplicity of use, lets catch Task returns and make the non-result to Unit also.
        public static LazyTaskF<Unit> Fmap<TInner>(this LazyTaskF<TInner> functor, Func<TInner, Task> f)
            => new LazyTaskF<Unit>(async () => { await f(await functor.NullCheck("functor").Unbox().NullCheck("unboxed")); return Unit.Instance; });

        // And Task support when chaining (yep, triple).
        public static LazyTaskF<Unit> Fmap<TInner>(this LazyTaskF<Task<TInner>> functor, Func<TInner, Task> f)
            => new LazyTaskF<Unit>(async () => { await f(await (await functor.NullCheck("functor").Unbox().NullCheck("unboxed"))); return Unit.Instance; });

        public static T NullCheck<T>(this T isNull, string name, [CallerMemberName] string member = null, [CallerFilePath] string file = null, [CallerLineNumber] int? line = null)
        {
            if (isNull == null)
            {
                Console.WriteLine($"NullCheck failed in {member}: '{name}' is null in {file}:line {line}");
            }
            return isNull;
        }
    }

    public class Unit
    {
        public static Unit Instance = new Unit();
    }

    public static class LazyTaskF
    {
        // Lets create a few helpers so we don't need to write out the initial type
        public static LazyTaskF<T> Create<T>(Func<Task<T>> initial) => new LazyTaskF<T>(initial);
        public static LazyTaskF<T> Create<T>(Lazy<Task<T>> initial) => new LazyTaskF<T>(initial);

        // Lets also support something that is not a task initially
        public static LazyTaskF<T> Create<T>(Func<T> initial) => new LazyTaskF<T>(() => Task.FromResult(initial()));
        public static LazyTaskF<T> Create<T>(Lazy<T> initial) => new LazyTaskF<T>(() => Task.FromResult(initial.Value));
    }
}
