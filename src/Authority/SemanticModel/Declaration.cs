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


    public class Declaration<T> :
        IDeclaration<T>
        where T : class
    {
        public Declaration(string name, string scope)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            Name = name;
            FullName = scope != null ? $"{scope}{SymbolTable.ScopeSeparator}{name}" : name;
        }

        public bool Equals(Declaration<T> other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return string.Equals(FullName, other.FullName);
        }

        public string Name { get; }
        public string FullName { get; }

        public Type DeclarationType => typeof(T);

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != GetType())
                return false;
            return Equals((Declaration<T>)obj);
        }

        public override int GetHashCode()
        {
            return FullName.GetHashCode();
        }

        public static bool operator ==(Declaration<T> left, Declaration<T> right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Declaration<T> left, Declaration<T> right)
        {
            return !Equals(left, right);
        }
    }
}