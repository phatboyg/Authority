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


    public class Declaration :
        IEquatable<Declaration>
    {
        readonly string _fullName;
        readonly string _name;
        readonly Type _type;

        internal Declaration(Type type, string name, string scopeName)
        {
            _type = type;
            _name = name;
            _fullName = scopeName == null ? _name : scopeName + SymbolTable.ScopeSeparator + _name;
        }

        /// <summary>
        /// Symbol name.
        /// </summary>
        public string Name
        {
            get { return _name; }
        }

        /// <summary>
        /// Symbol type.
        /// </summary>
        public Type Type
        {
            get { return _type; }
        }

        /// <summary>
        /// Rule element that this declaration is referencing.
        /// </summary>
        public RuleElement Target { get; internal set; }

        public bool Equals(Declaration other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return string.Equals(_fullName, other._fullName);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != GetType())
                return false;
            return Equals((Declaration)obj);
        }

        public override int GetHashCode()
        {
            return _fullName.GetHashCode();
        }
    }
}