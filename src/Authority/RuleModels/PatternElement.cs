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
    /// Rule element that represents a pattern that matches facts.
    /// </summary>
    public class PatternElement : RuleLeftElement
    {
        readonly List<ConditionElement> _conditions;
        readonly Declaration _declaration;
        readonly PatternSourceElement _source;
        readonly Type _valueType;

        internal PatternElement(Declaration declaration, IEnumerable<Declaration> declarations, IEnumerable<ConditionElement> conditions)
            : base(declarations)
        {
            _declaration = declaration;
            _valueType = declaration.Type;
            _conditions = new List<ConditionElement>(conditions);
        }

        internal PatternElement(Declaration declaration, IEnumerable<Declaration> declarations, IEnumerable<ConditionElement> conditions,
            PatternSourceElement source)
            : this(declaration, declarations, conditions)
        {
            _source = source;
        }

        /// <summary>
        /// Declaration that references the pattern.
        /// </summary>
        public Declaration Declaration
        {
            get { return _declaration; }
        }

        /// <summary>
        /// Optional pattern source element.
        /// </summary>
        public PatternSourceElement Source
        {
            get { return _source; }
        }

        /// <summary>
        /// Type of the values that the pattern matches.
        /// </summary>
        public Type ValueType
        {
            get { return _valueType; }
        }

        /// <summary>
        /// List of conditions the pattern checks.
        /// </summary>
        public IEnumerable<ConditionElement> Conditions
        {
            get { return _conditions; }
        }

        internal override void Accept<TContext>(TContext context, RuleElementVisitor<TContext> visitor)
        {
            visitor.VisitPattern(context, this);
        }
    }
}