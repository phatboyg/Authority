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
    /// Base class for rule elements on the right hand side of the rule definition.
    /// </summary>
    public abstract class RuleRightElement : RuleElement
    {
        internal RuleRightElement(IEnumerable<Declaration> declarations)
            : base(declarations)
        {
        }
    }
}