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
using Authority_IObserverConnector = Authority.IObserverConnector;

namespace Authority.Runtime
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Threading.Tasks;
    using GreenPipes;
    using Microsoft.Extensions.Logging;


    public class Network :
        INetwork
    {
        readonly ILogger<Network> _logger;
        readonly FactObservable _observers;
        readonly ConcurrentDictionary<Type, ITypeActivation> _typeNodes;

        public Network(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<Network>();

            _typeNodes = new ConcurrentDictionary<Type, ITypeActivation>();
            _observers = new FactObservable();
        }

        Task IFactSink.Insert<T>(FactContext<T> context)
        {
            if (!_typeNodes.ContainsKey(typeof(T)))
                GetTypeNode<T>();

            return Task.WhenAll(_typeNodes.Values.Select(x => x.FactSink.Insert(context)));
        }

        ConnectHandle Authority_IObserverConnector.ConnectObserver<T>(IFactObserver<T> observer)
        {
            return GetTypeNode<T>().ConnectObserver(observer);
        }

        ConnectHandle Authority_IObserverConnector.ConnectObserver(IFactObserver observer)
        {
            return _observers.Connect(observer);
        }

        public virtual void Accept<TContext>(RuntimeVisitor<TContext> visitor, TContext context)
        {
            foreach (var node in _typeNodes.Values)
            {
                if (visitor.IsCompleted)
                    return;

                node.FactSink.Accept(visitor, context);
            }
        }

        public TResult GetTypeNode<T, TResult>()
            where T : class
            where TResult : class
        {
            return GetTypeNode<T>().As<TResult>();
        }

        protected virtual ITypeActivation GetTypeNode<T>()
            where T : class
        {
            return _typeNodes.GetOrAdd(typeof(T), x =>
            {
                _logger.LogDebug($"Creating type node: {typeof(T).Name}");

                return new TypeActivation<T>(_observers);
            });
        }


        protected interface ITypeActivation :
            Authority_IObserverConnector
        {
            IFactSink FactSink { get; }

            TResult As<TResult>()
                where TResult : class;
        }


        sealed class TypeActivation<TFact> :
            ITypeActivation
            where TFact : class
        {
            readonly Lazy<ITypeNode<TFact>> _filter;
            readonly FactObservable _observers;

            public TypeActivation(FactObservable observers)
            {
                _observers = observers;
                _filter = new Lazy<ITypeNode<TFact>>(CreateTypeNode);
            }

            public IFactSink FactSink => _filter.Value;

            TResult ITypeActivation.As<TResult>()
            {
                return _filter.Value as TResult;
            }

            ConnectHandle Authority_IObserverConnector.ConnectObserver<T>(IFactObserver<T> observer)
            {
                var connector = _filter.Value as ITypeNode<T>;
                if (connector == null)
                    throw new ArgumentException($"The activation is not of the specified type: {typeof(T).Name}", nameof(observer));

                return connector.ConnectObserver(observer);
            }

            public ConnectHandle ConnectObserver(IFactObserver observer)
            {
                return _observers.Connect(observer);
            }

            ITypeNode<TFact> CreateTypeNode()
            {
                var typeNode = new TypeNode<TFact>();

                typeNode.ConnectObserver(new FactObservableAdapter<TFact>(_observers));

                return typeNode;
            }
        }
    }
}