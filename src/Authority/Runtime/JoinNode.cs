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


    public class JoinNode<TLeft, TRight> :
        BetaNode<TLeft, TRight>
        where TLeft : class
        where TRight : class
    {
        readonly Func<TLeft, TRight, bool> _comparator;

        public JoinNode(ITupleSource<TLeft> leftSource, IFactSource<TRight> rightSource, Func<TLeft, TRight, bool> comparator)
            : base(leftSource, rightSource)
        {
            _comparator = comparator;
        }

        protected override bool Evaluate(SessionContext context, ITuple<TLeft> left, TRight right)
        {
            return _comparator(left.Right, right);
        }
    }
}