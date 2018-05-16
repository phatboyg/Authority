namespace Authority.Rules.Facts
{
    using System.Collections.Generic;
    using Builders;


    public class BuilderContextCollection
    {
        readonly Dictionary<FactDeclaration, Value> _values;

        public BuilderContextCollection()
        {
            _values = new Dictionary<FactDeclaration, Value>();
        }

        public void Add<T>(AlphaBuilderContext<T> alphaContext, BetaBuilderContext<T> betaContext)
            where T : class
        {
            _values.Add(alphaContext.Declaration, new ContextValue<T>(alphaContext, betaContext));
        }


        interface Value
        {
        }


        class ContextValue<T> :
            Value
            where T : class
        {
            readonly AlphaBuilderContext<T> _alphaContext;
            readonly BetaBuilderContext<T> _betaContext;

            public ContextValue(AlphaBuilderContext<T> alphaContext, BetaBuilderContext<T> betaContext)
            {
                _alphaContext = alphaContext;
                _betaContext = betaContext;
            }
        }
    }
}