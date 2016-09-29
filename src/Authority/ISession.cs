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


    /// <summary>
    /// A session is a stateful execution of facts in the rule engine
    /// </summary>
    public interface ISession
    {
        /// <summary>
        /// Returns the elapsed time spent processing rules by the session
        /// </summary>
        TimeSpan ElapsedTime { get; }

        /// <summary>
        /// Adds a fact to the session
        /// </summary>
        /// <typeparam name="T">The type of the fact</typeparam>
        /// <param name="fact">The fact</param>
        /// <returns>A fact handle, which can be used to remove the fact from the session</returns>
        Task<FactHandle<T>> Add<T>(T fact)
            where T : class;

        /// <summary>
        /// Adds a fact to the session
        /// </summary>
        /// <param name="fact">The fact</param>
        /// <returns>A fact handle, which can be used to remove the fact from the session</returns>
        Task<FactHandle> Add(object fact);
    }
}