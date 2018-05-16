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


    /// <summary>
    /// A source of facts, such as an alpha node
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ITupleSource<out T>
        where T : class
    {
        Task All(SessionContext context, BetaContextCallback<T> callback);

        /// <summary>
        /// Connect a sink to the fact source, so that subsequent activations automatically pass
        /// through to the sink.
        /// </summary>
        /// <param name="sink"></param>
        /// <returns></returns>
        ConnectHandle Connect(ITupleSink<T> sink);
    }


    public interface ITupleChainAccessor<out T>
        where T : class
    {
        T GetFact(ITupleChain tupleChain);
    }


    public class IndexTupleChainAccessor<T> :
        ITupleChainAccessor<T>
        where T : class
    {
        readonly int _index;

        public IndexTupleChainAccessor(int index)
        {
            _index = index;
        }

        public T GetFact(ITupleChain tupleChain)
        {
            if (tupleChain.TryGetFact(_index, out T fact))
                return fact;

            throw new InvalidOperationException($"The fact type was not found in the tuple chain");
        }
    }
}