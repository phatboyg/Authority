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
namespace Authority.RuleModels
{
    using System.Collections.Generic;


    /// <summary>
    /// Base interface for fact aggregators.
    /// </summary>
    public interface IAggregator
    {
        /// <summary>
        /// Resulting aggregates.
        /// </summary>
        IEnumerable<object> Aggregates { get; }

        /// <summary>
        /// Called when the new aggregator is initialized.
        /// </summary>
        /// <returns>Results of the operation on the aggregate.</returns>
        IEnumerable<AggregationResult> Initial();

        /// <summary>
        /// Called by the rules engine when a new fact enters corresponding aggregator.
        /// </summary>
        /// <param name="fact">New fact to add to the aggregate.</param>
        /// <returns>Results of the operation on the aggregate, based on the added fact.</returns>
        IEnumerable<AggregationResult> Add(object fact);

        /// <summary>
        /// Called by the rules engine when an existing fact is modified in the corresponding aggregatosr.
        /// </summary>
        /// <param name="fact">Existing fact to update in the aggregate.</param>
        /// <returns>Results of the operation on the aggregate, based on the modified fact.</returns>
        IEnumerable<AggregationResult> Modify(object fact);

        /// <summary>
        /// Called by the rules engine when an existing fact is removed from the corresponding aggregator.
        /// </summary>
        /// <param name="fact">Existing fact to remove from the aggregate.</param>
        /// <returns>Results of the operation on the aggregate, based on the removed fact.</returns>
        IEnumerable<AggregationResult> Remove(object fact);
    }
}