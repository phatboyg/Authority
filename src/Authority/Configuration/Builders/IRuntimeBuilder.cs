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
namespace Authority.Builders
{
    using System;
    using System.Linq.Expressions;
    using Runtime;


    public interface IRuntimeBuilder
    {
//        IEnumerable<ITerminalNode> AddRule(IRuleDefinition ruleDefinition);
        BuilderContext CreateContext();

        /// <summary>
        /// Builds (or uses an existing) type node
        /// </summary>
        /// <typeparam name="T">The fact type</typeparam>
        /// <param name="context"></param>
        /// <returns></returns>
        ITypeNode<T> BuildTypeNode<T>(BuilderContext context)
            where T : class;

        /// <summary>
        /// Builds (or uses an existing) selection node
        /// </summary>
        /// <typeparam name="T">The fact type</typeparam>
        /// <param name="context"></param>
        /// <param name="conditionExpression"></param>
        /// <returns></returns>
        ISelectionNode<T> BuildSelectionNode<T>(BuilderContext context, Expression<Func<T, bool>> conditionExpression)
            where T : class;
    }
}