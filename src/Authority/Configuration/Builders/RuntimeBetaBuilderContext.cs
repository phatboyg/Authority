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


    public class RuntimeBetaBuilderContext<TLeft, TRight> :
        BetaBuilderContext<TLeft, TRight>
        where TLeft : class
        where TRight : class
    {
        public RuntimeBetaBuilderContext(FactDeclaration<TRight> declaration, IBetaNode<TLeft, TRight> currentNode)
        {
            Declaration = declaration;
            CurrentNode = currentNode;
            CurrentSource = currentNode.MemoryNode;
        }

        public Type FactType => typeof(TRight);
        public FactDeclaration<TRight> Declaration { get; }
        public IBetaNode<TLeft, TRight> CurrentNode { get; }
        public IBetaMemoryNode<TRight> CurrentSource { get; }

        public IndexMap CreateIndexMap(params FactDeclaration[] declarations)
        {
            return IndexMap.CreateMap(declarations, new[] {Declaration});
        }
    }
}