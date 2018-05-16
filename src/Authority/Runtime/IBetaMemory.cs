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
    using System.Threading.Tasks;


    public interface IBetaMemory :
        INodeMemory
    {
    }


    public interface IBetaMemory<in TRight>
        where TRight : class
    {
        void Add(ITupleChain<TRight> tupleChain);

        void Remove(ITupleChain<TRight> tupleChain);

        bool TryGetTuple(ITupleChain<TRight> leftTupleChain, TRight rightFact, out ITupleChain tupleChain);

        Task ForEach<T>(SessionContext context, BetaContextCallback<T> callback)
            where T : class;
    }
}