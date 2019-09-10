using System;
using System.Threading.Tasks;

namespace functors
{
    public class Composable<T1, T2> : IApplicative<T1, T2>
        where T1 : class
    {
        private readonly IApplicative<T1, T2> _functor;

        public Composable(IApplicative<T1, T2> functor)
        {
            _functor = functor;
        }

        public async Task<T3> With<T3, T4>(
            Func<T2, T4> composableConverter)
            where T3 : class, IFunctor<T3, T4>
        {
            T3 result = null;
            await _functor.UnboxImpl(
                x => { result = (T3)((T3)Activator.CreateInstance(typeof(T3))).WrapImpl(composableConverter(x)); return Task.CompletedTask; });
            return result;
        }

        //private AsyncLocal<object> _capturedResult {get;set;}
        private object _capturedResult { get; set; }

        public async Task<T3> With<T3>()
            where T3 : class, IFunctor<T3, T2>
        {
            T3 result = null;
            await _functor.UnboxImpl(
                x => {
                    result = (T3)((T3)Activator.CreateInstance(typeof(T3))).WrapImpl(x);
                    return Task.CompletedTask;
                });
            return result;
        }

        public IFunctor<T21, T3> FmapImpl<T21, T3>(Func<T1, T3> f) where T21 : class
        {
            return _functor.FmapImpl<T21, T3>(f);
        }

        public Task UnboxImpl(Func<T2, Task> handler)
        {
            return _functor.UnboxImpl(handler);
        }

        public IFunctor<T1, T2> WrapImpl(T2 value)
        {
            return _functor.WrapImpl(value);
        }
    }

    public static class Composable
    {
        public static Composable<T1, T2> Compose<T1, T2>(this IApplicative<T1, T2> applicative)
            where T1 : class
            => new Composable<T1, T2>(applicative);

        public static Task<TFrom> ToTask<TFrom>(TFrom x) => Task.FromResult(x);
    }
}
