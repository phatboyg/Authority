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
namespace Authority.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using GreenPipes.Util;


    public class FactIndex
    {
        readonly Dictionary<long, FactHandle> _connections;
        long _nextId;

        public FactIndex()
        {
            _connections = new Dictionary<long, FactHandle>();
        }

        public FactHandle<T> Add<T>(T fact)
            where T : class
        {
            if (fact == null)
                throw new ArgumentNullException(nameof(fact));

            var id = Interlocked.Increment(ref _nextId);

            var factHandle = new RuntimeFactHandle<T>(id, fact, Disconnect);

            lock (_connections)
            {
                _connections.Add(id, factHandle);
            }

            return factHandle;
        }

        /// <summary>
        /// Enumerate the connections invoking the callback for each connection
        /// </summary>
        /// <param name="callback">The callback</param>
        /// <returns>An awaitable Task for the operation</returns>
        public Task ForEachAsync(Func<FactHandle, Task> callback)
        {
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));

            FactHandle[] connected;
            lock (_connections)
            {
                connected = _connections.Values.ToArray();
            }

            if (connected.Length == 0)
                return TaskUtil.Completed;

            if (connected.Length == 1)
                return callback(connected[0]);

            return Task.WhenAll(connected.Select(callback));
        }

        /// <summary>
        /// Enumerate the connections invoking the callback for each connection
        /// </summary>
        /// <param name="callback">The callback</param>
        /// <returns>An awaitable Task for the operation</returns>
        public Task ForEachAsync<T>(Func<FactHandle<T>, Task> callback)
            where T : class
        {
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));

            FactHandle<T>[] connected;
            lock (_connections)
            {
                connected = _connections.Values.Where(x => x.FactObject is T).Cast<FactHandle<T>>().ToArray();
            }

            if (connected.Length == 0)
                return TaskUtil.Completed;

            if (connected.Length == 1)
                return callback(connected[0]);

            return Task.WhenAll(connected.Select(callback));
        }

        public bool All(Func<FactHandle, bool> callback)
        {
            FactHandle[] connected;
            lock (_connections)
            {
                connected = _connections.Values.ToArray();
            }

            if (connected.Length == 0)
                return true;

            if (connected.Length == 1)
                return callback(connected[0]);

            return connected.All(callback);
        }

        void Disconnect(long id)
        {
            lock (_connections)
            {
                _connections.Remove(id);
            }
        }
    }
}