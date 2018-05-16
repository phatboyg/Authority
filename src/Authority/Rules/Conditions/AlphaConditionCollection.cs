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
namespace Authority.Rules.Conditions
{
    using System.Collections.Generic;
    using Actions;
    using Builders;


    /// <summary>
    /// The conditions declared by a rule, organized by fact declaration
    /// </summary>
    public class AlphaConditionCollection<TFact>
        where TFact : class
    {
        readonly List<IRuleCondition<TFact>> _conditions;

        public AlphaConditionCollection()
        {
            _conditions = new List<IRuleCondition<TFact>>();
        }

        public void Add(IRuleCondition<TFact> condition)
        {
            _conditions.Add(condition);
        }

        public AlphaBuilderContext<TFact> Build(IRuntimeBuilder builder, AlphaBuilderContext<TFact> context)
        {
            foreach (var condition in _conditions)
            {
                context = builder.BuildSelectNode(context, condition.ConditionExpression);
            }

            return context;
        }
    }

    /// <summary>
    /// The conditions declared by a rule, organized by fact declaration
    /// </summary>
    public class AlphaActionCollection<TFact>
        where TFact : class
    {
        readonly List<IRuleAction<TFact>> _actions;

        public AlphaActionCollection()
        {
            _actions = new List<IRuleAction<TFact>>();
        }

        public void Add(IRuleAction<TFact> action)
        {
            _actions.Add(action);
        }

        public BetaBuilderContext<TFact> Build(IRuntimeBuilder builder, BetaBuilderContext<TFact> context)
        {
//            var terminalContext = builder.BuildTerminalNode(context);
//
//
//            terminalContext = builder.BuildRuleNode(terminalContext, _actions);
//
//            foreach (var action in _actions)
//            {
//
//                context = builder.BuildSelectionNode(context, action.ConditionExpression);
//            }

            return context;
        }
    }
}