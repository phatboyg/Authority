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
namespace Authority.Runtime
{
    using Microsoft.Extensions.Logging;


    /// <summary>
    /// Selects a single fact property and evaluates that property against a single condition.
    /// </summary>
    /// <typeparam name="T">The fact type</typeparam>
    public class SelectNode<T> :
        AlphaNode<T>,
        ISelectionNode<T>
        where T : class
    {
        readonly IAlphaCondition<T> _condition;

        public SelectNode(ILoggerFactory loggerFactory, IAlphaCondition<T> condition)
            : base(loggerFactory)
        {
            _condition = condition;
        }

        public IAlphaCondition<T> Condition => _condition;

        protected override bool Evaluate(AlphaContext<T> context)
        {
            return _condition.Evaluate(context);
        }

        public override void Accept<TContext>(RuntimeVisitor<TContext> visitor, TContext context)
        {
            if (visitor.IsCompleted)
                return;

            visitor.VisitSelectionNode(context, this);
        }
    }
}