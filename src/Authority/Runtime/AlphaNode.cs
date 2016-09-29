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
    using System.Linq;
    using System.Threading.Tasks;
    using GreenPipes.Util;
    using Util;


    public class AlphaNode<TFact> :
        IAlphaNode<TFact>
        where TFact : class
    {
        readonly OrderedHashSet<IActivation<TFact>> _activations;

        public AlphaNode()
        {
            _activations = new OrderedHashSet<IActivation<TFact>>();
        }

        public virtual Task Insert(FactContext<TFact> context)
        {
            if (Matches(context))
                return context.WorkingMemory.Access(this, memory =>
                {
                    memory.Add(context.Fact);

                    return Task.WhenAll(_activations.Select(x => x.Insert(context)));
                });

            return TaskUtil.Completed;
        }

        protected virtual bool Matches(FactContext<TFact> context)
        {
            return true;
        }
    }
}