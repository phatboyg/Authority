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
    using System.Linq;


    class SymbolTable
    {
        public const string ScopeSeparator = ":";
        readonly string _name;

        readonly HashSet<Declaration> _symbolTable;
        int _declarationCounter = 0;

        internal SymbolTable()
        {
            _symbolTable = new HashSet<Declaration>();
        }

        internal SymbolTable(string name)
        {
            _name = name;
            _symbolTable = new HashSet<Declaration>();
        }

        internal SymbolTable(IEnumerable<Declaration> declarations)
        {
            _symbolTable = new HashSet<Declaration>(declarations);
        }

        public SymbolTable ParentScope { get; private set; }

        public string FullName
        {
            get { return ParentScope?.FullName == null ? _name : ParentScope.FullName + ScopeSeparator + _name; }
        }

        public IEnumerable<Declaration> Declarations
        {
            get { return _symbolTable; }
        }

        public IEnumerable<Declaration> VisibleDeclarations
        {
            get { return ParentScope?.VisibleDeclarations.Concat(Declarations) ?? Declarations; }
        }

        internal SymbolTable New(string name)
        {
            var childScope = new SymbolTable(name);
            childScope.ParentScope = this;
            return childScope;
        }

        public void Add(Declaration declaration)
        {
            _symbolTable.Add(declaration);
        }

        public Declaration Declare(Type type, string name)
        {
            _declarationCounter++;
            var declarationName = name ?? $"$var{_declarationCounter}$";
            var declaration = new Declaration(type, declarationName, FullName);
            Add(declaration);
            return declaration;
        }

        public Declaration Lookup(string name, Type type)
        {
            var declaration = _symbolTable.FirstOrDefault(d => d.Name == name);
            if (declaration != null)
            {
                if (declaration.Type != type)
                    throw new ArgumentException($"Declaration type mismatch. Name={name}, ExpectedType={declaration.Type}, FoundType={type}");
                return declaration;
            }
            if (ParentScope != null)
                return ParentScope.Lookup(name, type);

            throw new ArgumentException($"Declaration not found. Name={name}, Type={type}");
        }
    }
}