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
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq.Expressions;


    /// <summary>
    /// Rule repeatability.
    /// </summary>
    public enum RuleRepeatability
    {
        /// <summary>
        /// Rule will fire every time a matching set of facts is inserted or updated.
        /// </summary>
        Repeatable = 0,

        /// <summary>
        /// Rule will not fire with the same combination of facts, unless that combination was previously deactivated (i.e. through retraction).
        /// </summary>
        NonRepeatable = 1,
    }


    [DebuggerDisplay("{Expression.ToString()}")]
    public class PriorityElement : RuleElement
    {
        private readonly List<Declaration> _references;
        private readonly LambdaExpression _expression;

        /// <summary>
        /// Expression that calculates rule's priority.
        /// </summary>
        public LambdaExpression Expression
        {
            get { return _expression; }
        }

        /// <summary>
        /// List of declarations the priority expression references.
        /// </summary>
        public IEnumerable<Declaration> References
        {
            get { return _references; }
        }

        public PriorityElement(IEnumerable<Declaration> declarations, IEnumerable<Declaration> references, LambdaExpression expression)
            : base(declarations)
        {
            _references = new List<Declaration>(references);
            _expression = expression;
        }

        internal override void Accept<TContext>(TContext context, RuleElementVisitor<TContext> visitor)
        {
            visitor.VisitPriority(context, this);
        }
    }
}