using System;
using System.Threading.Tasks;

namespace functors
{
    public interface IApplicative<TApplicative, TInner> : IFunctor<TApplicative, TInner>
        where TApplicative : class
    {
        Task UnboxImpl(Func<TInner, Task> handler);
    }
}
