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
    using System.Threading.Tasks;
    using GreenPipes;
    using Microsoft.Extensions.Logging;
    using Util;


    public abstract class AlphaNode<TFact> :
        IAlphaNode<TFact>
        where TFact : class
    {
        readonly ConnectableList<IAlphaNode<TFact>> _childNodes;
        readonly ILogger<AlphaNode<TFact>> _log;
        readonly Lazy<IAlphaMemoryNode<TFact>> _memoryNode;

        public AlphaNode(ILoggerFactory loggerFactory)
        {
            _memoryNode = new Lazy<IAlphaMemoryNode<TFact>>(() => new AlphaMemoryNode<TFact>());
            _childNodes = new ConnectableList<IAlphaNode<TFact>>();

            _log = loggerFactory.CreateLogger<AlphaNode<TFact>>();
        }

        public virtual async Task Insert(FactContext<TFact> context)
        {
            using (_log.BeginTypeScope(GetType()))
            {
                if (Evaluate(context))
                {
                    await _childNodes.All(node => node.Insert(context)).ConfigureAwait(false);

                    await _memoryNode.Value.Insert(context).ConfigureAwait(false);
                }
            }
        }

        public Task All(SessionContext context, Func<FactContext<TFact>, Task> callback)
        {
            return _memoryNode.Value.All(context, callback);
        }

        public ConnectHandle Connect(IFactSink<TFact> sink)
        {
            return _memoryNode.Value.Connect(sink);
        }

        public IEnumerable<T> GetChildNodes<T>()
            where T : class
        {
            return _childNodes.Where(x => x is T).Cast<T>();
        }

        ConnectHandle IAlphaNode.AddChild<T>(IAlphaNode<T> node)
        {
            var alphaNode = node as IAlphaNode<TFact>;
            if (alphaNode == null)
                throw new ArgumentException($"The node must match the fact type: {typeof(TFact).Name}", nameof(node));

            return _childNodes.Connect(alphaNode);
        }

        public virtual void Accept<TContext>(RuntimeVisitor<TContext> visitor, TContext context)
        {
            if (visitor.IsCompleted)
                return;

            visitor.VisitAlphaNode(context, this);
        }

        public IAlphaMemoryNode<TFact> MemoryNode => _memoryNode.Value;

        protected virtual bool Evaluate(FactContext<TFact> context)
        {
            return true;
        }
    }
}