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
    /// References a fact that has been added to a session
    /// </summary>
    public interface FactHandle
    {
        /// <summary>
        /// The fact type
        /// </summary>
        Type FactType { get; }

        /// <summary>
        /// The fact object
        /// </summary>
        object FactObject { get; }

        /// <summary>
        /// Removes the fact from the session
        /// </summary>
        Task Remove();
    }


    /// <summary>
    /// References a fact that has been added to a session
    /// </summary>
    /// <typeparam name="T">The fact type</typeparam>
    public interface FactHandle<out T> :
        FactHandle
        where T : class
    {
        /// <summary>
        /// The fact
        /// </summary>
        T Fact { get; }
    }
}