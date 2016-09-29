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


    public class AlphaCondition<T> :
        IAlphaCondition<T>,
        IEquatable<AlphaCondition<T>>
        where T : class
    {
        readonly Func<T, bool> _condition;
        readonly Expression<Func<T, bool>> _conditionExpression;

        public AlphaCondition(Expression<Func<T, bool>> conditionExpression)
        {
            _conditionExpression = conditionExpression;
            _condition = conditionExpression.Compile();
        }

        public bool Evaluate(FactContext<T> context)
        {
            return _condition(context.Fact);
        }

        public bool Equals(AlphaCondition<T> other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return ExpressionComparer.AreEqual(_conditionExpression, other._conditionExpression);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != GetType())
                return false;
            return Equals((AlphaCondition<T>)obj);
        }

        public override int GetHashCode()
        {
            return _conditionExpression.GetHashCode();
        }

        public override string ToString()
        {
            return _conditionExpression.ToString();
        }
    }
}