﻿// Copyright 2012-2016 Chris Patterson
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
    using GreenPipes;
    using Runtime;
    using Util;


    class TestTupleSource<T> :
        ITupleSource<T>
        where T : class
    {
        readonly ConnectableList<ITupleSink<T>> _sinks;
        readonly ITupleChain<T>[] _tupleChains;

        public TestTupleSource(params ITupleChain<T>[] tupleChains)
        {
            _tupleChains = tupleChains;
            _sinks = new ConnectableList<ITupleSink<T>>();
        }

        public ConnectHandle Connect(ITupleSink<T> sink)
        {
            return _sinks.Connect(sink);
        }

        public Task All(SessionContext context, BetaContextCallback<T> callback)
        {
            return Task.WhenAll(_tupleChains.Select(x => callback(new TestBetaContext<T>(context, x))));
        }
    }
}