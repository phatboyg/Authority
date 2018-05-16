namespace Authority.Builders
{
    using Runtime;


    public interface DeclarationContext<T>
        where T : class
    {
        IAlphaNode<T> AlphaNode { get; }
        IAlphaMemoryNode<T> AlphaMemoryNode { get; }
        IBetaMemoryNode<T> BetaMemoryNode { get; }


        /// <summary>
        /// Prepares the declaration for usage, which can only happen once
        /// </summary>
        /// <param name="alphaNode"></param>
        void InitializeAlphaNode(IAlphaNode<T> alphaNode);
    }
}