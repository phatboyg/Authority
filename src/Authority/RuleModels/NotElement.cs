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
    /// Negative existential quantifier.
    /// </summary>
    public class NotElement :
        RuleLeftElement
    {
        readonly RuleLeftElement _source;

        internal NotElement(IEnumerable<Declaration> declarations, RuleLeftElement source)
            : base(declarations)
        {
            _source = source;
        }

        /// <summary>
        /// Fact source of the not element.
        /// </summary>
        public RuleLeftElement Source
        {
            get { return _source; }
        }

        internal override void Accept<TContext>(TContext context, RuleElementVisitor<TContext> visitor)
        {
            visitor.VisitNot(context, this);
        }
    }
}