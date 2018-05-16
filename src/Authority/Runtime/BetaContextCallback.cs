namespace Authority.Runtime
{
    using System.Threading.Tasks;


    public delegate Task BetaContextCallback<in T>(BetaContext<T> context)
        where T : class;
}