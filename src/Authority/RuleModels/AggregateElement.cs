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
namespace Authority.RuleModels
{
    using System;
    using System.Collections.Generic;


    /// <summary>
    /// Rule element that creates new facts (aggregates) based on matching facts it receives as input.
    /// </summary>
    public class AggregateElement :
        PatternSourceElement
    {
        readonly ExpressionMap _expressionMap;

        readonly IAggregatorFactory _factory;
        readonly string _name;
        readonly PatternElement _source;

        internal AggregateElement(IEnumerable<Declaration> declarations, Type resultType, string name, ExpressionMap expressionMap, IAggregatorFactory factory,
            PatternElement source)
            : base(declarations, resultType)
        {
            _name = name;
            _expressionMap = expressionMap;
            _factory = factory;
            _source = source;
        }

        /// <summary>
        /// Factory to create aggregators of this type.
        /// </summary>
        public IAggregatorFactory AggregatorFactory
        {
            get { return _factory; }
        }

        /// <summary>
        /// Fact source of the aggregate.
        /// </summary>
        public PatternElement Source
        {
            get { return _source; }
        }

        /// <summary>
        /// Aggregate name.
        /// </summary>
        public string Name
        {
            get { return _name; }
        }

        /// <summary>
        /// Expressions used by the aggregate.
        /// </summary>
        public ExpressionMap ExpressionMap
        {
            get { return _expressionMap; }
        }

        internal override void Accept<TContext>(TContext context, RuleElementVisitor<TContext> visitor)
        {
            visitor.VisitAggregate(context, this);
        }
    }
}