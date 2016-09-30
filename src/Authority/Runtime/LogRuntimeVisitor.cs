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
    using Microsoft.Extensions.Logging;


    public class LogContext
    {
    }


    public class LogRuntimeVisitor :
        RuntimeVisitor<LogContext>
    {
        readonly ILogger _log;

        public LogRuntimeVisitor(ILoggerFactory loggerFactory)
        {
            _log = loggerFactory.CreateLogger(nameof(LogRuntimeVisitor));
        }

        public override void VisitAlphaNode<T>(LogContext context, IAlphaNode<T> node)
        {
            using (_log.BeginScope<AlphaNode<T>>())
            {
                base.VisitAlphaNode(context, node);
            }
        }

        public override void VisitBetaNode<TLeft, TRight>(LogContext context, IBetaNode<TLeft, TRight> node)
        {
            using (_log.BeginScope<BetaNode<TLeft, TRight>>())
            {
                base.VisitBetaNode(context, node);
            }
        }

        public override void VisitSelectionNode<T>(LogContext context, ISelectionNode<T> node)
        {
            using (_log.BeginScope<SelectionNode<T>>())
            {
                _log.LogDebug("Condition: {0}", node.Condition);

                base.VisitAlphaNode(context, node);
            }
        }

        public override void VisitConditionNode<T>(LogContext context, ConditionNode<T> node)
        {
            using (_log.BeginScope<ConditionNode<T>>())
            {
                base.VisitConditionNode(context, node);
            }
        }

        public override void VisitAlphaMemoryNode<T>(LogContext context, IAlphaMemoryNode<T> node)
        {
            using (_log.BeginScope<AlphaMemoryNode<T>>())
            {
                base.VisitAlphaMemoryNode(context, node);
            }
        }

        public override void VisitTypeNode<T>(LogContext context, ITypeNode<T> node)
        {
            using (_log.BeginScope<TypeNode<T>>())
            {
                base.VisitAlphaNode(context, node);
            }
        }
    }
}