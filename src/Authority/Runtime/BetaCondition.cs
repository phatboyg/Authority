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
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using GreenPipes.Util;
    using Internals;


    public class BetaCondition<TLeft, TRight> :
        IBetaCondition<TLeft, TRight>
        where TLeft : class
        where TRight : class
    {
        readonly Expression<Func<TLeft, TRight, bool>> _compareExpression;
        readonly Func<ITupleChain<TLeft>, TRight, bool> _compare;

        public static readonly IBetaCondition<TLeft, TRight> True = new TrueBetaCondition();

        public BetaCondition(Func<ITupleChain<TLeft>, TRight, bool> compare)
        {
            _compare = compare;
        }

        public BetaCondition(Expression<Func<TLeft, TRight, bool>> compareExpression)
        {
            _compareExpression = compareExpression;

            var compare = ExpressionCompiler.Compile<Func<TLeft, TRight, bool>>(compareExpression);

            bool Comparator(ITupleChain<TLeft> chain, TRight right) => compare(chain.Right, right);

            _compare = Comparator;
        }

        public Task<bool> Evaluate(SessionContext context, ITupleChain<TLeft> tupleChain, TRight fact)
        {
            return Task.FromResult(_compare(tupleChain, fact));
        }


        class TrueBetaCondition :
            IBetaCondition<TLeft, TRight>
        {
            public Task<bool> Evaluate(SessionContext context, ITupleChain<TLeft> tupleChain, TRight fact)
            {
                return TaskUtil.True;
            }
        }
    }


    public class BetaConditionAdapter<T, TLeft, TRight> :
        IBetaCondition<T, TRight>
        where T : class
        where TLeft : class
        where TRight : class
    {
        readonly Func<ITupleChain<TLeft>, TRight, bool> _compare;
        readonly int _index;

        public BetaConditionAdapter(Func<ITupleChain<TLeft>, TRight, bool> compare, int index)
        {
            _compare = compare;
            _index = index;
        }

        public Task<bool> Evaluate(SessionContext context, ITupleChain<T> tupleChain, TRight fact)
        {
            if (tupleChain.TryGetFact(_index, out ITupleChain<TLeft> value))
            {
                return Task.FromResult(_compare(value, fact));
            }

            throw new InvalidOperationException($"The tuple chain did not contain the specified fact type at index");
        }
    }
}