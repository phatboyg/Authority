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
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Util;


    /// <summary>
    /// An alpha memory contains the facts for an alpha node. All facts are of the same type.
    /// The memory includes a task schedule to synchronize access to the memory for the alpha
    /// node so that no two operations occur simultaneously, while maintaining the ability to
    /// execute node activations concurrently.
    /// </summary>
    /// <typeparam name="TFact"></typeparam>
    public class AlphaMemory<TFact> :
        IAlphaMemory<TFact>,
        IAlphaMemory
        where TFact : class
    {
        readonly OrderedHashSet<TFact> _facts;
        readonly LimitedConcurrencyLevelTaskScheduler _scheduler;

        public AlphaMemory()
        {
            _scheduler = new LimitedConcurrencyLevelTaskScheduler(1);
            _facts = new OrderedHashSet<TFact>();
        }

        Task INodeMemory.Access<T>(NodeMemoryAccessor<T> accessor)
        {
            var memoryAccess = accessor as NodeMemoryAccessor<IAlphaMemory<TFact>>;
            if (memoryAccess == null)
                throw new ArgumentException($"The memory type is invalid: {typeof(IAlphaMemory<TFact>)}", nameof(memoryAccess));

            return Task.Factory.StartNew(() => memoryAccess(this), CancellationToken.None, TaskCreationOptions.None, _scheduler);
        }

        void IAlphaMemory<TFact>.Add(TFact fact)
        {
            _facts.Add(fact);
        }

        bool IAlphaMemory<TFact>.Contains(TFact fact)
        {
            return _facts.Contains(fact);
        }

        void IAlphaMemory<TFact>.Remove(TFact fact)
        {
            _facts.Remove(fact);
        }

        Task IAlphaMemory<TFact>.ForEach(SessionContext context, Func<FactContext<TFact>, Task> callback)
        {
            return Task.WhenAll(_facts.Select(x => callback(new SessionFactContext<TFact>(context, x))));
        }
    }
}