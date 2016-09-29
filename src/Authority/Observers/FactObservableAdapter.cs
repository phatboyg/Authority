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
    using Runtime;


    public class FactObservableAdapter<TFact> :
        IFactObserver<TFact>
        where TFact : class
    {
        readonly IFactObserver _observer;

        public FactObservableAdapter(IFactObserver observer)
        {
            _observer = observer;
        }

        public Task PreInsert(FactContext<TFact> context)
        {
            return _observer.PreInsert(context);
        }

        public Task PostInsert(FactContext<TFact> context)
        {
            return _observer.PostInsert(context);
        }

        public Task InsertFault(FactContext<TFact> context, Exception exception)
        {
            return _observer.InsertFault(context, exception);
        }
    }
}