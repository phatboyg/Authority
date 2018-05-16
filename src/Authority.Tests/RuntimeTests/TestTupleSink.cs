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
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using GreenPipes.Util;
    using Runtime;


    class TestTupleSink<T> :
        ITupleSink<T>
        where T : class
    {
        readonly IList<BetaContext<T>> _tuples;

        public TestTupleSink()
        {
            _tuples = new List<BetaContext<T>>();
        }

        public IEnumerable<BetaContext<T>> Tuples => _tuples;

        public Task Insert(BetaContext<T> context)
        {
            _tuples.Add(context);

            return TaskUtil.Completed;
        }

        public void Accept<TContext>(RuntimeVisitor<TContext> visitor, TContext context)
        {
            throw new System.NotImplementedException();
        }
    }
}