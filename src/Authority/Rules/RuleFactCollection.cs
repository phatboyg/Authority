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
namespace Authority.Rules
{
    using System;
    using System.Collections.Generic;
    using Facts;
    using GreenPipes.Internals.Extensions;


    public class RuleFactCollection
    {
        readonly Dictionary<string, IRuleFact> _facts;

        public RuleFactCollection()
        {
            _facts = new Dictionary<string, IRuleFact>(StringComparer.OrdinalIgnoreCase);
        }

        public void Add<T>(string name, IRuleFact<T> fact)
            where T : class
        {
            _facts.Add(name, fact);
        }

        public IRuleFact GetFact(string name)
        {
            IRuleFact ruleFact;
            if (!_facts.TryGetValue(name, out ruleFact))
                throw new FactNotFoundException($"The fact is unknown or has not been declared: {name}");

            return ruleFact;
        }

        public IRuleFact<T> GetFact<T>(string name)
            where T : class
        {
            IRuleFact fact;
            if (!_facts.TryGetValue(name, out fact))
                throw new FactNotFoundException($"The fact is unknown or has not been declared: {name}");

            var ruleFact = fact as IRuleFact<T>;
            if (ruleFact == null)
                throw new ArgumentException(
                    $"The declared fact type does not match: {name} (specified: {TypeNameCache<T>.ShortName}, expected: {TypeNameCache.GetShortName(fact.FactType)})",
                    nameof(name));

            return ruleFact;
        }
    }
}