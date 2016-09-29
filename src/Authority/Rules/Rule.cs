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
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading.Tasks;
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
        string _name;
        readonly Dictionary<string, IRuleFact> _factCache;
        readonly ExpressionConverter _expressionConverter = new ExpressionConverter();
        readonly List<IRuleCondition> _conditions;

        protected Rule()
        {
            _name = GetType().Name;

            _factCache = new Dictionary<string, IRuleFact>();
            _conditions = new List<IRuleCondition>();
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
            where T: class
        {
            PropertyInfo property = propertyExpression.GetPropertyInfo();
            var parameter = _expressionConverter.GetRuleParameter(propertyExpression);

            DeclareFact(property, parameter);
        }

        /// <summary>
        /// Declares a fact based on the property, and adds it to the cache
        /// </summary>
        /// <typeparam name="T">The fact type</typeparam>
        /// <param name="property">The property referencing the fact</param>
        /// <param name="parameter">The parameter for this fact</param>
        void DeclareFact<T>(PropertyInfo property, RuleParameter<T> parameter) 
            where T : class
        {
            string name = property.Name;

            var fact = new RuleFact<T>(name, parameter);

            Fact<T> propertyValue = fact;

            property.SetValue(this, propertyValue);

            _factCache[name] = fact;
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
            var ruleFact = _factCache[fact.Name] as IRuleFact<T>;
            if (ruleFact == null)
                throw new ArgumentException($"The fact is unknown or has not been declared: {fact.Name}", nameof(fact));

            var condition = _expressionConverter.GetRuleCondition(ruleFact, conditionalExpression);

            _conditions.Add(condition);
        }

        protected void When<T1, T2>(Fact<T1> fact1, Fact<T2> fact2, Expression<Func<T1, T2, bool>> conditionalExpression) 
            where T1 : class
            where T2 : class
        {
        }

        protected void Then<T>(Fact<T> fact, Func<FactContext<T>, T, Task> action) 
            where T : class
        {
        }

        protected void Then<T1, T2>(Fact<T1> fact1, Fact<T2> fact2, Func<FactContext<Tuple<T1,T2>>, T1, T2, Task> action)
            where T1 : class
            where T2 : class
        {
        }

        public IRuleFact GetFact(string name)
        {
            IRuleFact fact;
            if (_factCache.TryGetValue(name, out fact))
            {
                return fact;
            }

            throw new FactNotFoundException($"The fact was not found: {name}");
        }

        public void Apply(IRuntimeBuilder builder)
        {
            
        }
    }
}