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
namespace Authority.Rules.Actions
{
    using System;
    using System.Threading.Tasks;
    using Builders;
    using Facts;
    using Runtime;


    public class RuleAction<T> :
        IRuleAction<T>
        where T : class
    {
        readonly Func<AlphaContext<T>, Task> _action;
        readonly FactDeclaration<T> _declaration;

        public RuleAction(FactDeclaration<T> declaration, Func<AlphaContext<T>, Task> action)
        {
            _declaration = declaration;
            _action = action;
        }

        public void Apply(IRuntimeBuilder builder, BuilderContext context)
        {
            var terminalNode = builder.BuildTerminalNode(context, _declaration);


        }
    }
}