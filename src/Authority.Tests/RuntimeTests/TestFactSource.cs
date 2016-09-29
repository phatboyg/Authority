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
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using GreenPipes;
    using Runtime;
    using Util;


    class TestFactSource<T> :
        IFactSource<T>
        where T : class
    {
        readonly T[] _facts;
        readonly ConnectableList<IFactSink<T>> _sinks;

        public TestFactSource(params T[] facts)
        {
            _facts = facts;
            _sinks = new ConnectableList<IFactSink<T>>();
        }

        public ConnectHandle Connect(IFactSink<T> sink)
        {
            return _sinks.Connect(sink);
        }

        public Task ForEachAsync(SessionContext context, Func<FactContext<T>, Task> callback)
        {
            return Task.WhenAll(_facts.Select(x => callback(new TestFactContext(context, x))));
        }


        class TestFactContext :
            FactContext<T>
        {
            readonly SessionContext _context;

            public TestFactContext(SessionContext context, T fact)
            {
                _context = context;
                Fact = fact;
            }

            public IWorkingMemory WorkingMemory => _context.WorkingMemory;

            public T Fact { get; }
        }
    }
}