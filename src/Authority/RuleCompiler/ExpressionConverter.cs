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
namespace Authority.RuleCompiler
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using GreenPipes.Internals.Extensions;
    using Rules;
    using Rules.Conditions;
    using Rules.Facts;


    public class ExpressionConverter
    {
        public ParameterExpression GetRuleParameter<T>(Expression<Func<Fact<T>>> propertyExpression)
            where T : class
        {
            if (propertyExpression == null)
                throw new ArgumentNullException(nameof(propertyExpression));

            var memberExpression = propertyExpression.Body as MemberExpression;
            if (memberExpression == null)
                throw new ArgumentException($"Invalid property expression. Expected={typeof(MemberExpression)}, Actual={propertyExpression.Body.GetType()}");

            var factType = memberExpression.Type.GetClosingArguments(typeof(Fact<>)).Single();

            if (factType != typeof(T))
                throw new ArgumentException($"Invalid property type. Expected={typeof(T)}, Actual={factType}");

            return Expression.Parameter(factType, memberExpression.Member.Name);
        }

        public RuleCondition<T> GetRuleCondition<T>(FactDeclaration<T> factDeclaration, Expression<Func<T, bool>> conditionExpression)
            where T : class
        {
            if (conditionExpression == null)
                throw new ArgumentNullException(nameof(conditionExpression));

            if (conditionExpression.Parameters.Count != 1)
                throw new ArgumentException($"Expected 1 parameter, found {conditionExpression.Parameters.Count}", nameof(conditionExpression));

            var condition = new RuleCondition<T>(factDeclaration, conditionExpression);

            return condition;
        }
    }
}