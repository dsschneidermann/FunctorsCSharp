using System;
using System.Threading.Tasks;

namespace functors
{
    public interface IApplicative<TApplicative, TApplicativeBase, TInner>
        where TApplicative : class
    {
        TApplicative PureImpl(TInner value);

        object ApplyFromImpl<TApplicativeFunc, TInnerFunc, TApplicativeOut, TOut>(TApplicativeFunc applyFrom)
            where TApplicativeOut : class, IApplicative<TApplicativeOut, TApplicativeBase, TOut>
            where TApplicativeFunc : class, IApplicative<TApplicativeFunc, TApplicativeBase, TInnerFunc>
            where TInnerFunc : FuncA<TInner, TOut>;

        IApplicative<TApplicativeResult, TApplicativeBase, TInnerResult> FmapImpl<TApplicativeResult, TInnerResult>(Func<TInner, TInnerResult> f)
            where TApplicativeResult : class, IApplicative<TApplicativeResult, TApplicativeBase, TInnerResult>, new();
    }
}
