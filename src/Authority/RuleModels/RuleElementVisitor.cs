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
    using System;
    using System.Collections.Generic;


    /// <summary>
    /// Rule element that serves as a source to pattern elements.
    /// </summary>
    public abstract class PatternSourceElement : RuleLeftElement
    {
        private readonly Type _resultType;

        /// <summary>
        /// Type of the result that this rule element yields.
        /// </summary>
        public Type ResultType
        {
            get { return _resultType; }
        }

        internal PatternSourceElement(IEnumerable<Declaration> declarations, Type resultType)
            : base(declarations)
        {
            _resultType = resultType;
        }
    }


    /// <summary>
    /// Visitor to traverse rule definition (or its part).
    /// </summary>
    /// <typeparam name="TContext">Traversal context.</typeparam>
    public class RuleElementVisitor<TContext>
    {
        public void Visit(TContext context, RuleElement element)
        {
            element.Accept(context, this);
        }

        protected internal virtual void VisitPattern(TContext context, PatternElement element)
        {
            foreach (ConditionElement condition in element.Conditions)
                condition.Accept(context, this);
            if (element.Source != null)
                element.Source.Accept(context, this);
        }

        protected internal virtual void VisitCondition(TContext context, ConditionElement element)
        {
        }

        protected internal virtual void VisitAggregate(TContext context, AggregateElement element)
        {
            if (element.Source != null)
                element.Source.Accept(context, this);
        }

        protected internal virtual void VisitNot(TContext context, NotElement element)
        {
            element.Source.Accept(context, this);
        }

        protected internal virtual void VisitExists(TContext context, ExistsElement element)
        {
            element.Source.Accept(context, this);
        }

        protected internal virtual void VisitForAll(TContext context, ForAllElement element)
        {
            element.BasePattern.Accept(context, this);
            foreach (PatternElement pattern in element.Patterns)
                pattern.Accept(context, this);
        }

        protected internal virtual void VisitAnd(TContext context, AndElement element)
        {
            VisitGroup(context, element);
        }

        protected internal virtual void VisitOr(TContext context, OrElement element)
        {
            VisitGroup(context, element);
        }

        void VisitGroup(TContext context, GroupElement element)
        {
            foreach (RuleLeftElement childElement in element.ChildElements)
                childElement.Accept(context, this);
        }

        protected internal virtual void VisitActionGroup(TContext context, ActionGroupElement element)
        {
            foreach (ActionElement action in element.Actions)
                action.Accept(context, this);
        }

        protected internal virtual void VisitAction(TContext context, ActionElement element)
        {
        }

        protected internal virtual void VisitDependencyGroup(TContext context, DependencyGroupElement element)
        {
            foreach (var dependency in element.Dependencies)
                dependency.Accept(context, this);
        }

        protected internal virtual void VisitDependency(TContext context, DependencyElement element)
        {
        }

        protected internal virtual void VisitPriority(TContext context, PriorityElement element)
        {
        }
    }
}