﻿// Copyright 2012-2016 Chris Patterson
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
namespace Authority.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;


    static class ExpressionComparer
    {
        public static bool AreEqual(Expression x, Expression y)
        {
            return ExpressionEqual(x, y, null, null);
        }

        static bool ExpressionEqual(Expression x, Expression y, LambdaExpression rootX, LambdaExpression rootY)
        {
            if (ReferenceEquals(x, y))
                return true;
            if ((x == null) || (y == null))
                return false;
            if ((x.NodeType != y.NodeType) || (x.Type != y.Type))
                return false;

            if (x is LambdaExpression)
            {
                var lx = (LambdaExpression)x;
                var ly = (LambdaExpression)y;
                ReadOnlyCollection<ParameterExpression> paramsX = lx.Parameters;
                ReadOnlyCollection<ParameterExpression> paramsY = ly.Parameters;
                return CollectionsEqual(paramsX, paramsY, lx, ly) && ExpressionEqual(lx.Body, ly.Body, lx, ly);
            }
            if (x is MemberExpression)
            {
                var mex = (MemberExpression)x;
                var mey = (MemberExpression)y;
                return MemberExpressionsEqual(mex, mey, rootX, rootY);
            }
            if (x is BinaryExpression)
            {
                var bx = (BinaryExpression)x;
                var by = (BinaryExpression)y;
                return (bx.Method == @by.Method) && ExpressionEqual(bx.Left, by.Left, rootX, rootY) &&
                    ExpressionEqual(bx.Right, @by.Right, rootX, rootY);
            }
            if (x is UnaryExpression)
            {
                var ux = (UnaryExpression)x;
                var uy = (UnaryExpression)y;
                return (ux.Method == uy.Method) && ExpressionEqual(ux.Operand, uy.Operand, rootX, rootY);
            }
            if (x is ParameterExpression)
            {
                var px = (ParameterExpression)x;
                var py = (ParameterExpression)y;
                return rootX.Parameters.IndexOf(px) == rootY.Parameters.IndexOf(py);
            }
            if (x is MethodCallExpression)
            {
                var cx = (MethodCallExpression)x;
                var cy = (MethodCallExpression)y;
                return (cx.Method == cy.Method)
                    && ExpressionEqual(cx.Object, cy.Object, rootX, rootY)
                    && CollectionsEqual(cx.Arguments, cy.Arguments, rootX, rootY);
            }
            if (x is InvocationExpression)
            {
                var ix = (InvocationExpression)x;
                var iy = (InvocationExpression)y;
                return ExpressionEqual(ix.Expression, iy.Expression, rootX, rootY)
                    && CollectionsEqual(ix.Arguments, iy.Arguments, rootX, rootY);
            }
            if (x is NewExpression)
            {
                var nx = (NewExpression)x;
                var ny = (NewExpression)y;
                return (nx.Constructor == ny.Constructor)
                    && CollectionsEqual(nx.Arguments, ny.Arguments, rootX, rootY);
            }
            if (x is ConstantExpression)
            {
                var cx = (ConstantExpression)x;
                var cy = (ConstantExpression)y;
                return Equals(cx.Value, cy.Value);
            }

            throw new NotImplementedException(x.ToString());
        }

        static bool CollectionsEqual(IEnumerable<Expression> x, IEnumerable<Expression> y, LambdaExpression rootX, LambdaExpression rootY)
        {
            return (x.Count() == y.Count())
                && x.Select((e, i) => new {Expr = e, Index = i})
                    .Join(y.Select((e, i) => new {Expr = e, Index = i}),
                        o => o.Index, o => o.Index, (xe, ye) => new {X = xe.Expr, Y = ye.Expr})
                    .All(o => ExpressionEqual(o.X, o.Y, rootX, rootY));
        }

        static bool MemberExpressionsEqual(MemberExpression x, MemberExpression y, LambdaExpression rootX, LambdaExpression rootY)
        {
            // Special case for static field and static property
            if (x.Expression == null)
                return Equals(x.Member, y.Member);

            if (x.Expression.NodeType != y.Expression.NodeType)
                return false;
            switch (x.Expression.NodeType)
            {
                case ExpressionType.Constant:
                    var constx = GetValueOfConstantExpression(x);
                    var consty = GetValueOfConstantExpression(y);
                    return Equals(constx, consty);
                case ExpressionType.Parameter:
                case ExpressionType.MemberAccess:
                    return Equals(x.Member, y.Member) && ExpressionEqual(x.Expression, y.Expression, rootX, rootY);
                case ExpressionType.New:
                case ExpressionType.Call:
                    return ExpressionEqual(x.Expression, y.Expression, rootX, rootY);
                default:
                    throw new NotImplementedException(x.ToString());
            }
        }

        static object GetValueOfConstantExpression(MemberExpression mex)
        {
            var o = ((ConstantExpression)mex.Expression).Value;
            return mex.Member is FieldInfo
                ? ((FieldInfo)mex.Member).GetValue(o)
                : ((PropertyInfo)mex.Member).GetValue(o, null);
        }
    }
}