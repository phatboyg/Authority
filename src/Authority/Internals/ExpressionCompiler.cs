﻿namespace Authority.Internals
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Reflection.Emit;


    /// <summary>
    ///     Compiles expression to delegate by emitting the IL directly.
    ///     The emitter is ~20 times faster than Expression.Compile.
    /// </summary>
    public static class ExpressionCompiler
    {
        /// <summary>First tries to compile fast and if failed (null result), then falls back to Expression.Compile.</summary>
        /// <typeparam name="T">Type of compiled delegate return result.</typeparam>
        /// <param name="lambdaExpr">Expr to compile.</param>
        /// <returns>Compiled delegate.</returns>
        public static Func<T> Compile<T>(Expression<Func<T>> lambdaExpr)
        {
            return TryCompile<Func<T>>(lambdaExpr.Body, lambdaExpr.Parameters, Arr.Empty<Type>(), typeof(T))
                   ?? lambdaExpr.Compile();
        }

        /// <summary>Compiles lambda expression to <typeparamref name="TDelegate" />.</summary>
        /// <typeparam name="TDelegate">The compatible delegate type, otherwise case will throw.</typeparam>
        /// <param name="lambdaExpr">Lambda expression to compile.</param>
        /// <returns>Compiled delegate.</returns>
        public static TDelegate Compile<TDelegate>(LambdaExpression lambdaExpr)
            where TDelegate : class
        {
            return TryCompile<TDelegate>(lambdaExpr) ?? (TDelegate) (object) lambdaExpr.Compile();
        }

        /// <summary>Tries to compile lambda expression to <typeparamref name="TDelegate" />.</summary>
        /// <typeparam name="TDelegate">The compatible delegate type, otherwise case will throw.</typeparam>
        /// <param name="lambdaExpr">Lambda expression to compile.</param>
        /// <returns>Compiled delegate.</returns>
        public static TDelegate TryCompile<TDelegate>(LambdaExpression lambdaExpr)
            where TDelegate : class
        {
            var paramExprs = lambdaExpr.Parameters;
            var paramTypes = GetParamExprTypes(paramExprs);
            var expr = lambdaExpr.Body;
            return TryCompile<TDelegate>(expr, paramExprs, paramTypes, expr.Type);
        }

        /// <summary>Performant method to get parameter types from parameter expressions.</summary>
        public static Type[] GetParamExprTypes(IList<ParameterExpression> paramExprs)
        {
            var paramsCount = paramExprs.Count;
            if (paramsCount == 0)
                return Arr.Empty<Type>();

            if (paramsCount == 1)
                return new[] {paramExprs[0].Type};

            var paramTypes = new Type[paramsCount];
            for (var i = 0; i < paramTypes.Length; i++)
                paramTypes[i] = paramExprs[i].Type;
            return paramTypes;
        }

        /// <summary>
        ///     Compiles expression to delegate by emitting the IL.
        ///     If sub-expressions are not supported by emitter, then the method returns null.
        ///     The usage should be calling the method, if result is null then calling the Expression.Compile.
        /// </summary>
        /// <param name="bodyExpr">Lambda body.</param>
        /// <param name="paramExprs">Lambda parameter expressions.</param>
        /// <param name="paramTypes">The types of parameters.</param>
        /// <param name="returnType">The return type.</param>
        /// <returns>Result delegate or null, if unable to compile.</returns>
        public static TDelegate TryCompile<TDelegate>(
            Expression bodyExpr,
            IList<ParameterExpression> paramExprs,
            Type[] paramTypes,
            Type returnType)
            where TDelegate : class
        {
            ClosureInfo ignored = null;
            return (TDelegate) TryCompile(ref ignored,
                typeof(TDelegate), paramTypes, returnType,
                bodyExpr, bodyExpr.NodeType, bodyExpr.Type, paramExprs);
        }

        /// <summary>Tries to compile lambda expression to <typeparamref name="TDelegate" />.</summary>
        /// <typeparam name="TDelegate">The compatible delegate type, otherwise case will throw.</typeparam>
        /// <param name="lambdaExpr">Lambda expression to compile.</param>
        /// <returns>Compiled delegate.</returns>
        public static TDelegate TryCompile<TDelegate>(LambdaExpressionInfo lambdaExpr)
            where TDelegate : class
        {
            var paramExprs = lambdaExpr.Parameters;
            var paramTypes = GetParamExprTypes(paramExprs);
            var expr = lambdaExpr.Body;
            return TryCompile<TDelegate>(expr, paramExprs, paramTypes, expr.Type);
        }

        /// <summary>
        ///     Compiles expression to delegate by emitting the IL.
        ///     If sub-expressions are not supported by emitter, then the method returns null.
        ///     The usage should be calling the method, if result is null then calling the Expression.Compile.
        /// </summary>
        /// <param name="bodyExpr">Lambda body.</param>
        /// <param name="paramExprs">Lambda parameter expressions.</param>
        /// <param name="paramTypes">The types of parameters.</param>
        /// <param name="returnType">The return type.</param>
        /// <returns>Result delegate or null, if unable to compile.</returns>
        public static TDelegate TryCompile<TDelegate>(
            ExpressionInfo bodyExpr,
            IList<ParameterExpression> paramExprs,
            Type[] paramTypes,
            Type returnType)
            where TDelegate : class
        {
            ClosureInfo ignored = null;
            return (TDelegate) TryCompile(ref ignored,
                typeof(TDelegate), paramTypes, returnType,
                bodyExpr, bodyExpr.NodeType, bodyExpr.Type, paramExprs);
        }

        static object TryCompile(ref ClosureInfo closureInfo,
            Type delegateType, Type[] paramTypes, Type returnType,
            object exprObj, ExpressionType exprNodeType, Type exprType,
            IList<ParameterExpression> paramExprs,
            bool isNestedLambda = false)
        {
            if (!TryCollectBoundConstants(ref closureInfo, exprObj, exprNodeType, exprType, paramExprs))
                return null;

            if (closureInfo == null)
                return TryCompileStaticDelegate(delegateType, paramTypes, returnType, exprObj, exprNodeType, exprType, paramExprs);

            var closureObject = closureInfo.ConstructClosure(isNestedLambda);
            var closureAndParamTypes = GetClosureAndParamTypes(paramTypes, closureInfo.ClosureType);

            var methodWithClosure = new DynamicMethod(string.Empty, returnType, closureAndParamTypes,
                typeof(ExpressionCompiler), true);

            if (!TryEmit(methodWithClosure, exprObj, exprNodeType, exprType, paramExprs, closureInfo))
                return null;

            // todo: Use sugar TryCompileStaticDelegate?
            if (isNestedLambda) // include closure as the first parameter, BUT don't bound to it. It will be bound later in EmitNestedLambda.
                return methodWithClosure.CreateDelegate(GetFuncOrActionType(closureAndParamTypes, returnType));

            return methodWithClosure.CreateDelegate(delegateType, closureObject);
        }

        static object TryCompileStaticDelegate(Type delegateType, Type[] paramTypes, Type returnType, object exprObj,
            ExpressionType exprNodeType, Type exprType, IList<ParameterExpression> paramExprs)
        {
            var method = new DynamicMethod(string.Empty, returnType, paramTypes,
                typeof(ExpressionCompiler), true);

            if (!TryEmit(method, exprObj, exprNodeType, exprType, paramExprs, null))
                return null;

            return method.CreateDelegate(delegateType);
        }

        static bool TryEmit(DynamicMethod method,
            object exprObj, ExpressionType exprNodeType, Type exprType,
            IList<ParameterExpression> paramExprs, ClosureInfo closureInfo)
        {
            var il = method.GetILGenerator();
            if (!EmittingVisitor.TryEmit(exprObj, exprNodeType, exprType, paramExprs, il, closureInfo))
                return false;

            il.Emit(OpCodes.Ret); // emits return from generated method
            return true;
        }

        static Type[] GetClosureAndParamTypes(Type[] paramTypes, Type closureType)
        {
            var paramCount = paramTypes.Length;
            if (paramCount == 0)
                return new[] {closureType};

            if (paramCount == 1)
                return new[] {closureType, paramTypes[0]};

            var closureAndParamTypes = new Type[paramCount + 1];
            closureAndParamTypes[0] = closureType;
            Array.Copy(paramTypes, 0, closureAndParamTypes, 1, paramCount);
            return closureAndParamTypes;
        }


        struct ConstantInfo
        {
            public readonly object ConstantExpr;
            public readonly Type Type;
            public readonly object Value;

            public ConstantInfo(object constantExpr, object value, Type type)
            {
                ConstantExpr = constantExpr;
                Value = value;
                Type = type;
            }
        }


        sealed class ClosureInfo
        {
            // Closed values used by expression and by its nested lambdas
            public ConstantInfo[] Constants = Arr.Empty<ConstantInfo>();

            // All nested lambdas recursively nested in expression
            public NestedLambdaInfo[] NestedLambdas = Arr.Empty<NestedLambdaInfo>();

            // Parameters not passed through lambda parameter list But used inside lambda body.
            // The top expression should not! contain non passed parameters.
            public ParameterExpression[] NonPassedParameters = Arr.Empty<ParameterExpression>();

            // Field infos are needed to load field of closure object on stack in emitter
            // It is also an indicator that we use typed Closure object and not an array
            public FieldInfo[] Fields { get; private set; }

            // Type of constructed closure, is known after ConstructClosure call
            public Type ClosureType { get; private set; }

            // Known after ConstructClosure call
            public int ClosedItemCount { get; private set; }

            public void AddConstant(object expr, object value, Type type)
            {
                if (Constants.Length == 0 || Constants.IndexOf(it => it.ConstantExpr == expr) == -1)
                    Constants = Constants.Append(new ConstantInfo(expr, value, type));
            }

            public void AddConstant(ConstantInfo info)
            {
                if (Constants.Length == 0 || Constants.IndexOf(it => it.ConstantExpr == info.ConstantExpr) == -1)
                    Constants = Constants.Append(info);
            }

            public void AddNonPassedParam(ParameterExpression expr)
            {
                if (NonPassedParameters.Length == 0 || NonPassedParameters.IndexOf(it => it == expr) == -1)
                    NonPassedParameters = NonPassedParameters.Append(expr);
            }

            public void AddNestedLambda(object lambdaExpr, object lambda, ClosureInfo closureInfo, bool isAction)
            {
                if (NestedLambdas.Length == 0 || NestedLambdas.IndexOf(it => it.LambdaExpr == lambdaExpr) == -1)
                    NestedLambdas = NestedLambdas.Append(new NestedLambdaInfo(closureInfo, lambdaExpr, lambda, isAction));
            }

            public void AddNestedLambda(NestedLambdaInfo info)
            {
                if (NestedLambdas.Length == 0 || NestedLambdas.IndexOf(it => it.LambdaExpr == info.LambdaExpr) == -1)
                    NestedLambdas = NestedLambdas.Append(info);
            }

            public object ConstructClosure(bool closureTypeOnly)
            {
                var constants = Constants;
                var nonPassedParams = NonPassedParameters;
                var nestedLambdas = NestedLambdas;

                var constPlusParamCount = constants.Length + nonPassedParams.Length;
                var totalItemCount = constPlusParamCount + nestedLambdas.Length;

                ClosedItemCount = totalItemCount;

                var closureCreateMethods = Closure.CreateMethods;

                // Construct the array based closure when number of values is bigger than
                // number of fields in biggest supported Closure class.
                if (totalItemCount > closureCreateMethods.Length)
                {
                    ClosureType = typeof(ArrayClosure);

                    if (closureTypeOnly)
                        return null;

                    var items = new object[totalItemCount];
                    if (constants.Length != 0)
                        for (var i = 0; i < constants.Length; i++)
                            items[i] = constants[i].Value;

                    // skip non passed parameters as it is only for nested lambdas

                    if (nestedLambdas.Length != 0)
                        for (var i = 0; i < nestedLambdas.Length; i++)
                            items[constPlusParamCount + i] = nestedLambdas[i].Lambda;

                    return new ArrayClosure(items);
                }

                // Construct the Closure Type and optionally Closure object with closed values stored as fields:
                object[] fieldValues = null;
                var fieldTypes = new Type[totalItemCount];
                if (closureTypeOnly)
                {
                    if (constants.Length != 0)
                        for (var i = 0; i < constants.Length; i++)
                            fieldTypes[i] = constants[i].Type;

                    if (nonPassedParams.Length != 0)
                        for (var i = 0; i < nonPassedParams.Length; i++)
                            fieldTypes[constants.Length + i] = nonPassedParams[i].Type;

                    if (nestedLambdas.Length != 0)
                        for (var i = 0; i < nestedLambdas.Length; i++)
                            fieldTypes[constPlusParamCount + i] = nestedLambdas[i].Lambda.GetType();
                }
                else
                {
                    fieldValues = new object[totalItemCount];

                    if (constants.Length != 0)
                        for (var i = 0; i < constants.Length; i++)
                        {
                            var constantExpr = constants[i];
                            fieldTypes[i] = constantExpr.Type;
                            fieldValues[i] = constantExpr.Value;
                        }

                    if (nonPassedParams.Length != 0)
                        for (var i = 0; i < nonPassedParams.Length; i++)
                            fieldTypes[constants.Length + i] = nonPassedParams[i].Type;

                    if (nestedLambdas.Length != 0)
                        for (var i = 0; i < nestedLambdas.Length; i++)
                        {
                            var lambda = nestedLambdas[i].Lambda;
                            fieldValues[constPlusParamCount + i] = lambda;
                            fieldTypes[constPlusParamCount + i] = lambda.GetType();
                        }
                }

                var createClosureMethod = closureCreateMethods[totalItemCount - 1];
                var createClosure = createClosureMethod.MakeGenericMethod(fieldTypes);
                ClosureType = createClosure.ReturnType;

                var fields = ClosureType.GetTypeInfo().DeclaredFields;
                Fields = fields as FieldInfo[] ?? fields.ToArray();

                if (fieldValues == null)
                    return null;
                return createClosure.Invoke(null, fieldValues);
            }
        }


        /// <summary>
        ///     Supports emitting of selected expressions, e.g. lambdaExpr are not supported yet.
        ///     When emitter find not supported expression it will return false from <see cref="TryEmit" />, so I could fallback
        ///     to normal and slow Expression.Compile.
        /// </summary>
        static class EmittingVisitor
        {
            static readonly MethodInfo _getTypeFromHandleMethod = typeof(Type).GetTypeInfo()
                .DeclaredMethods.First(m => m.Name == "GetTypeFromHandle");

            public static bool TryEmit(object exprObj, ExpressionType exprNodeType, Type exprType,
                IList<ParameterExpression> paramExprs, ILGenerator il, ClosureInfo closure)
            {
                switch (exprNodeType)
                {
                    case ExpressionType.Parameter:
                        var paramExprInfo = exprObj as ParameterExpressionInfo;
                        return EmitParameter(paramExprInfo != null ? paramExprInfo.ParamExpr : (ParameterExpression) exprObj,
                            paramExprs, il, closure);
                    case ExpressionType.Convert: return EmitConvert(exprObj, exprType, paramExprs, il, closure);
                    case ExpressionType.ArrayIndex: return EmitArrayIndex(exprObj, paramExprs, il, closure);
                    case ExpressionType.Constant: return EmitConstant(exprObj, exprType, il, closure);
                    case ExpressionType.Call: return EmitMethodCall(exprObj, paramExprs, il, closure);
                    case ExpressionType.MemberAccess: return EmitMemberAccess(exprObj, paramExprs, il, closure);
                    case ExpressionType.New: return EmitNew(exprObj, paramExprs, il, closure);
                    case ExpressionType.NewArrayInit: return EmitNewArray(exprObj, paramExprs, il, closure);
                    case ExpressionType.MemberInit: return EmitMemberInit(exprObj, exprType, paramExprs, il, closure);
                    case ExpressionType.Lambda: return EmitNestedLambda(exprObj, paramExprs, il, closure);

                    case ExpressionType.Invoke: return EmitInvokeLambda((InvocationExpression) exprObj, paramExprs, il, closure);

                    case ExpressionType.GreaterThan:
                    case ExpressionType.GreaterThanOrEqual:
                    case ExpressionType.LessThan:
                    case ExpressionType.LessThanOrEqual:
                    case ExpressionType.Equal:
                    case ExpressionType.NotEqual: return EmitComparison((BinaryExpression) exprObj, paramExprs, il, closure);

                    case ExpressionType.AndAlso:
                    case ExpressionType.OrElse: return EmitLogicalOperator((BinaryExpression) exprObj, paramExprs, il, closure);

                    case ExpressionType.Conditional: return EmitTernararyOperator((ConditionalExpression) exprObj, paramExprs, il, closure);

                    //case ExpressionType.Coalesce:
                    default: return false;
                }
            }

            static bool EmitParameter(ParameterExpression p, IList<ParameterExpression> ps, ILGenerator il, ClosureInfo closure)
            {
                var paramIndex = ps.IndexOf(p);

                // if parameter is passed, then just load it on stack
                if (paramIndex != -1)
                {
                    if (closure != null)
                        paramIndex += 1; // shift parameter indices by one, because the first one will be closure
                    LoadParamArg(il, paramIndex);
                    return true;
                }

                // if parameter isn't passed, then it is passed into some outer lambda,
                // so it should be loaded from closure. Then the closure is null will be an invalid case.
                if (closure == null)
                    return false;

                var nonPassedParamIndex = closure.NonPassedParameters.IndexOf(it => it == p);
                if (nonPassedParamIndex == -1)
                    return false; // what??? no chance

                var closureItemIndex = closure.Constants.Length + nonPassedParamIndex;

                il.Emit(OpCodes.Ldarg_0); // closure is always a first argument
                if (closure.Fields != null)
                    il.Emit(OpCodes.Ldfld, closure.Fields[closureItemIndex]);
                else
                    LoadArrayClosureItem(il, closureItemIndex, p.Type);

                return true;
            }

            static void LoadParamArg(ILGenerator il, int paramIndex)
            {
                switch (paramIndex)
                {
                    case 0:
                        il.Emit(OpCodes.Ldarg_0);
                        break;
                    case 1:
                        il.Emit(OpCodes.Ldarg_1);
                        break;
                    case 2:
                        il.Emit(OpCodes.Ldarg_2);
                        break;
                    case 3:
                        il.Emit(OpCodes.Ldarg_3);
                        break;
                    default:
                        if (paramIndex <= byte.MaxValue)
                            il.Emit(OpCodes.Ldarg_S, (byte) paramIndex);
                        else
                            il.Emit(OpCodes.Ldarg, paramIndex);
                        break;
                }
            }

            static bool EmitBinary(object exprObj, IList<ParameterExpression> ps, ILGenerator il, ClosureInfo closure)
            {
                var exprInfo = exprObj as BinaryExpressionInfo;
                if (exprInfo != null)
                {
                    var leftInfo = exprInfo.Left;
                    var rightInfo = exprInfo.Right;
                    return TryEmit(leftInfo, leftInfo.NodeType, leftInfo.Type, ps, il, closure)
                           && TryEmit(rightInfo, rightInfo.NodeType, rightInfo.Type, ps, il, closure);
                }

                var expr = (BinaryExpression) exprObj;
                var leftExpr = expr.Left;
                var rightExpr = expr.Right;
                return TryEmit(leftExpr, leftExpr.NodeType, leftExpr.Type, ps, il, closure)
                       && TryEmit(rightExpr, rightExpr.NodeType, rightExpr.Type, ps, il, closure);
            }

            static bool EmitMany(IList<Expression> exprs, IList<ParameterExpression> ps, ILGenerator il, ClosureInfo closure)
            {
                for (int i = 0, n = exprs.Count; i < n; i++)
                {
                    var expr = exprs[i];
                    if (!TryEmit(expr, expr.NodeType, expr.Type, ps, il, closure))
                        return false;
                }
                return true;
            }

            static bool EmitMany(IList<ExpressionInfo> infos, IList<ParameterExpression> ps, ILGenerator il, ClosureInfo closure)
            {
                for (int i = 0, n = infos.Count; i < n; i++)
                {
                    var info = infos[i];
                    if (!TryEmit(info, info.NodeType, info.Type, ps, il, closure))
                        return false;
                }
                return true;
            }

            static bool EmitConvert(object exprObj, Type targetType, IList<ParameterExpression> ps, ILGenerator il, ClosureInfo closure)
            {
                var exprInfo = exprObj as UnaryExpressionInfo;
                Type sourceType;
                if (exprInfo != null)
                {
                    var opInfo = exprInfo.Operand;
                    if (!TryEmit(opInfo, opInfo.NodeType, opInfo.Type, ps, il, closure))
                        return false;
                    sourceType = opInfo.Type;
                }
                else
                {
                    var expr = (UnaryExpression) exprObj;
                    var opExpr = expr.Operand;
                    if (!TryEmit(opExpr, opExpr.NodeType, opExpr.Type, ps, il, closure))
                        return false;
                    sourceType = opExpr.Type;
                }

                if (targetType == sourceType)
                    return true; // do nothing, no conversion is needed

                if (targetType == typeof(object))
                {
                    if (sourceType.GetTypeInfo().IsValueType)
                        il.Emit(OpCodes.Box, sourceType); // for valuy type to object, just box a value
                    return true; // for reference type we don't need to convert
                }

                // Just unbox type object to the target value type
                if (targetType.GetTypeInfo().IsValueType && sourceType == typeof(object))
                {
                    il.Emit(OpCodes.Unbox_Any, targetType);
                    return true;
                }

                if (targetType == typeof(int))
                    il.Emit(OpCodes.Conv_I4);
                else if (targetType == typeof(float))
                    il.Emit(OpCodes.Conv_R4);
                else if (targetType == typeof(uint))
                    il.Emit(OpCodes.Conv_U4);
                else if (targetType == typeof(sbyte))
                    il.Emit(OpCodes.Conv_I1);
                else if (targetType == typeof(byte))
                    il.Emit(OpCodes.Conv_U1);
                else if (targetType == typeof(short))
                    il.Emit(OpCodes.Conv_I2);
                else if (targetType == typeof(ushort))
                    il.Emit(OpCodes.Conv_U2);
                else if (targetType == typeof(long))
                    il.Emit(OpCodes.Conv_I8);
                else if (targetType == typeof(ulong))
                    il.Emit(OpCodes.Conv_U8);
                else if (targetType == typeof(double))
                    il.Emit(OpCodes.Conv_R8);
                else
                    il.Emit(OpCodes.Castclass, targetType);

                return true;
            }

            static bool EmitConstant(object exprObj, Type exprType, ILGenerator il, ClosureInfo closure)
            {
                var constExprInfo = exprObj as ConstantExpressionInfo;
                var constantValue = constExprInfo != null ? constExprInfo.Value : ((ConstantExpression) exprObj).Value;
                if (constantValue == null)
                {
                    il.Emit(OpCodes.Ldnull);
                    return true;
                }

                var constantActualType = constantValue.GetType();
                if (constantActualType.GetTypeInfo().IsEnum)
                    constantActualType = Enum.GetUnderlyingType(constantActualType);

                if (constantActualType == typeof(int))
                {
                    EmitLoadConstantInt(il, (int) constantValue);
                }
                else if (constantActualType == typeof(char))
                {
                    EmitLoadConstantInt(il, (char) constantValue);
                }
                else if (constantActualType == typeof(short))
                {
                    EmitLoadConstantInt(il, (short) constantValue);
                }
                else if (constantActualType == typeof(byte))
                {
                    EmitLoadConstantInt(il, (byte) constantValue);
                }
                else if (constantActualType == typeof(ushort))
                {
                    EmitLoadConstantInt(il, (ushort) constantValue);
                }
                else if (constantActualType == typeof(sbyte))
                {
                    EmitLoadConstantInt(il, (sbyte) constantValue);
                }
                else if (constantActualType == typeof(uint))
                {
                    unchecked
                    {
                        EmitLoadConstantInt(il, (int) (uint) constantValue);
                    }
                }
                else if (constantActualType == typeof(long))
                {
                    il.Emit(OpCodes.Ldc_I8, (long) constantValue);
                }
                else if (constantActualType == typeof(ulong))
                {
                    unchecked
                    {
                        il.Emit(OpCodes.Ldc_I8, (long) (ulong) constantValue);
                    }
                }
                else if (constantActualType == typeof(float))
                {
                    il.Emit(OpCodes.Ldc_R8, (float) constantValue);
                }
                else if (constantActualType == typeof(double))
                {
                    il.Emit(OpCodes.Ldc_R8, (double) constantValue);
                }
                else if (constantActualType == typeof(bool))
                {
                    il.Emit((bool) constantValue ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
                }
                else if (constantValue is string)
                {
                    il.Emit(OpCodes.Ldstr, (string) constantValue);
                }
                else if (constantValue is Type)
                {
                    il.Emit(OpCodes.Ldtoken, (Type) constantValue);
                    il.Emit(OpCodes.Call, _getTypeFromHandleMethod);
                }
                else if (closure != null)
                {
                    var constantIndex = closure.Constants.IndexOf(it => it.ConstantExpr == exprObj);
                    if (constantIndex == -1)
                        return false;

                    il.Emit(OpCodes.Ldarg_0); // closure is always a first argument
                    if (closure.Fields != null)
                        il.Emit(OpCodes.Ldfld, closure.Fields[constantIndex]);
                    else
                        LoadArrayClosureItem(il, constantIndex, exprType);
                }
                else
                {
                    return false;
                }

                // boxing the value type, otherwise we can get a strange result when 0 is treated as Null.
                if (exprType == typeof(object) && constantActualType.GetTypeInfo().IsValueType)
                    il.Emit(OpCodes.Box, constantValue.GetType()); // using normal type for Enum instead of underlying type

                return true;
            }

            // The @skipCastOrUnboxing option is for use-case when we loading and immediately storing the item,
            // it may happen when copying from one object array to another.
            static void LoadArrayClosureItem(ILGenerator il, int closedItemIndex, Type closedItemType)
            {
                // load array field
                il.Emit(OpCodes.Ldfld, ArrayClosure.ArrayField);

                // load array item index
                EmitLoadConstantInt(il, closedItemIndex);

                // load item from index
                il.Emit(OpCodes.Ldelem_Ref);

                // Cast or unbox the object item depending if it is a class or value type
                if (closedItemType.GetTypeInfo().IsValueType)
                    il.Emit(OpCodes.Unbox_Any, closedItemType);
                else
                    il.Emit(OpCodes.Castclass, closedItemType);
            }

            static bool EmitNew(object exprObj, IList<ParameterExpression> ps, ILGenerator il, ClosureInfo closure)
            {
                var exprInfo = exprObj as NewExpressionInfo;
                if (exprInfo != null)
                {
                    if (!EmitMany(exprInfo.Arguments, ps, il, closure))
                        return false;
                    il.Emit(OpCodes.Newobj, exprInfo.Constructor);
                }
                else
                {
                    var expr = (NewExpression) exprObj;
                    if (!EmitMany(expr.Arguments, ps, il, closure))
                        return false;
                    il.Emit(OpCodes.Newobj, expr.Constructor);
                }
                return true;
            }

            static bool EmitNewArray(object exprObj, IList<ParameterExpression> ps, ILGenerator il, ClosureInfo closure)
            {
                var exprInfo = exprObj as NewArrayExpressionInfo;
                if (exprInfo != null)
                    return EmitNewArrayInfo(exprInfo, ps, il, closure);

                var expr = (NewArrayExpression) exprObj;
                var elems = expr.Expressions;
                var arrType = expr.Type;
                var elemType = arrType.GetElementType();
                if (elemType == null)
                    return false;

                var isElemOfValueType = elemType.GetTypeInfo().IsValueType;

                var arrVar = il.DeclareLocal(arrType);

                EmitLoadConstantInt(il, elems.Count);
                il.Emit(OpCodes.Newarr, elemType);
                il.Emit(OpCodes.Stloc, arrVar);

                for (int i = 0, n = elems.Count; i < n; i++)
                {
                    il.Emit(OpCodes.Ldloc, arrVar);
                    EmitLoadConstantInt(il, i);

                    // loading element address for later copying of value into it.
                    if (isElemOfValueType)
                        il.Emit(OpCodes.Ldelema, elemType);

                    var elemExpr = elems[i];
                    if (!TryEmit(elemExpr, elemExpr.NodeType, elemExpr.Type, ps, il, closure))
                        return false;

                    if (isElemOfValueType)
                        il.Emit(OpCodes.Stobj, elemType); // store element of value type by array element address
                    else
                        il.Emit(OpCodes.Stelem_Ref);
                }

                il.Emit(OpCodes.Ldloc, arrVar);
                return true;
            }

            static bool EmitNewArrayInfo(NewArrayExpressionInfo expr, IList<ParameterExpression> ps, ILGenerator il, ClosureInfo closure)
            {
                var elems = expr.Arguments;
                var arrType = expr.Type;
                var elemType = arrType.GetElementType();
                if (elemType == null)
                    return false;

                var isElemOfValueType = elemType.GetTypeInfo().IsValueType;

                var arrVar = il.DeclareLocal(arrType);

                EmitLoadConstantInt(il, elems.Length);
                il.Emit(OpCodes.Newarr, elemType);
                il.Emit(OpCodes.Stloc, arrVar);

                for (var i = 0; i < elems.Length; i++)
                {
                    il.Emit(OpCodes.Ldloc, arrVar);
                    EmitLoadConstantInt(il, i);

                    // loading element address for later copying of value into it.
                    if (isElemOfValueType)
                        il.Emit(OpCodes.Ldelema, elemType);

                    var elemInfo = elems[i];
                    if (!TryEmit(elemInfo, elemInfo.NodeType, elemInfo.Type, ps, il, closure))
                        return false;

                    if (isElemOfValueType)
                        il.Emit(OpCodes.Stobj, elemType); // store element of value type by array element address
                    else
                        il.Emit(OpCodes.Stelem_Ref);
                }

                il.Emit(OpCodes.Ldloc, arrVar);
                return true;
            }

            static bool EmitArrayIndex(object exprObj, IList<ParameterExpression> ps, ILGenerator il, ClosureInfo closure)
            {
                if (!EmitBinary(exprObj, ps, il, closure))
                    return false;
                il.Emit(OpCodes.Ldelem_Ref);
                return true;
            }

            static bool EmitMemberInit(object exprObj, Type memberType, IList<ParameterExpression> ps, ILGenerator il, ClosureInfo closure)
            {
                var exprInfo = exprObj as MemberInitExpressionInfo;
                if (exprInfo != null)
                    return EmitMemberInitInfo(exprInfo, memberType, ps, il, closure);

                var expr = (MemberInitExpression) exprObj;
                if (!EmitNew(expr.NewExpression, ps, il, closure))
                    return false;

                var obj = il.DeclareLocal(memberType);
                il.Emit(OpCodes.Stloc, obj);

                var bindings = expr.Bindings;
                for (var i = 0; i < bindings.Count; i++)
                {
                    var binding = bindings[i];
                    if (binding.BindingType != MemberBindingType.Assignment)
                        return false;
                    il.Emit(OpCodes.Ldloc, obj);

                    var bindingExpr = ((MemberAssignment) binding).Expression;
                    if (!TryEmit(bindingExpr, bindingExpr.NodeType, bindingExpr.Type, ps, il, closure))
                        return false;

                    var prop = binding.Member as PropertyInfo;
                    if (prop != null)
                    {
                        var propSetMethodName = "set_" + prop.Name;
                        var setMethod = prop.DeclaringType.GetTypeInfo()
                            .DeclaredMethods.FirstOrDefault(m => m.Name == propSetMethodName);
                        if (setMethod == null)
                            return false;
                        EmitMethodCall(il, setMethod);
                    }
                    else
                    {
                        var field = binding.Member as FieldInfo;
                        if (field == null)
                            return false;
                        il.Emit(OpCodes.Stfld, field);
                    }
                }

                il.Emit(OpCodes.Ldloc, obj);
                return true;
            }

            static bool EmitMemberInitInfo(MemberInitExpressionInfo exprInfo, Type memberType, IList<ParameterExpression> ps, ILGenerator il, ClosureInfo closure)
            {
                if (!EmitNew(exprInfo.NewExpressionInfo, ps, il, closure))
                    return false;

                var obj = il.DeclareLocal(memberType);
                il.Emit(OpCodes.Stloc, obj);

                var bindings = exprInfo.Bindings;
                for (var i = 0; i < bindings.Length; i++)
                {
                    var binding = bindings[i];

                    il.Emit(OpCodes.Ldloc, obj);

                    var bindingExpr = binding.Expression;
                    if (!TryEmit(bindingExpr, bindingExpr.NodeType, bindingExpr.Type, ps, il, closure))
                        return false;

                    var prop = binding.Member as PropertyInfo;
                    if (prop != null)
                    {
                        var propSetMethodName = "set_" + prop.Name;
                        var setMethod = prop.DeclaringType.GetTypeInfo()
                            .DeclaredMethods.FirstOrDefault(m => m.Name == propSetMethodName);
                        if (setMethod == null)
                            return false;
                        EmitMethodCall(il, setMethod);
                    }
                    else
                    {
                        var field = binding.Member as FieldInfo;
                        if (field == null)
                            return false;
                        il.Emit(OpCodes.Stfld, field);
                    }
                }

                il.Emit(OpCodes.Ldloc, obj);
                return true;
            }

            static bool EmitMethodCall(object exprObj,
                IList<ParameterExpression> ps, ILGenerator il, ClosureInfo closure)
            {
                var exprInfo = exprObj as MethodCallExpressionInfo;
                if (exprInfo != null)
                {
                    var objInfo = exprInfo.Object;
                    if (objInfo != null)
                    {
                        if (!TryEmit(objInfo, objInfo.NodeType, objInfo.Type, ps, il, closure))
                            return false;
                        IfValueTypeStoreAndLoadValueAddress(il, objInfo.Type);
                    }

                    if (exprInfo.Arguments.Length != 0 && !EmitMany(exprInfo.Arguments, ps, il, closure))
                        return false;
                }
                else
                {
                    var expr = (MethodCallExpression) exprObj;
                    var objExpr = expr.Object;
                    if (objExpr != null)
                    {
                        if (!TryEmit(objExpr, objExpr.NodeType, objExpr.Type, ps, il, closure))
                            return false;
                        IfValueTypeStoreAndLoadValueAddress(il, objExpr.Type);
                    }

                    if (expr.Arguments.Count != 0 && !EmitMany(expr.Arguments, ps, il, closure))
                        return false;
                }

                var method = exprInfo != null ? exprInfo.Method : ((MethodCallExpression) exprObj).Method;
                EmitMethodCall(il, method);
                return true;
            }

            static void IfValueTypeStoreAndLoadValueAddress(ILGenerator il, Type ownerType)
            {
                if (ownerType.GetTypeInfo().IsValueType)
                {
                    var valueVar = il.DeclareLocal(ownerType);
                    il.Emit(OpCodes.Stloc, valueVar);
                    il.Emit(OpCodes.Ldloca, valueVar);
                }
            }

            static bool EmitMemberAccess(object exprObj, IList<ParameterExpression> ps, ILGenerator il, ClosureInfo closure)
            {
                var exprInfo = exprObj as MemberExpressionInfo;
                if (exprInfo != null)
                {
                    var instInfo = exprInfo.Expression;
                    if (instInfo != null)
                    {
                        if (!TryEmit(instInfo, instInfo.NodeType, instInfo.Type, ps, il, closure))
                            return false;
                        IfValueTypeStoreAndLoadValueAddress(il, instInfo.Type);
                    }
                }
                else
                {
                    var instExpr = ((MemberExpression) exprObj).Expression;
                    if (instExpr != null)
                    {
                        if (!TryEmit(instExpr, instExpr.NodeType, instExpr.Type, ps, il, closure))
                            return false;
                        IfValueTypeStoreAndLoadValueAddress(il, instExpr.Type);
                    }
                }

                var member = exprInfo != null ? exprInfo.Member : ((MemberExpression) exprObj).Member;
                var field = member as FieldInfo;
                if (field != null)
                {
                    il.Emit(field.IsStatic ? OpCodes.Ldsfld : OpCodes.Ldfld, field);
                    return true;
                }

                var prop = member as PropertyInfo;
                if (prop != null)
                {
                    var propGetMethodName = "get_" + prop.Name;
                    var getMethod = prop.DeclaringType.GetTypeInfo()
                        .DeclaredMethods.FirstOrDefault(m => m.Name == propGetMethodName);
                    if (getMethod == null)
                        return false;
                    EmitMethodCall(il, getMethod);
                }
                return true;
            }

            static bool EmitNestedLambda(object lambdaExpr,
                IList<ParameterExpression> paramExprs, ILGenerator il, ClosureInfo closure)
            {
                // First, find in closed compiled lambdas the one corresponding to the current lambda expression.
                // Situation with not found lambda is not possible/exceptional,
                // it means that we somehow skipped the lambda expression while collecting closure info.
                var outerNestedLambdas = closure.NestedLambdas;
                var outerNestedLambdaIndex = outerNestedLambdas.IndexOf(it => it.LambdaExpr == lambdaExpr);
                if (outerNestedLambdaIndex == -1)
                    return false;

                var nestedLambdaInfo = outerNestedLambdas[outerNestedLambdaIndex];
                var nestedLambda = nestedLambdaInfo.Lambda;

                var outerConstants = closure.Constants;
                var outerNonPassedParams = closure.NonPassedParameters;

                // Load compiled lambda on stack counting the offset
                outerNestedLambdaIndex += outerConstants.Length + outerNonPassedParams.Length;

                il.Emit(OpCodes.Ldarg_0); // closure is always a first argument
                if (closure.Fields != null)
                    il.Emit(OpCodes.Ldfld, closure.Fields[outerNestedLambdaIndex]);
                else
                    LoadArrayClosureItem(il, outerNestedLambdaIndex, nestedLambda.GetType());

                // If lambda does not use any outer parameters to be set in closure, then we're done
                var nestedClosureInfo = nestedLambdaInfo.ClosureInfo;
                if (nestedClosureInfo == null)
                    return true;

                // If closure is array-based, the create a new array to represent closure for the nested lambda
                var isNestedArrayClosure = nestedClosureInfo.Fields == null;
                if (isNestedArrayClosure)
                {
                    EmitLoadConstantInt(il, nestedClosureInfo.ClosedItemCount); // size of array
                    il.Emit(OpCodes.Newarr, typeof(object));
                }

                // Load constants on stack
                var nestedConstants = nestedClosureInfo.Constants;
                if (nestedConstants.Length != 0)
                    for (var nestedConstIndex = 0; nestedConstIndex < nestedConstants.Length; nestedConstIndex++)
                    {
                        var nestedConstant = nestedConstants[nestedConstIndex];

                        // Find constant index in the outer closure
                        var outerConstIndex = outerConstants.IndexOf(it => it.ConstantExpr == nestedConstant.ConstantExpr);
                        if (outerConstIndex == -1)
                            return false; // some error is here

                        if (isNestedArrayClosure)
                        {
                            // Duplicate nested array on stack to store the item, and load index to where to store
                            il.Emit(OpCodes.Dup);
                            EmitLoadConstantInt(il, nestedConstIndex);
                        }

                        il.Emit(OpCodes.Ldarg_0); // closure is always a first argument
                        if (closure.Fields != null)
                            il.Emit(OpCodes.Ldfld, closure.Fields[outerConstIndex]);
                        else
                            LoadArrayClosureItem(il, outerConstIndex, nestedConstant.Type);

                        if (isNestedArrayClosure)
                        {
                            if (nestedConstant.Type.GetTypeInfo().IsValueType)
                                il.Emit(OpCodes.Box, nestedConstant.Type);
                            il.Emit(OpCodes.Stelem_Ref); // store the item in array
                        }
                    }

                // Load used and closed parameter values on stack
                var nestedNonPassedParams = nestedClosureInfo.NonPassedParameters;
                for (var nestedParamIndex = 0; nestedParamIndex < nestedNonPassedParams.Length; nestedParamIndex++)
                {
                    var nestedUsedParam = nestedNonPassedParams[nestedParamIndex];

                    // Duplicate nested array on stack to store the item, and load index to where to store
                    if (isNestedArrayClosure)
                    {
                        il.Emit(OpCodes.Dup);
                        EmitLoadConstantInt(il, nestedConstants.Length + nestedParamIndex);
                    }

                    var paramIndex = paramExprs.IndexOf(nestedUsedParam);
                    if (paramIndex != -1) // load param from input params
                    {
                        // +1 is set cause of added first closure argument
                        LoadParamArg(il, 1 + paramIndex);
                    }
                    else // load parameter from outer closure
                    {
                        if (outerNonPassedParams.Length == 0)
                            return false; // impossible, better to throw?

                        var outerParamIndex = outerNonPassedParams.IndexOf(it => it == nestedUsedParam);
                        if (outerParamIndex == -1)
                            return false; // impossible, better to throw?

                        il.Emit(OpCodes.Ldarg_0); // closure is always a first argument
                        if (closure.Fields != null)
                            il.Emit(OpCodes.Ldfld, closure.Fields[outerConstants.Length + outerParamIndex]);
                        else
                            LoadArrayClosureItem(il, outerConstants.Length + outerParamIndex, nestedUsedParam.Type);
                    }

                    if (isNestedArrayClosure)
                    {
                        if (nestedUsedParam.Type.GetTypeInfo().IsValueType)
                            il.Emit(OpCodes.Box, nestedUsedParam.Type);
                        il.Emit(OpCodes.Stelem_Ref); // store the item in array
                    }
                }

                // Load nested lambdas on stack
                var nestedNestedLambdas = nestedClosureInfo.NestedLambdas;
                if (nestedNestedLambdas.Length != 0)
                    for (var nestedLambdaIndex = 0; nestedLambdaIndex < nestedNestedLambdas.Length; nestedLambdaIndex++)
                    {
                        var nestedNestedLambda = nestedNestedLambdas[nestedLambdaIndex];

                        // Find constant index in the outer closure
                        var outerLambdaIndex = outerNestedLambdas.IndexOf(it => it.LambdaExpr == nestedNestedLambda.LambdaExpr);
                        if (outerLambdaIndex == -1)
                            return false; // some error is here

                        // Duplicate nested array on stack to store the item, and load index to where to store
                        if (isNestedArrayClosure)
                        {
                            il.Emit(OpCodes.Dup);
                            EmitLoadConstantInt(il, nestedConstants.Length + nestedNonPassedParams.Length + nestedLambdaIndex);
                        }

                        outerLambdaIndex += outerConstants.Length + outerNonPassedParams.Length;

                        il.Emit(OpCodes.Ldarg_0); // closure is always a first argument
                        if (closure.Fields != null)
                            il.Emit(OpCodes.Ldfld, closure.Fields[outerLambdaIndex]);
                        else
                            LoadArrayClosureItem(il, outerLambdaIndex,
                                nestedNestedLambda.Lambda.GetType());

                        if (isNestedArrayClosure)
                            il.Emit(OpCodes.Stelem_Ref); // store the item in array
                    }

                // Create nested closure object composed of all constants, params, lambdas loaded on stack
                if (isNestedArrayClosure)
                    il.Emit(OpCodes.Newobj, ArrayClosure.Constructor);
                else
                    il.Emit(OpCodes.Newobj,
                        nestedClosureInfo.ClosureType.GetTypeInfo().DeclaredConstructors.First());

                EmitMethodCall(il, GetCurryClosureMethod(nestedLambda, nestedLambdaInfo.IsAction));
                return true;
            }

            static MethodInfo GetCurryClosureMethod(object lambda, bool isAction)
            {
                var lambdaTypeArgs = lambda.GetType().GetTypeInfo().GenericTypeArguments;
                return isAction
                    ? CurryClosureActions.Methods[lambdaTypeArgs.Length - 1].MakeGenericMethod(lambdaTypeArgs)
                    : CurryClosureFuncs.Methods[lambdaTypeArgs.Length - 2].MakeGenericMethod(lambdaTypeArgs);
            }

            static bool EmitInvokeLambda(InvocationExpression expr, IList<ParameterExpression> paramExprs, ILGenerator il, ClosureInfo closure)
            {
                var instanceExpr = expr.Expression;
                if (!TryEmit(instanceExpr, instanceExpr.NodeType, instanceExpr.Type, paramExprs, il, closure) || !EmitMany(expr.Arguments, paramExprs, il, closure))
                    return false;

                var invokeMethod = instanceExpr.Type.GetTypeInfo().DeclaredMethods.First(m => m.Name == "Invoke");
                EmitMethodCall(il, invokeMethod);
                return true;
            }

            static bool EmitComparison(BinaryExpression e, IList<ParameterExpression> ps, ILGenerator il, ClosureInfo closure)
            {
                var leftExpr = e.Left;
                var rightExpr = e.Right;
                if (!TryEmit(leftExpr, leftExpr.NodeType, leftExpr.Type, ps, il, closure) || !TryEmit(rightExpr, rightExpr.NodeType, rightExpr.Type, ps, il, closure))
                    return false;

                switch (e.NodeType)
                {
                    case ExpressionType.Equal:
                        il.Emit(OpCodes.Ceq);
                        break;
                    case ExpressionType.LessThan:
                        il.Emit(OpCodes.Clt);
                        break;
                    case ExpressionType.GreaterThan:
                        il.Emit(OpCodes.Cgt);
                        break;
                    case ExpressionType.NotEqual:
                        il.Emit(OpCodes.Ceq);
                        il.Emit(OpCodes.Ldc_I4_0);
                        il.Emit(OpCodes.Ceq);
                        break;
                    case ExpressionType.LessThanOrEqual:
                        il.Emit(OpCodes.Cgt);
                        il.Emit(OpCodes.Ldc_I4_0);
                        il.Emit(OpCodes.Ceq);
                        break;
                    case ExpressionType.GreaterThanOrEqual:
                        il.Emit(OpCodes.Clt);
                        il.Emit(OpCodes.Ldc_I4_0);
                        il.Emit(OpCodes.Ceq);
                        break;
                }
                return true;
            }

            static bool EmitLogicalOperator(BinaryExpression expr, IList<ParameterExpression> ps, ILGenerator il, ClosureInfo closure)
            {
                var leftExpr = expr.Left;
                if (!TryEmit(leftExpr, leftExpr.NodeType, leftExpr.Type, ps, il, closure))
                    return false;

                var labelSkipRight = il.DefineLabel();
                var isAnd = expr.NodeType == ExpressionType.AndAlso;
                il.Emit(isAnd ? OpCodes.Brfalse : OpCodes.Brtrue, labelSkipRight);

                var rightExpr = expr.Right;
                if (!TryEmit(rightExpr, rightExpr.NodeType, rightExpr.Type, ps, il, closure))
                    return false;

                var labelDone = il.DefineLabel();
                il.Emit(OpCodes.Br, labelDone);

                il.MarkLabel(labelSkipRight); // label the second branch
                il.Emit(isAnd ? OpCodes.Ldc_I4_0 : OpCodes.Ldc_I4_1);

                il.MarkLabel(labelDone);
                return true;
            }

            static bool EmitTernararyOperator(ConditionalExpression expr, IList<ParameterExpression> ps, ILGenerator il, ClosureInfo closure)
            {
                var testExpr = expr.Test;
                if (!TryEmit(testExpr, testExpr.NodeType, testExpr.Type, ps, il, closure))
                    return false;

                var labelIfFalse = il.DefineLabel();
                il.Emit(OpCodes.Brfalse, labelIfFalse);

                var ifTrueExpr = expr.IfTrue;
                if (!TryEmit(ifTrueExpr, ifTrueExpr.NodeType, ifTrueExpr.Type, ps, il, closure))
                    return false;

                var labelDone = il.DefineLabel();
                il.Emit(OpCodes.Br, labelDone);

                il.MarkLabel(labelIfFalse);
                var ifFalseExpr = expr.IfFalse;
                if (!TryEmit(ifFalseExpr, ifFalseExpr.NodeType, ifFalseExpr.Type, ps, il, closure))
                    return false;

                il.MarkLabel(labelDone);
                return true;
            }

            static void EmitMethodCall(ILGenerator il, MethodInfo method)
            {
                il.Emit(method.IsVirtual ? OpCodes.Callvirt : OpCodes.Call, method);
            }

            static void EmitLoadConstantInt(ILGenerator il, int i)
            {
                switch (i)
                {
                    case -1:
                        il.Emit(OpCodes.Ldc_I4_M1);
                        break;
                    case 0:
                        il.Emit(OpCodes.Ldc_I4_0);
                        break;
                    case 1:
                        il.Emit(OpCodes.Ldc_I4_1);
                        break;
                    case 2:
                        il.Emit(OpCodes.Ldc_I4_2);
                        break;
                    case 3:
                        il.Emit(OpCodes.Ldc_I4_3);
                        break;
                    case 4:
                        il.Emit(OpCodes.Ldc_I4_4);
                        break;
                    case 5:
                        il.Emit(OpCodes.Ldc_I4_5);
                        break;
                    case 6:
                        il.Emit(OpCodes.Ldc_I4_6);
                        break;
                    case 7:
                        il.Emit(OpCodes.Ldc_I4_7);
                        break;
                    case 8:
                        il.Emit(OpCodes.Ldc_I4_8);
                        break;
                    default:
                        il.Emit(OpCodes.Ldc_I4, i);
                        break;
                }
            }
        }


        #region Closures

        internal static class Closure
        {
            static readonly IEnumerable<MethodInfo> _methods =
                typeof(Closure).GetTypeInfo().DeclaredMethods;

            public static readonly MethodInfo[] CreateMethods =
                _methods as MethodInfo[] ?? _methods.ToArray();

            public static Closure<T1> CreateClosure<T1>(T1 v1)
            {
                return new Closure<T1>(v1);
            }

            public static Closure<T1, T2> CreateClosure<T1, T2>(T1 v1, T2 v2)
            {
                return new Closure<T1, T2>(v1, v2);
            }

            public static Closure<T1, T2, T3> CreateClosure<T1, T2, T3>(T1 v1, T2 v2, T3 v3)
            {
                return new Closure<T1, T2, T3>(v1, v2, v3);
            }

            public static Closure<T1, T2, T3, T4> CreateClosure<T1, T2, T3, T4>(T1 v1, T2 v2, T3 v3, T4 v4)
            {
                return new Closure<T1, T2, T3, T4>(v1, v2, v3, v4);
            }

            public static Closure<T1, T2, T3, T4, T5> CreateClosure<T1, T2, T3, T4, T5>(T1 v1, T2 v2, T3 v3, T4 v4,
                T5 v5)
            {
                return new Closure<T1, T2, T3, T4, T5>(v1, v2, v3, v4, v5);
            }

            public static Closure<T1, T2, T3, T4, T5, T6> CreateClosure<T1, T2, T3, T4, T5, T6>(T1 v1, T2 v2, T3 v3,
                T4 v4, T5 v5, T6 v6)
            {
                return new Closure<T1, T2, T3, T4, T5, T6>(v1, v2, v3, v4, v5, v6);
            }

            public static Closure<T1, T2, T3, T4, T5, T6, T7> CreateClosure<T1, T2, T3, T4, T5, T6, T7>(T1 v1, T2 v2,
                T3 v3, T4 v4, T5 v5, T6 v6, T7 v7)
            {
                return new Closure<T1, T2, T3, T4, T5, T6, T7>(v1, v2, v3, v4, v5, v6, v7);
            }

            public static Closure<T1, T2, T3, T4, T5, T6, T7, T8> CreateClosure<T1, T2, T3, T4, T5, T6, T7, T8>(
                T1 v1, T2 v2, T3 v3, T4 v4, T5 v5, T6 v6, T7 v7, T8 v8)
            {
                return new Closure<T1, T2, T3, T4, T5, T6, T7, T8>(v1, v2, v3, v4, v5, v6, v7, v8);
            }

            public static Closure<T1, T2, T3, T4, T5, T6, T7, T8, T9> CreateClosure<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
                T1 v1, T2 v2, T3 v3, T4 v4, T5 v5, T6 v6, T7 v7, T8 v8, T9 v9)
            {
                return new Closure<T1, T2, T3, T4, T5, T6, T7, T8, T9>(v1, v2, v3, v4, v5, v6, v7, v8, v9);
            }

            public static Closure<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> CreateClosure<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
                T1 v1, T2 v2, T3 v3, T4 v4, T5 v5, T6 v6, T7 v7, T8 v8, T9 v9, T10 v10)
            {
                return new Closure<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(v1, v2, v3, v4, v5, v6, v7, v8, v9, v10);
            }
        }


        internal sealed class Closure<T1>
        {
            public T1 V1;

            public Closure(T1 v1)
            {
                V1 = v1;
            }
        }


        internal sealed class Closure<T1, T2>
        {
            public T1 V1;
            public T2 V2;

            public Closure(T1 v1, T2 v2)
            {
                V1 = v1;
                V2 = v2;
            }
        }


        internal sealed class Closure<T1, T2, T3>
        {
            public T1 V1;
            public T2 V2;
            public T3 V3;

            public Closure(T1 v1, T2 v2, T3 v3)
            {
                V1 = v1;
                V2 = v2;
                V3 = v3;
            }
        }


        internal sealed class Closure<T1, T2, T3, T4>
        {
            public T1 V1;
            public T2 V2;
            public T3 V3;
            public T4 V4;

            public Closure(T1 v1, T2 v2, T3 v3, T4 v4)
            {
                V1 = v1;
                V2 = v2;
                V3 = v3;
                V4 = v4;
            }
        }


        internal sealed class Closure<T1, T2, T3, T4, T5>
        {
            public T1 V1;
            public T2 V2;
            public T3 V3;
            public T4 V4;
            public T5 V5;

            public Closure(T1 v1, T2 v2, T3 v3, T4 v4, T5 v5)
            {
                V1 = v1;
                V2 = v2;
                V3 = v3;
                V4 = v4;
                V5 = v5;
            }
        }


        internal sealed class Closure<T1, T2, T3, T4, T5, T6>
        {
            public T1 V1;
            public T2 V2;
            public T3 V3;
            public T4 V4;
            public T5 V5;
            public T6 V6;

            public Closure(T1 v1, T2 v2, T3 v3, T4 v4, T5 v5, T6 v6)
            {
                V1 = v1;
                V2 = v2;
                V3 = v3;
                V4 = v4;
                V5 = v5;
                V6 = v6;
            }
        }


        internal sealed class Closure<T1, T2, T3, T4, T5, T6, T7>
        {
            public T1 V1;
            public T2 V2;
            public T3 V3;
            public T4 V4;
            public T5 V5;
            public T6 V6;
            public T7 V7;

            public Closure(T1 v1, T2 v2, T3 v3, T4 v4, T5 v5, T6 v6, T7 v7)
            {
                V1 = v1;
                V2 = v2;
                V3 = v3;
                V4 = v4;
                V5 = v5;
                V6 = v6;
                V7 = v7;
            }
        }


        internal sealed class Closure<T1, T2, T3, T4, T5, T6, T7, T8>
        {
            public T1 V1;
            public T2 V2;
            public T3 V3;
            public T4 V4;
            public T5 V5;
            public T6 V6;
            public T7 V7;
            public T8 V8;

            public Closure(T1 v1, T2 v2, T3 v3, T4 v4, T5 v5, T6 v6, T7 v7, T8 v8)
            {
                V1 = v1;
                V2 = v2;
                V3 = v3;
                V4 = v4;
                V5 = v5;
                V6 = v6;
                V7 = v7;
                V8 = v8;
            }
        }


        internal sealed class Closure<T1, T2, T3, T4, T5, T6, T7, T8, T9>
        {
            public T1 V1;
            public T2 V2;
            public T3 V3;
            public T4 V4;
            public T5 V5;
            public T6 V6;
            public T7 V7;
            public T8 V8;
            public T9 V9;

            public Closure(T1 v1, T2 v2, T3 v3, T4 v4, T5 v5, T6 v6, T7 v7, T8 v8, T9 v9)
            {
                V1 = v1;
                V2 = v2;
                V3 = v3;
                V4 = v4;
                V5 = v5;
                V6 = v6;
                V7 = v7;
                V8 = v8;
                V9 = v9;
            }
        }


        internal sealed class Closure<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>
        {
            public T1 V1;
            public T10 V10;
            public T2 V2;
            public T3 V3;
            public T4 V4;
            public T5 V5;
            public T6 V6;
            public T7 V7;
            public T8 V8;
            public T9 V9;

            public Closure(T1 v1, T2 v2, T3 v3, T4 v4, T5 v5, T6 v6, T7 v7, T8 v8, T9 v9, T10 v10)
            {
                V1 = v1;
                V2 = v2;
                V3 = v3;
                V4 = v4;
                V5 = v5;
                V6 = v6;
                V7 = v7;
                V8 = v8;
                V9 = v9;
                V10 = v10;
            }
        }


        internal sealed class ArrayClosure
        {
            public static FieldInfo ArrayField = typeof(ArrayClosure).GetTypeInfo().DeclaredFields.First(f => !f.IsStatic);
            public static ConstructorInfo Constructor = typeof(ArrayClosure).GetTypeInfo().DeclaredConstructors.First();
            public readonly object[] Constants;

            public ArrayClosure(object[] constants)
            {
                Constants = constants;
            }
        }

        #endregion

        #region Nested Lambdas

        struct NestedLambdaInfo
        {
            public readonly ClosureInfo ClosureInfo;

            public readonly object LambdaExpr; // to find the lambda in bigger parent expression
            public readonly object Lambda;
            public readonly bool IsAction;

            public NestedLambdaInfo(ClosureInfo closureInfo, object lambdaExpr, object lambda, bool isAction)
            {
                ClosureInfo = closureInfo;
                Lambda = lambda;
                LambdaExpr = lambdaExpr;
                IsAction = isAction;
            }
        }


        internal static class CurryClosureFuncs
        {
            static readonly IEnumerable<MethodInfo> _methods =
                typeof(CurryClosureFuncs).GetTypeInfo().DeclaredMethods;

            public static readonly MethodInfo[] Methods = _methods as MethodInfo[] ?? _methods.ToArray();

            public static Func<R> Curry<C, R>(Func<C, R> f, C c)
            {
                return () => f(c);
            }

            public static Func<T1, R> Curry<C, T1, R>(Func<C, T1, R> f, C c)
            {
                return t1 => f(c, t1);
            }

            public static Func<T1, T2, R> Curry<C, T1, T2, R>(Func<C, T1, T2, R> f, C c)
            {
                return (t1, t2) => f(c, t1, t2);
            }

            public static Func<T1, T2, T3, R> Curry<C, T1, T2, T3, R>(Func<C, T1, T2, T3, R> f, C c)
            {
                return (t1, t2, t3) => f(c, t1, t2, t3);
            }

            public static Func<T1, T2, T3, T4, R> Curry<C, T1, T2, T3, T4, R>(Func<C, T1, T2, T3, T4, R> f, C c)
            {
                return (t1, t2, t3, t4) => f(c, t1, t2, t3, t4);
            }

            public static Func<T1, T2, T3, T4, T5, R> Curry<C, T1, T2, T3, T4, T5, R>(Func<C, T1, T2, T3, T4, T5, R> f, C c)
            {
                return (t1, t2, t3, t4, t5) => f(c, t1, t2, t3, t4, t5);
            }

            public static Func<T1, T2, T3, T4, T5, T6, R> Curry<C, T1, T2, T3, T4, T5, T6, R>(Func<C, T1, T2, T3, T4, T5, T6, R> f, C c)
            {
                return (t1, t2, t3, t4, t5, t6) => f(c, t1, t2, t3, t4, t5, t6);
            }
        }


        internal static class CurryClosureActions
        {
            static readonly IEnumerable<MethodInfo> _methods =
                typeof(CurryClosureActions).GetTypeInfo().DeclaredMethods;

            public static readonly MethodInfo[] Methods = _methods as MethodInfo[] ?? _methods.ToArray();

            internal static Action Curry<C>(Action<C> a, C c)
            {
                return () => a(c);
            }

            internal static Action<T1> Curry<C, T1>(Action<C, T1> f, C c)
            {
                return t1 => f(c, t1);
            }

            internal static Action<T1, T2> Curry<C, T1, T2>(Action<C, T1, T2> f, C c)
            {
                return (t1, t2) => f(c, t1, t2);
            }

            internal static Action<T1, T2, T3> Curry<C, T1, T2, T3>(Action<C, T1, T2, T3> f, C c)
            {
                return (t1, t2, t3) => f(c, t1, t2, t3);
            }

            internal static Action<T1, T2, T3, T4> Curry<C, T1, T2, T3, T4>(Action<C, T1, T2, T3, T4> f, C c)
            {
                return (t1, t2, t3, t4) => f(c, t1, t2, t3, t4);
            }

            internal static Action<T1, T2, T3, T4, T5> Curry<C, T1, T2, T3, T4, T5>(Action<C, T1, T2, T3, T4, T5> f, C c)
            {
                return (t1, t2, t3, t4, t5) => f(c, t1, t2, t3, t4, t5);
            }

            internal static Action<T1, T2, T3, T4, T5, T6> Curry<C, T1, T2, T3, T4, T5, T6>(Action<C, T1, T2, T3, T4, T5, T6> f, C c)
            {
                return (t1, t2, t3, t4, t5, t6) => f(c, t1, t2, t3, t4, t5, t6);
            }
        }

        #endregion

        #region Collect Bound Constants

        static bool IsBoundConstant(object value)
        {
            if (value == null)
                return false;

            var typeInfo = value.GetType().GetTypeInfo();
            return !typeInfo.IsPrimitive
                   && !(value is string)
                   && !(value is Type)
                   && !typeInfo.IsEnum;
        }

        // @paramExprs is required for nested lambda compilation
        static bool TryCollectBoundConstants(ref ClosureInfo closure,
            object exprObj, ExpressionType exprNodeType, Type exprType,
            IList<ParameterExpression> paramExprs)
        {
            if (exprObj == null)
                return false;

            switch (exprNodeType)
            {
                case ExpressionType.Constant:
                    var constExprInfo = exprObj as ConstantExpressionInfo;
                    var value = constExprInfo != null ? constExprInfo.Value : ((ConstantExpression) exprObj).Value;
                    if (value is Delegate || IsBoundConstant(value))
                        (closure ?? (closure = new ClosureInfo())).AddConstant(exprObj, value, exprType);
                    break;

                case ExpressionType.Parameter:
                    // if parameter is used But no passed (not in parameter expressions)
                    // it means parameter is provided by outer lambda and should be put in closure for current lambda
                    var exprInfo = exprObj as ParameterExpressionInfo;
                    var paramExpr = exprInfo ?? (ParameterExpression) exprObj;
                    if (paramExprs.IndexOf(paramExpr) == -1)
                        (closure ?? (closure = new ClosureInfo())).AddNonPassedParam(paramExpr);
                    break;

                case ExpressionType.Call:
                    var callInfo = exprObj as MethodCallExpressionInfo;
                    if (callInfo != null)
                    {
                        var objInfo = callInfo.Object;
                        return (objInfo == null
                                || TryCollectBoundConstants(ref closure, objInfo, objInfo.NodeType, objInfo.Type, paramExprs))
                               && TryCollectBoundConstants(ref closure, callInfo.Arguments, paramExprs);
                    }

                    var callExpr = (MethodCallExpression) exprObj;
                    var objExpr = callExpr.Object;
                    return (objExpr == null
                            || TryCollectBoundConstants(ref closure, objExpr, objExpr.NodeType, objExpr.Type, paramExprs))
                           && TryCollectBoundConstants(ref closure, callExpr.Arguments, paramExprs);

                case ExpressionType.MemberAccess:
                    var memberExprInfo = exprObj as MemberExpressionInfo;
                    if (memberExprInfo != null)
                    {
                        var maExprInfo = memberExprInfo.Expression;
                        return maExprInfo == null
                               || TryCollectBoundConstants(ref closure, maExprInfo, maExprInfo.NodeType, maExprInfo.Type, paramExprs);
                    }

                    var memberExpr = ((MemberExpression) exprObj).Expression;
                    return memberExpr == null
                           || TryCollectBoundConstants(ref closure, memberExpr, memberExpr.NodeType, memberExpr.Type, paramExprs);

                case ExpressionType.New:
                    var newExprInfo = exprObj as NewExpressionInfo;
                    return newExprInfo != null
                        ? TryCollectBoundConstants(ref closure, newExprInfo.Arguments, paramExprs)
                        : TryCollectBoundConstants(ref closure, ((NewExpression) exprObj).Arguments, paramExprs);

                case ExpressionType.NewArrayInit:
                    var newArrayInitInfo = exprObj as NewArrayExpressionInfo;
                    if (newArrayInitInfo != null)
                        return TryCollectBoundConstants(ref closure, newArrayInitInfo.Arguments, paramExprs);
                    return TryCollectBoundConstants(ref closure, ((NewArrayExpression) exprObj).Expressions, paramExprs);

                // property and field initializer
                case ExpressionType.MemberInit:

                    var memberInitExprInfo = exprObj as MemberInitExpressionInfo;
                    if (memberInitExprInfo != null)
                    {
                        var miNewInfo = memberInitExprInfo.NewExpressionInfo;
                        if (!TryCollectBoundConstants(ref closure, miNewInfo, miNewInfo.NodeType, miNewInfo.Type, paramExprs))
                            return false;

                        var memberBindingInfos = memberInitExprInfo.Bindings;
                        for (var i = 0; i < memberBindingInfos.Length; i++)
                        {
                            var maInfo = memberBindingInfos[i].Expression;
                            if (!TryCollectBoundConstants(ref closure, maInfo, maInfo.NodeType, maInfo.Type, paramExprs))
                                return false;
                        }
                        return true;
                    }
                    else
                    {
                        var memberInitExpr = (MemberInitExpression) exprObj;
                        var miNewExpr = memberInitExpr.NewExpression;
                        if (!TryCollectBoundConstants(ref closure, miNewExpr, miNewExpr.NodeType, miNewExpr.Type, paramExprs))
                            return false;
                        var memberBindings = memberInitExpr.Bindings;
                        for (var i = 0; i < memberBindings.Count; ++i)
                        {
                            var memberBinding = memberBindings[i];
                            var maExpr = ((MemberAssignment) memberBinding).Expression;
                            if (memberBinding.BindingType == MemberBindingType.Assignment
                                && !TryCollectBoundConstants(ref closure, maExpr, maExpr.NodeType, maExpr.Type, paramExprs))
                                return false;
                        }
                    }

                    break;

                // nested lambda expression
                case ExpressionType.Lambda:

                    // 1. Try to compile nested lambda in place
                    // 2. Check that parameters used in compiled lambda are passed or closed by outer lambda
                    // 3. Add the compiled lambda to closure of outer lambda for later invocation

                    object lambda;
                    Type lambdaReturnType;
                    ClosureInfo nestedClosure = null;

                    var lambdaExprInfo = exprObj as LambdaExpressionInfo;
                    if (lambdaExprInfo != null)
                    {
                        var lambdaParamExprs = lambdaExprInfo.Parameters;
                        var bodyExprInfo = lambdaExprInfo.Body;
                        lambdaReturnType = bodyExprInfo.Type;
                        lambda = TryCompile(ref nestedClosure,
                            lambdaExprInfo.Type, GetParamExprTypes(lambdaParamExprs), lambdaReturnType,
                            bodyExprInfo, bodyExprInfo.NodeType, bodyExprInfo.Type,
                            lambdaParamExprs, true);
                    }
                    else
                    {
                        var lambdaExpr = (LambdaExpression) exprObj;
                        var lambdaParamExprs = lambdaExpr.Parameters;
                        var bodyExpr = lambdaExpr.Body;
                        lambdaReturnType = bodyExpr.Type;
                        lambda = TryCompile(ref nestedClosure,
                            lambdaExpr.Type, GetParamExprTypes(lambdaParamExprs), lambdaReturnType,
                            bodyExpr, bodyExpr.NodeType, bodyExpr.Type,
                            lambdaParamExprs, true);
                    }

                    if (lambda == null)
                        return false;

                    // add the nested lambda into closure
                    (closure ?? (closure = new ClosureInfo()))
                        .AddNestedLambda(exprObj, lambda, nestedClosure, lambdaReturnType == typeof(void));

                    if (nestedClosure == null)
                        break;

                    // if nested non passed parameter is no matched with any outer passed parameter,
                    // then ensure it goes to outer non passed parameter.
                    // But check that have non passed parameter in root expression is invalid.
                    var nestedNonPassedParams = nestedClosure.NonPassedParameters;
                    if (nestedNonPassedParams.Length != 0)
                        for (var i = 0; i < nestedNonPassedParams.Length; i++)
                        {
                            var nestedNonPassedParam = nestedNonPassedParams[i];
                            if (paramExprs.Count == 0 || paramExprs.IndexOf(nestedNonPassedParam) == -1)
                                closure.AddNonPassedParam(nestedNonPassedParam);
                        }

                    // Promote found constants and nested lambdas into outer closure
                    var nestedConstants = nestedClosure.Constants;
                    if (nestedConstants.Length != 0)
                        for (var i = 0; i < nestedConstants.Length; i++)
                            closure.AddConstant(nestedConstants[i]);

                    var nestedNestedLambdas = nestedClosure.NestedLambdas;
                    if (nestedNestedLambdas.Length != 0)
                        for (var i = 0; i < nestedNestedLambdas.Length; i++)
                            closure.AddNestedLambda(nestedNestedLambdas[i]);

                    break;

                case ExpressionType.Invoke:
                    var invokeExpr = (InvocationExpression) exprObj;
                    var invokeExprExpr = invokeExpr.Expression;
                    return TryCollectBoundConstants(ref closure, invokeExprExpr, invokeExprExpr.NodeType, invokeExprExpr.Type, paramExprs)
                           && TryCollectBoundConstants(ref closure, invokeExpr.Arguments, paramExprs);

                case ExpressionType.Conditional:
                    var condExpr = (ConditionalExpression) exprObj;
                    return TryCollectBoundConstants(ref closure, condExpr.Test, condExpr.Test.NodeType, condExpr.Type, paramExprs)
                           && TryCollectBoundConstants(ref closure, condExpr.IfTrue, condExpr.IfTrue.NodeType, condExpr.Type, paramExprs)
                           && TryCollectBoundConstants(ref closure, condExpr.IfFalse, condExpr.IfFalse.NodeType, condExpr.IfFalse.Type, paramExprs);

                default:
                    if (exprObj is ExpressionInfo)
                    {
                        var unaryExprInfo = exprObj as UnaryExpressionInfo;
                        if (unaryExprInfo != null)
                        {
                            var opInfo = unaryExprInfo.Operand;
                            return TryCollectBoundConstants(ref closure, opInfo, opInfo.NodeType, opInfo.Type, paramExprs);
                        }

                        var binInfo = exprObj as BinaryExpressionInfo;
                        if (binInfo != null)
                        {
                            var leftInfo = binInfo.Left;
                            var rightInfo = binInfo.Right;
                            return TryCollectBoundConstants(ref closure, leftInfo, leftInfo.NodeType, leftInfo.Type, paramExprs)
                                   && TryCollectBoundConstants(ref closure, rightInfo, rightInfo.NodeType, rightInfo.Type, paramExprs);
                        }
                        break;
                    }

                    var unaryExpr = exprObj as UnaryExpression;
                    if (unaryExpr != null)
                    {
                        var opExpr = unaryExpr.Operand;
                        return TryCollectBoundConstants(ref closure, opExpr, opExpr.NodeType, opExpr.Type, paramExprs);
                    }

                    var binaryExpr = exprObj as BinaryExpression;
                    if (binaryExpr != null)
                    {
                        var leftExpr = binaryExpr.Left;
                        var rightExpr = binaryExpr.Right;
                        return TryCollectBoundConstants(ref closure, leftExpr, leftExpr.NodeType, leftExpr.Type, paramExprs)
                               && TryCollectBoundConstants(ref closure, rightExpr, rightExpr.NodeType, rightExpr.Type, paramExprs);
                    }
                    break;
            }

            return true;
        }

        static bool TryCollectBoundConstants(ref ClosureInfo closure, ExpressionInfo[] infos, IList<ParameterExpression> paramExprs)
        {
            for (var i = 0; i < infos.Length; i++)
            {
                var info = infos[i];
                if (!TryCollectBoundConstants(ref closure, info, info.NodeType, info.Type, paramExprs))
                    return false;
            }
            return true;
        }

        static bool TryCollectBoundConstants(ref ClosureInfo closure, IList<Expression> exprs, IList<ParameterExpression> paramExprs)
        {
            for (var i = 0; i < exprs.Count; i++)
            {
                var expr = exprs[i];
                if (!TryCollectBoundConstants(ref closure, expr, expr.NodeType, expr.Type, paramExprs))
                    return false;
            }
            return true;
        }

        /// <summary>Construct delegate type (Func or Action) from given input and return parameter types.</summary>
        public static Type GetFuncOrActionType(Type[] paramTypes, Type returnType)
        {
            if (returnType == typeof(void))
                switch (paramTypes.Length)
                {
                    case 0: return typeof(Action);
                    case 1: return typeof(Action<>).MakeGenericType(paramTypes);
                    case 2: return typeof(Action<,>).MakeGenericType(paramTypes);
                    case 3: return typeof(Action<,,>).MakeGenericType(paramTypes);
                    case 4: return typeof(Action<,,,>).MakeGenericType(paramTypes);
                    case 5: return typeof(Action<,,,,>).MakeGenericType(paramTypes);
                    case 6: return typeof(Action<,,,,,>).MakeGenericType(paramTypes);
                    case 7: return typeof(Action<,,,,,,>).MakeGenericType(paramTypes);
                    default:
                        throw new NotSupportedException(
                            string.Format("Action with so many ({0}) parameters is not supported!", paramTypes.Length));
                }

            paramTypes = paramTypes.Append(returnType);
            switch (paramTypes.Length)
            {
                case 1: return typeof(Func<>).MakeGenericType(paramTypes);
                case 2: return typeof(Func<,>).MakeGenericType(paramTypes);
                case 3: return typeof(Func<,,>).MakeGenericType(paramTypes);
                case 4: return typeof(Func<,,,>).MakeGenericType(paramTypes);
                case 5: return typeof(Func<,,,,>).MakeGenericType(paramTypes);
                case 6: return typeof(Func<,,,,,>).MakeGenericType(paramTypes);
                case 7: return typeof(Func<,,,,,,>).MakeGenericType(paramTypes);
                case 8: return typeof(Func<,,,,,,,>).MakeGenericType(paramTypes);
                default:
                    throw new NotSupportedException(
                        string.Format("Func with so many ({0}) parameters is not supported!", paramTypes.Length));
            }
        }

        #endregion
    }


    static class Arr
    {
        public static T[] Empty<T>()
        {
            return EmptyArray<T>.Value;
        }

        public static T[] Append<T>(this T[] source, T value)
        {
            if (source == null || source.Length == 0)
                return new[] {value};
            if (source.Length == 1)
                return new[] {source[0], value};
            if (source.Length == 2)
                return new[] {source[0], source[1], value};
            var sourceLength = source.Length;
            var result = new T[sourceLength + 1];
            Array.Copy(source, result, sourceLength);
            result[sourceLength] = value;
            return result;
        }

        public static int IndexOf<T>(this T[] source, Func<T, bool> predicate)
        {
            if (source == null || source.Length == 0)
                return -1;
            if (source.Length == 1)
                return predicate(source[0]) ? 0 : -1;
            for (var i = 0; i < source.Length; ++i)
                if (predicate(source[i]))
                    return i;
            return -1;
        }


        static class EmptyArray<T>
        {
            public static readonly T[] Value = new T[0];
        }
    }


    /// <summary>Base expression.</summary>
    public abstract class ExpressionInfo
    {
        /// <summary>Expression node type.</summary>
        public abstract ExpressionType NodeType { get; }

        /// <summary>All expressions should have a Type.</summary>
        public abstract Type Type { get; }

        /// <summary>Allow to change parameter expression as info interchangeable.</summary>
        public static implicit operator ExpressionInfo(ParameterExpression paramExpr)
        {
            return new ParameterExpressionInfo(paramExpr);
        }

        /// <summary>Analog of Expression.Constant</summary>
        public static ConstantExpressionInfo Constant(object value, Type type = null)
        {
            return new ConstantExpressionInfo(value, type);
        }

        /// <summary>Analog of Expression.New</summary>
        public static NewExpressionInfo New(ConstructorInfo ctor)
        {
            return new NewExpressionInfo(ctor, Arr.Empty<ExpressionInfo>());
        }

        /// <summary>Analog of Expression.New</summary>
        public static NewExpressionInfo New(ConstructorInfo ctor, params ExpressionInfo[] arguments)
        {
            return new NewExpressionInfo(ctor, arguments);
        }

        /// <summary>Static method call</summary>
        public static MethodCallExpressionInfo Call(MethodInfo method, params ExpressionInfo[] arguments)
        {
            return new MethodCallExpressionInfo(null, method, arguments);
        }

        /// <summary>Instance method call</summary>
        public static MethodCallExpressionInfo Call(
            ExpressionInfo instance, MethodInfo method, params ExpressionInfo[] arguments)
        {
            return new MethodCallExpressionInfo(instance, method, arguments);
        }

        /// <summary>Static property</summary>
        public static PropertyExpressionInfo Property(PropertyInfo property)
        {
            return new PropertyExpressionInfo(null, property);
        }

        /// <summary>Instance property</summary>
        public static PropertyExpressionInfo Property(ExpressionInfo instance, PropertyInfo property)
        {
            return new PropertyExpressionInfo(instance, property);
        }

        /// <summary>Static field</summary>
        public static FieldExpressionInfo Field(FieldInfo field)
        {
            return new FieldExpressionInfo(null, field);
        }

        /// <summary>Instance field</summary>
        public static FieldExpressionInfo Property(ExpressionInfo instance, FieldInfo field)
        {
            return new FieldExpressionInfo(instance, field);
        }

        /// <summary>Analog of Expression.Lambda</summary>
        public static LambdaExpressionInfo Lambda(ExpressionInfo body)
        {
            return new LambdaExpressionInfo(body, Arr.Empty<ParameterExpression>());
        }

        /// <summary>Analog of Expression.Lambda</summary>
        public static LambdaExpressionInfo Lambda(ExpressionInfo body, params ParameterExpression[] parameters)
        {
            return new LambdaExpressionInfo(body, parameters);
        }

        /// <summary>Analog of Expression.Convert</summary>
        public static UnaryExpressionInfo Convert(ExpressionInfo operand, Type targetType)
        {
            return new UnaryExpressionInfo(ExpressionType.Convert, operand, targetType);
        }

        /// <summary>Analog of Expression.Lambda</summary>
        public static ExpressionInfo<TDelegate> Lambda<TDelegate>(ExpressionInfo body)
        {
            return new ExpressionInfo<TDelegate>(body, Arr.Empty<ParameterExpression>());
        }

        /// <summary>Analog of Expression.Lambda</summary>
        public static ExpressionInfo<TDelegate> Lambda<TDelegate>(ExpressionInfo body, params ParameterExpression[] parameters)
        {
            return new ExpressionInfo<TDelegate>(body, parameters);
        }

        /// <summary>Analog of Expression.ArrayIdex</summary>
        public static BinaryExpressionInfo ArrayIndex(ExpressionInfo array, ExpressionInfo index)
        {
            return new BinaryExpressionInfo(ExpressionType.ArrayIndex, array, index, array.Type.GetElementType());
        }

        /// <summary>Expression.Bind used in Expression.MemberInit</summary>
        public static MemberAssignmentInfo Bind(MemberInfo member, ExpressionInfo expression)
        {
            return new MemberAssignmentInfo(member, expression);
        }

        /// <summary>Analog of Expression.MemberInit</summary>
        public static MemberInitExpressionInfo MemberInit(NewExpressionInfo newExpr, params MemberAssignmentInfo[] bindings)
        {
            return new MemberInitExpressionInfo(newExpr, bindings);
        }

        /// <summary>Constructs an array given the array type and item initializer expressions.</summary>
        public static NewArrayExpressionInfo NewArrayInit(Type type, params ExpressionInfo[] initializers)
        {
            return new NewArrayExpressionInfo(type, initializers);
        }
    }


    /// <summary>Analog of Convert expression.</summary>
    public class UnaryExpressionInfo : ExpressionInfo
    {
        /// <summary>Operand expression</summary>
        public readonly ExpressionInfo Operand;

        /// <summary>Constructor</summary>
        public UnaryExpressionInfo(ExpressionType nodeType, ExpressionInfo operand, Type targetType)
        {
            NodeType = nodeType;
            Operand = operand;
            Type = targetType;
        }

        /// <inheritdoc />
        public override ExpressionType NodeType { get; }

        /// <summary>Target type.</summary>
        public override Type Type { get; }
    }


    /// <summary>BinaryExpression analog.</summary>
    public class BinaryExpressionInfo : ExpressionInfo
    {
        /// <summary>Left expression</summary>
        public readonly ExpressionInfo Left;

        /// <summary>Right expression</summary>
        public readonly ExpressionInfo Right;

        /// <summary>Constructs from left and right expressions.</summary>
        public BinaryExpressionInfo(ExpressionType nodeType, ExpressionInfo left, ExpressionInfo right, Type type)
        {
            NodeType = nodeType;
            Type = type;
            Left = left;
            Right = right;
        }

        /// <inheritdoc />
        public override ExpressionType NodeType { get; }

        /// <inheritdoc />
        public override Type Type { get; }
    }


    /// <summary>Analog of MemberInitExpression</summary>
    public class MemberInitExpressionInfo : ExpressionInfo
    {
        /// <summary>Member assignments.</summary>
        public readonly MemberAssignmentInfo[] Bindings;

        /// <summary>New expression.</summary>
        public readonly NewExpressionInfo NewExpressionInfo;

        /// <summary>Constructs out of new expression and member initialization list.</summary>
        public MemberInitExpressionInfo(NewExpressionInfo newExpressionInfo, MemberAssignmentInfo[] bindings)
        {
            NewExpressionInfo = newExpressionInfo;
            Bindings = bindings;
        }

        /// <inheritdoc />
        public override ExpressionType NodeType => ExpressionType.MemberInit;

        /// <inheritdoc />
        public override Type Type => NewExpressionInfo.Type;
    }


    /// <summary>Wraps ParameterExpression and just it.</summary>
    public class ParameterExpressionInfo : ExpressionInfo
    {
        /// <summary>Constructor</summary>
        public ParameterExpressionInfo(ParameterExpression paramExpr)
        {
            ParamExpr = paramExpr;
        }

        /// <summary>Wrapped parameter expression.</summary>
        public ParameterExpression ParamExpr { get; }

        /// <inheritdoc />
        public override ExpressionType NodeType => ExpressionType.Parameter;

        /// <inheritdoc />
        public override Type Type => ParamExpr.Type;

        /// <summary>Optional name.</summary>
        public string Name => ParamExpr.Name;

        /// <summary>Allow to change parameter expression as info interchangeable.</summary>
        public static implicit operator ParameterExpression(ParameterExpressionInfo info)
        {
            return info.ParamExpr;
        }
    }


    /// <summary>Analog of ConstantExpression.</summary>
    public class ConstantExpressionInfo : ExpressionInfo
    {
        /// <summary>Value of constant.</summary>
        public readonly object Value;

        /// <summary>Constructor</summary>
        public ConstantExpressionInfo(object value, Type type = null)
        {
            Value = value;
            Type = type ?? (value == null ? typeof(object) : value.GetType());
        }

        /// <inheritdoc />
        public override ExpressionType NodeType => ExpressionType.Constant;

        /// <inheritdoc />
        public override Type Type { get; }
    }


    /// <summary>Base class for expressions with arguments.</summary>
    public abstract class ArgumentsExpressionInfo : ExpressionInfo
    {
        /// <summary>List of arguments</summary>
        public readonly ExpressionInfo[] Arguments;

        /// <summary>Constructor</summary>
        protected ArgumentsExpressionInfo(ExpressionInfo[] arguments)
        {
            Arguments = arguments;
        }
    }


    /// <summary>Analog of NewExpression</summary>
    public class NewExpressionInfo : ArgumentsExpressionInfo
    {
        /// <summary>The constructor info.</summary>
        public readonly ConstructorInfo Constructor;

        /// <summary>Construct from constructor info and argument expressions</summary>
        public NewExpressionInfo(ConstructorInfo constructor, params ExpressionInfo[] arguments) : base(arguments)
        {
            Constructor = constructor;
        }

        /// <inheritdoc />
        public override ExpressionType NodeType => ExpressionType.New;

        /// <inheritdoc />
        public override Type Type => Constructor.DeclaringType;
    }


    /// <summary>NewArrayExpression</summary>
    public class NewArrayExpressionInfo : ArgumentsExpressionInfo
    {
        /// <summary>Array type and initializer</summary>
        public NewArrayExpressionInfo(Type type, ExpressionInfo[] initializers) : base(initializers)
        {
            Type = type;
        }

        /// <inheritdoc />
        public override ExpressionType NodeType => ExpressionType.NewArrayInit;

        /// <inheritdoc />
        public override Type Type { get; }
    }


    /// <summary>Analog of MethodCallExpression</summary>
    public class MethodCallExpressionInfo : ArgumentsExpressionInfo
    {
        /// <summary>The method info.</summary>
        public readonly MethodInfo Method;

        /// <summary>Instance expression, null if static.</summary>
        public readonly ExpressionInfo Object;

        /// <summary>Construct from method info and argument expressions</summary>
        public MethodCallExpressionInfo(
            ExpressionInfo @object, MethodInfo method, params ExpressionInfo[] arguments) : base(arguments)
        {
            Object = @object;
            Method = method;
        }

        /// <inheritdoc />
        public override ExpressionType NodeType => ExpressionType.Call;

        /// <inheritdoc />
        public override Type Type => Method.ReturnType;
    }


    /// <summary>Analog of MemberExpression</summary>
    public abstract class MemberExpressionInfo : ExpressionInfo
    {
        /// <summary>Instance expression, null if static.</summary>
        public readonly ExpressionInfo Expression;

        /// <summary>Member info.</summary>
        public readonly MemberInfo Member;

        /// <summary>Constructs with</summary>
        protected MemberExpressionInfo(ExpressionInfo expression, MemberInfo member)
        {
            Expression = expression;
            Member = member;
        }

        /// <inheritdoc />
        public override ExpressionType NodeType => ExpressionType.MemberAccess;
    }


    /// <summary>Analog of PropertyExpression</summary>
    public class PropertyExpressionInfo : MemberExpressionInfo
    {
        /// <summary>Construct from property info</summary>
        public PropertyExpressionInfo(ExpressionInfo instance, PropertyInfo property)
            : base(instance, property)
        {
        }

        /// <inheritdoc />
        public override Type Type => ((PropertyInfo) Member).PropertyType;
    }


    /// <summary>Analog of PropertyExpression</summary>
    public class FieldExpressionInfo : MemberExpressionInfo
    {
        /// <summary>Construct from field info</summary>
        public FieldExpressionInfo(ExpressionInfo instance, FieldInfo field)
            : base(instance, field)
        {
        }

        /// <inheritdoc />
        public override Type Type => ((FieldInfo) Member).FieldType;
    }


    /// <summary>MemberAssignment analog.</summary>
    public struct MemberAssignmentInfo
    {
        /// <summary>Member to assign to.</summary>
        public MemberInfo Member;

        /// <summary>Expression to assign</summary>
        public ExpressionInfo Expression;

        /// <summary>Constructs out of member and expression to assign.</summary>
        public MemberAssignmentInfo(MemberInfo member, ExpressionInfo expression)
        {
            Member = member;
            Expression = expression;
        }
    }


    /// <summary>LambdaExpression</summary>
    public class LambdaExpressionInfo : ExpressionInfo
    {
        /// <summary>Lambda body.</summary>
        public readonly ExpressionInfo Body;

        /// <summary>List of parameters.</summary>
        public readonly ParameterExpression[] Parameters;

        /// <summary>Constructor</summary>
        public LambdaExpressionInfo(ExpressionInfo body, ParameterExpression[] parameters)
        {
            Body = body;
            Parameters = parameters;
            Type = ExpressionCompiler.GetFuncOrActionType(ExpressionCompiler.GetParamExprTypes(parameters), Body.Type);
        }

        /// <inheritdoc />
        public override ExpressionType NodeType => ExpressionType.Lambda;

        /// <inheritdoc />
        public override Type Type { get; }
    }


    /// <summary>Typed lambda expression.</summary>
    public sealed class ExpressionInfo<TDelegate> : LambdaExpressionInfo
    {
        /// <summary>Constructor</summary>
        public ExpressionInfo(ExpressionInfo body, ParameterExpression[] parameters) : base(body, parameters)
        {
        }

        /// <summary>Type of lambda</summary>
        public Type DelegateType => typeof(TDelegate);
    }
}