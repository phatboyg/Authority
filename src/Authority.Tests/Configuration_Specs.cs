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
    using Runtime;


    [TestFixture]
    public class Configuration_Specs
    {
        class Name
        {
            public Name(string first, string last)
            {
                First = first;
                Last = last;
            }

            public string First { get; }
            public string Last { get; }
        }


        class Friend
        {
            public Friend(string name)
            {
                Name = name;
            }

            public string Name { get; }
        }


        class Observer :
            IFactObserver
        {
            public Task PreInsert<T>(FactContext<T> fact) where T : class
            {
                return Console.Out.WriteLineAsync($"PreInsert:{typeof(T).Name}");
            }

            public Task PostInsert<T>(FactContext<T> fact) where T : class
            {
                return Console.Out.WriteLineAsync($"PostInsert:{typeof(T).Name}");
            }

            public Task InsertFault<T>(FactContext<T> fact, Exception exception) where T : class
            {
                return Console.Out.WriteLineAsync($"InsertFault:{typeof(T).Name}:{exception}");
            }
        }


        [Test]
        public async Task Configuring_an_authority()
        {
            var authority = Authority.Factory.CreateAuthority(cfg =>
            {
//                cfg.Rule<Name>(r =>
//                {
//                    r.When(context => context.Fact.First == "David")
//                        .Then(context => context.Add(new Friend($"{context.Fact.First} {context.Fact.Last}")));
//                });
            });

            authority.ConnectObserver(new Observer());

            var session = await authority.CreateSession();

            await session.Add(new Name("David", "Roth"));
        }
    }
}