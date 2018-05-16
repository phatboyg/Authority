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
        /// Creates a context for the declaration, starting at the type node for the fact type. This should be used
        /// distinctly as it doesn't use the builder context at all.
        /// </summary>
        /// <typeparam name="T">The fact type</typeparam>
        /// <param name="declaration"></param>
        /// <returns></returns>
        AlphaBuilderContext<T> CreateContext<T>(FactDeclaration<T> declaration)
            where T : class;

        /// <summary>
        /// Creates a new (or uses an existing with the same condition) SelectNode for the specified condition.
        /// </summary>
        /// <typeparam name="T">The fact type</typeparam>
        /// <param name="context"></param>
        /// <param name="fact">The fact</param>
        /// <param name="conditionExpression"></param>
        /// <returns></returns>
        AlphaBuilderContext<T> BuildSelectNode<T>(BuilderContext context, FactDeclaration<T> fact, Expression<Func<T, bool>> conditionExpression)
            where T : class;

        /// <summary>
        /// Builds a new, or uses an existing, selection node
        /// </summary>
        /// <typeparam name="T">The fact type</typeparam>
        /// <param name="context">The builder context containing the current position in the alpha network</param>
        /// <param name="conditionExpression">The conditional expression for selection</param>
        /// <returns></returns>
        AlphaBuilderContext<T> BuildSelectNode<T>(AlphaBuilderContext<T> context, Expression<Func<T, bool>> conditionExpression)
            where T : class;

        /// <summary>
        /// Creates a new join node for the specified AlphaBuilderContext
        /// </summary>
        /// <param name="context"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        BetaBuilderContext<T> BuildJoinNode<T>(AlphaBuilderContext<T> context)
            where T : class;

        /// <summary>
        /// Creates a new join node for the specified AlphaBuilderContext within an existing beta context
        /// </summary>
        /// <param name="betaContext"></param>
        /// <param name="alphaContext"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        BetaBuilderContext<T> BuildJoinNode<T>(BetaBuilderContext betaContext, AlphaBuilderContext<T> alphaContext)
            where T : class;

        BetaBuilderContext<TLeft, TRight> BuildJoinNode<TLeft, TRight>(BuilderContext context, FactDeclaration<TLeft> leftFact, FactDeclaration<TRight> rightFact)
            where TLeft : class
            where TRight : class;

        BetaBuilderContext<TLeft, TRight> BuildSelectNode<TLeft, TRight>(BuilderContext context, FactDeclaration<TLeft> leftFact, FactDeclaration<TRight> rightFact,
            Expression<Func<TLeft, TRight, bool>> conditionExpression)
            where TLeft : class
            where TRight : class;

        ITerminalNode<T> BuildTerminalNode<T>(BuilderContext context, FactDeclaration<T> factDeclaration)
            where T : class;
    }
}