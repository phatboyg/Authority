// Copyright 2012-2017 Chris Patterson
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
    using System.Threading.Tasks;
    using GreenPipes.Internals.Extensions;


    /// <summary>
    /// A list of facts in the order which they were evaluated from left to right into the network.
    /// </summary>
    /// <typeparam name="TRight">The last fact type in the list</typeparam>
    public struct TupleChain<TRight> :
        ITupleChain<TRight>
        where TRight : class
    {
        public static readonly ITupleChain<TRight> Empty = Cached.EmptyTupleChain;

        readonly ITupleChain _left;

        public TupleChain(ITupleChain left, TRight right)
        {
            Right = right;
            _left = left;
            Count = left.Count;

            if (right != null)
                Count++;
        }

        public TupleChain(TRight right)
        {
            Right = right;
            _left = Empty;
            Count = _left.Count;

            if (right != null)
                Count++;
        }

        public bool TryGetFact<T>(int index, out T value)
        {
            ITupleChain current = this;
            for (int i = Count - 1; i >= 0; i--)
            {
                if (i == index)
                {
                    if (current is ITupleChain<T> chain)
                    {
                        value = chain.Right;
                        return true;
                    }

                    throw new ArgumentException(
                        $"The tuple at index {index} did not match the specified type: {TypeNameCache<T>.ShortName} (was {TypeNameCache.GetShortName(current.FactType)})");
                }

                current = current.Left;
            }

            value = default;
            return false;
        }

        public int Count { get; }

        public Type FactType => typeof(TRight);

        public async Task ForEach<T>(SessionContext context, BetaContextCallback<T> callback)
            where T : class
        {
            BetaContext<T> betaContext = new BetaFactContext<TRight>(context, this) as BetaContext<T>;
            if (betaContext != null)
                await callback(betaContext).ConfigureAwait(false);

            if (_left.Count > 0)
                await _left.ForEach(context, callback).ConfigureAwait(false);
        }

        public TRight Right { get; }

        public ITupleChain Left => _left;


        static class Cached
        {
            internal static readonly ITupleChain<TRight> EmptyTupleChain = new TupleChain<TRight>();
        }


        struct BetaFactContext<T> :
            BetaContext<T>
            where T : class
        {
            readonly SessionContext _context;

            public BetaFactContext(SessionContext context, ITupleChain<T> tupleChain)
            {
                _context = context;
                TupleChain = tupleChain;
            }

            public IWorkingMemory WorkingMemory => _context.WorkingMemory;

            public ITupleChain<T> TupleChain { get; }
        }
    }
}