// Copyright 2012-2017 Chris Patterson
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
namespace Authority.Rules.Facts
{
    using System;
    using System.Linq.Expressions;


    public class RuleFactDeclaration<T> :
        FactDeclaration<T>,
        Fact<T>
        where T : class
    {
        readonly ParameterExpression _parameter;

        public RuleFactDeclaration(string name, ParameterExpression parameter)
        {
            Name = name;
            _parameter = parameter;

            FactType = typeof(T);
        }

        public string Name { get; }

        public Type FactType { get; }

        bool Equals(FactDeclaration other)
        {
            return string.Equals(Name, other.Name) && FactType == other.FactType;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj is FactDeclaration other)
                return Equals(other);

            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Name.GetHashCode() * 397) ^ FactType.GetHashCode();
            }
        }
    }
}