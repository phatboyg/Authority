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
namespace Authority
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Runtime;


    public class Session :
        ISession,
        SessionContext
    {
        readonly AuthorityContext _authorityContext;
        readonly FactIndex _factIndex;
        readonly INetwork _network;
        readonly Stopwatch _stopwatch;
        readonly IWorkingMemory _workingMemory;

        public Session(AuthorityContext authorityContext, INetwork network)
        {
            _authorityContext = authorityContext;
            _network = network;

            _factIndex = new FactIndex();
            _workingMemory = new WorkingMemory();

            _stopwatch = Stopwatch.StartNew();
        }

        public TimeSpan ElapsedTime => _stopwatch.Elapsed;

        async Task<FactHandle<T>> ISession.Add<T>(T fact)
        {
            FactHandle<T> factHandle = _factIndex.Add(fact);

            var activationContext = new SessionFactContext<T>(this, factHandle.Fact);

            await _network.Insert(activationContext).ConfigureAwait(false);

            return factHandle;
        }

        Task<FactHandle> ISession.Add(object fact)
        {
            throw new NotImplementedException();
        }

        IWorkingMemory SessionContext.WorkingMemory => _workingMemory;
    }
}