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


    public class BetaNode<TLeft, TRight> :
        IBetaNode<TLeft, TRight>
        where TRight : class
        where TLeft : class
    {
        readonly ITupleSource<TLeft> _leftSource;
        readonly Lazy<IBetaMemoryNode<TRight>> _memoryNode;
        readonly IFactSource<TRight> _rightSource;

        ConnectHandle _leftHandle;
        ConnectHandle _rightHandle;

        public BetaNode(ITupleSource<TLeft> leftSource, IFactSource<TRight> rightSource)
        {
            _leftSource = leftSource;
            _rightSource = rightSource;

            _memoryNode = new Lazy<IBetaMemoryNode<TRight>>(() => new BetaMemoryNode<TRight>());

            _leftHandle = leftSource.Connect(this);
            _rightHandle = rightSource.Connect(this);
        }

        protected IBetaMemoryNode<TRight> MemoryNode => _memoryNode.Value;

        public virtual Task Insert(FactContext<TRight> context)
        {
            return _leftSource.ForEach(context, x => Evaluate(context, x.Tuple, context.Fact)
                ? MemoryNode.Insert(context, x.Tuple, context.Fact)
                : TaskUtil.Completed);
        }

        Task ITupleSink<TLeft>.Insert(TupleContext<TLeft> context)
        {
            return _rightSource.ForEachAsync(context, x => Evaluate(context, context.Tuple, x.Fact)
                ? MemoryNode.Insert(context, context.Tuple, x.Fact)
                : TaskUtil.Completed);
        }

        public Task ForEach(SessionContext context, Func<TupleContext<TRight>, Task> callback)
        {
            return MemoryNode.ForEach(context, callback);
        }

        public ConnectHandle Connect(ITupleSink<TRight> sink)
        {
            return MemoryNode.Connect(sink);
        }

//        protected IEnumerable<Fact> MatchingFacts(IExecutionContext context, Tuple tuple)
//        {
//            return RightSource.GetFacts(context).Where(fact => MatchesConditions(context, tuple, fact));
//        }
//
//        protected IEnumerable<Tuple> MatchingTuples(IExecutionContext context, Fact<> fact)
//        {
//            return LeftSource.GetTuples(context).Where(tuple => MatchesConditions(context, tuple, fact));
//        }

        protected virtual bool Evaluate(SessionContext context, ITuple<TLeft> left, TRight right)
        {
            return true; //Conditions.All(joinCondition => joinCondition.IsSatisfiedBy(context, left, right));
        }
    }
}