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
    using Rules.Facts;
    using Runtime;


    /// <summary>
    /// A context returned from the build out of an alpha
    /// </summary>
    /// <typeparam name="TFact">The fact type</typeparam>
    public interface AlphaBuilderContext<TFact> :
        AlphaBuilderContext
        where TFact : class
    {
        /// <summary>
        /// The fact declaration for this alpha builder
        /// </summary>
        FactDeclaration<TFact> Declaration { get; }

        /// <summary>
        /// The current alpha node that is represented by this context
        /// </summary>
        IAlphaNode<TFact> CurrentNode { get; set; }

        /// <summary>
        /// The current memoryNode which sources the facts from the alpha network
        /// </summary>
        IAlphaMemoryNode<TFact> CurrentFactSource { get; }

        /// <summary>
        /// Indicates whether the fact has already been added to the beta network
        /// </summary>
        bool InBeta { get; set; }
    }


    public interface AlphaBuilderContext
    {
        Type FactType { get; }
    }
}