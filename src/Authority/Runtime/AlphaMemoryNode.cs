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
    using System.Threading.Tasks;
    using GreenPipes;
    using Util;


    /// <summary>
    /// For beta nodes that need to have memory, this is it
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AlphaMemoryNode<T> :
        IAlphaMemoryNode<T>
        where T : class
    {
        readonly ConnectableList<IFactSink<T>> _sinks;

        public AlphaMemoryNode()
        {
            _sinks = new ConnectableList<IFactSink<T>>();
        }

        public Task All(SessionContext context, AlphaContextCallback<T> callback)
        {
            return context.WorkingMemory.Access(this, x => x.ForEach(context, callback));
        }

        public ConnectHandle Connect(IFactSink<T> sink)
        {
            return _sinks.Connect(sink);
        }

        public Task Insert(AlphaContext<T> context)
        {
            return context.WorkingMemory.Access(this, x =>
            {
                x.Add(context.Fact);

                return _sinks.All(sink => sink.Insert(context));
            });
        }

        public IEnumerable<IFactSink<T>> GetSinks()
        {
            return _sinks;
        }

        public virtual void Accept<TContext>(RuntimeVisitor<TContext> visitor, TContext context)
        {
            if (visitor.IsCompleted)
                return;

            visitor.VisitAlphaMemoryNode(context, this);
        }
    }
}