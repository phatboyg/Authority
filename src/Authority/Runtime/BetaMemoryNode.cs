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
    using System.Threading.Tasks;
    using GreenPipes;
    using Util;


    /// <summary>
    /// For beta nodes that need to have memory, this is it
    /// </summary>
    /// <typeparam name="TRight"></typeparam>
    public class BetaMemoryNode<TRight> :
        IBetaMemoryNode<TRight>
        where TRight : class
    {
        readonly ConnectableList<ITupleSink<TRight>> _sinks;

        public BetaMemoryNode()
        {
            _sinks = new ConnectableList<ITupleSink<TRight>>();
        }

        public Task ForEach(SessionContext context, Func<TupleContext<TRight>, Task> callback)
        {
            return context.WorkingMemory.Access(this, x => x.ForEach(context, callback));
        }

        public ConnectHandle Connect(ITupleSink<TRight> sink)
        {
            return _sinks.Connect(sink);
        }

        public Task Insert(SessionContext context, ITuple tuple, TRight fact)
        {
            return context.WorkingMemory.Access(this, x =>
            {
                var childTuple = new Tuple<TRight>(tuple, fact);

                x.Add(childTuple);

                TupleContext<TRight> tupleContext = new TupleFactContext<TRight>(context, childTuple, childTuple.Right);

                return _sinks.ForEachAsync(sink => sink.Insert(tupleContext));
            });
        }


        class TupleFactContext<T> :
            TupleContext<T>
            where T : class
        {
            readonly SessionContext _context;

            public TupleFactContext(SessionContext context, ITuple<T> tuple, T fact)
            {
                _context = context;
                Tuple = tuple;
                Fact = fact;
            }

            public IWorkingMemory WorkingMemory => _context.WorkingMemory;

            public ITuple<T> Tuple { get; }
            public T Fact { get; }
        }
    }
}