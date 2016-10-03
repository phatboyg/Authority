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
namespace Authority.Configurators
{
    using System.Collections.Generic;
    using Builders;
    using Microsoft.Extensions.Logging;
    using Rules;


    public class SupremeAuthorityConfigurator :
        IAuthorityConfigurator,
        IBuildSupremeAuthority
    {
        readonly List<IRule> _rules;

        ILoggerFactory _loggerFactory;

        public SupremeAuthorityConfigurator()
        {
            _rules = new List<IRule>();
        }

        public void AddRule(IRule rule)
        {
            _rules.Add(rule);
        }

        public void SetLoggerFactory(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        public IAuthority Build()
        {
            var builder = new RuntimeBuilder(_loggerFactory);

            foreach (var rule in _rules)
                rule.Apply(builder);

            return new SupremeAuthority(builder.Build(), _loggerFactory);
        }
    }
}