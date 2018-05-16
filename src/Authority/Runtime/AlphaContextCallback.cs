namespace Authority.Runtime
{
    using System.Threading.Tasks;


    public delegate Task AlphaContextCallback<in T>(AlphaContext<T> context)
        where T : class;
}