// Copyright 2012-2016 Chris Patterson
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
namespace Authority.Rules.Facts
{
    using System;
    using System.Collections.Generic;
    using Builders;
    using Conditions;
    using GreenPipes.Internals.Extensions;


    /// <summary>
    /// Contains the facts that were declared by a rule
    /// </summary>
    public class RuleDeclarationCollection
    {
        readonly Dictionary<string, Declaration> _declarations;

        public RuleDeclarationCollection()
        {
            _declarations = new Dictionary<string, Declaration>(StringComparer.OrdinalIgnoreCase);
        }

        public void Add<T>(string name, FactDeclaration<T> factDeclaration)
            where T : class
        {
            _declarations.Add(name, new Declaration<T>(factDeclaration));
        }

        public FactDeclaration Get(string name)
        {
            var declaration = GetDeclaration(name);

            return declaration.Get();
        }

        public FactDeclaration<T> Get<T>(string name)
            where T : class
        {
            var declaration = GetDeclaration(name);

            return declaration.Get<T>();
        }

        Declaration GetDeclaration(string name)
        {
            if (!_declarations.TryGetValue(name, out var declaration))
                throw new DeclarationNotFoundException($"The declaration is unknown or has not been declared: {name}");

            return declaration;
        }

        public BuilderContextCollection Build(IRuntimeBuilder builder)
        {
            var builderContexts = new BuilderContextCollection();
            foreach (var declaration in _declarations.Values)
                declaration.Build(builder, builderContexts);

            return builderContexts;
        }

        public void AddCondition<T>(FactDeclaration<T> factDeclaration, IRuleCondition<T> condition)
            where T : class
        {
            var declaration = GetDeclaration(factDeclaration.Name);

            declaration.AddCondition(condition);
        }


        interface Declaration
        {
            FactDeclaration Get();

            FactDeclaration<T> Get<T>()
                where T : class;

            void AddCondition<T>(IRuleCondition<T> condition)
                where T : class;

            void Build(IRuntimeBuilder builder, BuilderContextCollection builderContexts);
        }


        class Declaration<TFact> :
            Declaration
            where TFact : class
        {
            readonly AlphaConditionCollection<TFact> _conditions;
            readonly FactDeclaration<TFact> _declaration;

            public Declaration(FactDeclaration<TFact> declaration)
            {
                _declaration = declaration;

                _conditions = new AlphaConditionCollection<TFact>();
            }

            FactDeclaration Declaration.Get() => _declaration;

            FactDeclaration<T> Declaration.Get<T>()
            {
                var ruleFact = _declaration as FactDeclaration<T>;
                if (ruleFact == null)
                    throw new ArgumentException(
                        $"The declared fact type does not match: {_declaration.Name} (specified: {TypeNameCache<T>.ShortName}, expected: {TypeNameCache.GetShortName(_declaration.FactType)})");

                return ruleFact;
            }

            void Declaration.AddCondition<T>(IRuleCondition<T> condition)
            {
                var ruleCondition = condition as IRuleCondition<TFact>;
                if (ruleCondition == null)
                    throw new ArgumentException(
                        $"The declared fact type does not match: {_declaration.Name} (specified: {TypeNameCache<T>.ShortName}, expected: {TypeNameCache.GetShortName(_declaration.FactType)})");

                _conditions.Add(ruleCondition);
            }

            public void Build(IRuntimeBuilder builder, BuilderContextCollection builderContexts)
            {
                AlphaBuilderContext<TFact> context = _conditions.Build(builder, builder.CreateContext(_declaration));

                BetaBuilderContext<TFact> betaBuilderContext = builder.BuildJoinNode(context);

                builderContexts.Add(context, betaBuilderContext);
            }
        }
    }
}