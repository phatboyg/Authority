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
    using Rules.Facts;
    using Runtime;


    public interface IRuntimeBuilder
    {
        BuilderContext CreateContext();

        /// <summary>
        /// Creates a context for the declaration, starting at the type node for the fact type
        /// </summary>
        /// <typeparam name="T">The fact type</typeparam>
        /// <param name="declaration"></param>
        /// <returns></returns>
        AlphaBuilderContext<T> CreateContext<T>(FactDeclaration<T> declaration)
            where T : class;

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

        /// <summary>
        /// Builds a new, or uses an existing, selection node
        /// </summary>
        /// <typeparam name="T">The fact type</typeparam>
        /// <param name="context">The builder context containing the current position in the alpha network</param>
        /// <param name="conditionExpression">The conditional expression for selection</param>
        /// <returns></returns>
        AlphaBuilderContext<T> BuildSelectionNode<T>(AlphaBuilderContext<T> context, Expression<Func<T, bool>> conditionExpression)
            where T : class;

        IBetaNode<T, T> BuildJoinNode<T>(BuilderContext context)
            where T : class;

        ITerminalNode<T> BuildTerminalNode<T>(BuilderContext context, FactDeclaration<T> factDeclaration)
            where T : class;

        BetaBuilderContext<T, T> BuildJoinNode<T>(AlphaBuilderContext<T> context)
            where T : class;
    }
}