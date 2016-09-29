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


    class RuleDefinition :
        IRuleDefinition
    {
        readonly DependencyGroupElement _dependencies;
        readonly string _description;
        readonly GroupElement _leftHandSide;
        readonly string _name;
        readonly PriorityElement _priority;
        readonly RuleRepeatability _repeatability;
        readonly ActionGroupElement _rightHandSide;
        readonly List<string> _tags;

        public RuleDefinition(string name, string description, RuleRepeatability repeatability, IEnumerable<string> tags,
            PriorityElement priority, DependencyGroupElement dependencies, GroupElement leftHandSide, ActionGroupElement rightHandSide)
        {
            _name = name;
            _description = description;
            _repeatability = repeatability;
            _tags = new List<string>(tags);

            _priority = priority;
            _dependencies = dependencies;
            _leftHandSide = leftHandSide;
            _rightHandSide = rightHandSide;
        }

        public static int DefaultPriority
        {
            get { return 0; }
        }

        public static RuleRepeatability DefaultRepeatability
        {
            get { return RuleRepeatability.Repeatable; }
        }

        public string Name
        {
            get { return _name; }
        }

        public string Description
        {
            get { return _description; }
        }

        public RuleRepeatability Repeatability
        {
            get { return _repeatability; }
        }

        public IEnumerable<string> Tags
        {
            get { return _tags; }
        }

        public PriorityElement Priority
        {
            get { return _priority; }
        }

        public DependencyGroupElement DependencyGroup
        {
            get { return _dependencies; }
        }

        public GroupElement LeftHandSide
        {
            get { return _leftHandSide; }
        }

        public ActionGroupElement RightHandSide
        {
            get { return _rightHandSide; }
        }
    }
}