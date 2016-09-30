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
namespace Authority.Tests.Visitation
{
    using System;
    using NUnit.Framework;
    using Rules;
    using Runtime;


    [TestFixture]
    public class Visiting_the_network
    {
        sealed class MyRule :
            Rule
        {
            public MyRule()
            {
                Name("Member lives in Beverly Hills");

                Fact(() => Name);
                Fact(() => Address);

                // simple conditionals
                When(Name, x => x.First == "David");
                When(Address, x => x.PostalCode == "90210");

                // join conditional
                When(Name, Address, (name, address) => name.MemberId == address.MemberId);

                // then using on a single member of the rule match
                Then(Name, (context, name) => Console.Out.WriteLineAsync($"His name was {name.First}"));

                // using the output of the join for a rule match
                Then(Name, Address, (context, name, address) => Console.Out.WriteLineAsync($"{name.First} lives in {address.PostalCode}"));
            }

            Fact<MemberName> Name { get; set; }
            Fact<MemberAddress> Address { get; set; }
        }


        class MemberName
        {
            public int MemberId { get; set; }
            public string First { get; set; }
        }


        class MemberAddress
        {
            public int MemberId { get; set; }
            public string PostalCode { get; set; }
        }


        [Test]
        public void Should_find_all_alpha_nodes()
        {
            var visitor = new LogRuntimeVisitor(ContextSetup.LoggerFactory);


            var authority = Authority.Factory.CreateAuthority(cfg =>
            {
                cfg.SetLoggerFactory(ContextSetup.LoggerFactory);

                cfg.AddRule(new MyRule());
            });

            visitor.Visit(new LogContext(), authority);
        }
    }
}