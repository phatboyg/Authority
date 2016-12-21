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
namespace Authority.Tests
{
    using System;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using Rules;


    [TestFixture]
    public class Declaring_a_rule
    {
        [Test]
        public async Task Should_add_conditions_to_the_engine()
        {
            IRule rule = new MyRule();

            var authority = Authority.Factory.CreateAuthority(cfg =>
            {
                cfg.SetLoggerFactory(ContextSetup.LoggerFactory);

                cfg.AddRule(rule);
            });


            var visitor = new GraphRuntimeVisitor();

            var graphContext = new GraphContext();

            visitor.Visit(graphContext, authority);

            graphContext.Dump(Console.Out);


            var session = await authority.CreateSession();

            FactHandle<MemberName> memberName = await session.Insert(new MemberName()
            {
                First = "Brandon",
                MemberId = 27
            });

            FactHandle<MemberAddress> memberAddress = await session.Insert(new MemberAddress()
            {
                MemberId = 27,
                PostalCode = "90210",
            });
        }

        [Test]
        public void Should_find_all_expressed_facts()
        {
            IRule rule = new MyRule();

            Assert.That(rule.GetFact("Name"), Is.Not.Null);
           // Assert.That(rule.GetFact("Address"), Is.Not.Null);
        }


        sealed class MyRule :
            Rule
        {
            public MyRule()
            {
                Name("Member is named Brandon");

                Fact(() => Name);

                // simple conditionals
                When(Name, x => x.First == "Brandon");

                // then using on a single member of the rule match
                Then(Name, (context, name) => Console.Out.WriteLineAsync($"His name was {name.First}"));

                // then using on a single member of the rule match
                Then(Name, (context) => Console.Out.WriteLineAsync($"His name was {context.Fact.First}"));
            }

            Fact<MemberName> Name { get; set; }
        }

        sealed class OldRule :
            Rule
        {
            public OldRule()
            {
                Name("Member lives in Beverly Hills");

                Fact(() => Name);
                Fact(() => Address);

                // simple conditionals
                When(Name, x => x.First == "Brandon");
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

            public override string ToString()
            {
                return $"MemberName (MemberId={MemberId},First='{First}')";
            }
        }


        class MemberAddress
        {
            public int MemberId { get; set; }
            public string PostalCode { get; set; }

            public override string ToString()
            {
                return $"MemberAddress (MemberId={MemberId},PostalCode='{PostalCode}')";
            }
        }
    }
}