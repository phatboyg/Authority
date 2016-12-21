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


    public class RuntimeAlphaBuilderContext<T> :
        AlphaBuilderContext<T>
        where T : class
    {
        public RuntimeAlphaBuilderContext(FactDeclaration<T> declaration, IAlphaNode<T> currentNode)
        {
            Declaration = declaration;
            CurrentNode = currentNode;
            CurrentSource = currentNode.MemoryNode;
        }

        public Type FactType => typeof(T);
        public FactDeclaration<T> Declaration { get; }
        public IAlphaNode<T> CurrentNode { get; }
        public IAlphaMemoryNode<T> CurrentSource { get; }
    }
}