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
namespace Authority.Tests.RuntimeTests
{
    using System.Linq;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using Runtime;


    [TestFixture]
    public class Running_a_beta_node
    {
        class Member
        {
            public int MemberId { get; set; }
            public string Name { get; set; }
        }


        class Address
        {
            public int MemberId { get; set; }
            public string PostalCode { get; set; }
        }


        [Test]
        public async Task Should_handle_inverse_activation()
        {
            ITupleSource<Member> leftSource = new TestTupleSource<Member>();

            IFactSource<Address> rightSource = new TestFactSource<Address>(
                new Address() {MemberId = 27, PostalCode = "68106"},
                new Address() {MemberId = 27, PostalCode = "74011"},
                new Address() {MemberId = 42, PostalCode = "74011"});

            var testSink = new TestTupleSink<Address>();

            IBetaNode<Member, Address> betaNode = new JoinNode<Member, Address>(leftSource, rightSource,
                new BetaCondition<Member, Address>((m, a) => m.Right.MemberId == a.MemberId));
            betaNode.Connect(testSink);

            var sessionContext = new TestSession();
            var tupleContext = new TestTupleContext<Member>(sessionContext, new Member {MemberId = 27, Name = "Frank"});

            await betaNode.Insert(tupleContext);

            Assert.That(testSink.Tuples.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task Should_properly_handle_activation()
        {
            ITupleSource<Member> leftSource = new TestTupleSource<Member>(new Tuple<Member>(new Member() {MemberId = 27, Name = "Frank"}));

            IFactSource<Address> rightSource = new TestFactSource<Address>();

            var testSink = new TestTupleSink<Address>();

            IBetaNode<Member, Address> betaNode = new JoinNode<Member, Address>(leftSource, rightSource,
                new BetaCondition<Member, Address>((m, a) => true));
            betaNode.Connect(testSink);

            var sessionContext = new TestSession();
            var factContext = new TestFactContext<Address>(sessionContext, new Address() {MemberId = 42, PostalCode = "68106"});

            await betaNode.Insert(factContext);

            Assert.That(testSink.Tuples.Count(), Is.EqualTo(1));
        }

        [Test]
        public async Task Should_properly_handle_activation_multiple()
        {
            ITupleSource<Member> leftSource = new TestTupleSource<Member>(new Tuple<Member>(new Member() {MemberId = 27, Name = "Frank"}));

            IFactSource<Address> rightSource = new TestFactSource<Address>();

            var testSink = new TestTupleSink<Address>();

            IBetaNode<Member, Address> betaNode = new JoinNode<Member, Address>(leftSource, rightSource,
                new BetaCondition<Member, Address>((m, a) => true));
            betaNode.Connect(testSink);

            var sessionContext = new TestSession();
            var factContext = new TestFactContext<Address>(sessionContext, new Address() {MemberId = 42, PostalCode = "68106"});

            await betaNode.Insert(factContext);

            factContext = new TestFactContext<Address>(sessionContext, new Address() {MemberId = 69, PostalCode = "74011"});

            await betaNode.Insert(factContext);

            Assert.That(testSink.Tuples.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task Should_properly_ignore_join_by_condition()
        {
            ITupleSource<Member> leftSource = new TestTupleSource<Member>(new Tuple<Member>(new Member() {MemberId = 27, Name = "Frank"}));

            IFactSource<Address> rightSource = new TestFactSource<Address>();

            var testSink = new TestTupleSink<Address>();

            IBetaNode<Member, Address> betaNode = new JoinNode<Member, Address>(leftSource, rightSource,
                new BetaCondition<Member, Address>((m, a) => m.Right.MemberId == a.MemberId));
            betaNode.Connect(testSink);

            var sessionContext = new TestSession();
            var factContext = new TestFactContext<Address>(sessionContext, new Address() {MemberId = 42, PostalCode = "68106"});

            await betaNode.Insert(factContext);

            Assert.That(testSink.Tuples.Count(), Is.EqualTo(0));
        }

        [Test]
        public async Task Should_properly_join_by_condition()
        {
            ITupleSource<Member> leftSource = new TestTupleSource<Member>(new Tuple<Member>(new Member() {MemberId = 27, Name = "Frank"}));

            IFactSource<Address> rightSource = new TestFactSource<Address>();

            var testSink = new TestTupleSink<Address>();

            IBetaNode<Member, Address> betaNode = new JoinNode<Member, Address>(leftSource, rightSource,
                new BetaCondition<Member, Address>((m, a) => m.Right.MemberId == a.MemberId));
            betaNode.Connect(testSink);

            var sessionContext = new TestSession();
            var factContext = new TestFactContext<Address>(sessionContext, new Address() {MemberId = 27, PostalCode = "68106"});

            await betaNode.Insert(factContext);

            Assert.That(testSink.Tuples.Count(), Is.EqualTo(1));
        }

        [Test]
        public async Task Should_properly_join_by_condition_multiple()
        {
            ITupleSource<Member> leftSource = new TestTupleSource<Member>(new Tuple<Member>(new Member() {MemberId = 27, Name = "Frank"}));

            IFactSource<Address> rightSource = new TestFactSource<Address>();

            var testSink = new TestTupleSink<Address>();

            IBetaNode<Member, Address> betaNode = new JoinNode<Member, Address>(leftSource, rightSource,
                new BetaCondition<Member, Address>((m, a) => m.Right.MemberId == a.MemberId));
            betaNode.Connect(testSink);

            var sessionContext = new TestSession();
            var factContext = new TestFactContext<Address>(sessionContext, new Address() {MemberId = 27, PostalCode = "68106"});

            await betaNode.Insert(factContext);

            factContext = new TestFactContext<Address>(sessionContext, new Address() {MemberId = 27, PostalCode = "74011"});

            await betaNode.Insert(factContext);

            factContext = new TestFactContext<Address>(sessionContext, new Address() {MemberId = 42, PostalCode = "74011"});

            await betaNode.Insert(factContext);

            Assert.That(testSink.Tuples.Count(), Is.EqualTo(2));
        }
    }
}