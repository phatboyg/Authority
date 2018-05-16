// Copyright 2012-2017 Chris Patterson
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Authority.Builders
{
    using System;
    using System.Collections.Generic;
    using GreenPipes.Internals.Extensions;
    using RuleCompiler;
    using Rules.Facts;
    using Util;


    public class RuntimeBuilderContext :
        BuilderContext
    {
        readonly IRuntimeBuilder _builder;
        readonly Dictionary<FactDeclaration, DeclarationContext> _declarations;

        readonly OrderedHashSet<IRuleParameter> _parameters;

        BetaBuilderContext _betaContext;

        public RuntimeBuilderContext(IRuntimeBuilder builder)
        {
            _builder = builder;
            _parameters = new OrderedHashSet<IRuleParameter>();
            _declarations = new Dictionary<FactDeclaration, DeclarationContext>(EqualityComparer);
        }

        public static IEqualityComparer<FactDeclaration> EqualityComparer { get; } = new FactDeclarationEqualityComparer();

        public AlphaBuilderContext<T> GetAlphaBuilderContext<T>(FactDeclaration<T> fact)
            where T : class
        {
            if (_declarations.TryGetValue(fact, out var result))
                return result.GetAlphaBuilderContext<T>();

            var alphaContext = _builder.CreateContext(fact);

            var context = new Cached<T>(alphaContext);

            _declarations.Add(fact, context);

            return context.AlphaBuilderContext;
        }

        public BetaBuilderContext<T> GetBetaBuilderContext<T>(FactDeclaration<T> fact)
            where T : class
        {
            var alphaBuilderContext = GetAlphaBuilderContext(fact);

            if (_betaContext == null)
            {
                BetaBuilderContext<T> betaBuilderContext = _builder.BuildJoinNode(alphaBuilderContext);

                _betaContext = betaBuilderContext;

                alphaBuilderContext.InBeta = true;

                return betaBuilderContext;
            }

            var joinContext = _builder.BuildJoinNode(_betaContext, alphaBuilderContext);

            _betaContext = joinContext;

            return joinContext;
        }


        interface DeclarationContext
        {
            AlphaBuilderContext<T> GetAlphaBuilderContext<T>()
                where T : class;
        }


        interface DeclarationContext<T> :
            DeclarationContext
            where T : class
        {
            AlphaBuilderContext<T> AlphaBuilderContext { get; }
        }


        class Cached<TFact> :
            DeclarationContext<TFact>
            where TFact : class
        {
            public Cached(AlphaBuilderContext<TFact> alphaBuilderContext)
            {
                AlphaBuilderContext = alphaBuilderContext;
            }

            AlphaBuilderContext<T> DeclarationContext.GetAlphaBuilderContext<T>()
            {
                if (this is DeclarationContext<T> context)
                    return context.AlphaBuilderContext;

                throw new ArgumentException(
                    $"The specified fact type did not match the cached fact type: (specified: {TypeNameCache<T>.ShortName}, expected: {TypeNameCache<TFact>.ShortName})");
            }

            public AlphaBuilderContext<TFact> AlphaBuilderContext { get; }
        }


        sealed class FactDeclarationEqualityComparer :
            IEqualityComparer<FactDeclaration>
        {
            public bool Equals(FactDeclaration x, FactDeclaration y)
            {
                if (ReferenceEquals(x, y))
                    return true;
                if (ReferenceEquals(x, null))
                    return false;
                if (ReferenceEquals(y, null))
                    return false;
                if (x.FactType != y.FactType)
                    return false;
                return string.Equals(x.Name, y.Name, StringComparison.OrdinalIgnoreCase);
            }

            public int GetHashCode(FactDeclaration obj)
            {
                return (obj.FactType.GetHashCode() * 397) ^ obj.Name.GetHashCode();
            }
        }
    }
}