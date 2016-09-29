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
        readonly Dictionary<ITuple, Dictionary<TRight, ITuple>> _parentToChildMap = new Dictionary<ITuple, Dictionary<TRight, ITuple>>();
        readonly LimitedConcurrencyLevelTaskScheduler _scheduler;

        readonly OrderedHashSet<ITuple> _tuples;

        public BetaMemory()
        {
            _tuples = new OrderedHashSet<ITuple>();
            _scheduler = new LimitedConcurrencyLevelTaskScheduler(1);
        }

        Task INodeMemory.Access<T>(NodeMemoryAccessor<T> accessor)
        {
            var memoryAccess = accessor as NodeMemoryAccessor<IBetaMemory<TRight>>;
            if (memoryAccess == null)
                throw new ArgumentException($"The memory type is invalid: {typeof(IBetaMemory<TRight>)}", nameof(memoryAccess));

            return Task.Factory.StartNew(() => memoryAccess(this), CancellationToken.None, TaskCreationOptions.None, _scheduler);
        }

        void IBetaMemory<TRight>.Add(ITuple<TRight> tuple)
        {
            _tuples.Add(tuple);

            AddMapping(tuple);
        }

        void IBetaMemory<TRight>.Remove(ITuple<TRight> tuple)
        {
            _tuples.Remove(tuple);

            RemoveMapping(tuple);
        }

        bool IBetaMemory<TRight>.TryGetTuple(ITuple<TRight> leftTuple, TRight rightFact, out ITuple tuple)
        {
            Dictionary<TRight, ITuple> subMap;
            if (_parentToChildMap.TryGetValue(leftTuple, out subMap))
            {
                ITuple childTuple;
                subMap.TryGetValue(rightFact ?? NullFact, out childTuple);

                tuple = childTuple;
                return tuple != null;
            }

            tuple = null;
            return false;
        }

        Task IBetaMemory<TRight>.ForEach<T>(SessionContext context, Func<TupleContext<T>, Task> callback)
        {
            return Task.WhenAll(_tuples.Select(x => x.ForEach(context, callback)));
        }

        void AddMapping(ITuple<TRight> tuple)
        {
            if (tuple.Left == null)
                return;

            Dictionary<TRight, ITuple> subMap;
            if (!_parentToChildMap.TryGetValue(tuple.Left, out subMap))
            {
                subMap = new Dictionary<TRight, ITuple>();
                _parentToChildMap[tuple.Left] = subMap;
            }
            subMap[tuple.Right ?? NullFact] = tuple;
        }

        void RemoveMapping(ITuple<TRight> tuple)
        {
            if (tuple.Left == null)
                return;

            Dictionary<TRight, ITuple> subMap;
            if (_parentToChildMap.TryGetValue(tuple.Left, out subMap))
            {
                subMap.Remove(tuple.Right ?? NullFact);
                if (subMap.Count == 0)
                    _parentToChildMap.Remove(tuple.Left);
            }
        }
    }
}