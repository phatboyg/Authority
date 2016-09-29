﻿// Copyright 2012-2016 Chris Patterson
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
namespace Authority.Runtime
{
    using System.Threading.Tasks;


    public interface IWorkingMemory
    {
        Task Access<T>(IAlphaNode<T> node, NodeMemoryAccessor<IAlphaMemory<T>> accessor)
            where T : class;

        Task Access<TRight>(IBetaMemoryNode<TRight> node, NodeMemoryAccessor<IBetaMemory<TRight>> accessor)
            where TRight : class;
    }
}