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
    using System.Threading.Tasks;


    public class Tuple<TRight> :
        ITuple<TRight>
        where TRight : class
    {
        readonly int _count;
        readonly ITuple _left;
        readonly TRight _right;

        public Tuple()
        {
        }

        public Tuple(ITuple left, TRight right)
        {
            _right = right;
            _left = left;
            _count = left.Count;

            if (right != null)
                _count++;
        }


        public Tuple(TRight right)
        {
            _right = right;
            _left = new Tuple<TRight>();
            _count = _left.Count;

            if (right != null)
                _count++;
        }

        public int Count => _count;

        public async Task ForEach<T>(SessionContext context, Func<TupleContext<T>, Task> callback)
            where T : class
        {
            var self = this as Tuple<T>;
            if (self != null)
                await callback(new TupleFactContext<T>(context, self, self._right)).ConfigureAwait(false);

            await _left.ForEach(context, callback).ConfigureAwait(false);
        }

        public TRight Right => _right;

        public ITuple Left => _left;


        class TupleFactContext<T> :
            TupleContext<T>
            where T : class
        {
            readonly SessionContext _context;

            public TupleFactContext(SessionContext context, ITuple<T> tuple, T fact)
            {
                _context = context;
                Tuple = tuple;
                Fact = fact;
            }

            public IWorkingMemory WorkingMemory => _context.WorkingMemory;

            public ITuple<T> Tuple { get; }
            public T Fact { get; }
        }
    }
}