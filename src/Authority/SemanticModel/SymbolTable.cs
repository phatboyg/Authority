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
    using System.Linq;
    using System.Threading;


    public class SymbolTable : 
        ISymbolTable
    {
        public const string ScopeSeparator = ":";

        readonly HashSet<IDeclaration> _declarations;
        readonly string _name;
        int _nextId = 0;

        internal SymbolTable()
        {
            _declarations = new HashSet<IDeclaration>();
        }

        internal SymbolTable(string name)
        {
            _name = name;
            _declarations = new HashSet<IDeclaration>();
        }

        internal SymbolTable(SymbolTable parentScope, string name)
        {
            ParentScope = parentScope;
            _name = name;

            _declarations = new HashSet<IDeclaration>();
        }

        internal SymbolTable(IEnumerable<IDeclaration> declarations)
        {
            _declarations = new HashSet<IDeclaration>(declarations);
        }

        public SymbolTable ParentScope { get; }

        public string FullName => ParentScope?.FullName == null ? _name : ParentScope.FullName + ScopeSeparator + _name;

        public IEnumerable<IDeclaration> Declarations => _declarations;

        public IEnumerable<IDeclaration> VisibleDeclarations => ParentScope?.VisibleDeclarations?.Concat(Declarations) ?? Declarations;

        public IDeclaration<T> Declare<T>(string name)
            where T : class
        {
            var id = Interlocked.Increment(ref _nextId);

            var declaration = new Declaration<T>(name ?? $"$var{id}$", FullName);

            Add(declaration);

            return declaration;
        }

        public IDeclaration Lookup(Type type, string name)
        {
            var declaration = _declarations.FirstOrDefault(d => d.Name == name);
            if (declaration != null)
            {
                if (declaration.DeclarationType != type)
                    throw new ArgumentException($"Declaration type mismatch. Name={name}, ExpectedType={declaration.DeclarationType}, FoundType={type}");

                return declaration;
            }

            if (ParentScope != null)
                return ParentScope.Lookup(type, name);

            throw new ArgumentException($"Declaration not found. Name={name}, Type={type}");
        }

        public IDeclaration<T> Lookup<T>(string name)
            where T : class
        {
            var declaration = _declarations.FirstOrDefault(d => d.Name == name);
            if (declaration != null)
            {
                if (declaration is IDeclaration<T>)
                    return (IDeclaration<T>)declaration;

                throw new ArgumentException($"Declaration type mismatch. Name={name}, ExpectedType={declaration.DeclarationType}, FoundType={typeof(T)}");
            }

            if (ParentScope != null)
                return ParentScope.Lookup<T>(name);

            throw new ArgumentException($"Declaration not found. Name={name}, Type={typeof(T)}");
        }

        public SymbolTable New(string name)
        {
            return new SymbolTable(this, name);
        }

        public void Add<T>(IDeclaration<T> declaration)
            where T : class
        {
            _declarations.Add(declaration);
        }
    }
}