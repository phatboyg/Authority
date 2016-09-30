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
    using System.Collections.Generic;
    using System.Linq;
    using RuleCompiler;
    using Rules.Facts;


    public class IndexMap
    {
        readonly int[] _map;

        public IndexMap(int[] map)
        {
            _map = map;
        }

        public int this[int index]
        {
            get { return index >= 0 ? _map[index] : -1; }
        }

        public static void SetElementAt(ref object[] target, int index, int offset, object value)
        {
            if (index >= 0)
                target[index + offset] = value;
        }

        public static IndexMap CreateMap(IEnumerable<IRuleParameter> facts, IEnumerable<IRuleFact> baseFacts)
        {
            IDictionary<IRuleParameter, int> positionMap = GetPositionMap(facts);

            int[] map = baseFacts.Select(x => IndexOrDefault(positionMap, x.Parameter)).ToArray();

            return new IndexMap(map);
        }

        static IDictionary<IRuleParameter, int> GetPositionMap(IEnumerable<IRuleParameter> facts)
        {
            return facts.Select((x, index) => new {Index = index, Fact = x}).ToDictionary(x => x.Fact, x => x.Index);
        }

        static int IndexOrDefault<TElement>(IDictionary<TElement, int> indexMap, TElement element)
        {
            int index;
            if (indexMap.TryGetValue(element, out index))
                return index;
            return -1;
        }
    }
}