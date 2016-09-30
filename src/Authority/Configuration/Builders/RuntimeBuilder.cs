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
namespace Authority.Builders
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using GreenPipes.Internals.Extensions;
    using Microsoft.Extensions.Logging;
    using RuleModels;
    using Rules;
    using Rules.Facts;
    using Runtime;


    public class RuntimeBuilder :
        IRuntimeBuilder
    {
        readonly Network _network;
        readonly ILogger<RuntimeBuilder> _logger;

        public RuntimeBuilder(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<RuntimeBuilder>();
            _network = new Network(loggerFactory);
        }

//        public IEnumerable<ITerminalNode> AddRule(IRuleDefinition ruleDefinition)
//        {
//            List<Declaration> ruleDeclarations = ruleDefinition.LeftHandSide.Declarations.ToList();
//            var terminals = new List<ITerminalNode>();
//            ruleDefinition.LeftHandSide.Match(
//                and =>
//                {
//                    var context = new ReteBuilderContext(_dummyNode);
//                    Visit(context, and);
//                    var terminalNode = BuildTerminalNode(context, ruleDeclarations);
//                    terminals.Add(terminalNode);
//                },
//                or =>
//                {
//                    foreach (var childElement in or.ChildElements)
//                    {
//                        var context = new ReteBuilderContext(_dummyNode);
//                        Visit(context, childElement);
//                        var terminalNode = BuildTerminalNode(context, ruleDeclarations);
//                        terminals.Add(terminalNode);
//                    }
//                });
//            return terminals;
//        }
//
        public INetwork Build()
        {
            return _network;
        }

        public ITerminalNode<T> BuildTerminalNode<T>(BuilderContext context, IRuleFact<T> fact)
            where T : class
        {
//            if (context.AlphaSource != null)
//                BuildJoinNode(context);

            var factIndexMap = context.CreateIndexMap(fact);

            var terminalNode = new TerminalNode<T>(context.BetaSource, factIndexMap);

            return terminalNode;
        }



//
//        TerminalNode BuildTerminalNode(ReteBuilderContext context, IEnumerable<Declaration> ruleDeclarations)
//        {
//            if (context.AlphaSource != null)
//                BuildJoinNode(context);
//            var factIndexMap = IndexMap.CreateMap(ruleDeclarations, context.Declarations);
//            var terminalNode = new TerminalNode(context.BetaSource, factIndexMap);
//            return terminalNode;
//        }
//
//        protected override void VisitAnd(ReteBuilderContext context, AndElement element)
//        {
//            foreach (var childElement in element.ChildElements)
//            {
//                if (context.AlphaSource != null)
//                    BuildJoinNode(context);
//                Visit(context, childElement);
//            }
//        }
//
//        protected override void VisitOr(ReteBuilderContext context, OrElement element)
//        {
//            throw new InvalidOperationException("Group Or element must be normalized");
//        }
//
//        protected override void VisitForAll(ReteBuilderContext context, ForAllElement element)
//        {
//            throw new InvalidOperationException("ForAll element must be normalized");
//        }
//
//        protected override void VisitNot(ReteBuilderContext context, NotElement element)
//        {
//            BuildSubnet(context, element.Source);
//            BuildNotNode(context);
//        }
//
//        protected override void VisitExists(ReteBuilderContext context, ExistsElement element)
//        {
//            BuildSubnet(context, element.Source);
//            BuildExistsNode(context);
//        }
//
//        protected override void VisitAggregate(ReteBuilderContext context, AggregateElement element)
//        {
//            BuildSubnet(context, element.Source);
//            BuildAggregateNode(context, element);
//        }
//
//        protected override void VisitPattern(ReteBuilderContext context, PatternElement element)
//        {
//            if (element.Source == null)
//            {
//                context.CurrentAlphaNode = _root;
//                context.RegisterDeclaration(element.Declaration);
//
//                BuildTypeNode(context, element.ValueType);
//                List<ConditionElement> alphaConditions = element.Conditions.Where(x => x.References.Count() == 1).ToList();
//                foreach (var alphaCondition in alphaConditions)
//                    BuildSelectionNode(context, alphaCondition);
//                BuildAlphaMemoryNode(context);
//
//                List<ConditionElement> betaConditions = element.Conditions.Where(x => x.References.Count() > 1).ToList();
//                if (betaConditions.Count > 0)
//                    BuildJoinNode(context, betaConditions);
//            }
//            else
//            {
//                if (element.Conditions.Any())
//                {
//                    BuildSubnet(context, element.Source);
//                    context.RegisterDeclaration(element.Declaration);
//
//                    BuildJoinNode(context, element.Conditions);
//                }
//                else
//                {
//                    Visit(context, element.Source);
//                    context.RegisterDeclaration(element.Declaration);
//                }
//            }
//        }
//
//        void BuildSubnet(ReteBuilderContext context, RuleElement element)
//        {
//            var subnetContext = new ReteBuilderContext(context);
//            Visit(subnetContext, element);
//
//            if (subnetContext.AlphaSource == null)
//            {
//                var adapter = subnetContext.BetaSource
//                    .Sinks.OfType<ObjectInputAdapter>()
//                    .SingleOrDefault();
//                if (adapter == null)
//                    adapter = new ObjectInputAdapter(subnetContext.BetaSource);
//                subnetContext.AlphaSource = adapter;
//                context.HasSubnet = true;
//            }
//            context.AlphaSource = subnetContext.AlphaSource;
//        }
//
//        void BuildJoinNode(ReteBuilderContext context, IEnumerable<ConditionElement> conditions = null)
//        {
//            var betaConditions = new List<BetaCondition>();
//            if (conditions != null)
//                foreach (var condition in conditions)
//                {
//                    var factIndexMap = IndexMap.CreateMap(condition.References, context.Declarations);
//                    var betaCondition = new BetaCondition(condition.Expression, factIndexMap);
//                    betaConditions.Add(betaCondition);
//                }
//
//            var node = context.BetaSource
//                .Sinks.OfType<JoinNode>()
//                .FirstOrDefault(x =>
//                    (x.RightSource == context.AlphaSource) &&
//                        (x.LeftSource == context.BetaSource) &&
//                        ConditionComparer.AreEqual(x.Conditions, betaConditions));
//            if (node == null)
//            {
//                node = new JoinNode(context.BetaSource, context.AlphaSource);
//                if (context.HasSubnet)
//                    node.Conditions.Insert(0, new SubnetCondition());
//                foreach (var betaCondition in betaConditions)
//                    node.Conditions.Add(betaCondition);
//            }
//            BuildBetaMemoryNode(context, node);
//            context.ResetAlphaSource();
//        }
//
//        void BuildNotNode(ReteBuilderContext context)
//        {
//            var node = context.AlphaSource
//                .Sinks.OfType<NotNode>()
//                .FirstOrDefault(x =>
//                    (x.RightSource == context.AlphaSource) &&
//                        (x.LeftSource == context.BetaSource));
//            if (node == null)
//            {
//                node = new NotNode(context.BetaSource, context.AlphaSource);
//                if (context.HasSubnet)
//                    node.Conditions.Insert(0, new SubnetCondition());
//            }
//            BuildBetaMemoryNode(context, node);
//            context.ResetAlphaSource();
//        }
//
//        void BuildExistsNode(ReteBuilderContext context)
//        {
//            var node = context.AlphaSource
//                .Sinks.OfType<ExistsNode>()
//                .FirstOrDefault(x =>
//                    (x.RightSource == context.AlphaSource) &&
//                        (x.LeftSource == context.BetaSource));
//            if (node == null)
//            {
//                node = new ExistsNode(context.BetaSource, context.AlphaSource);
//                if (context.HasSubnet)
//                    node.Conditions.Insert(0, new SubnetCondition());
//            }
//            BuildBetaMemoryNode(context, node);
//            context.ResetAlphaSource();
//        }
//
//        void BuildAggregateNode(ReteBuilderContext context, AggregateElement element)
//        {
//            var node = context.AlphaSource
//                .Sinks.OfType<AggregateNode>()
//                .FirstOrDefault(x =>
//                    (x.RightSource == context.AlphaSource) &&
//                        (x.LeftSource == context.BetaSource) &&
//                        (x.Name == element.Name) &&
//                        ExpressionMapComparer.AreEqual(x.ExpressionMap, element.ExpressionMap));
//            if (node == null)
//            {
//                node = new AggregateNode(context.BetaSource, context.AlphaSource, element.Name, element.ExpressionMap, element.AggregatorFactory);
//                if (context.HasSubnet)
//                    node.Conditions.Insert(0, new SubnetCondition());
//            }
//            BuildBetaMemoryNode(context, node);
//            context.ResetAlphaSource();
//        }
//
//        void BuildBetaMemoryNode(ReteBuilderContext context, BetaNode betaNode)
//        {
//            if (betaNode.MemoryNode == null)
//                betaNode.MemoryNode = new BetaMemoryNode();
//            context.BetaSource = betaNode.MemoryNode;
//        }
//
//
//        void BuildAlphaMemoryNode(ReteBuilderContext context)
//        {
//            AlphaMemoryNode memoryNode = context.CurrentAlphaNode.MemoryNode;
//
//            if (memoryNode == null)
//            {
//                memoryNode = new AlphaMemoryNode();
//                context.CurrentAlphaNode.MemoryNode = memoryNode;
//            }
//
//            context.AlphaSource = memoryNode;
//        }
        public BuilderContext CreateContext()
        {
            return new RuntimeBuilderContext();
        }

        public ITypeNode<T> BuildTypeNode<T>(BuilderContext context)
            where T : class
        {
            using (_logger.BeginScope($"{nameof(BuildTypeNode)}<{typeof(T).Name}>"))
            {
                var typeNode = _network.GetTypeNode<T, TypeNode<T>>();

                context.CurrentAlphaNode = typeNode;

                return typeNode;
            }
        }

        public ISelectionNode<T> BuildSelectionNode<T>(BuilderContext context, Expression<Func<T, bool>> conditionExpression)
            where T : class
        {
            using (_logger.BeginScope($"{nameof(BuildSelectionNode)}<{typeof(T).Name}>"))
            {
                var alphaCondition = new AlphaCondition<T>(conditionExpression);

                _logger.LogDebug($"Condition: {alphaCondition}");

                var selectionNode = context.CurrentAlphaNode.GetChildNodes<SelectionNode<T>>()
                    .FirstOrDefault(x => x.Condition.Equals(alphaCondition));
                if (selectionNode == null)
                {
                    using (_logger.BeginScope("Create"))
                    {
                        selectionNode = new SelectionNode<T>(alphaCondition);
                        var handle = context.CurrentAlphaNode.AddChild(selectionNode);

                        context.AddHandle(handle);
                    }
                }

                context.CurrentAlphaNode = selectionNode;

                return selectionNode;
            }
        }
    }
}