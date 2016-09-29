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
    using System.Collections.Generic;
    using RuleModels;
    using Runtime;


    class ReteBuilderContext
    {
        readonly List<Declaration> _declarations;

//        public ReteBuilderContext(DummyNode dummyNode)
//        {
//            _declarations = new List<Declaration>();
//            BetaSource = dummyNode;
//        }

        public ReteBuilderContext(ReteBuilderContext context)
        {
            BetaSource = context.BetaSource;
            _declarations = new List<Declaration>(context._declarations);
        }

        public IEnumerable<Declaration> Declarations
        {
            get { return _declarations; }
        }

//        public AlphaNode CurrentAlphaNode { get; set; }
        public IAlphaMemoryNode AlphaSource { get; set; }
        public IBetaMemoryNode BetaSource { get; set; }
        public bool HasSubnet { get; set; }

        public void RegisterDeclaration(Declaration declaration)
        {
            _declarations.Add(declaration);
        }

        public void ResetAlphaSource()
        {
            AlphaSource = null;
            HasSubnet = false;
        }
    }
}