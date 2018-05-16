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
    using Builders;
    using Facts;


    public interface IRuleCondition
    {
        void Apply(IRuntimeBuilder builder, BuilderContext context);
    }


    public interface IRuleCondition<T> :
        IRuleCondition
        where T : class
    {
        FactDeclaration<T> FactDeclaration { get; }

        Expression<Func<T, bool>> ConditionExpression { get; }
    }


    public interface IRuleCondition<TLeft, TRight> :
        IRuleCondition
        where TLeft : class
        where TRight : class
    {
        FactDeclaration<TLeft> LeftFact { get; }
        FactDeclaration<TRight> RightFact { get; }

        Expression<Func<TLeft, TRight, bool>> ConditionExpression { get; }
    }
}