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
namespace Authority.RuleModels
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;


    /// <summary>
    /// Sorted readonly map of named expressions.
    /// </summary>
    public class ExpressionMap :
        IEnumerable<NamedExpression>
    {
        readonly SortedDictionary<string, NamedExpression> _expressions;

        public ExpressionMap(IEnumerable<NamedExpression> expressions)
        {
            _expressions = new SortedDictionary<string, NamedExpression>(expressions.ToDictionary(x => x.Name));
        }

        /// <summary>
        /// Number of expressions in the map.
        /// </summary>
        public int Count
        {
            get { return _expressions.Count; }
        }

        /// <summary>
        /// Retrieves expression by name.
        /// </summary>
        /// <param name="name">Expression name.</param>
        /// <returns>Matching expression.</returns>
        public LambdaExpression this[string name]
        {
            get
            {
                NamedExpression result;
                var found = _expressions.TryGetValue(name, out result);
                if (!found)
                    throw new ArgumentException($"Expression with the given name not found. Name={name}", "name");
                return result.Expression;
            }
        }

        public IEnumerator<NamedExpression> GetEnumerator()
        {
            return _expressions.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}