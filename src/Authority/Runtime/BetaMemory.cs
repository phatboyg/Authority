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
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Util;


    public class BetaMemory<TRight> :
        IBetaMemory<TRight>,
        IBetaMemory
        where TRight : class
    {
        static readonly TRight NullFact = null;
        readonly Dictionary<ITupleChain, Dictionary<TRight, ITupleChain>> _parentToChildMap = new Dictionary<ITupleChain, Dictionary<TRight, ITupleChain>>();
        readonly LimitedConcurrencyLevelTaskScheduler _scheduler;

        readonly OrderedHashSet<ITupleChain> _tuples;

        public BetaMemory()
        {
            _tuples = new OrderedHashSet<ITupleChain>();
            _scheduler = new LimitedConcurrencyLevelTaskScheduler(1);
        }

        Task INodeMemory.Access<T>(NodeMemoryAccessor<T> accessor)
        {
            var memoryAccess = accessor as NodeMemoryAccessor<IBetaMemory<TRight>>;
            if (memoryAccess == null)
                throw new ArgumentException($"The memory type is invalid: {typeof(IBetaMemory<TRight>)}", nameof(memoryAccess));

            return Task.Factory.StartNew(() => memoryAccess(this), CancellationToken.None, TaskCreationOptions.None, _scheduler);
        }

        void IBetaMemory<TRight>.Add(ITupleChain<TRight> tupleChain)
        {
            _tuples.Add(tupleChain);

            AddMapping(tupleChain);
        }

        void IBetaMemory<TRight>.Remove(ITupleChain<TRight> tupleChain)
        {
            _tuples.Remove(tupleChain);

            RemoveMapping(tupleChain);
        }

        bool IBetaMemory<TRight>.TryGetTuple(ITupleChain<TRight> leftTupleChain, TRight rightFact, out ITupleChain tupleChain)
        {
            Dictionary<TRight, ITupleChain> subMap;
            if (_parentToChildMap.TryGetValue(leftTupleChain, out subMap))
            {
                ITupleChain childTupleChain;
                subMap.TryGetValue(rightFact ?? NullFact, out childTupleChain);

                tupleChain = childTupleChain;
                return tupleChain != null;
            }

            tupleChain = null;
            return false;
        }

        Task IBetaMemory<TRight>.ForEach<T>(SessionContext context, BetaContextCallback<T> callback)
        {
            return Task.WhenAll(_tuples.Select(x => x.ForEach(context, callback)));
        }

        void AddMapping(ITupleChain<TRight> tupleChain)
        {
            if (tupleChain.Left == null)
                return;

            Dictionary<TRight, ITupleChain> subMap;
            if (!_parentToChildMap.TryGetValue(tupleChain.Left, out subMap))
            {
                subMap = new Dictionary<TRight, ITupleChain>();
                _parentToChildMap[tupleChain.Left] = subMap;
            }
            subMap[tupleChain.Right ?? NullFact] = tupleChain;
        }

        void RemoveMapping(ITupleChain<TRight> tupleChain)
        {
            if (tupleChain.Left == null)
                return;

            Dictionary<TRight, ITupleChain> subMap;
            if (_parentToChildMap.TryGetValue(tupleChain.Left, out subMap))
            {
                subMap.Remove(tupleChain.Right ?? NullFact);
                if (subMap.Count == 0)
                    _parentToChildMap.Remove(tupleChain.Left);
            }
        }
    }
}