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
    using GreenPipes;
    using RuleCompiler;
    using Rules.Facts;
    using Runtime;


    public class RuntimeBuilderContext :
        BuilderContext
    {
        readonly List<ConnectHandle> _handles;
        readonly List<IRuleParameter> _parameters;

        public RuntimeBuilderContext()
        {
            _handles = new List<ConnectHandle>();
        }

        public IAlphaNode CurrentAlphaNode { get; set; }

        public void AddHandle(ConnectHandle handle)
        {
            _handles.Add(handle);
        }

        public void AddParameter<T>(RuleParameter<T> parameter)
            where T : class
        {
            _parameters.Add(parameter);
        }

        public IndexMap CreateIndexMap(IRuleFact fact)
        {
            return IndexMap.CreateMap(_parameters, _parameters);
        }
    }
}