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
    using System.Threading.Tasks;
    using GreenPipes.Util;
    using Runtime;


    public class FactObservable :
        Connectable<IFactObserver>,
        IFactObserver
    {
        public Task PreInsert<T>(FactContext<T> fact)
            where T : class
        {
            return ForEachAsync(x => x.PreInsert(fact));
        }

        public Task PostInsert<T>(FactContext<T> fact)
            where T : class
        {
            return ForEachAsync(x => x.PostInsert(fact));
        }

        public Task InsertFault<T>(FactContext<T> fact, Exception exception)
            where T : class
        {
            return ForEachAsync(x => x.InsertFault(fact, exception));
        }
    }


    public class FactObservable<T> :
        Connectable<IFactObserver<T>>,
        IFactObserver<T>
        where T : class
    {
        public Task PreInsert(FactContext<T> fact)
        {
            return ForEachAsync(x => x.PreInsert(fact));
        }

        public Task PostInsert(FactContext<T> fact)
        {
            return ForEachAsync(x => x.PostInsert(fact));
        }

        public Task InsertFault(FactContext<T> fact, Exception exception)
        {
            return ForEachAsync(x => x.InsertFault(fact, exception));
        }
    }
}