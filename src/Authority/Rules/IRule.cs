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
namespace Authority.Rules
{
    using Builders;
    using Facts;


    /// <summary>
    /// Used to access the details about a rule once it has been constructed.
    /// </summary>
    public interface IRule
    {
        /// <summary>
        /// Returns the fact by name if it exists.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        FactDeclaration GetFact(string name);

        /// <summary>
        /// Applies the rule configuration to the runtime builder
        /// </summary>
        /// <param name="builder"></param>
        void Apply(IRuntimeBuilder builder);
    }
}