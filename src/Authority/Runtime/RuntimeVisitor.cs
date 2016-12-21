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
    /// <summary>
    /// Visits the runtime model of the engine
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    public class RuntimeVisitor<TContext>
    {
        bool _completed;

        public bool IsCompleted => _completed;

        protected void Complete()
        {
            _completed = true;
        }

        public virtual void Visit(TContext context, INode node)
        {
            node.Accept(this, context);
        }

        public virtual void Visit(TContext context, IAuthority authority)
        {
            authority.Accept(this, context);
        }

        public virtual void VisitAlphaNode<T>(TContext context, IAlphaNode<T> node)
            where T : class
        {
            VisitAlphaMemoryNode(context, node.MemoryNode);

            foreach (var childNode in node.GetChildNodes<INode>())
                childNode.Accept(this, context);
        }

        public virtual void VisitConditionNode<T>(TContext context, ConditionNode<T> node)
            where T : class
        {
            VisitAlphaMemoryNode(context, node.MemoryNode);

            foreach (var childNode in node.GetChildNodes<INode>())
                childNode.Accept(this, context);
        }

        public virtual void VisitAlphaMemoryNode<T>(TContext context, IAlphaMemoryNode<T> node)
            where T : class
        {
            foreach (IFactSink<T> sink in node.GetSinks())
                sink.Accept(this, context);
        }

        public virtual void VisitSelectionNode<T>(TContext context, ISelectionNode<T> node)
            where T : class
        {
            VisitAlphaMemoryNode(context, node.MemoryNode);

            foreach (var childNode in node.GetChildNodes<INode>())
                childNode.Accept(this, context);
        }

        public virtual void VisitJoinNode<TLeft, TRight>(TContext context, JoinNode<TLeft, TRight> node)
            where TLeft : class
            where TRight : class
        {
        }

        public virtual void VisitBetaNode<TLeft, TRight>(TContext context, IBetaNode<TLeft, TRight> node)
            where TLeft : class
            where TRight : class
        {
            VisitBetaMemoryNode(context, node.MemoryNode);
        }

        public virtual void VisitBetaMemoryNode<TRight>(TContext context, IBetaMemoryNode<TRight> node)
            where TRight : class
        {
        }

        public virtual void VisitDummyNode<TRight>(TContext context, IBetaMemoryNode<TRight> node)
            where TRight : class
        {
        }

        public virtual void VisitTypeNode<T>(TContext context, ITypeNode<T> node)
            where T : class
        {
            VisitAlphaMemoryNode(context, node.MemoryNode);

            foreach (var childNode in node.GetChildNodes<INode>())
                childNode.Accept(this, context);
        }

        public virtual void VisitRuleNode<T>(TContext context, RuleNode<T> ruleNode)
            where T : class
        {
        }
    }
}