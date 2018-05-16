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


    public interface ITupleChain
    {
        int Count { get; }

        ITupleChain Left { get; }

        /// <summary>
        /// The fact type for the rightmost Tuple
        /// </summary>
        Type FactType { get; }

        Task ForEach<T>(SessionContext context, BetaContextCallback<T> callback)
            where T : class;

        bool TryGetFact<T>(int index, out T value);
    }


    public interface ITupleChain<out T> :
        ITupleChain
    {
        T Right { get; }
    }
}