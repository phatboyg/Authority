// Copyright 2012-2017 Chris Patterson
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
namespace Authority.Builders
{
    using Rules.Facts;


    /// <summary>
    /// A builder context is used to add rules to the network, using a runtime builder. Rules need to be smart as the context
    /// is incremental. Alpha networks should be added in fact order, and composed into beta networks.
    ///
    /// For instance, add fact A, all alpha conditions of A, all beta conditions of A, add fact B, all alpha conditions of B,
    /// all beta conditions of B, all beta conditions of A+B, add fact C, all alpha conditions of C, all beta conditions of B,
    /// all beta conditions of A+B, B+C, A+C, etc.
    /// </summary>
    public interface BuilderContext
    {
        /// <summary>
        /// Get the AlphaBuilderContext for the specified fact.
        /// </summary>
        /// <param name="fact"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        AlphaBuilderContext<T> GetAlphaBuilderContext<T>(FactDeclaration<T> fact)
            where T : class;

        /// <summary>
        /// Get the BetaBuilderContext for the specified fact
        /// </summary>
        /// <param name="fact"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        BetaBuilderContext<T> GetBetaBuilderContext<T>(FactDeclaration<T> fact)
            where T : class;
    }
}