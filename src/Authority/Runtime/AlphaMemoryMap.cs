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
namespace Authority.Runtime
{
    using System.Collections.Concurrent;
    using System.Threading.Tasks;


    /// <summary>
    /// Retains the memory maps for the alpha nodes in the network
    /// </summary>
    public class AlphaMemoryMap : 
        IAlphaMemoryMap
    {
        readonly ConcurrentDictionary<IAlphaMemoryNode, IAlphaMemory> _memories;

        public AlphaMemoryMap()
        {
            _memories = new ConcurrentDictionary<IAlphaMemoryNode, IAlphaMemory>();
        }

        Task IAlphaMemoryMap.Access<T>(IAlphaMemoryNode<T> node, NodeMemoryAccessor<IAlphaMemory<T>> accessor)
        {
            var memory = _memories.GetOrAdd(node, add => new AlphaMemory<T>());

            return memory.Access(accessor);
        }
    }
}