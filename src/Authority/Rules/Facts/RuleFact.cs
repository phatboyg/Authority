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
namespace Authority.Rules.Facts
{
    using RuleCompiler;


    public class RuleFact<T> :
        Fact<T>
        where T : class
    {
        readonly string _name;
        readonly RuleParameter<T> _parameter;

        public RuleFact(string name, RuleParameter<T> parameter)
        {
            _parameter = parameter;
            _name = name;
        }

        IRuleParameter<T> IRuleFact<T>.Parameter => _parameter;

        string Fact<T>.Name => _name;

        IRuleParameter IRuleFact.Parameter => _parameter;
    }
}