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


    public class RuntimeBetaBuilderContext<TLeft, TRight> :
        BetaBuilderContext<TLeft, TRight>
        where TLeft : class
        where TRight : class
    {
        readonly BetaBuilderContext<TLeft> _previous;

        public RuntimeBetaBuilderContext(FactDeclaration<TRight> declaration, IBetaNode<TLeft, TRight> currentNode, BetaBuilderContext<TLeft> previous)
        {
            Declaration = declaration;
            CurrentNode = currentNode;
            _previous = previous;
        }

        public FactDeclaration<TRight> Declaration { get; }
        public IBetaNode<TLeft, TRight> CurrentNode { get; }

        public IBetaMemoryNode<TRight> CurrentTupleSource => CurrentNode.MemoryNode;

        public IndexMap CreateIndexMap(params FactDeclaration[] declarations)
        {
            return IndexMap.CreateMap(declarations, new[] {Declaration});
        }

        public bool TryGetTupleIndex<T>(FactDeclaration<T> fact, out int index)
            where T : class
        {
            if (Declaration is FactDeclaration<T> right && right.Equals(fact) && CurrentNode is IBetaNode<T> node)
            {
                index = 0;
                return true;
            }

            if (_previous != null && _previous.TryGetTupleIndex(fact, out var previousIndex))
            {
                index = previousIndex + 1;
                return true;
            }

            index = default;
            return false;
        }
    }
}