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
namespace Authority.Rules
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading.Tasks;
    using Actions;
    using Builders;
    using Conditions;
    using Facts;
    using GreenPipes.Internals.Extensions;
    using RuleCompiler;
    using Runtime;





    /// <summary>
    /// This is a base class that is used to define a rule. This is inspired
    /// by Automatonymous, and supports the creation of rules and facts based
    /// on this dynamic language.
    /// </summary>
    public abstract class Rule :
        IRule
    {
        readonly List<IRuleAction> _actions;
        readonly ExpressionConverter _expressionConverter = new ExpressionConverter();
        readonly RuleDeclarationCollection _declarations;
        string _name;

        protected Rule()
        {
            _name = GetType().Name;

            _declarations = new RuleDeclarationCollection();
            _actions = new List<IRuleAction>();
        }

        public FactDeclaration GetFact(string name)
        {
            return _declarations.Get(name);
        }

        public void Apply(IRuntimeBuilder builder)
        {
            var contextCollection = _declarations.Build(builder);




            var context = builder.CreateContext();


//            foreach (var condition in _conditions)
//                condition.Apply(builder, context);

//            foreach (var action in _actions)
//                action.Apply(builder, context);
        }

        protected void Name(string ruleName)
        {
            if (string.IsNullOrWhiteSpace(ruleName))
                throw new ArgumentException("The machine name must not be empty", nameof(ruleName));

            _name = ruleName;
        }

        /// <summary>
        /// Declares an event, and initializes the event property
        /// </summary>
        /// <param name="propertyExpression"></param>
        protected virtual void Fact<T>(Expression<Func<Fact<T>>> propertyExpression)
            where T : class
        {
            var property = propertyExpression.GetPropertyInfo();
            ParameterExpression parameter = _expressionConverter.GetRuleParameter(propertyExpression);

            DeclareFact<T>(property, parameter);
        }

        /// <summary>
        /// Declares a fact based on the property, and adds it to the cache
        /// </summary>
        /// <typeparam name="T">The fact type</typeparam>
        /// <param name="property">The property referencing the fact</param>
        /// <param name="parameterExpression">The parameter for this fact</param>
        void DeclareFact<T>(PropertyInfo property, ParameterExpression parameterExpression)
            where T : class
        {
            var name = property.Name;

            var fact = new RuleFactDeclaration<T>(name, parameterExpression);

            Fact<T> propertyValue = fact;

            property.SetValue(this, propertyValue);

            _declarations.Add(name, fact);
        }

        /// <summary>
        /// When the event is fired in this state, execute the chained activities
        /// </summary>
        /// <param name="fact"></param>
        /// <param name="conditionalExpression"></param>
        /// <returns></returns>
        protected void When<T>(Fact<T> fact, Expression<Func<T, bool>> conditionalExpression)
            where T : class
        {
            var factDeclaration = _declarations.Get<T>(fact.Name);

            RuleCondition<T> condition = RuleCondition<T>.Factory.New(factDeclaration, conditionalExpression);

            _declarations.AddCondition(factDeclaration, condition);
        }

        protected void When<T1, T2>(Fact<T1> fact1, Fact<T2> fact2, Expression<Func<T1, T2, bool>> conditionalExpression)
            where T1 : class
            where T2 : class
        {
            var fact1Declaration = _declarations.Get<T1>(fact1.Name);
            var fact2Declaration = _declarations.Get<T2>(fact2.Name);



        }

        protected void Then<T>(Fact<T> fact, Func<AlphaContext<T>, T, Task> action)
            where T : class
        {
            var factDeclaration = _declarations.Get<T>(fact.Name);

            var ruleAction = new RuleAction<T>(factDeclaration, context => action(context, context.Fact));

            _actions.Add(ruleAction);
        }

        protected void Then<T>(Fact<T> fact, Func<AlphaContext<T>, Task> action)
            where T : class
        {
            var factDeclaration = _declarations.Get<T>(fact.Name);

            var ruleAction = new RuleAction<T>(factDeclaration, action);

            _actions.Add(ruleAction);
        }

        protected void Then<T1, T2>(Fact<T1> fact1, Fact<T2> fact2, Func<AlphaContext<Tuple<T1, T2>>, T1, T2, Task> action)
            where T1 : class
            where T2 : class
        {
        }
    }
}