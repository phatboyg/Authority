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
    using GreenPipes.Util;
    using Util;


    class DummyNode<TRight> :
        IBetaMemoryNode<TRight>
        where TRight : class
    {
        readonly ConnectableList<ITupleSink<TRight>> _sinks;

        public DummyNode(ConnectableList<ITupleSink<TRight>> sinks)
        {
            _sinks = sinks;
        }

        public Task All(SessionContext context, Func<TupleContext<TRight>, Task> callback)
        {
            return context.WorkingMemory.Access(this, x => x.ForEach(context, callback));
        }

        public ConnectHandle Connect(ITupleSink<TRight> sink)
        {
            return _sinks.Connect(sink);
        }

        public Task Insert(SessionContext context, ITuple tuple, TRight fact)
        {
            return TaskUtil.Completed;
        }

        public Task Activate(SessionContext context)
        {
            return context.WorkingMemory.Access(this, x =>
            {
                var childTuple = new Tuple<TRight>();

                x.Add(childTuple);

                TupleContext<TRight> tupleContext = new SessionTupleContext<TRight>(context, childTuple);

                return _sinks.All(sink => sink.Insert(tupleContext));
            });
        }
    }
}