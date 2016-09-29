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


    /// <summary>
    /// Universal quantifier.
    /// </summary>
    public class ForAllElement :
        RuleLeftElement
    {
        readonly PatternElement _basePattern;
        readonly List<PatternElement> _patterns;

        internal ForAllElement(IEnumerable<Declaration> declarations, PatternElement source, IEnumerable<PatternElement> patterns)
            : base(declarations)
        {
            _basePattern = source;
            _patterns = new List<PatternElement>(patterns);
        }

        /// <summary>
        /// Base pattern that determines the universe of facts that the universal quantifier is applied to.
        /// </summary>
        public PatternElement BasePattern
        {
            get { return _basePattern; }
        }

        /// <summary>
        /// Patterns that must all match for the selected facts.
        /// </summary>
        public IEnumerable<PatternElement> Patterns
        {
            get { return _patterns; }
        }

        internal override void Accept<TContext>(TContext context, RuleElementVisitor<TContext> visitor)
        {
            visitor.VisitForAll(context, this);
        }
    }
}