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
    using Rules.Facts;
    using Runtime;


    public interface BetaBuilderContext<in TLeft, T> :
        BetaBuilderContext<T>
        where T : class
        where TLeft : class
    {
        /// <summary>
        /// The current alpha node that is represented by this context
        /// </summary>
        IBetaNode<TLeft, T> CurrentNode { get; }
    }


    /// <summary>
    /// Context for building out the beta network (right side), which knows the type of the
    /// output of the last beta node. Contains knowledge on the entire tuple chain leading up
    /// to the node, so that additional nodes may be added.
    /// </summary>
    /// <typeparam name="T">The rightmost fact type</typeparam>
    public interface BetaBuilderContext<T> :
        BetaBuilderContext
        where T : class
    {
        /// <summary>
        /// The fact declaration for this alpha builder
        /// </summary>
        FactDeclaration<T> Declaration { get; }

        /// <summary>
        /// The current memoryNode which sources the facts from the alpha network
        /// </summary>
        IBetaMemoryNode<T> CurrentTupleSource { get; }
    }


    public interface BetaBuilderContext
    {
        IndexMap CreateIndexMap(params FactDeclaration[] declarations);

        /// <summary>
        /// Try to obtain the proper tuple source from the beta builder
        /// </summary>
        /// <param name="fact"></param>
        /// <param name="index"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        bool TryGetTupleIndex<T>(FactDeclaration<T> fact, out int index)
            where T : class;
    }
}