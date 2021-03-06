﻿// Copyright 2012-2016 Chris Patterson
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
namespace Authority.Rules.Conditions
{
    using System;
    using System.Linq.Expressions;
    using Facts;


    public class DefaultRuleConditionFactory<T> :
        IRuleConditionFactory<T>
        where T : class
    {
        public RuleCondition<T> New(FactDeclaration<T> factDeclaration, Expression<Func<T, bool>> conditionExpression)
        {
            if (factDeclaration == null)
                throw new ArgumentNullException(nameof(factDeclaration));
            if (conditionExpression == null)
                throw new ArgumentNullException(nameof(conditionExpression));

            if (conditionExpression.Parameters.Count != 1)
                throw new ArgumentException($"Expected 1 parameter, found {conditionExpression.Parameters.Count}", nameof(conditionExpression));

            return new RuleCondition<T>(factDeclaration, conditionExpression);
        }
    }
}