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
    /// Dependency that the rule uses when its actions runs.
    /// </summary>
    public class DependencyElement :
        RuleElement
    {
        readonly Declaration _declaration;
        readonly Type _serviceType;

        internal DependencyElement(Declaration declaration, IEnumerable<Declaration> declarations, Type serviceType)
            : base(declarations)
        {
            _declaration = declaration;
            _serviceType = serviceType;
        }

        /// <summary>
        /// Declaration that references the dependency.
        /// </summary>
        public Declaration Declaration
        {
            get { return _declaration; }
        }

        /// <summary>
        /// Type of service that this dependency configures.
        /// </summary>
        public Type ServiceType
        {
            get { return _serviceType; }
        }

        internal override void Accept<TContext>(TContext context, RuleElementVisitor<TContext> visitor)
        {
            visitor.VisitDependency(context, this);
        }
    }
}