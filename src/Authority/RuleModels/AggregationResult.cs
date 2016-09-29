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
    /// <summary>
    /// Result of the aggregation.
    /// </summary>
    public struct AggregationResult
    {
        readonly AggregationAction _action;
        readonly object _aggregate;
        public static AggregationResult[] Empty = new AggregationResult[0];

        AggregationResult(AggregationAction action, object aggregate)
        {
            _action = action;
            _aggregate = aggregate;
        }

        /// <summary>
        /// Constructs an aggregation result that indicates no changes at the aggregate level.
        /// </summary>
        /// <param name="result">Aggregate.</param>
        /// <returns>Aggregation result.</returns>
        public static AggregationResult None(object result)
        {
            return new AggregationResult(AggregationAction.None, result);
        }

        /// <summary>
        /// Constructs an aggregation result that indicates a new aggregate.
        /// </summary>
        /// <param name="result">Aggregate.</param>
        /// <returns>Aggregation result.</returns>
        public static AggregationResult Added(object result)
        {
            return new AggregationResult(AggregationAction.Added, result);
        }

        /// <summary>
        /// Constructs an aggregation result that indicates a modification at the aggregate level.
        /// </summary>
        /// <param name="result">Aggregate.</param>
        /// <returns>Aggregation result.</returns>
        public static AggregationResult Modified(object result)
        {
            return new AggregationResult(AggregationAction.Modified, result);
        }

        /// <summary>
        /// Constructs an aggregation result that indicates an aggregate was removed.
        /// </summary>
        /// <param name="result">Aggregate.</param>
        /// <returns>Aggregation result.</returns>
        public static AggregationResult Removed(object result)
        {
            return new AggregationResult(AggregationAction.Removed, result);
        }

        /// <summary>
        /// Action that aggregation performed on the aggregate.
        /// </summary>
        public AggregationAction Action
        {
            get { return _action; }
        }

        /// <summary>
        /// Resulting aggregate.
        /// </summary>
        public object Aggregate
        {
            get { return _aggregate; }
        }
    }
}