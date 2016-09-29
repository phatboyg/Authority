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
    using System.Threading.Tasks;
    using GreenPipes.Util;
    using Util;


    public class RuntimeFactHandle<T> :
        FactHandle<T>
        where T : class
    {
        static readonly IEqualityComparer<RuntimeFactHandle<T>> IdComparerInstance = new IdEqualityComparer();
        readonly T _fact;
        readonly long _id;
        readonly Action<long> _remove;

        public RuntimeFactHandle(long id, T fact, Action<long> remove)
        {
            _id = id;
            _fact = fact;
            _remove = remove;
        }

        public static IEqualityComparer<RuntimeFactHandle<T>> IdComparer => IdComparerInstance;

        public Type FactType => typeof(T);

        public object FactObject => _fact;

        public Task Remove()
        {
            _remove(_id);

            return TaskUtil.Completed;
        }

        public T Fact => _fact;


        sealed class IdEqualityComparer :
            IEqualityComparer<RuntimeFactHandle<T>>
        {
            public bool Equals(RuntimeFactHandle<T> x, RuntimeFactHandle<T> y)
            {
                if (ReferenceEquals(x, y))
                    return true;
                if (ReferenceEquals(x, null))
                    return false;
                if (ReferenceEquals(y, null))
                    return false;
                if (x.GetType() != y.GetType())
                    return false;
                return x._id == y._id;
            }

            public int GetHashCode(RuntimeFactHandle<T> obj)
            {
                return obj._id.GetHashCode();
            }
        }
    }
}