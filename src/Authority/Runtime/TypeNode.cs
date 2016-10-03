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
    using Microsoft.Extensions.Logging;


    public class TypeNode<TFact> :
        AlphaNode<TFact>,
        ITypeNode<TFact>
        where TFact : class
    {
        readonly FactObservable<TFact> _observers;

        public TypeNode(ILoggerFactory loggerFactory)
            : base(loggerFactory)
        {
            _observers = new FactObservable<TFact>();
        }

        public ObserverHandle ConnectObserver(IFactObserver<TFact> observer)
        {
            return _observers.Connect(observer).ToObserverHandle();
        }

        async Task IFactSink.Insert<T>(FactContext<T> factContext)
        {
            var typeContext = factContext as FactContext<TFact>;
            if (typeContext == null)
                return;

            try
            {
                await _observers.PreInsert(typeContext).ConfigureAwait(false);

                await Insert(typeContext).ConfigureAwait(false);

                await _observers.PostInsert(typeContext).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                await _observers.InsertFault(typeContext, exception).ConfigureAwait(false);

                throw;
            }
        }

        public override void Accept<TContext>(RuntimeVisitor<TContext> visitor, TContext context)
        {
            if (visitor.IsCompleted)
                return;

            visitor.VisitTypeNode(context, this);
        }
    }
}