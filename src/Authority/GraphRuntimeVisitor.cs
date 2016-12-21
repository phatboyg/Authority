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
    using System.IO;
    using System.Threading;
    using Runtime;
    using Util;


    public class GraphContext
    {
        readonly OrderedHashSet<NodeLink> _links;
        readonly OrderedHashSet<NodeInfo> _nodes;

        public GraphContext()
        {
            _nodes = new OrderedHashSet<NodeInfo>();
            _links = new OrderedHashSet<NodeLink>();
        }

        public void Add(NodeInfo node)
        {
            _nodes.Add(node);
        }

        public void Link(NodeInfo node, NodeInfo target)
        {
            _links.Add(new NodeLink(node, target));
        }

        public void Dump(TextWriter textWriter)
        {
            textWriter.WriteLine();
            textWriter.WriteLine("digraph network {");

            textWriter.WriteLine("node[style = \"rounded,filled\", width = 0, height = 0, shape = box, fillcolor = \"#E5E5E5\", concentrate = true]");

            foreach (var node in _nodes)
                textWriter.WriteLine($"{node.Id}[label = \"{node.Text} ({node.FactType.Name})\" width=2]");

            foreach (var link in _links)
            {
                NodeInfo targetNode;
                if (_nodes.TryGetValue(link.Target, out targetNode))
                {
                    textWriter.WriteLine($"{link.Node.Id} -> {targetNode.Id}");
                }
            }

            textWriter.WriteLine("}");
        }


        class NodeLink : IEquatable<NodeLink>
        {
            public NodeLink(NodeInfo node, NodeInfo target)
            {
                Node = node;
                Target = target;
            }

            public NodeInfo Node { get; }
            public NodeInfo Target { get; }

            public bool Equals(NodeLink other)
            {
                if (ReferenceEquals(null, other))
                    return false;
                if (ReferenceEquals(this, other))
                    return true;
                return Node.Equals(other.Node) && Target.Equals(other.Target);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj))
                    return false;
                if (ReferenceEquals(this, obj))
                    return true;
                if (obj.GetType() != GetType())
                    return false;
                return Equals((NodeLink)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (Node.GetHashCode() * 397) ^ Target.GetHashCode();
                }
            }
        }
    }


    public interface NodeInfo
    {
        Type FactType { get; }

        string Id { get; }

        string Text { get; }
    }


    public class GraphRuntimeVisitor :
        RuntimeVisitor<GraphContext>
    {
        long _id;

        public override void VisitAlphaNode<T>(GraphContext context, IAlphaNode<T> node)
        {
            var nodeInfo = new Node(Interlocked.Increment(ref _id), node, typeof(T), "alpha");

            context.Add(nodeInfo);
            context.Link(nodeInfo, new Node(-1, node.MemoryNode,typeof(T), ""));

            foreach (var childNode in node.GetChildNodes<INode>())
            {
                context.Link(nodeInfo, new Node(-1, childNode, typeof(T), ""));
            }

            base.VisitAlphaNode(context, node);
        }

        public override void VisitSelectionNode<T>(GraphContext context, ISelectionNode<T> node)
        {
            var nodeInfo = new Node(Interlocked.Increment(ref _id), node, typeof(T), "selection");

            context.Add(nodeInfo);
            context.Link(nodeInfo, new Node(-1, node.MemoryNode, typeof(T), ""));

            foreach (var childNode in node.GetChildNodes<INode>())
            {
                context.Link(nodeInfo, new Node(-1, childNode, typeof(T), ""));
            }

            base.VisitSelectionNode(context, node);
        }

        public override void VisitTypeNode<T>(GraphContext context, ITypeNode<T> node)
        {
            base.VisitTypeNode(context, node);
        }

        public override void VisitConditionNode<T>(GraphContext context, ConditionNode<T> node)
        {
            var nodeInfo = new Node(Interlocked.Increment(ref _id), node, typeof(T), "condition");

            context.Add(nodeInfo);
            context.Link(nodeInfo, new Node(-1, node.MemoryNode, typeof(T), ""));

            foreach (var childNode in node.GetChildNodes<INode>())
            {
                context.Link(nodeInfo, new Node(-1, childNode, typeof(T), ""));
            }

            base.VisitConditionNode(context, node);
        }

        public override void VisitBetaNode<TLeft, TRight>(GraphContext context, IBetaNode<TLeft, TRight> node)
        {
            var nodeInfo = new Node(Interlocked.Increment(ref _id), node, typeof(TRight), "beta");

            context.Add(nodeInfo);
            context.Link(nodeInfo, new Node(-1, node.MemoryNode, typeof(TRight), ""));

            base.VisitBetaNode(context, node);
        }

        public override void VisitJoinNode<TLeft, TRight>(GraphContext context, JoinNode<TLeft, TRight> node)
        {
            var nodeInfo = new Node(Interlocked.Increment(ref _id), node, typeof(TRight), "join");

            context.Add(nodeInfo);
            context.Link(nodeInfo, new Node(-1, node.MemoryNode, typeof(TRight), ""));

            base.VisitJoinNode(context, node);
        }

        public override void VisitAlphaMemoryNode<T>(GraphContext context, IAlphaMemoryNode<T> node)
        {
            var nodeInfo = new Node(Interlocked.Increment(ref _id), node, typeof(T), "a-memory");

            context.Add(nodeInfo);
            foreach (var sink in node.GetSinks())
            {
                context.Link(nodeInfo, new Node(-1, sink as INode, typeof(T), ""));
            }

            base.VisitAlphaMemoryNode(context, node);
        }

        public override void VisitBetaMemoryNode<TRight>(GraphContext context, IBetaMemoryNode<TRight> node)
        {
            var nodeInfo = new Node(Interlocked.Increment(ref _id), node, typeof(TRight), "b-memory");

            context.Add(nodeInfo);
            foreach (var sink in node.GetSinks())
            {
                context.Link(nodeInfo, new Node(-1, sink as INode, typeof(TRight), ""));
            }

            base.VisitBetaMemoryNode(context, node);
        }


        class Node :
            NodeInfo,
            IEquatable<Node>
        {
            readonly INode _node;

            public Node(long id, INode node, Type factType, string text)
            {
                _node = node;
                FactType = factType;
                Text = text;
                Id = $"n{id}";
            }

            public bool Equals(Node other)
            {
                if (ReferenceEquals(null, other))
                    return false;
                if (ReferenceEquals(this, other))
                    return true;
                return _node.Equals(other._node);
            }

            public string Id { get; }

            public Type FactType { get; }
            public string Text { get; }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj))
                    return false;
                if (ReferenceEquals(this, obj))
                    return true;
                if (obj.GetType() != GetType())
                    return false;
                return Equals((Node)obj);
            }

            public override int GetHashCode()
            {
                return _node.GetHashCode();
            }
        }
    }
}