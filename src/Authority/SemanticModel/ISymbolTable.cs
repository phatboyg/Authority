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
namespace Authority.SemanticModel
{
    using System;
    using System.Collections.Generic;


    public interface ISymbolTable
    {
        IEnumerable<IDeclaration> Declarations { get; }
        IEnumerable<IDeclaration> VisibleDeclarations { get; }

        IDeclaration<T> Declare<T>(string name)
            where T : class;

        IDeclaration Lookup(Type type, string name);

        IDeclaration<T> Lookup<T>(string name)
            where T : class;
    }
}