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
    /// Rule element that groups dependencies that the rule uses when its actions runs.
    /// </summary>
    public class DependencyGroupElement :
        RuleElement
    {
        readonly List<DependencyElement> _dependencies;

        internal DependencyGroupElement(IEnumerable<Declaration> declarations, IEnumerable<DependencyElement> dependencies)
            : base(declarations)
        {
            _dependencies = new List<DependencyElement>(dependencies);
        }

        /// <summary>
        /// List of dependencies the group element contains.
        /// </summary>
        public IEnumerable<DependencyElement> Dependencies
        {
            get { return _dependencies; }
        }

        internal override void Accept<TContext>(TContext context, RuleElementVisitor<TContext> visitor)
        {
            visitor.VisitDependencyGroup(context, this);
        }
    }
}