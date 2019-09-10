using System;
using System.Threading.Tasks;

namespace functors
{
    public interface IFunctor<T1, TInner> where T1 : class
    {
        IFunctor<T1, TInner> WrapImpl(TInner value);
        IFunctor<T2, T3> FmapImpl<T2, T3>(Func<T1, T3> f) where T2 : class;
    }
}
