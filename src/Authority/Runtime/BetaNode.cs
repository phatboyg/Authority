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


    public class BetaNode<TLeft, TRight> :
        IBetaNode<TLeft, TRight>
        where TRight : class
        where TLeft : class
    {
        readonly IBetaCondition<TLeft, TRight> _condition;
        readonly ITupleSource<TLeft> _leftSource;
        readonly Lazy<IBetaMemoryNode<TRight>> _memoryNode;
        readonly IFactSource<TRight> _rightSource;

        ConnectHandle _leftHandle;
        ConnectHandle _rightHandle;

        public BetaNode(ITupleSource<TLeft> leftSource, IFactSource<TRight> rightSource, IBetaCondition<TLeft, TRight> condition)
        {
            _leftSource = leftSource;
            _rightSource = rightSource;
            _condition = condition;

            _memoryNode = new Lazy<IBetaMemoryNode<TRight>>(() => new BetaMemoryNode<TRight>());

            _leftHandle = leftSource.Connect(this);
            _rightHandle = rightSource.Connect(this);
        }

        public IBetaMemoryNode<TRight> MemoryNode => _memoryNode.Value;

        public virtual Task Insert(FactContext<TRight> context)
        {
            return _leftSource.All(context, async tupleContext =>
            {
                var match = await Evaluate(context, tupleContext.Tuple, context.Fact).ConfigureAwait(false);
                if (match)
                    await MemoryNode.Insert(context, tupleContext.Tuple, context.Fact).ConfigureAwait(false);
            });
        }

        public virtual Task Insert(TupleContext<TLeft> context)
        {
            return _rightSource.All(context, async factContext =>
            {
                var match = await Evaluate(context, context.Tuple, factContext.Fact).ConfigureAwait(false);
                if (match)
                    await MemoryNode.Insert(context, context.Tuple, factContext.Fact).ConfigureAwait(false);
            });
        }

        public virtual Task All(SessionContext context, Func<TupleContext<TRight>, Task> callback)
        {
            return MemoryNode.All(context, callback);
        }

        public ConnectHandle Connect(ITupleSink<TRight> sink)
        {
            return MemoryNode.Connect(sink);
        }

        protected Task Matching(TupleContext<TLeft> context, Func<FactContext<TRight>, Task> callback)
        {
            return _rightSource.All(context, async factContext =>
            {
                var match = await Evaluate(context, context.Tuple, factContext.Fact).ConfigureAwait(false);
                if (match)
                    await callback(new SessionFactContext<TRight>(context, factContext.Fact)).ConfigureAwait(false);
            });
        }

        protected Task Matching(FactContext<TRight> context, Func<TupleContext<TLeft>, Task> callback)
        {
            return _leftSource.All(context, async tupleContext =>
            {
                var match = await Evaluate(context, tupleContext.Tuple, context.Fact).ConfigureAwait(false);
                if (match)
                    await callback(new SessionTupleContext<TLeft>(context, tupleContext.Tuple)).ConfigureAwait(false);
            });
        }

        protected virtual Task<bool> Evaluate(SessionContext context, ITuple<TLeft> left, TRight right)
        {
            return _condition.Evaluate(context, left, right);
//            return true; //Conditions.All(joinCondition => joinCondition.IsSatisfiedBy(context, left, right));
        }

        public virtual void Accept<TContext>(RuntimeVisitor<TContext> visitor, TContext context)
        {
            if (visitor.IsCompleted)
                return;

            visitor.VisitBetaNode(context, this);

            if (_memoryNode.IsValueCreated)
                _memoryNode.Value.Accept(visitor, context);
        }
    }
}