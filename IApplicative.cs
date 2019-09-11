using System;
using System.Threading.Tasks;

namespace functors
{
    public interface IApplicative<TApplicative, TApplicativeBase, TInner> : IFunctor<TApplicative, TInner>
        where TApplicative : class
    {
        TApplicative PureImpl(TInner value);

        object ApplyFromImpl<TApplicativeFunc, TInnerFunc, TApplicativeOut, TOut>(TApplicativeFunc applyFrom)
            where TApplicativeOut : class, IApplicative<TApplicativeOut, TApplicativeBase, TOut>
            where TApplicativeFunc : class, IApplicative<TApplicativeFunc, TApplicativeBase, TInnerFunc>
            where TInnerFunc : FuncA<TInner, TOut>;
    }
}
