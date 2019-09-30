// Decompiled with JetBrains decompiler
// Type: System.Linq.Expressions.Expression
// Assembly: System.Linq.Expressions, Version=4.2.1.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// MVID: 906C1187-7F7C-4920-B66F-513638DD8FFF
// Assembly location: /opt/dotnet/shared/Microsoft.NETCore.App/2.2.3/System.Linq.Expressions.dll

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic.Utils;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace System.Linq.Expressions
{
  public abstract class Expression
  {
    private static readonly CacheDict<Type, MethodInfo> s_lambdaDelegateCache = new CacheDict<Type, MethodInfo>(40);
    private static volatile CacheDict<Type, Func<Expression, string, bool, ReadOnlyCollection<ParameterExpression>, LambdaExpression>> s_lambdaFactories;
    private static ConditionalWeakTable<Expression, Expression.ExtensionInfo> s_legacyCtorSupportTable;

    public static BinaryExpression Assign(Expression left, Expression right)
    {
      Expression.RequiresCanWrite(left, nameof (left));
      ExpressionUtils.RequiresCanRead(right, nameof (right));
      TypeUtils.ValidateType(left.Type, nameof (left), true, true);
      TypeUtils.ValidateType(right.Type, nameof (right), true, true);
      if (!TypeUtils.AreReferenceAssignable(left.Type, right.Type))
        throw Error.ExpressionTypeDoesNotMatchAssignment((object) right.Type, (object) left.Type);
      return (BinaryExpression) new AssignBinaryExpression(left, right);
    }

    private static BinaryExpression GetUserDefinedBinaryOperator(
      ExpressionType binaryType,
      string name,
      Expression left,
      Expression right,
      bool liftToNull)
    {
      MethodInfo definedBinaryOperator1 = Expression.GetUserDefinedBinaryOperator(binaryType, left.Type, right.Type, name);
      if (definedBinaryOperator1 != (MethodInfo) null)
        return (BinaryExpression) new MethodBinaryExpression(binaryType, left, right, definedBinaryOperator1.ReturnType, definedBinaryOperator1);
      if (left.Type.IsNullableType() && right.Type.IsNullableType())
      {
        Type nonNullableType1 = left.Type.GetNonNullableType();
        Type nonNullableType2 = right.Type.GetNonNullableType();
        MethodInfo definedBinaryOperator2 = Expression.GetUserDefinedBinaryOperator(binaryType, nonNullableType1, nonNullableType2, name);
        if (definedBinaryOperator2 != (MethodInfo) null && definedBinaryOperator2.ReturnType.IsValueType && !definedBinaryOperator2.ReturnType.IsNullableType())
        {
          if (definedBinaryOperator2.ReturnType != typeof (bool) | liftToNull)
            return (BinaryExpression) new MethodBinaryExpression(binaryType, left, right, definedBinaryOperator2.ReturnType.GetNullableType(), definedBinaryOperator2);
          return (BinaryExpression) new MethodBinaryExpression(binaryType, left, right, typeof (bool), definedBinaryOperator2);
        }
      }
      return (BinaryExpression) null;
    }

    private static BinaryExpression GetMethodBasedBinaryOperator(
      ExpressionType binaryType,
      Expression left,
      Expression right,
      MethodInfo method,
      bool liftToNull)
    {
      Expression.ValidateOperator(method);
      ParameterInfo[] parametersCached = method.GetParametersCached();
      if (parametersCached.Length != 2)
        throw Error.IncorrectNumberOfMethodCallArguments((object) method, nameof (method));
      if (Expression.ParameterIsAssignable(parametersCached[0], left.Type) && Expression.ParameterIsAssignable(parametersCached[1], right.Type))
      {
        Expression.ValidateParamswithOperandsOrThrow(parametersCached[0].ParameterType, left.Type, binaryType, method.Name);
        Expression.ValidateParamswithOperandsOrThrow(parametersCached[1].ParameterType, right.Type, binaryType, method.Name);
        return (BinaryExpression) new MethodBinaryExpression(binaryType, left, right, method.ReturnType, method);
      }
      if (!left.Type.IsNullableType() || !right.Type.IsNullableType() || (!Expression.ParameterIsAssignable(parametersCached[0], left.Type.GetNonNullableType()) || !Expression.ParameterIsAssignable(parametersCached[1], right.Type.GetNonNullableType())) || (!method.ReturnType.IsValueType || method.ReturnType.IsNullableType()))
        throw Error.OperandTypesDoNotMatchParameters((object) binaryType, (object) method.Name);
      if (method.ReturnType != typeof (bool) | liftToNull)
        return (BinaryExpression) new MethodBinaryExpression(binaryType, left, right, method.ReturnType.GetNullableType(), method);
      return (BinaryExpression) new MethodBinaryExpression(binaryType, left, right, typeof (bool), method);
    }

    private static BinaryExpression GetMethodBasedAssignOperator(
      ExpressionType binaryType,
      Expression left,
      Expression right,
      MethodInfo method,
      LambdaExpression conversion,
      bool liftToNull)
    {
      BinaryExpression binaryExpression = Expression.GetMethodBasedBinaryOperator(binaryType, left, right, method, liftToNull);
      if (conversion == null)
      {
        if (!TypeUtils.AreReferenceAssignable(left.Type, binaryExpression.Type))
          throw Error.UserDefinedOpMustHaveValidReturnType((object) binaryType, (object) binaryExpression.Method.Name);
      }
      else
      {
        Expression.ValidateOpAssignConversionLambda(conversion, binaryExpression.Left, binaryExpression.Method, binaryExpression.NodeType);
        binaryExpression = (BinaryExpression) new OpAssignMethodConversionBinaryExpression(binaryExpression.NodeType, binaryExpression.Left, binaryExpression.Right, binaryExpression.Left.Type, binaryExpression.Method, conversion);
      }
      return binaryExpression;
    }

    private static BinaryExpression GetUserDefinedBinaryOperatorOrThrow(
      ExpressionType binaryType,
      string name,
      Expression left,
      Expression right,
      bool liftToNull)
    {
      BinaryExpression definedBinaryOperator = Expression.GetUserDefinedBinaryOperator(binaryType, name, left, right, liftToNull);
      if (definedBinaryOperator == null)
        throw Error.BinaryOperatorNotDefined((object) binaryType, (object) left.Type, (object) right.Type);
      ParameterInfo[] parametersCached = definedBinaryOperator.Method.GetParametersCached();
      Expression.ValidateParamswithOperandsOrThrow(parametersCached[0].ParameterType, left.Type, binaryType, name);
      Expression.ValidateParamswithOperandsOrThrow(parametersCached[1].ParameterType, right.Type, binaryType, name);
      return definedBinaryOperator;
    }

    private static BinaryExpression GetUserDefinedAssignOperatorOrThrow(
      ExpressionType binaryType,
      string name,
      Expression left,
      Expression right,
      LambdaExpression conversion,
      bool liftToNull)
    {
      BinaryExpression binaryExpression = Expression.GetUserDefinedBinaryOperatorOrThrow(binaryType, name, left, right, liftToNull);
      if (conversion == null)
      {
        if (!TypeUtils.AreReferenceAssignable(left.Type, binaryExpression.Type))
          throw Error.UserDefinedOpMustHaveValidReturnType((object) binaryType, (object) binaryExpression.Method.Name);
      }
      else
      {
        Expression.ValidateOpAssignConversionLambda(conversion, binaryExpression.Left, binaryExpression.Method, binaryExpression.NodeType);
        binaryExpression = (BinaryExpression) new OpAssignMethodConversionBinaryExpression(binaryExpression.NodeType, binaryExpression.Left, binaryExpression.Right, binaryExpression.Left.Type, binaryExpression.Method, conversion);
      }
      return binaryExpression;
    }

    private static MethodInfo GetUserDefinedBinaryOperator(
      ExpressionType binaryType,
      Type leftType,
      Type rightType,
      string name)
    {
      Type[] types = new Type[2]{ leftType, rightType };
      Type nonNullableType1 = leftType.GetNonNullableType();
      Type nonNullableType2 = rightType.GetNonNullableType();
      MethodInfo method = nonNullableType1.GetAnyStaticMethodValidated(name, types);
      if (method == (MethodInfo) null && !TypeUtils.AreEquivalent(leftType, rightType))
        method = nonNullableType2.GetAnyStaticMethodValidated(name, types);
      if (Expression.IsLiftingConditionalLogicalOperator(leftType, rightType, method, binaryType))
        method = Expression.GetUserDefinedBinaryOperator(binaryType, nonNullableType1, nonNullableType2, name);
      return method;
    }

    private static bool IsLiftingConditionalLogicalOperator(
      Type left,
      Type right,
      MethodInfo method,
      ExpressionType binaryType)
    {
      if (!right.IsNullableType() || !left.IsNullableType() || !(method == (MethodInfo) null))
        return false;
      if (binaryType != ExpressionType.AndAlso)
        return binaryType == ExpressionType.OrElse;
      return true;
    }

    internal static bool ParameterIsAssignable(ParameterInfo pi, Type argType)
    {
      Type dest = pi.ParameterType;
      if (dest.IsByRef)
        dest = dest.GetElementType();
      return TypeUtils.AreReferenceAssignable(dest, argType);
    }

    private static void ValidateParamswithOperandsOrThrow(
      Type paramType,
      Type operandType,
      ExpressionType exprType,
      string name)
    {
      if (paramType.IsNullableType() && !operandType.IsNullableType())
        throw Error.OperandTypesDoNotMatchParameters((object) exprType, (object) name);
    }

    private static void ValidateOperator(MethodInfo method)
    {
      Expression.ValidateMethodInfo(method, nameof (method));
      if (!method.IsStatic)
        throw Error.UserDefinedOperatorMustBeStatic((object) method, nameof (method));
      if (method.ReturnType == typeof (void))
        throw Error.UserDefinedOperatorMustNotBeVoid((object) method, nameof (method));
    }

    private static void ValidateMethodInfo(MethodInfo method, string paramName)
    {
      if (method.ContainsGenericParameters)
        throw method.IsGenericMethodDefinition ? Error.MethodIsGeneric((object) method, paramName) : Error.MethodContainsGenericParameters((object) method, paramName);
    }

    private static bool IsNullComparison(Expression left, Expression right)
    {
      if (!Expression.IsNullConstant(left))
      {
        if (Expression.IsNullConstant(right))
          return left.Type.IsNullableType();
        return false;
      }
      if (!Expression.IsNullConstant(right))
        return right.Type.IsNullableType();
      return false;
    }

    private static bool IsNullConstant(Expression e)
    {
      ConstantExpression constantExpression = e as ConstantExpression;
      if (constantExpression != null)
        return constantExpression.Value == null;
      return false;
    }

    private static void ValidateUserDefinedConditionalLogicOperator(
      ExpressionType nodeType,
      Type left,
      Type right,
      MethodInfo method)
    {
      Expression.ValidateOperator(method);
      ParameterInfo[] parametersCached = method.GetParametersCached();
      if (parametersCached.Length != 2)
        throw Error.IncorrectNumberOfMethodCallArguments((object) method, nameof (method));
      if (!Expression.ParameterIsAssignable(parametersCached[0], left) && (!left.IsNullableType() || !Expression.ParameterIsAssignable(parametersCached[0], left.GetNonNullableType())))
        throw Error.OperandTypesDoNotMatchParameters((object) nodeType, (object) method.Name);
      if (!Expression.ParameterIsAssignable(parametersCached[1], right) && (!right.IsNullableType() || !Expression.ParameterIsAssignable(parametersCached[1], right.GetNonNullableType())))
        throw Error.OperandTypesDoNotMatchParameters((object) nodeType, (object) method.Name);
      if (parametersCached[0].ParameterType != parametersCached[1].ParameterType)
        throw Error.UserDefinedOpMustHaveConsistentTypes((object) nodeType, (object) method.Name);
      if (method.ReturnType != parametersCached[0].ParameterType)
        throw Error.UserDefinedOpMustHaveConsistentTypes((object) nodeType, (object) method.Name);
      if (Expression.IsValidLiftedConditionalLogicalOperator(left, right, parametersCached))
        left = left.GetNonNullableType();
      Type declaringType = method.DeclaringType;
      if (declaringType == (Type) null)
        throw Error.LogicalOperatorMustHaveBooleanOperators((object) nodeType, (object) method.Name);
      MethodInfo booleanOperator1 = TypeUtils.GetBooleanOperator(declaringType, "op_True");
      MethodInfo booleanOperator2 = TypeUtils.GetBooleanOperator(declaringType, "op_False");
      if (booleanOperator1 == (MethodInfo) null || booleanOperator1.ReturnType != typeof (bool) || (booleanOperator2 == (MethodInfo) null || booleanOperator2.ReturnType != typeof (bool)))
        throw Error.LogicalOperatorMustHaveBooleanOperators((object) nodeType, (object) method.Name);
      Expression.VerifyOpTrueFalse(nodeType, left, booleanOperator2, nameof (method));
      Expression.VerifyOpTrueFalse(nodeType, left, booleanOperator1, nameof (method));
    }

    private static void VerifyOpTrueFalse(
      ExpressionType nodeType,
      Type left,
      MethodInfo opTrue,
      string paramName)
    {
      ParameterInfo[] parametersCached = opTrue.GetParametersCached();
      if (parametersCached.Length != 1)
        throw Error.IncorrectNumberOfMethodCallArguments((object) opTrue, paramName);
      if (!Expression.ParameterIsAssignable(parametersCached[0], left) && (!left.IsNullableType() || !Expression.ParameterIsAssignable(parametersCached[0], left.GetNonNullableType())))
        throw Error.OperandTypesDoNotMatchParameters((object) nodeType, (object) opTrue.Name);
    }

    private static bool IsValidLiftedConditionalLogicalOperator(
      Type left,
      Type right,
      ParameterInfo[] pms)
    {
      if (TypeUtils.AreEquivalent(left, right) && right.IsNullableType())
        return TypeUtils.AreEquivalent(pms[1].ParameterType, right.GetNonNullableType());
      return false;
    }

    public static BinaryExpression MakeBinary(
      ExpressionType binaryType,
      Expression left,
      Expression right)
    {
      return Expression.MakeBinary(binaryType, left, right, false, (MethodInfo) null, (LambdaExpression) null);
    }

    public static BinaryExpression MakeBinary(
      ExpressionType binaryType,
      Expression left,
      Expression right,
      bool liftToNull,
      MethodInfo method)
    {
      return Expression.MakeBinary(binaryType, left, right, liftToNull, method, (LambdaExpression) null);
    }

    public static BinaryExpression MakeBinary(
      ExpressionType binaryType,
      Expression left,
      Expression right,
      bool liftToNull,
      MethodInfo method,
      LambdaExpression conversion)
    {
      switch (binaryType)
      {
        case ExpressionType.Add:
          return Expression.Add(left, right, method);
        case ExpressionType.AddChecked:
          return Expression.AddChecked(left, right, method);
        case ExpressionType.And:
          return Expression.And(left, right, method);
        case ExpressionType.AndAlso:
          return Expression.AndAlso(left, right, method);
        case ExpressionType.ArrayIndex:
          return Expression.ArrayIndex(left, right);
        case ExpressionType.Coalesce:
          return Expression.Coalesce(left, right, conversion);
        case ExpressionType.Divide:
          return Expression.Divide(left, right, method);
        case ExpressionType.Equal:
          return Expression.Equal(left, right, liftToNull, method);
        case ExpressionType.ExclusiveOr:
          return Expression.ExclusiveOr(left, right, method);
        case ExpressionType.GreaterThan:
          return Expression.GreaterThan(left, right, liftToNull, method);
        case ExpressionType.GreaterThanOrEqual:
          return Expression.GreaterThanOrEqual(left, right, liftToNull, method);
        case ExpressionType.LeftShift:
          return Expression.LeftShift(left, right, method);
        case ExpressionType.LessThan:
          return Expression.LessThan(left, right, liftToNull, method);
        case ExpressionType.LessThanOrEqual:
          return Expression.LessThanOrEqual(left, right, liftToNull, method);
        case ExpressionType.Modulo:
          return Expression.Modulo(left, right, method);
        case ExpressionType.Multiply:
          return Expression.Multiply(left, right, method);
        case ExpressionType.MultiplyChecked:
          return Expression.MultiplyChecked(left, right, method);
        case ExpressionType.NotEqual:
          return Expression.NotEqual(left, right, liftToNull, method);
        case ExpressionType.Or:
          return Expression.Or(left, right, method);
        case ExpressionType.OrElse:
          return Expression.OrElse(left, right, method);
        case ExpressionType.Power:
          return Expression.Power(left, right, method);
        case ExpressionType.RightShift:
          return Expression.RightShift(left, right, method);
        case ExpressionType.Subtract:
          return Expression.Subtract(left, right, method);
        case ExpressionType.SubtractChecked:
          return Expression.SubtractChecked(left, right, method);
        case ExpressionType.Assign:
          return Expression.Assign(left, right);
        case ExpressionType.AddAssign:
          return Expression.AddAssign(left, right, method, conversion);
        case ExpressionType.AndAssign:
          return Expression.AndAssign(left, right, method, conversion);
        case ExpressionType.DivideAssign:
          return Expression.DivideAssign(left, right, method, conversion);
        case ExpressionType.ExclusiveOrAssign:
          return Expression.ExclusiveOrAssign(left, right, method, conversion);
        case ExpressionType.LeftShiftAssign:
          return Expression.LeftShiftAssign(left, right, method, conversion);
        case ExpressionType.ModuloAssign:
          return Expression.ModuloAssign(left, right, method, conversion);
        case ExpressionType.MultiplyAssign:
          return Expression.MultiplyAssign(left, right, method, conversion);
        case ExpressionType.OrAssign:
          return Expression.OrAssign(left, right, method, conversion);
        case ExpressionType.PowerAssign:
          return Expression.PowerAssign(left, right, method, conversion);
        case ExpressionType.RightShiftAssign:
          return Expression.RightShiftAssign(left, right, method, conversion);
        case ExpressionType.SubtractAssign:
          return Expression.SubtractAssign(left, right, method, conversion);
        case ExpressionType.AddAssignChecked:
          return Expression.AddAssignChecked(left, right, method, conversion);
        case ExpressionType.MultiplyAssignChecked:
          return Expression.MultiplyAssignChecked(left, right, method, conversion);
        case ExpressionType.SubtractAssignChecked:
          return Expression.SubtractAssignChecked(left, right, method, conversion);
        default:
          throw Error.UnhandledBinary((object) binaryType, nameof (binaryType));
      }
    }

    public static BinaryExpression Equal(Expression left, Expression right)
    {
      return Expression.Equal(left, right, false, (MethodInfo) null);
    }

    public static BinaryExpression Equal(
      Expression left,
      Expression right,
      bool liftToNull,
      MethodInfo method)
    {
      ExpressionUtils.RequiresCanRead(left, nameof (left));
      ExpressionUtils.RequiresCanRead(right, nameof (right));
      if (method == (MethodInfo) null)
        return Expression.GetEqualityComparisonOperator(ExpressionType.Equal, "op_Equality", left, right, liftToNull);
      return Expression.GetMethodBasedBinaryOperator(ExpressionType.Equal, left, right, method, liftToNull);
    }

    public static BinaryExpression ReferenceEqual(Expression left, Expression right)
    {
      ExpressionUtils.RequiresCanRead(left, nameof (left));
      ExpressionUtils.RequiresCanRead(right, nameof (right));
      if (TypeUtils.HasReferenceEquality(left.Type, right.Type))
        return (BinaryExpression) new LogicalBinaryExpression(ExpressionType.Equal, left, right);
      throw Error.ReferenceEqualityNotDefined((object) left.Type, (object) right.Type);
    }

    public static BinaryExpression NotEqual(Expression left, Expression right)
    {
      return Expression.NotEqual(left, right, false, (MethodInfo) null);
    }

    public static BinaryExpression NotEqual(
      Expression left,
      Expression right,
      bool liftToNull,
      MethodInfo method)
    {
      ExpressionUtils.RequiresCanRead(left, nameof (left));
      ExpressionUtils.RequiresCanRead(right, nameof (right));
      if (method == (MethodInfo) null)
        return Expression.GetEqualityComparisonOperator(ExpressionType.NotEqual, "op_Inequality", left, right, liftToNull);
      return Expression.GetMethodBasedBinaryOperator(ExpressionType.NotEqual, left, right, method, liftToNull);
    }

    public static BinaryExpression ReferenceNotEqual(
      Expression left,
      Expression right)
    {
      ExpressionUtils.RequiresCanRead(left, nameof (left));
      ExpressionUtils.RequiresCanRead(right, nameof (right));
      if (TypeUtils.HasReferenceEquality(left.Type, right.Type))
        return (BinaryExpression) new LogicalBinaryExpression(ExpressionType.NotEqual, left, right);
      throw Error.ReferenceEqualityNotDefined((object) left.Type, (object) right.Type);
    }

    private static BinaryExpression GetEqualityComparisonOperator(
      ExpressionType binaryType,
      string opName,
      Expression left,
      Expression right,
      bool liftToNull)
    {
      if (left.Type == right.Type && (left.Type.IsNumeric() || left.Type == typeof (object) || (left.Type.IsBool() || left.Type.GetNonNullableType().IsEnum)))
      {
        if (left.Type.IsNullableType() & liftToNull)
          return (BinaryExpression) new SimpleBinaryExpression(binaryType, left, right, typeof (bool?));
        return (BinaryExpression) new LogicalBinaryExpression(binaryType, left, right);
      }
      BinaryExpression definedBinaryOperator = Expression.GetUserDefinedBinaryOperator(binaryType, opName, left, right, liftToNull);
      if (definedBinaryOperator != null)
        return definedBinaryOperator;
      if (!TypeUtils.HasBuiltInEqualityOperator(left.Type, right.Type) && !Expression.IsNullComparison(left, right))
        throw Error.BinaryOperatorNotDefined((object) binaryType, (object) left.Type, (object) right.Type);
      if (left.Type.IsNullableType() & liftToNull)
        return (BinaryExpression) new SimpleBinaryExpression(binaryType, left, right, typeof (bool?));
      return (BinaryExpression) new LogicalBinaryExpression(binaryType, left, right);
    }

    public static BinaryExpression GreaterThan(Expression left, Expression right)
    {
      return Expression.GreaterThan(left, right, false, (MethodInfo) null);
    }

    public static BinaryExpression GreaterThan(
      Expression left,
      Expression right,
      bool liftToNull,
      MethodInfo method)
    {
      ExpressionUtils.RequiresCanRead(left, nameof (left));
      ExpressionUtils.RequiresCanRead(right, nameof (right));
      if (method == (MethodInfo) null)
        return Expression.GetComparisonOperator(ExpressionType.GreaterThan, "op_GreaterThan", left, right, liftToNull);
      return Expression.GetMethodBasedBinaryOperator(ExpressionType.GreaterThan, left, right, method, liftToNull);
    }

    public static BinaryExpression LessThan(Expression left, Expression right)
    {
      return Expression.LessThan(left, right, false, (MethodInfo) null);
    }

    public static BinaryExpression LessThan(
      Expression left,
      Expression right,
      bool liftToNull,
      MethodInfo method)
    {
      ExpressionUtils.RequiresCanRead(left, nameof (left));
      ExpressionUtils.RequiresCanRead(right, nameof (right));
      if (method == (MethodInfo) null)
        return Expression.GetComparisonOperator(ExpressionType.LessThan, "op_LessThan", left, right, liftToNull);
      return Expression.GetMethodBasedBinaryOperator(ExpressionType.LessThan, left, right, method, liftToNull);
    }

    public static BinaryExpression GreaterThanOrEqual(
      Expression left,
      Expression right)
    {
      return Expression.GreaterThanOrEqual(left, right, false, (MethodInfo) null);
    }

    public static BinaryExpression GreaterThanOrEqual(
      Expression left,
      Expression right,
      bool liftToNull,
      MethodInfo method)
    {
      ExpressionUtils.RequiresCanRead(left, nameof (left));
      ExpressionUtils.RequiresCanRead(right, nameof (right));
      if (method == (MethodInfo) null)
        return Expression.GetComparisonOperator(ExpressionType.GreaterThanOrEqual, "op_GreaterThanOrEqual", left, right, liftToNull);
      return Expression.GetMethodBasedBinaryOperator(ExpressionType.GreaterThanOrEqual, left, right, method, liftToNull);
    }

    public static BinaryExpression LessThanOrEqual(
      Expression left,
      Expression right)
    {
      return Expression.LessThanOrEqual(left, right, false, (MethodInfo) null);
    }

    public static BinaryExpression LessThanOrEqual(
      Expression left,
      Expression right,
      bool liftToNull,
      MethodInfo method)
    {
      ExpressionUtils.RequiresCanRead(left, nameof (left));
      ExpressionUtils.RequiresCanRead(right, nameof (right));
      if (method == (MethodInfo) null)
        return Expression.GetComparisonOperator(ExpressionType.LessThanOrEqual, "op_LessThanOrEqual", left, right, liftToNull);
      return Expression.GetMethodBasedBinaryOperator(ExpressionType.LessThanOrEqual, left, right, method, liftToNull);
    }

    private static BinaryExpression GetComparisonOperator(
      ExpressionType binaryType,
      string opName,
      Expression left,
      Expression right,
      bool liftToNull)
    {
      if (!(left.Type == right.Type) || !left.Type.IsNumeric())
        return Expression.GetUserDefinedBinaryOperatorOrThrow(binaryType, opName, left, right, liftToNull);
      if (left.Type.IsNullableType() & liftToNull)
        return (BinaryExpression) new SimpleBinaryExpression(binaryType, left, right, typeof (bool?));
      return (BinaryExpression) new LogicalBinaryExpression(binaryType, left, right);
    }

    public static BinaryExpression AndAlso(Expression left, Expression right)
    {
      return Expression.AndAlso(left, right, (MethodInfo) null);
    }

    public static BinaryExpression AndAlso(
      Expression left,
      Expression right,
      MethodInfo method)
    {
      ExpressionUtils.RequiresCanRead(left, nameof (left));
      ExpressionUtils.RequiresCanRead(right, nameof (right));
      if (method == (MethodInfo) null)
      {
        if (left.Type == right.Type)
        {
          if (left.Type == typeof (bool))
            return (BinaryExpression) new LogicalBinaryExpression(ExpressionType.AndAlso, left, right);
          if (left.Type == typeof (bool?))
            return (BinaryExpression) new SimpleBinaryExpression(ExpressionType.AndAlso, left, right, left.Type);
        }
        method = Expression.GetUserDefinedBinaryOperator(ExpressionType.AndAlso, left.Type, right.Type, "op_BitwiseAnd");
        if (!(method != (MethodInfo) null))
          throw Error.BinaryOperatorNotDefined((object) ExpressionType.AndAlso, (object) left.Type, (object) right.Type);
        Expression.ValidateUserDefinedConditionalLogicOperator(ExpressionType.AndAlso, left.Type, right.Type, method);
        Type type = !left.Type.IsNullableType() || !TypeUtils.AreEquivalent(method.ReturnType, left.Type.GetNonNullableType()) ? method.ReturnType : left.Type;
        return (BinaryExpression) new MethodBinaryExpression(ExpressionType.AndAlso, left, right, type, method);
      }
      Expression.ValidateUserDefinedConditionalLogicOperator(ExpressionType.AndAlso, left.Type, right.Type, method);
      Type type1 = !left.Type.IsNullableType() || !TypeUtils.AreEquivalent(method.ReturnType, left.Type.GetNonNullableType()) ? method.ReturnType : left.Type;
      return (BinaryExpression) new MethodBinaryExpression(ExpressionType.AndAlso, left, right, type1, method);
    }

    public static BinaryExpression OrElse(Expression left, Expression right)
    {
      return Expression.OrElse(left, right, (MethodInfo) null);
    }

    public static BinaryExpression OrElse(
      Expression left,
      Expression right,
      MethodInfo method)
    {
      ExpressionUtils.RequiresCanRead(left, nameof (left));
      ExpressionUtils.RequiresCanRead(right, nameof (right));
      if (method == (MethodInfo) null)
      {
        if (left.Type == right.Type)
        {
          if (left.Type == typeof (bool))
            return (BinaryExpression) new LogicalBinaryExpression(ExpressionType.OrElse, left, right);
          if (left.Type == typeof (bool?))
            return (BinaryExpression) new SimpleBinaryExpression(ExpressionType.OrElse, left, right, left.Type);
        }
        method = Expression.GetUserDefinedBinaryOperator(ExpressionType.OrElse, left.Type, right.Type, "op_BitwiseOr");
        if (!(method != (MethodInfo) null))
          throw Error.BinaryOperatorNotDefined((object) ExpressionType.OrElse, (object) left.Type, (object) right.Type);
        Expression.ValidateUserDefinedConditionalLogicOperator(ExpressionType.OrElse, left.Type, right.Type, method);
        Type type = !left.Type.IsNullableType() || !(method.ReturnType == left.Type.GetNonNullableType()) ? method.ReturnType : left.Type;
        return (BinaryExpression) new MethodBinaryExpression(ExpressionType.OrElse, left, right, type, method);
      }
      Expression.ValidateUserDefinedConditionalLogicOperator(ExpressionType.OrElse, left.Type, right.Type, method);
      Type type1 = !left.Type.IsNullableType() || !(method.ReturnType == left.Type.GetNonNullableType()) ? method.ReturnType : left.Type;
      return (BinaryExpression) new MethodBinaryExpression(ExpressionType.OrElse, left, right, type1, method);
    }

    public static BinaryExpression Coalesce(Expression left, Expression right)
    {
      return Expression.Coalesce(left, right, (LambdaExpression) null);
    }

    public static BinaryExpression Coalesce(
      Expression left,
      Expression right,
      LambdaExpression conversion)
    {
      ExpressionUtils.RequiresCanRead(left, nameof (left));
      ExpressionUtils.RequiresCanRead(right, nameof (right));
      if (conversion == null)
      {
        Type type = Expression.ValidateCoalesceArgTypes(left.Type, right.Type);
        return (BinaryExpression) new SimpleBinaryExpression(ExpressionType.Coalesce, left, right, type);
      }
      if (left.Type.IsValueType && !left.Type.IsNullableType())
        throw Error.CoalesceUsedOnNonNullType();
      MethodInfo invokeMethod = conversion.Type.GetInvokeMethod();
      if (invokeMethod.ReturnType == typeof (void))
        throw Error.UserDefinedOperatorMustNotBeVoid((object) conversion, nameof (conversion));
      ParameterInfo[] parametersCached = invokeMethod.GetParametersCached();
      if (parametersCached.Length != 1)
        throw Error.IncorrectNumberOfMethodCallArguments((object) conversion, nameof (conversion));
      if (!TypeUtils.AreEquivalent(invokeMethod.ReturnType, right.Type))
        throw Error.OperandTypesDoNotMatchParameters((object) ExpressionType.Coalesce, (object) conversion.ToString());
      if (!Expression.ParameterIsAssignable(parametersCached[0], left.Type.GetNonNullableType()) && !Expression.ParameterIsAssignable(parametersCached[0], left.Type))
        throw Error.OperandTypesDoNotMatchParameters((object) ExpressionType.Coalesce, (object) conversion.ToString());
      return (BinaryExpression) new CoalesceConversionBinaryExpression(left, right, conversion);
    }

    private static Type ValidateCoalesceArgTypes(Type left, Type right)
    {
      Type nonNullableType = left.GetNonNullableType();
      if (left.IsValueType && !left.IsNullableType())
        throw Error.CoalesceUsedOnNonNullType();
      if (left.IsNullableType() && right.IsImplicitlyConvertibleTo(nonNullableType))
        return nonNullableType;
      if (right.IsImplicitlyConvertibleTo(left))
        return left;
      if (nonNullableType.IsImplicitlyConvertibleTo(right))
        return right;
      throw Error.ArgumentTypesMustMatch();
    }

    public static BinaryExpression Add(Expression left, Expression right)
    {
      return Expression.Add(left, right, (MethodInfo) null);
    }

    public static BinaryExpression Add(
      Expression left,
      Expression right,
      MethodInfo method)
    {
      ExpressionUtils.RequiresCanRead(left, nameof (left));
      ExpressionUtils.RequiresCanRead(right, nameof (right));
      if (!(method == (MethodInfo) null))
        return Expression.GetMethodBasedBinaryOperator(ExpressionType.Add, left, right, method, true);
      if (left.Type == right.Type && left.Type.IsArithmetic())
        return (BinaryExpression) new SimpleBinaryExpression(ExpressionType.Add, left, right, left.Type);
      return Expression.GetUserDefinedBinaryOperatorOrThrow(ExpressionType.Add, "op_Addition", left, right, true);
    }

    public static BinaryExpression AddAssign(Expression left, Expression right)
    {
      return Expression.AddAssign(left, right, (MethodInfo) null, (LambdaExpression) null);
    }

    public static BinaryExpression AddAssign(
      Expression left,
      Expression right,
      MethodInfo method)
    {
      return Expression.AddAssign(left, right, method, (LambdaExpression) null);
    }

    public static BinaryExpression AddAssign(
      Expression left,
      Expression right,
      MethodInfo method,
      LambdaExpression conversion)
    {
      ExpressionUtils.RequiresCanRead(left, nameof (left));
      Expression.RequiresCanWrite(left, nameof (left));
      ExpressionUtils.RequiresCanRead(right, nameof (right));
      if (!(method == (MethodInfo) null))
        return Expression.GetMethodBasedAssignOperator(ExpressionType.AddAssign, left, right, method, conversion, true);
      if (!(left.Type == right.Type) || !left.Type.IsArithmetic())
        return Expression.GetUserDefinedAssignOperatorOrThrow(ExpressionType.AddAssign, "op_Addition", left, right, conversion, true);
      if (conversion != null)
        throw Error.ConversionIsNotSupportedForArithmeticTypes();
      return (BinaryExpression) new SimpleBinaryExpression(ExpressionType.AddAssign, left, right, left.Type);
    }

    private static void ValidateOpAssignConversionLambda(
      LambdaExpression conversion,
      Expression left,
      MethodInfo method,
      ExpressionType nodeType)
    {
      MethodInfo invokeMethod = conversion.Type.GetInvokeMethod();
      ParameterInfo[] parametersCached = invokeMethod.GetParametersCached();
      if (parametersCached.Length != 1)
        throw Error.IncorrectNumberOfMethodCallArguments((object) conversion, nameof (conversion));
      if (!TypeUtils.AreEquivalent(invokeMethod.ReturnType, left.Type))
        throw Error.OperandTypesDoNotMatchParameters((object) nodeType, (object) conversion.ToString());
      if (!TypeUtils.AreEquivalent(parametersCached[0].ParameterType, method.ReturnType))
        throw Error.OverloadOperatorTypeDoesNotMatchConversionType((object) nodeType, (object) conversion.ToString());
    }

    public static BinaryExpression AddAssignChecked(
      Expression left,
      Expression right)
    {
      return Expression.AddAssignChecked(left, right, (MethodInfo) null);
    }

    public static BinaryExpression AddAssignChecked(
      Expression left,
      Expression right,
      MethodInfo method)
    {
      return Expression.AddAssignChecked(left, right, method, (LambdaExpression) null);
    }

    public static BinaryExpression AddAssignChecked(
      Expression left,
      Expression right,
      MethodInfo method,
      LambdaExpression conversion)
    {
      ExpressionUtils.RequiresCanRead(left, nameof (left));
      Expression.RequiresCanWrite(left, nameof (left));
      ExpressionUtils.RequiresCanRead(right, nameof (right));
      if (!(method == (MethodInfo) null))
        return Expression.GetMethodBasedAssignOperator(ExpressionType.AddAssignChecked, left, right, method, conversion, true);
      if (!(left.Type == right.Type) || !left.Type.IsArithmetic())
        return Expression.GetUserDefinedAssignOperatorOrThrow(ExpressionType.AddAssignChecked, "op_Addition", left, right, conversion, true);
      if (conversion != null)
        throw Error.ConversionIsNotSupportedForArithmeticTypes();
      return (BinaryExpression) new SimpleBinaryExpression(ExpressionType.AddAssignChecked, left, right, left.Type);
    }

    public static BinaryExpression AddChecked(Expression left, Expression right)
    {
      return Expression.AddChecked(left, right, (MethodInfo) null);
    }

    public static BinaryExpression AddChecked(
      Expression left,
      Expression right,
      MethodInfo method)
    {
      ExpressionUtils.RequiresCanRead(left, nameof (left));
      ExpressionUtils.RequiresCanRead(right, nameof (right));
      if (!(method == (MethodInfo) null))
        return Expression.GetMethodBasedBinaryOperator(ExpressionType.AddChecked, left, right, method, true);
      if (left.Type == right.Type && left.Type.IsArithmetic())
        return (BinaryExpression) new SimpleBinaryExpression(ExpressionType.AddChecked, left, right, left.Type);
      return Expression.GetUserDefinedBinaryOperatorOrThrow(ExpressionType.AddChecked, "op_Addition", left, right, true);
    }

    public static BinaryExpression Subtract(Expression left, Expression right)
    {
      return Expression.Subtract(left, right, (MethodInfo) null);
    }

    public static BinaryExpression Subtract(
      Expression left,
      Expression right,
      MethodInfo method)
    {
      ExpressionUtils.RequiresCanRead(left, nameof (left));
      ExpressionUtils.RequiresCanRead(right, nameof (right));
      if (!(method == (MethodInfo) null))
        return Expression.GetMethodBasedBinaryOperator(ExpressionType.Subtract, left, right, method, true);
      if (left.Type == right.Type && left.Type.IsArithmetic())
        return (BinaryExpression) new SimpleBinaryExpression(ExpressionType.Subtract, left, right, left.Type);
      return Expression.GetUserDefinedBinaryOperatorOrThrow(ExpressionType.Subtract, "op_Subtraction", left, right, true);
    }

    public static BinaryExpression SubtractAssign(Expression left, Expression right)
    {
      return Expression.SubtractAssign(left, right, (MethodInfo) null, (LambdaExpression) null);
    }

    public static BinaryExpression SubtractAssign(
      Expression left,
      Expression right,
      MethodInfo method)
    {
      return Expression.SubtractAssign(left, right, method, (LambdaExpression) null);
    }

    public static BinaryExpression SubtractAssign(
      Expression left,
      Expression right,
      MethodInfo method,
      LambdaExpression conversion)
    {
      ExpressionUtils.RequiresCanRead(left, nameof (left));
      Expression.RequiresCanWrite(left, nameof (left));
      ExpressionUtils.RequiresCanRead(right, nameof (right));
      if (!(method == (MethodInfo) null))
        return Expression.GetMethodBasedAssignOperator(ExpressionType.SubtractAssign, left, right, method, conversion, true);
      if (!(left.Type == right.Type) || !left.Type.IsArithmetic())
        return Expression.GetUserDefinedAssignOperatorOrThrow(ExpressionType.SubtractAssign, "op_Subtraction", left, right, conversion, true);
      if (conversion != null)
        throw Error.ConversionIsNotSupportedForArithmeticTypes();
      return (BinaryExpression) new SimpleBinaryExpression(ExpressionType.SubtractAssign, left, right, left.Type);
    }

    public static BinaryExpression SubtractAssignChecked(
      Expression left,
      Expression right)
    {
      return Expression.SubtractAssignChecked(left, right, (MethodInfo) null);
    }

    public static BinaryExpression SubtractAssignChecked(
      Expression left,
      Expression right,
      MethodInfo method)
    {
      return Expression.SubtractAssignChecked(left, right, method, (LambdaExpression) null);
    }

    public static BinaryExpression SubtractAssignChecked(
      Expression left,
      Expression right,
      MethodInfo method,
      LambdaExpression conversion)
    {
      ExpressionUtils.RequiresCanRead(left, nameof (left));
      Expression.RequiresCanWrite(left, nameof (left));
      ExpressionUtils.RequiresCanRead(right, nameof (right));
      if (!(method == (MethodInfo) null))
        return Expression.GetMethodBasedAssignOperator(ExpressionType.SubtractAssignChecked, left, right, method, conversion, true);
      if (!(left.Type == right.Type) || !left.Type.IsArithmetic())
        return Expression.GetUserDefinedAssignOperatorOrThrow(ExpressionType.SubtractAssignChecked, "op_Subtraction", left, right, conversion, true);
      if (conversion != null)
        throw Error.ConversionIsNotSupportedForArithmeticTypes();
      return (BinaryExpression) new SimpleBinaryExpression(ExpressionType.SubtractAssignChecked, left, right, left.Type);
    }

    public static BinaryExpression SubtractChecked(
      Expression left,
      Expression right)
    {
      return Expression.SubtractChecked(left, right, (MethodInfo) null);
    }

    public static BinaryExpression SubtractChecked(
      Expression left,
      Expression right,
      MethodInfo method)
    {
      ExpressionUtils.RequiresCanRead(left, nameof (left));
      ExpressionUtils.RequiresCanRead(right, nameof (right));
      if (!(method == (MethodInfo) null))
        return Expression.GetMethodBasedBinaryOperator(ExpressionType.SubtractChecked, left, right, method, true);
      if (left.Type == right.Type && left.Type.IsArithmetic())
        return (BinaryExpression) new SimpleBinaryExpression(ExpressionType.SubtractChecked, left, right, left.Type);
      return Expression.GetUserDefinedBinaryOperatorOrThrow(ExpressionType.SubtractChecked, "op_Subtraction", left, right, true);
    }

    public static BinaryExpression Divide(Expression left, Expression right)
    {
      return Expression.Divide(left, right, (MethodInfo) null);
    }

    public static BinaryExpression Divide(
      Expression left,
      Expression right,
      MethodInfo method)
    {
      ExpressionUtils.RequiresCanRead(left, nameof (left));
      ExpressionUtils.RequiresCanRead(right, nameof (right));
      if (!(method == (MethodInfo) null))
        return Expression.GetMethodBasedBinaryOperator(ExpressionType.Divide, left, right, method, true);
      if (left.Type == right.Type && left.Type.IsArithmetic())
        return (BinaryExpression) new SimpleBinaryExpression(ExpressionType.Divide, left, right, left.Type);
      return Expression.GetUserDefinedBinaryOperatorOrThrow(ExpressionType.Divide, "op_Division", left, right, true);
    }

    public static BinaryExpression DivideAssign(Expression left, Expression right)
    {
      return Expression.DivideAssign(left, right, (MethodInfo) null, (LambdaExpression) null);
    }

    public static BinaryExpression DivideAssign(
      Expression left,
      Expression right,
      MethodInfo method)
    {
      return Expression.DivideAssign(left, right, method, (LambdaExpression) null);
    }

    public static BinaryExpression DivideAssign(
      Expression left,
      Expression right,
      MethodInfo method,
      LambdaExpression conversion)
    {
      ExpressionUtils.RequiresCanRead(left, nameof (left));
      Expression.RequiresCanWrite(left, nameof (left));
      ExpressionUtils.RequiresCanRead(right, nameof (right));
      if (!(method == (MethodInfo) null))
        return Expression.GetMethodBasedAssignOperator(ExpressionType.DivideAssign, left, right, method, conversion, true);
      if (!(left.Type == right.Type) || !left.Type.IsArithmetic())
        return Expression.GetUserDefinedAssignOperatorOrThrow(ExpressionType.DivideAssign, "op_Division", left, right, conversion, true);
      if (conversion != null)
        throw Error.ConversionIsNotSupportedForArithmeticTypes();
      return (BinaryExpression) new SimpleBinaryExpression(ExpressionType.DivideAssign, left, right, left.Type);
    }

    public static BinaryExpression Modulo(Expression left, Expression right)
    {
      return Expression.Modulo(left, right, (MethodInfo) null);
    }

    public static BinaryExpression Modulo(
      Expression left,
      Expression right,
      MethodInfo method)
    {
      ExpressionUtils.RequiresCanRead(left, nameof (left));
      ExpressionUtils.RequiresCanRead(right, nameof (right));
      if (!(method == (MethodInfo) null))
        return Expression.GetMethodBasedBinaryOperator(ExpressionType.Modulo, left, right, method, true);
      if (left.Type == right.Type && left.Type.IsArithmetic())
        return (BinaryExpression) new SimpleBinaryExpression(ExpressionType.Modulo, left, right, left.Type);
      return Expression.GetUserDefinedBinaryOperatorOrThrow(ExpressionType.Modulo, "op_Modulus", left, right, true);
    }

    public static BinaryExpression ModuloAssign(Expression left, Expression right)
    {
      return Expression.ModuloAssign(left, right, (MethodInfo) null, (LambdaExpression) null);
    }

    public static BinaryExpression ModuloAssign(
      Expression left,
      Expression right,
      MethodInfo method)
    {
      return Expression.ModuloAssign(left, right, method, (LambdaExpression) null);
    }

    public static BinaryExpression ModuloAssign(
      Expression left,
      Expression right,
      MethodInfo method,
      LambdaExpression conversion)
    {
      ExpressionUtils.RequiresCanRead(left, nameof (left));
      Expression.RequiresCanWrite(left, nameof (left));
      ExpressionUtils.RequiresCanRead(right, nameof (right));
      if (!(method == (MethodInfo) null))
        return Expression.GetMethodBasedAssignOperator(ExpressionType.ModuloAssign, left, right, method, conversion, true);
      if (!(left.Type == right.Type) || !left.Type.IsArithmetic())
        return Expression.GetUserDefinedAssignOperatorOrThrow(ExpressionType.ModuloAssign, "op_Modulus", left, right, conversion, true);
      if (conversion != null)
        throw Error.ConversionIsNotSupportedForArithmeticTypes();
      return (BinaryExpression) new SimpleBinaryExpression(ExpressionType.ModuloAssign, left, right, left.Type);
    }

    public static BinaryExpression Multiply(Expression left, Expression right)
    {
      return Expression.Multiply(left, right, (MethodInfo) null);
    }

    public static BinaryExpression Multiply(
      Expression left,
      Expression right,
      MethodInfo method)
    {
      ExpressionUtils.RequiresCanRead(left, nameof (left));
      ExpressionUtils.RequiresCanRead(right, nameof (right));
      if (!(method == (MethodInfo) null))
        return Expression.GetMethodBasedBinaryOperator(ExpressionType.Multiply, left, right, method, true);
      if (left.Type == right.Type && left.Type.IsArithmetic())
        return (BinaryExpression) new SimpleBinaryExpression(ExpressionType.Multiply, left, right, left.Type);
      return Expression.GetUserDefinedBinaryOperatorOrThrow(ExpressionType.Multiply, "op_Multiply", left, right, true);
    }

    public static BinaryExpression MultiplyAssign(Expression left, Expression right)
    {
      return Expression.MultiplyAssign(left, right, (MethodInfo) null, (LambdaExpression) null);
    }

    public static BinaryExpression MultiplyAssign(
      Expression left,
      Expression right,
      MethodInfo method)
    {
      return Expression.MultiplyAssign(left, right, method, (LambdaExpression) null);
    }

    public static BinaryExpression MultiplyAssign(
      Expression left,
      Expression right,
      MethodInfo method,
      LambdaExpression conversion)
    {
      ExpressionUtils.RequiresCanRead(left, nameof (left));
      Expression.RequiresCanWrite(left, nameof (left));
      ExpressionUtils.RequiresCanRead(right, nameof (right));
      if (!(method == (MethodInfo) null))
        return Expression.GetMethodBasedAssignOperator(ExpressionType.MultiplyAssign, left, right, method, conversion, true);
      if (!(left.Type == right.Type) || !left.Type.IsArithmetic())
        return Expression.GetUserDefinedAssignOperatorOrThrow(ExpressionType.MultiplyAssign, "op_Multiply", left, right, conversion, true);
      if (conversion != null)
        throw Error.ConversionIsNotSupportedForArithmeticTypes();
      return (BinaryExpression) new SimpleBinaryExpression(ExpressionType.MultiplyAssign, left, right, left.Type);
    }

    public static BinaryExpression MultiplyAssignChecked(
      Expression left,
      Expression right)
    {
      return Expression.MultiplyAssignChecked(left, right, (MethodInfo) null);
    }

    public static BinaryExpression MultiplyAssignChecked(
      Expression left,
      Expression right,
      MethodInfo method)
    {
      return Expression.MultiplyAssignChecked(left, right, method, (LambdaExpression) null);
    }

    public static BinaryExpression MultiplyAssignChecked(
      Expression left,
      Expression right,
      MethodInfo method,
      LambdaExpression conversion)
    {
      ExpressionUtils.RequiresCanRead(left, nameof (left));
      Expression.RequiresCanWrite(left, nameof (left));
      ExpressionUtils.RequiresCanRead(right, nameof (right));
      if (!(method == (MethodInfo) null))
        return Expression.GetMethodBasedAssignOperator(ExpressionType.MultiplyAssignChecked, left, right, method, conversion, true);
      if (!(left.Type == right.Type) || !left.Type.IsArithmetic())
        return Expression.GetUserDefinedAssignOperatorOrThrow(ExpressionType.MultiplyAssignChecked, "op_Multiply", left, right, conversion, true);
      if (conversion != null)
        throw Error.ConversionIsNotSupportedForArithmeticTypes();
      return (BinaryExpression) new SimpleBinaryExpression(ExpressionType.MultiplyAssignChecked, left, right, left.Type);
    }

    public static BinaryExpression MultiplyChecked(
      Expression left,
      Expression right)
    {
      return Expression.MultiplyChecked(left, right, (MethodInfo) null);
    }

    public static BinaryExpression MultiplyChecked(
      Expression left,
      Expression right,
      MethodInfo method)
    {
      ExpressionUtils.RequiresCanRead(left, nameof (left));
      ExpressionUtils.RequiresCanRead(right, nameof (right));
      if (!(method == (MethodInfo) null))
        return Expression.GetMethodBasedBinaryOperator(ExpressionType.MultiplyChecked, left, right, method, true);
      if (left.Type == right.Type && left.Type.IsArithmetic())
        return (BinaryExpression) new SimpleBinaryExpression(ExpressionType.MultiplyChecked, left, right, left.Type);
      return Expression.GetUserDefinedBinaryOperatorOrThrow(ExpressionType.MultiplyChecked, "op_Multiply", left, right, true);
    }

    private static bool IsSimpleShift(Type left, Type right)
    {
      if (left.IsInteger())
        return right.GetNonNullableType() == typeof (int);
      return false;
    }

    private static Type GetResultTypeOfShift(Type left, Type right)
    {
      if (left.IsNullableType() || !right.IsNullableType())
        return left;
      return typeof (Nullable<>).MakeGenericType(left);
    }

    public static BinaryExpression LeftShift(Expression left, Expression right)
    {
      return Expression.LeftShift(left, right, (MethodInfo) null);
    }

    public static BinaryExpression LeftShift(
      Expression left,
      Expression right,
      MethodInfo method)
    {
      ExpressionUtils.RequiresCanRead(left, nameof (left));
      ExpressionUtils.RequiresCanRead(right, nameof (right));
      if (!(method == (MethodInfo) null))
        return Expression.GetMethodBasedBinaryOperator(ExpressionType.LeftShift, left, right, method, true);
      if (!Expression.IsSimpleShift(left.Type, right.Type))
        return Expression.GetUserDefinedBinaryOperatorOrThrow(ExpressionType.LeftShift, "op_LeftShift", left, right, true);
      Type resultTypeOfShift = Expression.GetResultTypeOfShift(left.Type, right.Type);
      return (BinaryExpression) new SimpleBinaryExpression(ExpressionType.LeftShift, left, right, resultTypeOfShift);
    }

    public static BinaryExpression LeftShiftAssign(
      Expression left,
      Expression right)
    {
      return Expression.LeftShiftAssign(left, right, (MethodInfo) null, (LambdaExpression) null);
    }

    public static BinaryExpression LeftShiftAssign(
      Expression left,
      Expression right,
      MethodInfo method)
    {
      return Expression.LeftShiftAssign(left, right, method, (LambdaExpression) null);
    }

    public static BinaryExpression LeftShiftAssign(
      Expression left,
      Expression right,
      MethodInfo method,
      LambdaExpression conversion)
    {
      ExpressionUtils.RequiresCanRead(left, nameof (left));
      Expression.RequiresCanWrite(left, nameof (left));
      ExpressionUtils.RequiresCanRead(right, nameof (right));
      if (!(method == (MethodInfo) null))
        return Expression.GetMethodBasedAssignOperator(ExpressionType.LeftShiftAssign, left, right, method, conversion, true);
      if (!Expression.IsSimpleShift(left.Type, right.Type))
        return Expression.GetUserDefinedAssignOperatorOrThrow(ExpressionType.LeftShiftAssign, "op_LeftShift", left, right, conversion, true);
      if (conversion != null)
        throw Error.ConversionIsNotSupportedForArithmeticTypes();
      Type resultTypeOfShift = Expression.GetResultTypeOfShift(left.Type, right.Type);
      return (BinaryExpression) new SimpleBinaryExpression(ExpressionType.LeftShiftAssign, left, right, resultTypeOfShift);
    }

    public static BinaryExpression RightShift(Expression left, Expression right)
    {
      return Expression.RightShift(left, right, (MethodInfo) null);
    }

    public static BinaryExpression RightShift(
      Expression left,
      Expression right,
      MethodInfo method)
    {
      ExpressionUtils.RequiresCanRead(left, nameof (left));
      ExpressionUtils.RequiresCanRead(right, nameof (right));
      if (!(method == (MethodInfo) null))
        return Expression.GetMethodBasedBinaryOperator(ExpressionType.RightShift, left, right, method, true);
      if (!Expression.IsSimpleShift(left.Type, right.Type))
        return Expression.GetUserDefinedBinaryOperatorOrThrow(ExpressionType.RightShift, "op_RightShift", left, right, true);
      Type resultTypeOfShift = Expression.GetResultTypeOfShift(left.Type, right.Type);
      return (BinaryExpression) new SimpleBinaryExpression(ExpressionType.RightShift, left, right, resultTypeOfShift);
    }

    public static BinaryExpression RightShiftAssign(
      Expression left,
      Expression right)
    {
      return Expression.RightShiftAssign(left, right, (MethodInfo) null, (LambdaExpression) null);
    }

    public static BinaryExpression RightShiftAssign(
      Expression left,
      Expression right,
      MethodInfo method)
    {
      return Expression.RightShiftAssign(left, right, method, (LambdaExpression) null);
    }

    public static BinaryExpression RightShiftAssign(
      Expression left,
      Expression right,
      MethodInfo method,
      LambdaExpression conversion)
    {
      ExpressionUtils.RequiresCanRead(left, nameof (left));
      Expression.RequiresCanWrite(left, nameof (left));
      ExpressionUtils.RequiresCanRead(right, nameof (right));
      if (!(method == (MethodInfo) null))
        return Expression.GetMethodBasedAssignOperator(ExpressionType.RightShiftAssign, left, right, method, conversion, true);
      if (!Expression.IsSimpleShift(left.Type, right.Type))
        return Expression.GetUserDefinedAssignOperatorOrThrow(ExpressionType.RightShiftAssign, "op_RightShift", left, right, conversion, true);
      if (conversion != null)
        throw Error.ConversionIsNotSupportedForArithmeticTypes();
      Type resultTypeOfShift = Expression.GetResultTypeOfShift(left.Type, right.Type);
      return (BinaryExpression) new SimpleBinaryExpression(ExpressionType.RightShiftAssign, left, right, resultTypeOfShift);
    }

    public static BinaryExpression And(Expression left, Expression right)
    {
      return Expression.And(left, right, (MethodInfo) null);
    }

    public static BinaryExpression And(
      Expression left,
      Expression right,
      MethodInfo method)
    {
      ExpressionUtils.RequiresCanRead(left, nameof (left));
      ExpressionUtils.RequiresCanRead(right, nameof (right));
      if (!(method == (MethodInfo) null))
        return Expression.GetMethodBasedBinaryOperator(ExpressionType.And, left, right, method, true);
      if (left.Type == right.Type && left.Type.IsIntegerOrBool())
        return (BinaryExpression) new SimpleBinaryExpression(ExpressionType.And, left, right, left.Type);
      return Expression.GetUserDefinedBinaryOperatorOrThrow(ExpressionType.And, "op_BitwiseAnd", left, right, true);
    }

    public static BinaryExpression AndAssign(Expression left, Expression right)
    {
      return Expression.AndAssign(left, right, (MethodInfo) null, (LambdaExpression) null);
    }

    public static BinaryExpression AndAssign(
      Expression left,
      Expression right,
      MethodInfo method)
    {
      return Expression.AndAssign(left, right, method, (LambdaExpression) null);
    }

    public static BinaryExpression AndAssign(
      Expression left,
      Expression right,
      MethodInfo method,
      LambdaExpression conversion)
    {
      ExpressionUtils.RequiresCanRead(left, nameof (left));
      Expression.RequiresCanWrite(left, nameof (left));
      ExpressionUtils.RequiresCanRead(right, nameof (right));
      if (!(method == (MethodInfo) null))
        return Expression.GetMethodBasedAssignOperator(ExpressionType.AndAssign, left, right, method, conversion, true);
      if (!(left.Type == right.Type) || !left.Type.IsIntegerOrBool())
        return Expression.GetUserDefinedAssignOperatorOrThrow(ExpressionType.AndAssign, "op_BitwiseAnd", left, right, conversion, true);
      if (conversion != null)
        throw Error.ConversionIsNotSupportedForArithmeticTypes();
      return (BinaryExpression) new SimpleBinaryExpression(ExpressionType.AndAssign, left, right, left.Type);
    }

    public static BinaryExpression Or(Expression left, Expression right)
    {
      return Expression.Or(left, right, (MethodInfo) null);
    }

    public static BinaryExpression Or(
      Expression left,
      Expression right,
      MethodInfo method)
    {
      ExpressionUtils.RequiresCanRead(left, nameof (left));
      ExpressionUtils.RequiresCanRead(right, nameof (right));
      if (!(method == (MethodInfo) null))
        return Expression.GetMethodBasedBinaryOperator(ExpressionType.Or, left, right, method, true);
      if (left.Type == right.Type && left.Type.IsIntegerOrBool())
        return (BinaryExpression) new SimpleBinaryExpression(ExpressionType.Or, left, right, left.Type);
      return Expression.GetUserDefinedBinaryOperatorOrThrow(ExpressionType.Or, "op_BitwiseOr", left, right, true);
    }

    public static BinaryExpression OrAssign(Expression left, Expression right)
    {
      return Expression.OrAssign(left, right, (MethodInfo) null, (LambdaExpression) null);
    }

    public static BinaryExpression OrAssign(
      Expression left,
      Expression right,
      MethodInfo method)
    {
      return Expression.OrAssign(left, right, method, (LambdaExpression) null);
    }

    public static BinaryExpression OrAssign(
      Expression left,
      Expression right,
      MethodInfo method,
      LambdaExpression conversion)
    {
      ExpressionUtils.RequiresCanRead(left, nameof (left));
      Expression.RequiresCanWrite(left, nameof (left));
      ExpressionUtils.RequiresCanRead(right, nameof (right));
      if (!(method == (MethodInfo) null))
        return Expression.GetMethodBasedAssignOperator(ExpressionType.OrAssign, left, right, method, conversion, true);
      if (!(left.Type == right.Type) || !left.Type.IsIntegerOrBool())
        return Expression.GetUserDefinedAssignOperatorOrThrow(ExpressionType.OrAssign, "op_BitwiseOr", left, right, conversion, true);
      if (conversion != null)
        throw Error.ConversionIsNotSupportedForArithmeticTypes();
      return (BinaryExpression) new SimpleBinaryExpression(ExpressionType.OrAssign, left, right, left.Type);
    }

    public static BinaryExpression ExclusiveOr(Expression left, Expression right)
    {
      return Expression.ExclusiveOr(left, right, (MethodInfo) null);
    }

    public static BinaryExpression ExclusiveOr(
      Expression left,
      Expression right,
      MethodInfo method)
    {
      ExpressionUtils.RequiresCanRead(left, nameof (left));
      ExpressionUtils.RequiresCanRead(right, nameof (right));
      if (!(method == (MethodInfo) null))
        return Expression.GetMethodBasedBinaryOperator(ExpressionType.ExclusiveOr, left, right, method, true);
      if (left.Type == right.Type && left.Type.IsIntegerOrBool())
        return (BinaryExpression) new SimpleBinaryExpression(ExpressionType.ExclusiveOr, left, right, left.Type);
      return Expression.GetUserDefinedBinaryOperatorOrThrow(ExpressionType.ExclusiveOr, "op_ExclusiveOr", left, right, true);
    }

    public static BinaryExpression ExclusiveOrAssign(
      Expression left,
      Expression right)
    {
      return Expression.ExclusiveOrAssign(left, right, (MethodInfo) null, (LambdaExpression) null);
    }

    public static BinaryExpression ExclusiveOrAssign(
      Expression left,
      Expression right,
      MethodInfo method)
    {
      return Expression.ExclusiveOrAssign(left, right, method, (LambdaExpression) null);
    }

    public static BinaryExpression ExclusiveOrAssign(
      Expression left,
      Expression right,
      MethodInfo method,
      LambdaExpression conversion)
    {
      ExpressionUtils.RequiresCanRead(left, nameof (left));
      Expression.RequiresCanWrite(left, nameof (left));
      ExpressionUtils.RequiresCanRead(right, nameof (right));
      if (!(method == (MethodInfo) null))
        return Expression.GetMethodBasedAssignOperator(ExpressionType.ExclusiveOrAssign, left, right, method, conversion, true);
      if (!(left.Type == right.Type) || !left.Type.IsIntegerOrBool())
        return Expression.GetUserDefinedAssignOperatorOrThrow(ExpressionType.ExclusiveOrAssign, "op_ExclusiveOr", left, right, conversion, true);
      if (conversion != null)
        throw Error.ConversionIsNotSupportedForArithmeticTypes();
      return (BinaryExpression) new SimpleBinaryExpression(ExpressionType.ExclusiveOrAssign, left, right, left.Type);
    }

    public static BinaryExpression Power(Expression left, Expression right)
    {
      return Expression.Power(left, right, (MethodInfo) null);
    }

    public static BinaryExpression Power(
      Expression left,
      Expression right,
      MethodInfo method)
    {
      ExpressionUtils.RequiresCanRead(left, nameof (left));
      ExpressionUtils.RequiresCanRead(right, nameof (right));
      if (method == (MethodInfo) null)
      {
        if (left.Type == right.Type && left.Type.IsArithmetic())
        {
          method = CachedReflectionInfo.Math_Pow_Double_Double;
        }
        else
        {
          string name = "op_Exponent";
          BinaryExpression definedBinaryOperator = Expression.GetUserDefinedBinaryOperator(ExpressionType.Power, name, left, right, true);
          if (definedBinaryOperator == null)
          {
            name = "op_Exponentiation";
            definedBinaryOperator = Expression.GetUserDefinedBinaryOperator(ExpressionType.Power, name, left, right, true);
            if (definedBinaryOperator == null)
              throw Error.BinaryOperatorNotDefined((object) ExpressionType.Power, (object) left.Type, (object) right.Type);
          }
          ParameterInfo[] parametersCached = definedBinaryOperator.Method.GetParametersCached();
          Expression.ValidateParamswithOperandsOrThrow(parametersCached[0].ParameterType, left.Type, ExpressionType.Power, name);
          Expression.ValidateParamswithOperandsOrThrow(parametersCached[1].ParameterType, right.Type, ExpressionType.Power, name);
          return definedBinaryOperator;
        }
      }
      return Expression.GetMethodBasedBinaryOperator(ExpressionType.Power, left, right, method, true);
    }

    public static BinaryExpression PowerAssign(Expression left, Expression right)
    {
      return Expression.PowerAssign(left, right, (MethodInfo) null, (LambdaExpression) null);
    }

    public static BinaryExpression PowerAssign(
      Expression left,
      Expression right,
      MethodInfo method)
    {
      return Expression.PowerAssign(left, right, method, (LambdaExpression) null);
    }

    public static BinaryExpression PowerAssign(
      Expression left,
      Expression right,
      MethodInfo method,
      LambdaExpression conversion)
    {
      ExpressionUtils.RequiresCanRead(left, nameof (left));
      Expression.RequiresCanWrite(left, nameof (left));
      ExpressionUtils.RequiresCanRead(right, nameof (right));
      if (method == (MethodInfo) null)
      {
        method = CachedReflectionInfo.Math_Pow_Double_Double;
        if (method == (MethodInfo) null)
          throw Error.BinaryOperatorNotDefined((object) ExpressionType.PowerAssign, (object) left.Type, (object) right.Type);
      }
      return Expression.GetMethodBasedAssignOperator(ExpressionType.PowerAssign, left, right, method, conversion, true);
    }

    public static BinaryExpression ArrayIndex(Expression array, Expression index)
    {
      ExpressionUtils.RequiresCanRead(array, nameof (array));
      ExpressionUtils.RequiresCanRead(index, nameof (index));
      if (index.Type != typeof (int))
        throw Error.ArgumentMustBeArrayIndexType(nameof (index));
      Type type = array.Type;
      if (!type.IsArray)
        throw Error.ArgumentMustBeArray(nameof (array));
      if (type.GetArrayRank() != 1)
        throw Error.IncorrectNumberOfIndexes();
      return (BinaryExpression) new SimpleBinaryExpression(ExpressionType.ArrayIndex, array, index, type.GetElementType());
    }

    public static BlockExpression Block(Expression arg0, Expression arg1)
    {
      ExpressionUtils.RequiresCanRead(arg0, nameof (arg0));
      ExpressionUtils.RequiresCanRead(arg1, nameof (arg1));
      return (BlockExpression) new Block2(arg0, arg1);
    }

    public static BlockExpression Block(
      Expression arg0,
      Expression arg1,
      Expression arg2)
    {
      ExpressionUtils.RequiresCanRead(arg0, nameof (arg0));
      ExpressionUtils.RequiresCanRead(arg1, nameof (arg1));
      ExpressionUtils.RequiresCanRead(arg2, nameof (arg2));
      return (BlockExpression) new Block3(arg0, arg1, arg2);
    }

    public static BlockExpression Block(
      Expression arg0,
      Expression arg1,
      Expression arg2,
      Expression arg3)
    {
      ExpressionUtils.RequiresCanRead(arg0, nameof (arg0));
      ExpressionUtils.RequiresCanRead(arg1, nameof (arg1));
      ExpressionUtils.RequiresCanRead(arg2, nameof (arg2));
      ExpressionUtils.RequiresCanRead(arg3, nameof (arg3));
      return (BlockExpression) new Block4(arg0, arg1, arg2, arg3);
    }

    public static BlockExpression Block(
      Expression arg0,
      Expression arg1,
      Expression arg2,
      Expression arg3,
      Expression arg4)
    {
      ExpressionUtils.RequiresCanRead(arg0, nameof (arg0));
      ExpressionUtils.RequiresCanRead(arg1, nameof (arg1));
      ExpressionUtils.RequiresCanRead(arg2, nameof (arg2));
      ExpressionUtils.RequiresCanRead(arg3, nameof (arg3));
      ExpressionUtils.RequiresCanRead(arg4, nameof (arg4));
      return (BlockExpression) new Block5(arg0, arg1, arg2, arg3, arg4);
    }

    public static BlockExpression Block(params Expression[] expressions)
    {
      ContractUtils.RequiresNotNull((object) expressions, nameof (expressions));
      Expression.RequiresCanRead((IReadOnlyList<Expression>) expressions, nameof (expressions));
      return Expression.GetOptimizedBlockExpression((IReadOnlyList<Expression>) expressions);
    }

    public static BlockExpression Block(IEnumerable<Expression> expressions)
    {
      return Expression.Block((IEnumerable<ParameterExpression>) EmptyReadOnlyCollection<ParameterExpression>.Instance, expressions);
    }

    public static BlockExpression Block(Type type, params Expression[] expressions)
    {
      ContractUtils.RequiresNotNull((object) expressions, nameof (expressions));
      return Expression.Block(type, (IEnumerable<Expression>) expressions);
    }

    public static BlockExpression Block(
      Type type,
      IEnumerable<Expression> expressions)
    {
      return Expression.Block(type, (IEnumerable<ParameterExpression>) EmptyReadOnlyCollection<ParameterExpression>.Instance, expressions);
    }

    public static BlockExpression Block(
      IEnumerable<ParameterExpression> variables,
      params Expression[] expressions)
    {
      return Expression.Block(variables, (IEnumerable<Expression>) expressions);
    }

    public static BlockExpression Block(
      Type type,
      IEnumerable<ParameterExpression> variables,
      params Expression[] expressions)
    {
      return Expression.Block(type, variables, (IEnumerable<Expression>) expressions);
    }

    public static BlockExpression Block(
      IEnumerable<ParameterExpression> variables,
      IEnumerable<Expression> expressions)
    {
      ContractUtils.RequiresNotNull((object) expressions, nameof (expressions));
      ReadOnlyCollection<ParameterExpression> variables1 = variables.ToReadOnly<ParameterExpression>();
      if (variables1.Count == 0)
      {
        IReadOnlyList<Expression> expressionList = expressions as IReadOnlyList<Expression> ?? (IReadOnlyList<Expression>) expressions.ToReadOnly<Expression>();
        Expression.RequiresCanRead(expressionList, nameof (expressions));
        return Expression.GetOptimizedBlockExpression(expressionList);
      }
      ReadOnlyCollection<Expression> expressions1 = expressions.ToReadOnly<Expression>();
      Expression.RequiresCanRead((IReadOnlyList<Expression>) expressions1, nameof (expressions));
      return Expression.BlockCore((Type) null, variables1, expressions1);
    }

    public static BlockExpression Block(
      Type type,
      IEnumerable<ParameterExpression> variables,
      IEnumerable<Expression> expressions)
    {
      ContractUtils.RequiresNotNull((object) type, nameof (type));
      ContractUtils.RequiresNotNull((object) expressions, nameof (expressions));
      ReadOnlyCollection<Expression> expressions1 = expressions.ToReadOnly<Expression>();
      Expression.RequiresCanRead((IReadOnlyList<Expression>) expressions1, nameof (expressions));
      ReadOnlyCollection<ParameterExpression> variables1 = variables.ToReadOnly<ParameterExpression>();
      if (variables1.Count == 0 && expressions1.Count != 0)
      {
        int count = expressions1.Count;
        if (count != 0 && expressions1[count - 1].Type == type)
          return Expression.GetOptimizedBlockExpression((IReadOnlyList<Expression>) expressions1);
      }
      return Expression.BlockCore(type, variables1, expressions1);
    }

    private static BlockExpression BlockCore(
      Type type,
      ReadOnlyCollection<ParameterExpression> variables,
      ReadOnlyCollection<Expression> expressions)
    {
      Expression.ValidateVariables(variables, nameof (variables));
      if (type != (Type) null)
      {
        if (expressions.Count == 0)
        {
          if (type != typeof (void))
            throw Error.ArgumentTypesMustMatch();
          return (BlockExpression) new ScopeWithType((IReadOnlyList<ParameterExpression>) variables, (IReadOnlyList<Expression>) expressions, type);
        }
        Expression expression = expressions.Last<Expression>();
        if (type != typeof (void) && !TypeUtils.AreReferenceAssignable(type, expression.Type))
          throw Error.ArgumentTypesMustMatch();
        if (!TypeUtils.AreEquivalent(type, expression.Type))
          return (BlockExpression) new ScopeWithType((IReadOnlyList<ParameterExpression>) variables, (IReadOnlyList<Expression>) expressions, type);
      }
      switch (expressions.Count)
      {
        case 0:
          return (BlockExpression) new ScopeWithType((IReadOnlyList<ParameterExpression>) variables, (IReadOnlyList<Expression>) expressions, typeof (void));
        case 1:
          return (BlockExpression) new Scope1((IReadOnlyList<ParameterExpression>) variables, expressions[0]);
        default:
          return (BlockExpression) new ScopeN((IReadOnlyList<ParameterExpression>) variables, (IReadOnlyList<Expression>) expressions);
      }
    }

    internal static void ValidateVariables(
      ReadOnlyCollection<ParameterExpression> varList,
      string collectionName)
    {
      int count = varList.Count;
      if (count == 0)
        return;
      HashSet<ParameterExpression> parameterExpressionSet = new HashSet<ParameterExpression>();
      for (int index = 0; index < count; ++index)
      {
        ParameterExpression var = varList[index];
        ContractUtils.RequiresNotNull((object) var, collectionName, index);
        if (var.IsByRef)
          throw Error.VariableMustNotBeByRef((object) var, (object) var.Type, collectionName, index);
        if (!parameterExpressionSet.Add(var))
          throw Error.DuplicateVariable((object) var, collectionName, index);
      }
    }

    private static BlockExpression GetOptimizedBlockExpression(
      IReadOnlyList<Expression> expressions)
    {
      switch (expressions.Count)
      {
        case 0:
          return Expression.BlockCore(typeof (void), EmptyReadOnlyCollection<ParameterExpression>.Instance, EmptyReadOnlyCollection<Expression>.Instance);
        case 2:
          return (BlockExpression) new Block2(expressions[0], expressions[1]);
        case 3:
          return (BlockExpression) new Block3(expressions[0], expressions[1], expressions[2]);
        case 4:
          return (BlockExpression) new Block4(expressions[0], expressions[1], expressions[2], expressions[3]);
        case 5:
          return (BlockExpression) new Block5(expressions[0], expressions[1], expressions[2], expressions[3], expressions[4]);
        default:
          return (BlockExpression) new BlockN((IReadOnlyList<Expression>) ((object) (expressions as ReadOnlyCollection<Expression>) ?? (object) expressions.ToArray<Expression>()));
      }
    }

    public static CatchBlock Catch(Type type, Expression body)
    {
      return Expression.MakeCatchBlock(type, (ParameterExpression) null, body, (Expression) null);
    }

    public static CatchBlock Catch(ParameterExpression variable, Expression body)
    {
      ContractUtils.RequiresNotNull((object) variable, nameof (variable));
      return Expression.MakeCatchBlock(variable.Type, variable, body, (Expression) null);
    }

    public static CatchBlock Catch(Type type, Expression body, Expression filter)
    {
      return Expression.MakeCatchBlock(type, (ParameterExpression) null, body, filter);
    }

    public static CatchBlock Catch(
      ParameterExpression variable,
      Expression body,
      Expression filter)
    {
      ContractUtils.RequiresNotNull((object) variable, nameof (variable));
      return Expression.MakeCatchBlock(variable.Type, variable, body, filter);
    }

    public static CatchBlock MakeCatchBlock(
      Type type,
      ParameterExpression variable,
      Expression body,
      Expression filter)
    {
      ContractUtils.RequiresNotNull((object) type, nameof (type));
      ContractUtils.Requires(variable == null || TypeUtils.AreEquivalent(variable.Type, type), nameof (variable));
      if (variable == null)
        TypeUtils.ValidateType(type, nameof (type));
      else if (variable.IsByRef)
        throw Error.VariableMustNotBeByRef((object) variable, (object) variable.Type, nameof (variable));
      ExpressionUtils.RequiresCanRead(body, nameof (body));
      if (filter != null)
      {
        ExpressionUtils.RequiresCanRead(filter, nameof (filter));
        if (filter.Type != typeof (bool))
          throw Error.ArgumentMustBeBoolean(nameof (filter));
      }
      return new CatchBlock(type, variable, body, filter);
    }

    public static ConditionalExpression Condition(
      Expression test,
      Expression ifTrue,
      Expression ifFalse)
    {
      ExpressionUtils.RequiresCanRead(test, nameof (test));
      ExpressionUtils.RequiresCanRead(ifTrue, nameof (ifTrue));
      ExpressionUtils.RequiresCanRead(ifFalse, nameof (ifFalse));
      if (test.Type != typeof (bool))
        throw Error.ArgumentMustBeBoolean(nameof (test));
      if (!TypeUtils.AreEquivalent(ifTrue.Type, ifFalse.Type))
        throw Error.ArgumentTypesMustMatch();
      return ConditionalExpression.Make(test, ifTrue, ifFalse, ifTrue.Type);
    }

    public static ConditionalExpression Condition(
      Expression test,
      Expression ifTrue,
      Expression ifFalse,
      Type type)
    {
      ExpressionUtils.RequiresCanRead(test, nameof (test));
      ExpressionUtils.RequiresCanRead(ifTrue, nameof (ifTrue));
      ExpressionUtils.RequiresCanRead(ifFalse, nameof (ifFalse));
      ContractUtils.RequiresNotNull((object) type, nameof (type));
      if (test.Type != typeof (bool))
        throw Error.ArgumentMustBeBoolean(nameof (test));
      if (type != typeof (void) && (!TypeUtils.AreReferenceAssignable(type, ifTrue.Type) || !TypeUtils.AreReferenceAssignable(type, ifFalse.Type)))
        throw Error.ArgumentTypesMustMatch();
      return ConditionalExpression.Make(test, ifTrue, ifFalse, type);
    }

    public static ConditionalExpression IfThen(
      Expression test,
      Expression ifTrue)
    {
      return Expression.Condition(test, ifTrue, (Expression) Expression.Empty(), typeof (void));
    }

    public static ConditionalExpression IfThenElse(
      Expression test,
      Expression ifTrue,
      Expression ifFalse)
    {
      return Expression.Condition(test, ifTrue, ifFalse, typeof (void));
    }

    public static ConstantExpression Constant(object value)
    {
      return new ConstantExpression(value);
    }

    public static ConstantExpression Constant(object value, Type type)
    {
      ContractUtils.RequiresNotNull((object) type, nameof (type));
      TypeUtils.ValidateType(type, nameof (type));
      if (value == null)
      {
        if (type == typeof (object))
          return new ConstantExpression((object) null);
        if (!type.IsValueType || type.IsNullableType())
          return (ConstantExpression) new TypedConstantExpression((object) null, type);
      }
      else
      {
        Type type1 = value.GetType();
        if (type == type1)
          return new ConstantExpression(value);
        if (type.IsAssignableFrom(type1))
          return (ConstantExpression) new TypedConstantExpression(value, type);
      }
      throw Error.ArgumentTypesMustMatch();
    }

    public static DebugInfoExpression DebugInfo(
      SymbolDocumentInfo document,
      int startLine,
      int startColumn,
      int endLine,
      int endColumn)
    {
      ContractUtils.RequiresNotNull((object) document, nameof (document));
      if (startLine == 16707566 && startColumn == 0 && (endLine == 16707566 && endColumn == 0))
        return (DebugInfoExpression) new ClearDebugInfoExpression(document);
      Expression.ValidateSpan(startLine, startColumn, endLine, endColumn);
      return (DebugInfoExpression) new SpanDebugInfoExpression(document, startLine, startColumn, endLine, endColumn);
    }

    public static DebugInfoExpression ClearDebugInfo(SymbolDocumentInfo document)
    {
      ContractUtils.RequiresNotNull((object) document, nameof (document));
      return (DebugInfoExpression) new ClearDebugInfoExpression(document);
    }

    private static void ValidateSpan(int startLine, int startColumn, int endLine, int endColumn)
    {
      if (startLine < 1)
        throw Error.OutOfRange(nameof (startLine), (object) 1);
      if (startColumn < 1)
        throw Error.OutOfRange(nameof (startColumn), (object) 1);
      if (endLine < 1)
        throw Error.OutOfRange(nameof (endLine), (object) 1);
      if (endColumn < 1)
        throw Error.OutOfRange(nameof (endColumn), (object) 1);
      if (startLine > endLine)
        throw Error.StartEndMustBeOrdered();
      if (startLine == endLine && startColumn > endColumn)
        throw Error.StartEndMustBeOrdered();
    }

    public static DefaultExpression Empty()
    {
      return new DefaultExpression(typeof (void));
    }

    public static DefaultExpression Default(Type type)
    {
      ContractUtils.RequiresNotNull((object) type, nameof (type));
      TypeUtils.ValidateType(type, nameof (type));
      return new DefaultExpression(type);
    }

    public static ElementInit ElementInit(
      MethodInfo addMethod,
      params Expression[] arguments)
    {
      return Expression.ElementInit(addMethod, (IEnumerable<Expression>) arguments);
    }

    public static ElementInit ElementInit(
      MethodInfo addMethod,
      IEnumerable<Expression> arguments)
    {
      ContractUtils.RequiresNotNull((object) addMethod, nameof (addMethod));
      ContractUtils.RequiresNotNull((object) arguments, nameof (arguments));
      ReadOnlyCollection<Expression> arguments1 = arguments.ToReadOnly<Expression>();
      Expression.RequiresCanRead((IReadOnlyList<Expression>) arguments1, nameof (arguments));
      Expression.ValidateElementInitAddMethodInfo(addMethod, nameof (addMethod));
      Expression.ValidateArgumentTypes((MethodBase) addMethod, ExpressionType.Call, ref arguments1, nameof (addMethod));
      return new ElementInit(addMethod, arguments1);
    }

    private static void ValidateElementInitAddMethodInfo(MethodInfo addMethod, string paramName)
    {
      Expression.ValidateMethodInfo(addMethod, paramName);
      ParameterInfo[] parametersCached = addMethod.GetParametersCached();
      if (parametersCached.Length == 0)
        throw Error.ElementInitializerMethodWithZeroArgs(paramName);
      if (!addMethod.Name.Equals("Add", StringComparison.OrdinalIgnoreCase))
        throw Error.ElementInitializerMethodNotAdd(paramName);
      if (addMethod.IsStatic)
        throw Error.ElementInitializerMethodStatic(paramName);
      foreach (ParameterInfo parameterInfo in parametersCached)
      {
        if (parameterInfo.ParameterType.IsByRef)
          throw Error.ElementInitializerMethodNoRefOutParam((object) parameterInfo.Name, (object) addMethod.Name, paramName);
      }
    }

    [Obsolete("use a different constructor that does not take ExpressionType. Then override NodeType and Type properties to provide the values that would be specified to this constructor.")]
    protected Expression(ExpressionType nodeType, Type type)
    {
      if (Expression.s_legacyCtorSupportTable == null)
        Interlocked.CompareExchange<ConditionalWeakTable<Expression, Expression.ExtensionInfo>>(ref Expression.s_legacyCtorSupportTable, new ConditionalWeakTable<Expression, Expression.ExtensionInfo>(), (ConditionalWeakTable<Expression, Expression.ExtensionInfo>) null);
      Expression.s_legacyCtorSupportTable.Add(this, new Expression.ExtensionInfo(nodeType, type));
    }

    protected Expression()
    {
    }

    public virtual ExpressionType NodeType
    {
      get
      {
        Expression.ExtensionInfo extensionInfo;
        if (Expression.s_legacyCtorSupportTable != null && Expression.s_legacyCtorSupportTable.TryGetValue(this, out extensionInfo))
          return extensionInfo.NodeType;
        throw Error.ExtensionNodeMustOverrideProperty((object) "Expression.NodeType");
      }
    }

    public virtual Type Type
    {
      get
      {
        Expression.ExtensionInfo extensionInfo;
        if (Expression.s_legacyCtorSupportTable != null && Expression.s_legacyCtorSupportTable.TryGetValue(this, out extensionInfo))
          return extensionInfo.Type;
        throw Error.ExtensionNodeMustOverrideProperty((object) "Expression.Type");
      }
    }

    public virtual bool CanReduce
    {
      get
      {
        return false;
      }
    }

    public virtual Expression Reduce()
    {
      if (this.CanReduce)
        throw Error.ReducibleMustOverrideReduce();
      return this;
    }

    protected internal virtual Expression VisitChildren(ExpressionVisitor visitor)
    {
      if (!this.CanReduce)
        throw Error.MustBeReducible();
      return visitor.Visit(this.ReduceAndCheck());
    }

    protected internal virtual Expression Accept(ExpressionVisitor visitor)
    {
      return visitor.VisitExtension(this);
    }

    public Expression ReduceAndCheck()
    {
      if (!this.CanReduce)
        throw Error.MustBeReducible();
      Expression expression = this.Reduce();
      if (expression == null || expression == this)
        throw Error.MustReduceToDifferent();
      if (!TypeUtils.AreReferenceAssignable(this.Type, expression.Type))
        throw Error.ReducedNotCompatible();
      return expression;
    }

    public Expression ReduceExtensions()
    {
      Expression expression = this;
      while (expression.NodeType == ExpressionType.Extension)
        expression = expression.ReduceAndCheck();
      return expression;
    }

    public override string ToString()
    {
      return ExpressionStringBuilder.ExpressionToString(this);
    }

    private string DebugView
    {
      get
      {
        using (StringWriter stringWriter = new StringWriter((IFormatProvider) CultureInfo.CurrentCulture))
        {
          DebugViewWriter.WriteTo(this, (TextWriter) stringWriter);
          return stringWriter.ToString();
        }
      }
    }

    private static void RequiresCanRead(IReadOnlyList<Expression> items, string paramName)
    {
      int idx = 0;
      for (int count = items.Count; idx < count; ++idx)
        ExpressionUtils.RequiresCanRead(items[idx], paramName, idx);
    }

    private static void RequiresCanWrite(Expression expression, string paramName)
    {
      if (expression == null)
        throw new ArgumentNullException(paramName);
      switch (expression.NodeType)
      {
        case ExpressionType.MemberAccess:
          MemberInfo member = ((MemberExpression) expression).Member;
          PropertyInfo propertyInfo = member as PropertyInfo;
          if (propertyInfo != (PropertyInfo) null)
          {
            if (propertyInfo.CanWrite)
              return;
            break;
          }
          FieldInfo fieldInfo = (FieldInfo) member;
          if (!fieldInfo.IsInitOnly && !fieldInfo.IsLiteral)
            return;
          break;
        case ExpressionType.Parameter:
          return;
        case ExpressionType.Index:
          PropertyInfo indexer = ((IndexExpression) expression).Indexer;
          if (indexer == (PropertyInfo) null || indexer.CanWrite)
            return;
          break;
      }
      throw Error.ExpressionMustBeWriteable(paramName);
    }

    public static DynamicExpression Dynamic(
      CallSiteBinder binder,
      Type returnType,
      IEnumerable<Expression> arguments)
    {
      return DynamicExpression.Dynamic(binder, returnType, arguments);
    }

    public static DynamicExpression Dynamic(
      CallSiteBinder binder,
      Type returnType,
      Expression arg0)
    {
      return DynamicExpression.Dynamic(binder, returnType, arg0);
    }

    public static DynamicExpression Dynamic(
      CallSiteBinder binder,
      Type returnType,
      Expression arg0,
      Expression arg1)
    {
      return DynamicExpression.Dynamic(binder, returnType, arg0, arg1);
    }

    public static DynamicExpression Dynamic(
      CallSiteBinder binder,
      Type returnType,
      Expression arg0,
      Expression arg1,
      Expression arg2)
    {
      return DynamicExpression.Dynamic(binder, returnType, arg0, arg1, arg2);
    }

    public static DynamicExpression Dynamic(
      CallSiteBinder binder,
      Type returnType,
      Expression arg0,
      Expression arg1,
      Expression arg2,
      Expression arg3)
    {
      return DynamicExpression.Dynamic(binder, returnType, arg0, arg1, arg2, arg3);
    }

    public static DynamicExpression Dynamic(
      CallSiteBinder binder,
      Type returnType,
      params Expression[] arguments)
    {
      return DynamicExpression.Dynamic(binder, returnType, arguments);
    }

    public static DynamicExpression MakeDynamic(
      Type delegateType,
      CallSiteBinder binder,
      IEnumerable<Expression> arguments)
    {
      return DynamicExpression.MakeDynamic(delegateType, binder, arguments);
    }

    public static DynamicExpression MakeDynamic(
      Type delegateType,
      CallSiteBinder binder,
      Expression arg0)
    {
      return DynamicExpression.MakeDynamic(delegateType, binder, arg0);
    }

    public static DynamicExpression MakeDynamic(
      Type delegateType,
      CallSiteBinder binder,
      Expression arg0,
      Expression arg1)
    {
      return DynamicExpression.MakeDynamic(delegateType, binder, arg0, arg1);
    }

    public static DynamicExpression MakeDynamic(
      Type delegateType,
      CallSiteBinder binder,
      Expression arg0,
      Expression arg1,
      Expression arg2)
    {
      return DynamicExpression.MakeDynamic(delegateType, binder, arg0, arg1, arg2);
    }

    public static DynamicExpression MakeDynamic(
      Type delegateType,
      CallSiteBinder binder,
      Expression arg0,
      Expression arg1,
      Expression arg2,
      Expression arg3)
    {
      return DynamicExpression.MakeDynamic(delegateType, binder, arg0, arg1, arg2, arg3);
    }

    public static DynamicExpression MakeDynamic(
      Type delegateType,
      CallSiteBinder binder,
      params Expression[] arguments)
    {
      return Expression.MakeDynamic(delegateType, binder, (IEnumerable<Expression>) arguments);
    }

    public static GotoExpression Break(LabelTarget target)
    {
      return Expression.MakeGoto(GotoExpressionKind.Break, target, (Expression) null, typeof (void));
    }

    public static GotoExpression Break(LabelTarget target, Expression value)
    {
      return Expression.MakeGoto(GotoExpressionKind.Break, target, value, typeof (void));
    }

    public static GotoExpression Break(LabelTarget target, Type type)
    {
      return Expression.MakeGoto(GotoExpressionKind.Break, target, (Expression) null, type);
    }

    public static GotoExpression Break(
      LabelTarget target,
      Expression value,
      Type type)
    {
      return Expression.MakeGoto(GotoExpressionKind.Break, target, value, type);
    }

    public static GotoExpression Continue(LabelTarget target)
    {
      return Expression.MakeGoto(GotoExpressionKind.Continue, target, (Expression) null, typeof (void));
    }

    public static GotoExpression Continue(LabelTarget target, Type type)
    {
      return Expression.MakeGoto(GotoExpressionKind.Continue, target, (Expression) null, type);
    }

    public static GotoExpression Return(LabelTarget target)
    {
      return Expression.MakeGoto(GotoExpressionKind.Return, target, (Expression) null, typeof (void));
    }

    public static GotoExpression Return(LabelTarget target, Type type)
    {
      return Expression.MakeGoto(GotoExpressionKind.Return, target, (Expression) null, type);
    }

    public static GotoExpression Return(LabelTarget target, Expression value)
    {
      return Expression.MakeGoto(GotoExpressionKind.Return, target, value, typeof (void));
    }

    public static GotoExpression Return(
      LabelTarget target,
      Expression value,
      Type type)
    {
      return Expression.MakeGoto(GotoExpressionKind.Return, target, value, type);
    }

    public static GotoExpression Goto(LabelTarget target)
    {
      return Expression.MakeGoto(GotoExpressionKind.Goto, target, (Expression) null, typeof (void));
    }

    public static GotoExpression Goto(LabelTarget target, Type type)
    {
      return Expression.MakeGoto(GotoExpressionKind.Goto, target, (Expression) null, type);
    }

    public static GotoExpression Goto(LabelTarget target, Expression value)
    {
      return Expression.MakeGoto(GotoExpressionKind.Goto, target, value, typeof (void));
    }

    public static GotoExpression Goto(
      LabelTarget target,
      Expression value,
      Type type)
    {
      return Expression.MakeGoto(GotoExpressionKind.Goto, target, value, type);
    }

    public static GotoExpression MakeGoto(
      GotoExpressionKind kind,
      LabelTarget target,
      Expression value,
      Type type)
    {
      Expression.ValidateGoto(target, ref value, nameof (target), nameof (value), type);
      return new GotoExpression(kind, target, value, type);
    }

    private static void ValidateGoto(
      LabelTarget target,
      ref Expression value,
      string targetParameter,
      string valueParameter,
      Type type)
    {
      ContractUtils.RequiresNotNull((object) target, targetParameter);
      if (value == null)
      {
        if (target.Type != typeof (void))
          throw Error.LabelMustBeVoidOrHaveExpression(nameof (target));
        if (!(type != (Type) null))
          return;
        TypeUtils.ValidateType(type, nameof (type));
      }
      else
        Expression.ValidateGotoType(target.Type, ref value, valueParameter);
    }

    private static void ValidateGotoType(Type expectedType, ref Expression value, string paramName)
    {
      ExpressionUtils.RequiresCanRead(value, paramName);
      if (expectedType != typeof (void) && !TypeUtils.AreReferenceAssignable(expectedType, value.Type) && !Expression.TryQuote(expectedType, ref value))
        throw Error.ExpressionTypeDoesNotMatchLabel((object) value.Type, (object) expectedType);
    }

    public static IndexExpression MakeIndex(
      Expression instance,
      PropertyInfo indexer,
      IEnumerable<Expression> arguments)
    {
      if (indexer != (PropertyInfo) null)
        return Expression.Property(instance, indexer, arguments);
      return Expression.ArrayAccess(instance, arguments);
    }

    public static IndexExpression ArrayAccess(
      Expression array,
      params Expression[] indexes)
    {
      return Expression.ArrayAccess(array, (IEnumerable<Expression>) indexes);
    }

    public static IndexExpression ArrayAccess(
      Expression array,
      IEnumerable<Expression> indexes)
    {
      ExpressionUtils.RequiresCanRead(array, nameof (array));
      Type type = array.Type;
      if (!type.IsArray)
        throw Error.ArgumentMustBeArray(nameof (array));
      ReadOnlyCollection<Expression> readOnlyCollection = indexes.ToReadOnly<Expression>();
      if (type.GetArrayRank() != readOnlyCollection.Count)
        throw Error.IncorrectNumberOfIndexes();
      foreach (Expression expression in readOnlyCollection)
      {
        ExpressionUtils.RequiresCanRead(expression, nameof (indexes));
        if (expression.Type != typeof (int))
          throw Error.ArgumentMustBeArrayIndexType(nameof (indexes));
      }
      return new IndexExpression(array, (PropertyInfo) null, (IReadOnlyList<Expression>) readOnlyCollection);
    }

    public static IndexExpression Property(
      Expression instance,
      string propertyName,
      params Expression[] arguments)
    {
      ExpressionUtils.RequiresCanRead(instance, nameof (instance));
      ContractUtils.RequiresNotNull((object) propertyName, nameof (propertyName));
      PropertyInfo instanceProperty = Expression.FindInstanceProperty(instance.Type, propertyName, arguments);
      return Expression.MakeIndexProperty(instance, instanceProperty, nameof (propertyName), ((IEnumerable<Expression>) arguments).ToReadOnly<Expression>());
    }

    private static PropertyInfo FindInstanceProperty(
      Type type,
      string propertyName,
      Expression[] arguments)
    {
      BindingFlags flags1 = BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy;
      PropertyInfo property = Expression.FindProperty(type, propertyName, arguments, flags1);
      if (property == (PropertyInfo) null)
      {
        BindingFlags flags2 = BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
        property = Expression.FindProperty(type, propertyName, arguments, flags2);
      }
      if (!(property == (PropertyInfo) null))
        return property;
      if (arguments == null || arguments.Length == 0)
        throw Error.InstancePropertyWithoutParameterNotDefinedForType((object) propertyName, (object) type);
      throw Error.InstancePropertyWithSpecifiedParametersNotDefinedForType((object) propertyName, (object) Expression.GetArgTypesString(arguments), (object) type, nameof (propertyName));
    }

    private static string GetArgTypesString(Expression[] arguments)
    {
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.Append('(');
      for (int index = 0; index < arguments.Length; ++index)
      {
        if (index != 0)
          stringBuilder.Append(", ");
        stringBuilder.Append(arguments[index]?.Type.Name);
      }
      stringBuilder.Append(')');
      return stringBuilder.ToString();
    }

    private static PropertyInfo FindProperty(
      Type type,
      string propertyName,
      Expression[] arguments,
      BindingFlags flags)
    {
      PropertyInfo propertyInfo = (PropertyInfo) null;
      foreach (PropertyInfo property in type.GetProperties(flags))
      {
        if (property.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase) && Expression.IsCompatible(property, arguments))
        {
          if (!(propertyInfo == (PropertyInfo) null))
            throw Error.PropertyWithMoreThanOneMatch((object) propertyName, (object) type);
          propertyInfo = property;
        }
      }
      return propertyInfo;
    }

    private static bool IsCompatible(PropertyInfo pi, Expression[] args)
    {
      MethodInfo getMethod = pi.GetGetMethod(true);
      ParameterInfo[] parameterInfoArray;
      if (getMethod != (MethodInfo) null)
      {
        parameterInfoArray = getMethod.GetParametersCached();
      }
      else
      {
        MethodInfo setMethod = pi.GetSetMethod(true);
        if (setMethod == (MethodInfo) null)
          return false;
        ParameterInfo[] parametersCached = setMethod.GetParametersCached();
        if (parametersCached.Length == 0)
          return false;
        parameterInfoArray = parametersCached.RemoveLast<ParameterInfo>();
      }
      if (args == null)
        return parameterInfoArray.Length == 0;
      if (parameterInfoArray.Length != args.Length)
        return false;
      for (int index = 0; index < args.Length; ++index)
      {
        if (args[index] == null || !TypeUtils.AreReferenceAssignable(parameterInfoArray[index].ParameterType, args[index].Type))
          return false;
      }
      return true;
    }

    public static IndexExpression Property(
      Expression instance,
      PropertyInfo indexer,
      params Expression[] arguments)
    {
      return Expression.Property(instance, indexer, (IEnumerable<Expression>) arguments);
    }

    public static IndexExpression Property(
      Expression instance,
      PropertyInfo indexer,
      IEnumerable<Expression> arguments)
    {
      return Expression.MakeIndexProperty(instance, indexer, nameof (indexer), arguments.ToReadOnly<Expression>());
    }

    private static IndexExpression MakeIndexProperty(
      Expression instance,
      PropertyInfo indexer,
      string paramName,
      ReadOnlyCollection<Expression> argList)
    {
      Expression.ValidateIndexedProperty(instance, indexer, paramName, ref argList);
      return new IndexExpression(instance, indexer, (IReadOnlyList<Expression>) argList);
    }

    private static void ValidateIndexedProperty(
      Expression instance,
      PropertyInfo indexer,
      string paramName,
      ref ReadOnlyCollection<Expression> argList)
    {
      ContractUtils.RequiresNotNull((object) indexer, paramName);
      if (indexer.PropertyType.IsByRef)
        throw Error.PropertyCannotHaveRefType(paramName);
      if (indexer.PropertyType == typeof (void))
        throw Error.PropertyTypeCannotBeVoid(paramName);
      ParameterInfo[] indexes = (ParameterInfo[]) null;
      MethodInfo getMethod = indexer.GetGetMethod(true);
      if (getMethod != (MethodInfo) null)
      {
        if (getMethod.ReturnType != indexer.PropertyType)
          throw Error.PropertyTypeMustMatchGetter(paramName);
        indexes = getMethod.GetParametersCached();
        Expression.ValidateAccessor(instance, getMethod, indexes, ref argList, paramName);
      }
      MethodInfo setMethod = indexer.GetSetMethod(true);
      if (setMethod != (MethodInfo) null)
      {
        ParameterInfo[] parametersCached = setMethod.GetParametersCached();
        if (parametersCached.Length == 0)
          throw Error.SetterHasNoParams(paramName);
        Type parameterType = parametersCached[parametersCached.Length - 1].ParameterType;
        if (parameterType.IsByRef)
          throw Error.PropertyCannotHaveRefType(paramName);
        if (setMethod.ReturnType != typeof (void))
          throw Error.SetterMustBeVoid(paramName);
        if (indexer.PropertyType != parameterType)
          throw Error.PropertyTypeMustMatchSetter(paramName);
        if (getMethod != (MethodInfo) null)
        {
          if (getMethod.IsStatic ^ setMethod.IsStatic)
            throw Error.BothAccessorsMustBeStatic(paramName);
          if (indexes.Length != parametersCached.Length - 1)
            throw Error.IndexesOfSetGetMustMatch(paramName);
          for (int index = 0; index < indexes.Length; ++index)
          {
            if (indexes[index].ParameterType != parametersCached[index].ParameterType)
              throw Error.IndexesOfSetGetMustMatch(paramName);
          }
        }
        else
          Expression.ValidateAccessor(instance, setMethod, parametersCached.RemoveLast<ParameterInfo>(), ref argList, paramName);
      }
      else if (getMethod == (MethodInfo) null)
        throw Error.PropertyDoesNotHaveAccessor((object) indexer, paramName);
    }

    private static void ValidateAccessor(
      Expression instance,
      MethodInfo method,
      ParameterInfo[] indexes,
      ref ReadOnlyCollection<Expression> arguments,
      string paramName)
    {
      ContractUtils.RequiresNotNull((object) arguments, nameof (arguments));
      Expression.ValidateMethodInfo(method, nameof (method));
      if ((method.CallingConvention & CallingConventions.VarArgs) != (CallingConventions) 0)
        throw Error.AccessorsCannotHaveVarArgs(paramName);
      if (method.IsStatic)
      {
        if (instance != null)
          throw Error.OnlyStaticPropertiesHaveNullInstance(nameof (instance));
      }
      else
      {
        if (instance == null)
          throw Error.OnlyStaticPropertiesHaveNullInstance(nameof (instance));
        ExpressionUtils.RequiresCanRead(instance, nameof (instance));
        Expression.ValidateCallInstanceType(instance.Type, method);
      }
      Expression.ValidateAccessorArgumentTypes(method, indexes, ref arguments, paramName);
    }

    private static void ValidateAccessorArgumentTypes(
      MethodInfo method,
      ParameterInfo[] indexes,
      ref ReadOnlyCollection<Expression> arguments,
      string paramName)
    {
      if (indexes.Length != 0)
      {
        if (indexes.Length != arguments.Count)
          throw Error.IncorrectNumberOfMethodCallArguments((object) method, paramName);
        Expression[] expressionArray = (Expression[]) null;
        int index1 = 0;
        for (int length = indexes.Length; index1 < length; ++index1)
        {
          Expression expression = arguments[index1];
          ParameterInfo index2 = indexes[index1];
          ExpressionUtils.RequiresCanRead(expression, nameof (arguments), index1);
          Type parameterType = index2.ParameterType;
          if (parameterType.IsByRef)
            throw Error.AccessorsCannotHaveByRefArgs(nameof (indexes), index1);
          TypeUtils.ValidateType(parameterType, nameof (indexes), index1);
          if (!TypeUtils.AreReferenceAssignable(parameterType, expression.Type) && !Expression.TryQuote(parameterType, ref expression))
            throw Error.ExpressionTypeDoesNotMatchMethodParameter((object) expression.Type, (object) parameterType, (object) method, nameof (arguments), index1);
          if (expressionArray == null && expression != arguments[index1])
          {
            expressionArray = new Expression[arguments.Count];
            for (int index3 = 0; index3 < index1; ++index3)
              expressionArray[index3] = arguments[index3];
          }
          if (expressionArray != null)
            expressionArray[index1] = expression;
        }
        if (expressionArray == null)
          return;
        arguments = (ReadOnlyCollection<Expression>) new TrueReadOnlyCollection<Expression>(expressionArray);
      }
      else if (arguments.Count > 0)
        throw Error.IncorrectNumberOfMethodCallArguments((object) method, paramName);
    }

    internal static InvocationExpression Invoke(Expression expression)
    {
      ExpressionUtils.RequiresCanRead(expression, nameof (expression));
      MethodInfo invokeMethod = Expression.GetInvokeMethod(expression);
      ParameterInfo[] parametersForValidation = Expression.GetParametersForValidation((MethodBase) invokeMethod, ExpressionType.Invoke);
      Expression.ValidateArgumentCount((MethodBase) invokeMethod, ExpressionType.Invoke, 0, parametersForValidation);
      return (InvocationExpression) new InvocationExpression0(expression, invokeMethod.ReturnType);
    }

    internal static InvocationExpression Invoke(
      Expression expression,
      Expression arg0)
    {
      ExpressionUtils.RequiresCanRead(expression, nameof (expression));
      MethodInfo invokeMethod = Expression.GetInvokeMethod(expression);
      ParameterInfo[] parametersForValidation = Expression.GetParametersForValidation((MethodBase) invokeMethod, ExpressionType.Invoke);
      Expression.ValidateArgumentCount((MethodBase) invokeMethod, ExpressionType.Invoke, 1, parametersForValidation);
      arg0 = Expression.ValidateOneArgument((MethodBase) invokeMethod, ExpressionType.Invoke, arg0, parametersForValidation[0], nameof (expression), nameof (arg0));
      return (InvocationExpression) new InvocationExpression1(expression, invokeMethod.ReturnType, arg0);
    }

    internal static InvocationExpression Invoke(
      Expression expression,
      Expression arg0,
      Expression arg1)
    {
      ExpressionUtils.RequiresCanRead(expression, nameof (expression));
      MethodInfo invokeMethod = Expression.GetInvokeMethod(expression);
      ParameterInfo[] parametersForValidation = Expression.GetParametersForValidation((MethodBase) invokeMethod, ExpressionType.Invoke);
      Expression.ValidateArgumentCount((MethodBase) invokeMethod, ExpressionType.Invoke, 2, parametersForValidation);
      arg0 = Expression.ValidateOneArgument((MethodBase) invokeMethod, ExpressionType.Invoke, arg0, parametersForValidation[0], nameof (expression), nameof (arg0));
      arg1 = Expression.ValidateOneArgument((MethodBase) invokeMethod, ExpressionType.Invoke, arg1, parametersForValidation[1], nameof (expression), nameof (arg1));
      return (InvocationExpression) new InvocationExpression2(expression, invokeMethod.ReturnType, arg0, arg1);
    }

    internal static InvocationExpression Invoke(
      Expression expression,
      Expression arg0,
      Expression arg1,
      Expression arg2)
    {
      ExpressionUtils.RequiresCanRead(expression, nameof (expression));
      MethodInfo invokeMethod = Expression.GetInvokeMethod(expression);
      ParameterInfo[] parametersForValidation = Expression.GetParametersForValidation((MethodBase) invokeMethod, ExpressionType.Invoke);
      Expression.ValidateArgumentCount((MethodBase) invokeMethod, ExpressionType.Invoke, 3, parametersForValidation);
      arg0 = Expression.ValidateOneArgument((MethodBase) invokeMethod, ExpressionType.Invoke, arg0, parametersForValidation[0], nameof (expression), nameof (arg0));
      arg1 = Expression.ValidateOneArgument((MethodBase) invokeMethod, ExpressionType.Invoke, arg1, parametersForValidation[1], nameof (expression), nameof (arg1));
      arg2 = Expression.ValidateOneArgument((MethodBase) invokeMethod, ExpressionType.Invoke, arg2, parametersForValidation[2], nameof (expression), nameof (arg2));
      return (InvocationExpression) new InvocationExpression3(expression, invokeMethod.ReturnType, arg0, arg1, arg2);
    }

    internal static InvocationExpression Invoke(
      Expression expression,
      Expression arg0,
      Expression arg1,
      Expression arg2,
      Expression arg3)
    {
      ExpressionUtils.RequiresCanRead(expression, nameof (expression));
      MethodInfo invokeMethod = Expression.GetInvokeMethod(expression);
      ParameterInfo[] parametersForValidation = Expression.GetParametersForValidation((MethodBase) invokeMethod, ExpressionType.Invoke);
      Expression.ValidateArgumentCount((MethodBase) invokeMethod, ExpressionType.Invoke, 4, parametersForValidation);
      arg0 = Expression.ValidateOneArgument((MethodBase) invokeMethod, ExpressionType.Invoke, arg0, parametersForValidation[0], nameof (expression), nameof (arg0));
      arg1 = Expression.ValidateOneArgument((MethodBase) invokeMethod, ExpressionType.Invoke, arg1, parametersForValidation[1], nameof (expression), nameof (arg1));
      arg2 = Expression.ValidateOneArgument((MethodBase) invokeMethod, ExpressionType.Invoke, arg2, parametersForValidation[2], nameof (expression), nameof (arg2));
      arg3 = Expression.ValidateOneArgument((MethodBase) invokeMethod, ExpressionType.Invoke, arg3, parametersForValidation[3], nameof (expression), nameof (arg3));
      return (InvocationExpression) new InvocationExpression4(expression, invokeMethod.ReturnType, arg0, arg1, arg2, arg3);
    }

    internal static InvocationExpression Invoke(
      Expression expression,
      Expression arg0,
      Expression arg1,
      Expression arg2,
      Expression arg3,
      Expression arg4)
    {
      ExpressionUtils.RequiresCanRead(expression, nameof (expression));
      MethodInfo invokeMethod = Expression.GetInvokeMethod(expression);
      ParameterInfo[] parametersForValidation = Expression.GetParametersForValidation((MethodBase) invokeMethod, ExpressionType.Invoke);
      Expression.ValidateArgumentCount((MethodBase) invokeMethod, ExpressionType.Invoke, 5, parametersForValidation);
      arg0 = Expression.ValidateOneArgument((MethodBase) invokeMethod, ExpressionType.Invoke, arg0, parametersForValidation[0], nameof (expression), nameof (arg0));
      arg1 = Expression.ValidateOneArgument((MethodBase) invokeMethod, ExpressionType.Invoke, arg1, parametersForValidation[1], nameof (expression), nameof (arg1));
      arg2 = Expression.ValidateOneArgument((MethodBase) invokeMethod, ExpressionType.Invoke, arg2, parametersForValidation[2], nameof (expression), nameof (arg2));
      arg3 = Expression.ValidateOneArgument((MethodBase) invokeMethod, ExpressionType.Invoke, arg3, parametersForValidation[3], nameof (expression), nameof (arg3));
      arg4 = Expression.ValidateOneArgument((MethodBase) invokeMethod, ExpressionType.Invoke, arg4, parametersForValidation[4], nameof (expression), nameof (arg4));
      return (InvocationExpression) new InvocationExpression5(expression, invokeMethod.ReturnType, arg0, arg1, arg2, arg3, arg4);
    }

    public static InvocationExpression Invoke(
      Expression expression,
      params Expression[] arguments)
    {
      return Expression.Invoke(expression, (IEnumerable<Expression>) arguments);
    }

    public static InvocationExpression Invoke(
      Expression expression,
      IEnumerable<Expression> arguments)
    {
      IReadOnlyList<Expression> enumerable = arguments as IReadOnlyList<Expression> ?? (IReadOnlyList<Expression>) arguments.ToReadOnly<Expression>();
      switch (enumerable.Count)
      {
        case 0:
          return Expression.Invoke(expression);
        case 1:
          return Expression.Invoke(expression, enumerable[0]);
        case 2:
          return Expression.Invoke(expression, enumerable[0], enumerable[1]);
        case 3:
          return Expression.Invoke(expression, enumerable[0], enumerable[1], enumerable[2]);
        case 4:
          return Expression.Invoke(expression, enumerable[0], enumerable[1], enumerable[2], enumerable[3]);
        case 5:
          return Expression.Invoke(expression, enumerable[0], enumerable[1], enumerable[2], enumerable[3], enumerable[4]);
        default:
          ExpressionUtils.RequiresCanRead(expression, nameof (expression));
          ReadOnlyCollection<Expression> arguments1 = enumerable.ToReadOnly<Expression>();
          MethodInfo invokeMethod = Expression.GetInvokeMethod(expression);
          Expression.ValidateArgumentTypes((MethodBase) invokeMethod, ExpressionType.Invoke, ref arguments1, nameof (expression));
          return (InvocationExpression) new InvocationExpressionN(expression, (IReadOnlyList<Expression>) arguments1, invokeMethod.ReturnType);
      }
    }

    internal static MethodInfo GetInvokeMethod(Expression expression)
    {
      Type delegateType = expression.Type;
      if (!expression.Type.IsSubclassOf(typeof (MulticastDelegate)))
      {
        Type genericType = TypeUtils.FindGenericType(typeof (Expression<>), expression.Type);
        if (genericType == (Type) null)
          throw Error.ExpressionTypeNotInvocable((object) expression.Type, nameof (expression));
        delegateType = genericType.GetGenericArguments()[0];
      }
      return delegateType.GetInvokeMethod();
    }

    public static LabelExpression Label(LabelTarget target)
    {
      return Expression.Label(target, (Expression) null);
    }

    public static LabelExpression Label(LabelTarget target, Expression defaultValue)
    {
      Expression.ValidateGoto(target, ref defaultValue, nameof (target), nameof (defaultValue), (Type) null);
      return new LabelExpression(target, defaultValue);
    }

    public static LabelTarget Label()
    {
      return Expression.Label(typeof (void), (string) null);
    }

    public static LabelTarget Label(string name)
    {
      return Expression.Label(typeof (void), name);
    }

    public static LabelTarget Label(Type type)
    {
      return Expression.Label(type, (string) null);
    }

    public static LabelTarget Label(Type type, string name)
    {
      ContractUtils.RequiresNotNull((object) type, nameof (type));
      TypeUtils.ValidateType(type, nameof (type));
      return new LabelTarget(type, name);
    }

    internal static LambdaExpression CreateLambda(
      Type delegateType,
      Expression body,
      string name,
      bool tailCall,
      ReadOnlyCollection<ParameterExpression> parameters)
    {
      CacheDict<Type, Func<Expression, string, bool, ReadOnlyCollection<ParameterExpression>, LambdaExpression>> cacheDict = Expression.s_lambdaFactories;
      if (cacheDict == null)
        Expression.s_lambdaFactories = cacheDict = new CacheDict<Type, Func<Expression, string, bool, ReadOnlyCollection<ParameterExpression>, LambdaExpression>>(50);
      Func<Expression, string, bool, ReadOnlyCollection<ParameterExpression>, LambdaExpression> func;
      if (!cacheDict.TryGetValue(delegateType, out func))
      {
        MethodInfo method = typeof (Expression<>).MakeGenericType(delegateType).GetMethod("Create", BindingFlags.Static | BindingFlags.NonPublic);
        if (delegateType.IsCollectible)
          return (LambdaExpression) method.Invoke((object) null, new object[4]
          {
            (object) body,
            (object) name,
            (object) tailCall,
            (object) parameters
          });
        cacheDict[delegateType] = func = (Func<Expression, string, bool, ReadOnlyCollection<ParameterExpression>, LambdaExpression>) method.CreateDelegate(typeof (Func<Expression, string, bool, ReadOnlyCollection<ParameterExpression>, LambdaExpression>));
      }
      return func(body, name, tailCall, parameters);
    }

    public static Expression<TDelegate> Lambda<TDelegate>(
      Expression body,
      params ParameterExpression[] parameters)
    {
      return Expression.Lambda<TDelegate>(body, false, (IEnumerable<ParameterExpression>) parameters);
    }

    public static Expression<TDelegate> Lambda<TDelegate>(
      Expression body,
      bool tailCall,
      params ParameterExpression[] parameters)
    {
      return Expression.Lambda<TDelegate>(body, tailCall, (IEnumerable<ParameterExpression>) parameters);
    }

    public static Expression<TDelegate> Lambda<TDelegate>(
      Expression body,
      IEnumerable<ParameterExpression> parameters)
    {
      return Expression.Lambda<TDelegate>(body, (string) null, false, parameters);
    }

    public static Expression<TDelegate> Lambda<TDelegate>(
      Expression body,
      bool tailCall,
      IEnumerable<ParameterExpression> parameters)
    {
      return Expression.Lambda<TDelegate>(body, (string) null, tailCall, parameters);
    }

    public static Expression<TDelegate> Lambda<TDelegate>(
      Expression body,
      string name,
      IEnumerable<ParameterExpression> parameters)
    {
      return Expression.Lambda<TDelegate>(body, name, false, parameters);
    }

    public static Expression<TDelegate> Lambda<TDelegate>(
      Expression body,
      string name,
      bool tailCall,
      IEnumerable<ParameterExpression> parameters)
    {
      ReadOnlyCollection<ParameterExpression> parameters1 = parameters.ToReadOnly<ParameterExpression>();
      Expression.ValidateLambdaArgs(typeof (TDelegate), ref body, parameters1, nameof (TDelegate));
      return (Expression<TDelegate>) Expression.CreateLambda(typeof (TDelegate), body, name, tailCall, parameters1);
    }

    public static LambdaExpression Lambda(
      Expression body,
      params ParameterExpression[] parameters)
    {
      return Expression.Lambda(body, false, (IEnumerable<ParameterExpression>) parameters);
    }

    public static LambdaExpression Lambda(
      Expression body,
      bool tailCall,
      params ParameterExpression[] parameters)
    {
      return Expression.Lambda(body, tailCall, (IEnumerable<ParameterExpression>) parameters);
    }

    public static LambdaExpression Lambda(
      Expression body,
      IEnumerable<ParameterExpression> parameters)
    {
      return Expression.Lambda(body, (string) null, false, parameters);
    }

    public static LambdaExpression Lambda(
      Expression body,
      bool tailCall,
      IEnumerable<ParameterExpression> parameters)
    {
      return Expression.Lambda(body, (string) null, tailCall, parameters);
    }

    public static LambdaExpression Lambda(
      Type delegateType,
      Expression body,
      params ParameterExpression[] parameters)
    {
      return Expression.Lambda(delegateType, body, (string) null, false, (IEnumerable<ParameterExpression>) parameters);
    }

    public static LambdaExpression Lambda(
      Type delegateType,
      Expression body,
      bool tailCall,
      params ParameterExpression[] parameters)
    {
      return Expression.Lambda(delegateType, body, (string) null, tailCall, (IEnumerable<ParameterExpression>) parameters);
    }

    public static LambdaExpression Lambda(
      Type delegateType,
      Expression body,
      IEnumerable<ParameterExpression> parameters)
    {
      return Expression.Lambda(delegateType, body, (string) null, false, parameters);
    }

    public static LambdaExpression Lambda(
      Type delegateType,
      Expression body,
      bool tailCall,
      IEnumerable<ParameterExpression> parameters)
    {
      return Expression.Lambda(delegateType, body, (string) null, tailCall, parameters);
    }

    public static LambdaExpression Lambda(
      Expression body,
      string name,
      IEnumerable<ParameterExpression> parameters)
    {
      return Expression.Lambda(body, name, false, parameters);
    }

    public static LambdaExpression Lambda(
      Expression body,
      string name,
      bool tailCall,
      IEnumerable<ParameterExpression> parameters)
    {
      ContractUtils.RequiresNotNull((object) body, nameof (body));
      ReadOnlyCollection<ParameterExpression> parameters1 = parameters.ToReadOnly<ParameterExpression>();
      int count = parameters1.Count;
      Type[] types = new Type[count + 1];
      if (count > 0)
      {
        HashSet<ParameterExpression> parameterExpressionSet = new HashSet<ParameterExpression>();
        for (int index = 0; index < count; ++index)
        {
          ParameterExpression parameterExpression = parameters1[index];
          ContractUtils.RequiresNotNull((object) parameterExpression, "parameter");
          types[index] = parameterExpression.IsByRef ? parameterExpression.Type.MakeByRefType() : parameterExpression.Type;
          if (!parameterExpressionSet.Add(parameterExpression))
            throw Error.DuplicateVariable((object) parameterExpression, nameof (parameters), index);
        }
      }
      types[count] = body.Type;
      return Expression.CreateLambda(System.Linq.Expressions.Compiler.DelegateHelpers.MakeDelegateType(types), body, name, tailCall, parameters1);
    }

    public static LambdaExpression Lambda(
      Type delegateType,
      Expression body,
      string name,
      IEnumerable<ParameterExpression> parameters)
    {
      ReadOnlyCollection<ParameterExpression> parameters1 = parameters.ToReadOnly<ParameterExpression>();
      Expression.ValidateLambdaArgs(delegateType, ref body, parameters1, nameof (delegateType));
      return Expression.CreateLambda(delegateType, body, name, false, parameters1);
    }

    public static LambdaExpression Lambda(
      Type delegateType,
      Expression body,
      string name,
      bool tailCall,
      IEnumerable<ParameterExpression> parameters)
    {
      ReadOnlyCollection<ParameterExpression> parameters1 = parameters.ToReadOnly<ParameterExpression>();
      Expression.ValidateLambdaArgs(delegateType, ref body, parameters1, nameof (delegateType));
      return Expression.CreateLambda(delegateType, body, name, tailCall, parameters1);
    }

    private static void ValidateLambdaArgs(
      Type delegateType,
      ref Expression body,
      ReadOnlyCollection<ParameterExpression> parameters,
      string paramName)
    {
      ContractUtils.RequiresNotNull((object) delegateType, nameof (delegateType));
      ExpressionUtils.RequiresCanRead(body, nameof (body));
      if (!typeof (MulticastDelegate).IsAssignableFrom(delegateType) || delegateType == typeof (MulticastDelegate))
        throw Error.LambdaTypeMustBeDerivedFromSystemDelegate(paramName);
      TypeUtils.ValidateType(delegateType, nameof (delegateType), true, true);
      CacheDict<Type, MethodInfo> lambdaDelegateCache = Expression.s_lambdaDelegateCache;
      MethodInfo invokeMethod;
      if (!lambdaDelegateCache.TryGetValue(delegateType, out invokeMethod))
      {
        invokeMethod = delegateType.GetInvokeMethod();
        if (!delegateType.IsCollectible)
          lambdaDelegateCache[delegateType] = invokeMethod;
      }
      ParameterInfo[] parametersCached = invokeMethod.GetParametersCached();
      if (parametersCached.Length != 0)
      {
        if (parametersCached.Length != parameters.Count)
          throw Error.IncorrectNumberOfLambdaDeclarationParameters();
        HashSet<ParameterExpression> parameterExpressionSet = new HashSet<ParameterExpression>();
        int index = 0;
        for (int length = parametersCached.Length; index < length; ++index)
        {
          ParameterExpression parameter = parameters[index];
          ParameterInfo parameterInfo = parametersCached[index];
          ExpressionUtils.RequiresCanRead((Expression) parameter, nameof (parameters), index);
          Type src = parameterInfo.ParameterType;
          if (parameter.IsByRef)
          {
            if (!src.IsByRef)
              throw Error.ParameterExpressionNotValidAsDelegate((object) parameter.Type.MakeByRefType(), (object) src);
            src = src.GetElementType();
          }
          if (!TypeUtils.AreReferenceAssignable(parameter.Type, src))
            throw Error.ParameterExpressionNotValidAsDelegate((object) parameter.Type, (object) src);
          if (!parameterExpressionSet.Add(parameter))
            throw Error.DuplicateVariable((object) parameter, nameof (parameters), index);
        }
      }
      else if (parameters.Count > 0)
        throw Error.IncorrectNumberOfLambdaDeclarationParameters();
      if (invokeMethod.ReturnType != typeof (void) && !TypeUtils.AreReferenceAssignable(invokeMethod.ReturnType, body.Type) && !Expression.TryQuote(invokeMethod.ReturnType, ref body))
        throw Error.ExpressionTypeDoesNotMatchReturn((object) body.Type, (object) invokeMethod.ReturnType);
    }

    private static Expression.TryGetFuncActionArgsResult ValidateTryGetFuncActionArgs(
      Type[] typeArgs)
    {
      if (typeArgs == null)
        return Expression.TryGetFuncActionArgsResult.ArgumentNull;
      for (int index = 0; index < typeArgs.Length; ++index)
      {
        Type typeArg = typeArgs[index];
        if (typeArg == (Type) null)
          return Expression.TryGetFuncActionArgsResult.ArgumentNull;
        if (typeArg.IsByRef)
          return Expression.TryGetFuncActionArgsResult.ByRef;
        if (typeArg == typeof (void) || typeArg.IsPointer)
          return Expression.TryGetFuncActionArgsResult.PointerOrVoid;
      }
      return Expression.TryGetFuncActionArgsResult.Valid;
    }

    public static Type GetFuncType(params Type[] typeArgs)
    {
      switch (Expression.ValidateTryGetFuncActionArgs(typeArgs))
      {
        case Expression.TryGetFuncActionArgsResult.ArgumentNull:
          throw new ArgumentNullException(nameof (typeArgs));
        case Expression.TryGetFuncActionArgsResult.ByRef:
          throw Error.TypeMustNotBeByRef(nameof (typeArgs));
        default:
          Type funcType = System.Linq.Expressions.Compiler.DelegateHelpers.GetFuncType(typeArgs);
          if (funcType == (Type) null)
            throw Error.IncorrectNumberOfTypeArgsForFunc(nameof (typeArgs));
          return funcType;
      }
    }

    public static bool TryGetFuncType(Type[] typeArgs, out Type funcType)
    {
      if (Expression.ValidateTryGetFuncActionArgs(typeArgs) == Expression.TryGetFuncActionArgsResult.Valid)
        return (funcType = System.Linq.Expressions.Compiler.DelegateHelpers.GetFuncType(typeArgs)) != (Type) null;
      funcType = (Type) null;
      return false;
    }

    public static Type GetActionType(params Type[] typeArgs)
    {
      switch (Expression.ValidateTryGetFuncActionArgs(typeArgs))
      {
        case Expression.TryGetFuncActionArgsResult.ArgumentNull:
          throw new ArgumentNullException(nameof (typeArgs));
        case Expression.TryGetFuncActionArgsResult.ByRef:
          throw Error.TypeMustNotBeByRef(nameof (typeArgs));
        default:
          Type actionType = System.Linq.Expressions.Compiler.DelegateHelpers.GetActionType(typeArgs);
          if (actionType == (Type) null)
            throw Error.IncorrectNumberOfTypeArgsForAction(nameof (typeArgs));
          return actionType;
      }
    }

    public static bool TryGetActionType(Type[] typeArgs, out Type actionType)
    {
      if (Expression.ValidateTryGetFuncActionArgs(typeArgs) == Expression.TryGetFuncActionArgsResult.Valid)
        return (actionType = System.Linq.Expressions.Compiler.DelegateHelpers.GetActionType(typeArgs)) != (Type) null;
      actionType = (Type) null;
      return false;
    }

    public static Type GetDelegateType(params Type[] typeArgs)
    {
      ContractUtils.RequiresNotEmpty<Type>((ICollection<Type>) typeArgs, nameof (typeArgs));
      ContractUtils.RequiresNotNullItems<Type>((IList<Type>) typeArgs, nameof (typeArgs));
      return System.Linq.Expressions.Compiler.DelegateHelpers.MakeDelegateType(typeArgs);
    }

    public static ListInitExpression ListInit(
      NewExpression newExpression,
      params Expression[] initializers)
    {
      return Expression.ListInit(newExpression, (IEnumerable<Expression>) initializers);
    }

    public static ListInitExpression ListInit(
      NewExpression newExpression,
      IEnumerable<Expression> initializers)
    {
      ContractUtils.RequiresNotNull((object) newExpression, nameof (newExpression));
      ContractUtils.RequiresNotNull((object) initializers, nameof (initializers));
      ReadOnlyCollection<Expression> readOnlyCollection = initializers.ToReadOnly<Expression>();
      if (readOnlyCollection.Count == 0)
        return new ListInitExpression(newExpression, EmptyReadOnlyCollection<ElementInit>.Instance);
      MethodInfo method = Expression.FindMethod(newExpression.Type, "Add", (Type[]) null, new Expression[1]
      {
        readOnlyCollection[0]
      }, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
      return Expression.ListInit(newExpression, method, (IEnumerable<Expression>) readOnlyCollection);
    }

    public static ListInitExpression ListInit(
      NewExpression newExpression,
      MethodInfo addMethod,
      params Expression[] initializers)
    {
      return Expression.ListInit(newExpression, addMethod, (IEnumerable<Expression>) initializers);
    }

    public static ListInitExpression ListInit(
      NewExpression newExpression,
      MethodInfo addMethod,
      IEnumerable<Expression> initializers)
    {
      if (addMethod == (MethodInfo) null)
        return Expression.ListInit(newExpression, initializers);
      ContractUtils.RequiresNotNull((object) newExpression, nameof (newExpression));
      ContractUtils.RequiresNotNull((object) initializers, nameof (initializers));
      ReadOnlyCollection<Expression> readOnlyCollection = initializers.ToReadOnly<Expression>();
      ElementInit[] elementInitArray = new ElementInit[readOnlyCollection.Count];
      for (int index = 0; index < readOnlyCollection.Count; ++index)
        elementInitArray[index] = Expression.ElementInit(addMethod, readOnlyCollection[index]);
      return Expression.ListInit(newExpression, (IEnumerable<ElementInit>) new TrueReadOnlyCollection<ElementInit>(elementInitArray));
    }

    public static ListInitExpression ListInit(
      NewExpression newExpression,
      params ElementInit[] initializers)
    {
      return Expression.ListInit(newExpression, (IEnumerable<ElementInit>) initializers);
    }

    public static ListInitExpression ListInit(
      NewExpression newExpression,
      IEnumerable<ElementInit> initializers)
    {
      ContractUtils.RequiresNotNull((object) newExpression, nameof (newExpression));
      ContractUtils.RequiresNotNull((object) initializers, nameof (initializers));
      ReadOnlyCollection<ElementInit> initializers1 = initializers.ToReadOnly<ElementInit>();
      Expression.ValidateListInitArgs(newExpression.Type, initializers1, nameof (newExpression));
      return new ListInitExpression(newExpression, initializers1);
    }

    public static LoopExpression Loop(Expression body)
    {
      return Expression.Loop(body, (LabelTarget) null);
    }

    public static LoopExpression Loop(Expression body, LabelTarget @break)
    {
      return Expression.Loop(body, @break, (LabelTarget) null);
    }

    public static LoopExpression Loop(
      Expression body,
      LabelTarget @break,
      LabelTarget @continue)
    {
      ExpressionUtils.RequiresCanRead(body, nameof (body));
      if (@continue != null && @continue.Type != typeof (void))
        throw Error.LabelTypeMustBeVoid(nameof (@continue));
      return new LoopExpression(body, @break, @continue);
    }

    public static MemberAssignment Bind(MemberInfo member, Expression expression)
    {
      ContractUtils.RequiresNotNull((object) member, nameof (member));
      ExpressionUtils.RequiresCanRead(expression, nameof (expression));
      Type memberType;
      Expression.ValidateSettableFieldOrPropertyMember(member, out memberType);
      if (!memberType.IsAssignableFrom(expression.Type))
        throw Error.ArgumentTypesMustMatch();
      return new MemberAssignment(member, expression);
    }

    public static MemberAssignment Bind(
      MethodInfo propertyAccessor,
      Expression expression)
    {
      ContractUtils.RequiresNotNull((object) propertyAccessor, nameof (propertyAccessor));
      ContractUtils.RequiresNotNull((object) expression, nameof (expression));
      Expression.ValidateMethodInfo(propertyAccessor, nameof (propertyAccessor));
      return Expression.Bind((MemberInfo) Expression.GetProperty(propertyAccessor, nameof (propertyAccessor), -1), expression);
    }

    private static void ValidateSettableFieldOrPropertyMember(
      MemberInfo member,
      out Type memberType)
    {
      Type declaringType = member.DeclaringType;
      if (declaringType == (Type) null)
        throw Error.NotAMemberOfAnyType((object) member, nameof (member));
      TypeUtils.ValidateType(declaringType, (string) null);
      MemberInfo memberInfo = member;
      if ((object) memberInfo != null)
      {
        if (!(memberInfo is PropertyInfo propertyInfo))
        {
          if (memberInfo is FieldInfo fieldInfo)
          {
            memberType = fieldInfo.FieldType;
            return;
          }
        }
        else
        {
          if (!propertyInfo.CanWrite)
            throw Error.PropertyDoesNotHaveSetter((object) propertyInfo, nameof (member));
          memberType = propertyInfo.PropertyType;
          return;
        }
      }
      throw Error.ArgumentMustBeFieldInfoOrPropertyInfo(nameof (member));
    }

    public static MemberExpression Field(Expression expression, FieldInfo field)
    {
      ContractUtils.RequiresNotNull((object) field, nameof (field));
      if (field.IsStatic)
      {
        if (expression != null)
          throw Error.OnlyStaticFieldsHaveNullInstance(nameof (expression));
      }
      else
      {
        if (expression == null)
          throw Error.OnlyStaticFieldsHaveNullInstance(nameof (field));
        ExpressionUtils.RequiresCanRead(expression, nameof (expression));
        if (!TypeUtils.AreReferenceAssignable(field.DeclaringType, expression.Type))
          throw Error.FieldInfoNotDefinedForType((object) field.DeclaringType, (object) field.Name, (object) expression.Type);
      }
      return (MemberExpression) MemberExpression.Make(expression, field);
    }

    public static MemberExpression Field(Expression expression, string fieldName)
    {
      ExpressionUtils.RequiresCanRead(expression, nameof (expression));
      ContractUtils.RequiresNotNull((object) fieldName, nameof (fieldName));
      FieldInfo field1 = expression.Type.GetField(fieldName, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
      if ((object) field1 == null)
        field1 = expression.Type.GetField(fieldName, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
      FieldInfo field2 = field1;
      if (field2 == (FieldInfo) null)
        throw Error.InstanceFieldNotDefinedForType((object) fieldName, (object) expression.Type);
      return Expression.Field(expression, field2);
    }

    public static MemberExpression Field(
      Expression expression,
      Type type,
      string fieldName)
    {
      ContractUtils.RequiresNotNull((object) type, nameof (type));
      FieldInfo field1 = type.GetField(fieldName, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
      if ((object) field1 == null)
        field1 = type.GetField(fieldName, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
      FieldInfo field2 = field1;
      if (field2 == (FieldInfo) null)
        throw Error.FieldNotDefinedForType((object) fieldName, (object) type);
      return Expression.Field(expression, field2);
    }

    public static MemberExpression Property(
      Expression expression,
      string propertyName)
    {
      ExpressionUtils.RequiresCanRead(expression, nameof (expression));
      ContractUtils.RequiresNotNull((object) propertyName, nameof (propertyName));
      PropertyInfo property1 = expression.Type.GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
      if ((object) property1 == null)
        property1 = expression.Type.GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
      PropertyInfo property2 = property1;
      if (property2 == (PropertyInfo) null)
        throw Error.InstancePropertyNotDefinedForType((object) propertyName, (object) expression.Type, nameof (propertyName));
      return Expression.Property(expression, property2);
    }

    public static MemberExpression Property(
      Expression expression,
      Type type,
      string propertyName)
    {
      ContractUtils.RequiresNotNull((object) type, nameof (type));
      ContractUtils.RequiresNotNull((object) propertyName, nameof (propertyName));
      PropertyInfo property1 = type.GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
      if ((object) property1 == null)
        property1 = type.GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
      PropertyInfo property2 = property1;
      if (property2 == (PropertyInfo) null)
        throw Error.PropertyNotDefinedForType((object) propertyName, (object) type, nameof (propertyName));
      return Expression.Property(expression, property2);
    }

    public static MemberExpression Property(
      Expression expression,
      PropertyInfo property)
    {
      ContractUtils.RequiresNotNull((object) property, nameof (property));
      MethodInfo method = property.GetGetMethod(true);
      if (method == (MethodInfo) null)
      {
        method = property.GetSetMethod(true);
        if (method == (MethodInfo) null)
          throw Error.PropertyDoesNotHaveAccessor((object) property, nameof (property));
        if (method.GetParametersCached().Length != 1)
          throw Error.IncorrectNumberOfMethodCallArguments((object) method, nameof (property));
      }
      else if (method.GetParametersCached().Length != 0)
        throw Error.IncorrectNumberOfMethodCallArguments((object) method, nameof (property));
      if (method.IsStatic)
      {
        if (expression != null)
          throw Error.OnlyStaticPropertiesHaveNullInstance(nameof (expression));
      }
      else
      {
        if (expression == null)
          throw Error.OnlyStaticPropertiesHaveNullInstance(nameof (property));
        ExpressionUtils.RequiresCanRead(expression, nameof (expression));
        if (!TypeUtils.IsValidInstanceType((MemberInfo) property, expression.Type))
          throw Error.PropertyNotDefinedForType((object) property, (object) expression.Type, nameof (property));
      }
      Expression.ValidateMethodInfo(method, nameof (property));
      return (MemberExpression) MemberExpression.Make(expression, property);
    }

    public static MemberExpression Property(
      Expression expression,
      MethodInfo propertyAccessor)
    {
      ContractUtils.RequiresNotNull((object) propertyAccessor, nameof (propertyAccessor));
      Expression.ValidateMethodInfo(propertyAccessor, nameof (propertyAccessor));
      return Expression.Property(expression, Expression.GetProperty(propertyAccessor, nameof (propertyAccessor), -1));
    }

    private static PropertyInfo GetProperty(MethodInfo mi, string paramName, int index = -1)
    {
      Type declaringType = mi.DeclaringType;
      if (declaringType != (Type) null)
      {
        BindingFlags bindingAttr = (BindingFlags) (48 | (mi.IsStatic ? 8 : 4));
        foreach (PropertyInfo property in declaringType.GetProperties(bindingAttr))
        {
          if (property.CanRead && Expression.CheckMethod(mi, property.GetGetMethod(true)) || property.CanWrite && Expression.CheckMethod(mi, property.GetSetMethod(true)))
            return property;
        }
      }
      throw Error.MethodNotPropertyAccessor((object) mi.DeclaringType, (object) mi.Name, paramName, index);
    }

    private static bool CheckMethod(MethodInfo method, MethodInfo propertyMethod)
    {
      if (method.Equals((object) propertyMethod))
        return true;
      Type declaringType = method.DeclaringType;
      return declaringType.IsInterface && method.Name == propertyMethod.Name && declaringType.GetMethod(method.Name) == propertyMethod;
    }

    public static MemberExpression PropertyOrField(
      Expression expression,
      string propertyOrFieldName)
    {
      ExpressionUtils.RequiresCanRead(expression, nameof (expression));
      PropertyInfo property1 = expression.Type.GetProperty(propertyOrFieldName, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
      if (property1 != (PropertyInfo) null)
        return Expression.Property(expression, property1);
      FieldInfo field1 = expression.Type.GetField(propertyOrFieldName, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
      if (field1 != (FieldInfo) null)
        return Expression.Field(expression, field1);
      PropertyInfo property2 = expression.Type.GetProperty(propertyOrFieldName, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
      if (property2 != (PropertyInfo) null)
        return Expression.Property(expression, property2);
      FieldInfo field2 = expression.Type.GetField(propertyOrFieldName, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
      if (field2 != (FieldInfo) null)
        return Expression.Field(expression, field2);
      throw Error.NotAMemberOfType((object) propertyOrFieldName, (object) expression.Type, nameof (propertyOrFieldName));
    }

    public static MemberExpression MakeMemberAccess(
      Expression expression,
      MemberInfo member)
    {
      ContractUtils.RequiresNotNull((object) member, nameof (member));
      FieldInfo field = member as FieldInfo;
      if (field != (FieldInfo) null)
        return Expression.Field(expression, field);
      PropertyInfo property = member as PropertyInfo;
      if (property != (PropertyInfo) null)
        return Expression.Property(expression, property);
      throw Error.MemberNotFieldOrProperty((object) member, nameof (member));
    }

    public static MemberInitExpression MemberInit(
      NewExpression newExpression,
      params MemberBinding[] bindings)
    {
      return Expression.MemberInit(newExpression, (IEnumerable<MemberBinding>) bindings);
    }

    public static MemberInitExpression MemberInit(
      NewExpression newExpression,
      IEnumerable<MemberBinding> bindings)
    {
      ContractUtils.RequiresNotNull((object) newExpression, nameof (newExpression));
      ContractUtils.RequiresNotNull((object) bindings, nameof (bindings));
      ReadOnlyCollection<MemberBinding> bindings1 = bindings.ToReadOnly<MemberBinding>();
      Expression.ValidateMemberInitArgs(newExpression.Type, bindings1);
      return new MemberInitExpression(newExpression, bindings1);
    }

    public static MemberListBinding ListBind(
      MemberInfo member,
      params ElementInit[] initializers)
    {
      return Expression.ListBind(member, (IEnumerable<ElementInit>) initializers);
    }

    public static MemberListBinding ListBind(
      MemberInfo member,
      IEnumerable<ElementInit> initializers)
    {
      ContractUtils.RequiresNotNull((object) member, nameof (member));
      ContractUtils.RequiresNotNull((object) initializers, nameof (initializers));
      Type memberType;
      Expression.ValidateGettableFieldOrPropertyMember(member, out memberType);
      ReadOnlyCollection<ElementInit> initializers1 = initializers.ToReadOnly<ElementInit>();
      Expression.ValidateListInitArgs(memberType, initializers1, nameof (member));
      return new MemberListBinding(member, initializers1);
    }

    public static MemberListBinding ListBind(
      MethodInfo propertyAccessor,
      params ElementInit[] initializers)
    {
      return Expression.ListBind(propertyAccessor, (IEnumerable<ElementInit>) initializers);
    }

    public static MemberListBinding ListBind(
      MethodInfo propertyAccessor,
      IEnumerable<ElementInit> initializers)
    {
      ContractUtils.RequiresNotNull((object) propertyAccessor, nameof (propertyAccessor));
      ContractUtils.RequiresNotNull((object) initializers, nameof (initializers));
      return Expression.ListBind((MemberInfo) Expression.GetProperty(propertyAccessor, nameof (propertyAccessor), -1), initializers);
    }

    private static void ValidateListInitArgs(
      Type listType,
      ReadOnlyCollection<ElementInit> initializers,
      string listTypeParamName)
    {
      if (!typeof (IEnumerable).IsAssignableFrom(listType))
        throw Error.TypeNotIEnumerable((object) listType, listTypeParamName);
      int index = 0;
      for (int count = initializers.Count; index < count; ++index)
      {
        ElementInit initializer = initializers[index];
        ContractUtils.RequiresNotNull((object) initializer, nameof (initializers), index);
        Expression.ValidateCallInstanceType(listType, initializer.AddMethod);
      }
    }

    public static MemberMemberBinding MemberBind(
      MemberInfo member,
      params MemberBinding[] bindings)
    {
      return Expression.MemberBind(member, (IEnumerable<MemberBinding>) bindings);
    }

    public static MemberMemberBinding MemberBind(
      MemberInfo member,
      IEnumerable<MemberBinding> bindings)
    {
      ContractUtils.RequiresNotNull((object) member, nameof (member));
      ContractUtils.RequiresNotNull((object) bindings, nameof (bindings));
      ReadOnlyCollection<MemberBinding> bindings1 = bindings.ToReadOnly<MemberBinding>();
      Type memberType;
      Expression.ValidateGettableFieldOrPropertyMember(member, out memberType);
      Expression.ValidateMemberInitArgs(memberType, bindings1);
      return new MemberMemberBinding(member, bindings1);
    }

    public static MemberMemberBinding MemberBind(
      MethodInfo propertyAccessor,
      params MemberBinding[] bindings)
    {
      return Expression.MemberBind(propertyAccessor, (IEnumerable<MemberBinding>) bindings);
    }

    public static MemberMemberBinding MemberBind(
      MethodInfo propertyAccessor,
      IEnumerable<MemberBinding> bindings)
    {
      ContractUtils.RequiresNotNull((object) propertyAccessor, nameof (propertyAccessor));
      return Expression.MemberBind((MemberInfo) Expression.GetProperty(propertyAccessor, nameof (propertyAccessor), -1), bindings);
    }

    private static void ValidateGettableFieldOrPropertyMember(
      MemberInfo member,
      out Type memberType)
    {
      Type declaringType = member.DeclaringType;
      if (declaringType == (Type) null)
        throw Error.NotAMemberOfAnyType((object) member, nameof (member));
      TypeUtils.ValidateType(declaringType, (string) null, true, true);
      MemberInfo memberInfo = member;
      if ((object) memberInfo != null)
      {
        if (!(memberInfo is PropertyInfo propertyInfo))
        {
          if (memberInfo is FieldInfo fieldInfo)
          {
            memberType = fieldInfo.FieldType;
            return;
          }
        }
        else
        {
          if (!propertyInfo.CanRead)
            throw Error.PropertyDoesNotHaveGetter((object) propertyInfo, nameof (member));
          memberType = propertyInfo.PropertyType;
          return;
        }
      }
      throw Error.ArgumentMustBeFieldInfoOrPropertyInfo(nameof (member));
    }

    private static void ValidateMemberInitArgs(
      Type type,
      ReadOnlyCollection<MemberBinding> bindings)
    {
      int index = 0;
      for (int count = bindings.Count; index < count; ++index)
      {
        MemberBinding binding = bindings[index];
        ContractUtils.RequiresNotNull((object) binding, nameof (bindings));
        binding.ValidateAsDefinedHere(index);
        if (!binding.Member.DeclaringType.IsAssignableFrom(type))
          throw Error.NotAMemberOfType((object) binding.Member.Name, (object) type, nameof (bindings), index);
      }
    }

    internal static MethodCallExpression Call(MethodInfo method)
    {
      ContractUtils.RequiresNotNull((object) method, nameof (method));
      ParameterInfo[] parameters = Expression.ValidateMethodAndGetParameters((Expression) null, method);
      Expression.ValidateArgumentCount((MethodBase) method, ExpressionType.Call, 0, parameters);
      return (MethodCallExpression) new MethodCallExpression0(method);
    }

    public static MethodCallExpression Call(MethodInfo method, Expression arg0)
    {
      ContractUtils.RequiresNotNull((object) method, nameof (method));
      ContractUtils.RequiresNotNull((object) arg0, nameof (arg0));
      ParameterInfo[] parameters = Expression.ValidateMethodAndGetParameters((Expression) null, method);
      Expression.ValidateArgumentCount((MethodBase) method, ExpressionType.Call, 1, parameters);
      arg0 = Expression.ValidateOneArgument((MethodBase) method, ExpressionType.Call, arg0, parameters[0], nameof (method), nameof (arg0));
      return (MethodCallExpression) new MethodCallExpression1(method, arg0);
    }

    public static MethodCallExpression Call(
      MethodInfo method,
      Expression arg0,
      Expression arg1)
    {
      ContractUtils.RequiresNotNull((object) method, nameof (method));
      ContractUtils.RequiresNotNull((object) arg0, nameof (arg0));
      ContractUtils.RequiresNotNull((object) arg1, nameof (arg1));
      ParameterInfo[] parameters = Expression.ValidateMethodAndGetParameters((Expression) null, method);
      Expression.ValidateArgumentCount((MethodBase) method, ExpressionType.Call, 2, parameters);
      arg0 = Expression.ValidateOneArgument((MethodBase) method, ExpressionType.Call, arg0, parameters[0], nameof (method), nameof (arg0));
      arg1 = Expression.ValidateOneArgument((MethodBase) method, ExpressionType.Call, arg1, parameters[1], nameof (method), nameof (arg1));
      return (MethodCallExpression) new MethodCallExpression2(method, arg0, arg1);
    }

    public static MethodCallExpression Call(
      MethodInfo method,
      Expression arg0,
      Expression arg1,
      Expression arg2)
    {
      ContractUtils.RequiresNotNull((object) method, nameof (method));
      ContractUtils.RequiresNotNull((object) arg0, nameof (arg0));
      ContractUtils.RequiresNotNull((object) arg1, nameof (arg1));
      ContractUtils.RequiresNotNull((object) arg2, nameof (arg2));
      ParameterInfo[] parameters = Expression.ValidateMethodAndGetParameters((Expression) null, method);
      Expression.ValidateArgumentCount((MethodBase) method, ExpressionType.Call, 3, parameters);
      arg0 = Expression.ValidateOneArgument((MethodBase) method, ExpressionType.Call, arg0, parameters[0], nameof (method), nameof (arg0));
      arg1 = Expression.ValidateOneArgument((MethodBase) method, ExpressionType.Call, arg1, parameters[1], nameof (method), nameof (arg1));
      arg2 = Expression.ValidateOneArgument((MethodBase) method, ExpressionType.Call, arg2, parameters[2], nameof (method), nameof (arg2));
      return (MethodCallExpression) new MethodCallExpression3(method, arg0, arg1, arg2);
    }

    public static MethodCallExpression Call(
      MethodInfo method,
      Expression arg0,
      Expression arg1,
      Expression arg2,
      Expression arg3)
    {
      ContractUtils.RequiresNotNull((object) method, nameof (method));
      ContractUtils.RequiresNotNull((object) arg0, nameof (arg0));
      ContractUtils.RequiresNotNull((object) arg1, nameof (arg1));
      ContractUtils.RequiresNotNull((object) arg2, nameof (arg2));
      ContractUtils.RequiresNotNull((object) arg3, nameof (arg3));
      ParameterInfo[] parameters = Expression.ValidateMethodAndGetParameters((Expression) null, method);
      Expression.ValidateArgumentCount((MethodBase) method, ExpressionType.Call, 4, parameters);
      arg0 = Expression.ValidateOneArgument((MethodBase) method, ExpressionType.Call, arg0, parameters[0], nameof (method), nameof (arg0));
      arg1 = Expression.ValidateOneArgument((MethodBase) method, ExpressionType.Call, arg1, parameters[1], nameof (method), nameof (arg1));
      arg2 = Expression.ValidateOneArgument((MethodBase) method, ExpressionType.Call, arg2, parameters[2], nameof (method), nameof (arg2));
      arg3 = Expression.ValidateOneArgument((MethodBase) method, ExpressionType.Call, arg3, parameters[3], nameof (method), nameof (arg3));
      return (MethodCallExpression) new MethodCallExpression4(method, arg0, arg1, arg2, arg3);
    }

    public static MethodCallExpression Call(
      MethodInfo method,
      Expression arg0,
      Expression arg1,
      Expression arg2,
      Expression arg3,
      Expression arg4)
    {
      ContractUtils.RequiresNotNull((object) method, nameof (method));
      ContractUtils.RequiresNotNull((object) arg0, nameof (arg0));
      ContractUtils.RequiresNotNull((object) arg1, nameof (arg1));
      ContractUtils.RequiresNotNull((object) arg2, nameof (arg2));
      ContractUtils.RequiresNotNull((object) arg3, nameof (arg3));
      ContractUtils.RequiresNotNull((object) arg4, nameof (arg4));
      ParameterInfo[] parameters = Expression.ValidateMethodAndGetParameters((Expression) null, method);
      Expression.ValidateArgumentCount((MethodBase) method, ExpressionType.Call, 5, parameters);
      arg0 = Expression.ValidateOneArgument((MethodBase) method, ExpressionType.Call, arg0, parameters[0], nameof (method), nameof (arg0));
      arg1 = Expression.ValidateOneArgument((MethodBase) method, ExpressionType.Call, arg1, parameters[1], nameof (method), nameof (arg1));
      arg2 = Expression.ValidateOneArgument((MethodBase) method, ExpressionType.Call, arg2, parameters[2], nameof (method), nameof (arg2));
      arg3 = Expression.ValidateOneArgument((MethodBase) method, ExpressionType.Call, arg3, parameters[3], nameof (method), nameof (arg3));
      arg4 = Expression.ValidateOneArgument((MethodBase) method, ExpressionType.Call, arg4, parameters[4], nameof (method), nameof (arg4));
      return (MethodCallExpression) new MethodCallExpression5(method, arg0, arg1, arg2, arg3, arg4);
    }

    public static MethodCallExpression Call(
      MethodInfo method,
      params Expression[] arguments)
    {
      return Expression.Call((Expression) null, method, arguments);
    }

    public static MethodCallExpression Call(
      MethodInfo method,
      IEnumerable<Expression> arguments)
    {
      return Expression.Call((Expression) null, method, arguments);
    }

    public static MethodCallExpression Call(
      Expression instance,
      MethodInfo method)
    {
      ContractUtils.RequiresNotNull((object) method, nameof (method));
      ParameterInfo[] parameters = Expression.ValidateMethodAndGetParameters(instance, method);
      Expression.ValidateArgumentCount((MethodBase) method, ExpressionType.Call, 0, parameters);
      if (instance != null)
        return (MethodCallExpression) new InstanceMethodCallExpression0(method, instance);
      return (MethodCallExpression) new MethodCallExpression0(method);
    }

    public static MethodCallExpression Call(
      Expression instance,
      MethodInfo method,
      params Expression[] arguments)
    {
      return Expression.Call(instance, method, (IEnumerable<Expression>) arguments);
    }

    internal static MethodCallExpression Call(
      Expression instance,
      MethodInfo method,
      Expression arg0)
    {
      ContractUtils.RequiresNotNull((object) method, nameof (method));
      ContractUtils.RequiresNotNull((object) arg0, nameof (arg0));
      ParameterInfo[] parameters = Expression.ValidateMethodAndGetParameters(instance, method);
      Expression.ValidateArgumentCount((MethodBase) method, ExpressionType.Call, 1, parameters);
      arg0 = Expression.ValidateOneArgument((MethodBase) method, ExpressionType.Call, arg0, parameters[0], nameof (method), nameof (arg0));
      if (instance != null)
        return (MethodCallExpression) new InstanceMethodCallExpression1(method, instance, arg0);
      return (MethodCallExpression) new MethodCallExpression1(method, arg0);
    }

    public static MethodCallExpression Call(
      Expression instance,
      MethodInfo method,
      Expression arg0,
      Expression arg1)
    {
      ContractUtils.RequiresNotNull((object) method, nameof (method));
      ContractUtils.RequiresNotNull((object) arg0, nameof (arg0));
      ContractUtils.RequiresNotNull((object) arg1, nameof (arg1));
      ParameterInfo[] parameters = Expression.ValidateMethodAndGetParameters(instance, method);
      Expression.ValidateArgumentCount((MethodBase) method, ExpressionType.Call, 2, parameters);
      arg0 = Expression.ValidateOneArgument((MethodBase) method, ExpressionType.Call, arg0, parameters[0], nameof (method), nameof (arg0));
      arg1 = Expression.ValidateOneArgument((MethodBase) method, ExpressionType.Call, arg1, parameters[1], nameof (method), nameof (arg1));
      if (instance != null)
        return (MethodCallExpression) new InstanceMethodCallExpression2(method, instance, arg0, arg1);
      return (MethodCallExpression) new MethodCallExpression2(method, arg0, arg1);
    }

    public static MethodCallExpression Call(
      Expression instance,
      MethodInfo method,
      Expression arg0,
      Expression arg1,
      Expression arg2)
    {
      ContractUtils.RequiresNotNull((object) method, nameof (method));
      ContractUtils.RequiresNotNull((object) arg0, nameof (arg0));
      ContractUtils.RequiresNotNull((object) arg1, nameof (arg1));
      ContractUtils.RequiresNotNull((object) arg2, nameof (arg2));
      ParameterInfo[] parameters = Expression.ValidateMethodAndGetParameters(instance, method);
      Expression.ValidateArgumentCount((MethodBase) method, ExpressionType.Call, 3, parameters);
      arg0 = Expression.ValidateOneArgument((MethodBase) method, ExpressionType.Call, arg0, parameters[0], nameof (method), nameof (arg0));
      arg1 = Expression.ValidateOneArgument((MethodBase) method, ExpressionType.Call, arg1, parameters[1], nameof (method), nameof (arg1));
      arg2 = Expression.ValidateOneArgument((MethodBase) method, ExpressionType.Call, arg2, parameters[2], nameof (method), nameof (arg2));
      if (instance != null)
        return (MethodCallExpression) new InstanceMethodCallExpression3(method, instance, arg0, arg1, arg2);
      return (MethodCallExpression) new MethodCallExpression3(method, arg0, arg1, arg2);
    }

    public static MethodCallExpression Call(
      Expression instance,
      string methodName,
      Type[] typeArguments,
      params Expression[] arguments)
    {
      ContractUtils.RequiresNotNull((object) instance, nameof (instance));
      ContractUtils.RequiresNotNull((object) methodName, nameof (methodName));
      if (arguments == null)
        arguments = Array.Empty<Expression>();
      BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
      return Expression.Call(instance, Expression.FindMethod(instance.Type, methodName, typeArguments, arguments, flags), arguments);
    }

    public static MethodCallExpression Call(
      Type type,
      string methodName,
      Type[] typeArguments,
      params Expression[] arguments)
    {
      ContractUtils.RequiresNotNull((object) type, nameof (type));
      ContractUtils.RequiresNotNull((object) methodName, nameof (methodName));
      if (arguments == null)
        arguments = Array.Empty<Expression>();
      BindingFlags flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
      return Expression.Call((Expression) null, Expression.FindMethod(type, methodName, typeArguments, arguments, flags), arguments);
    }

    public static MethodCallExpression Call(
      Expression instance,
      MethodInfo method,
      IEnumerable<Expression> arguments)
    {
      IReadOnlyList<Expression> enumerable = arguments as IReadOnlyList<Expression> ?? (IReadOnlyList<Expression>) arguments.ToReadOnly<Expression>();
      int count = enumerable.Count;
      switch (count)
      {
        case 0:
          return Expression.Call(instance, method);
        case 1:
          return Expression.Call(instance, method, enumerable[0]);
        case 2:
          return Expression.Call(instance, method, enumerable[0], enumerable[1]);
        case 3:
          return Expression.Call(instance, method, enumerable[0], enumerable[1], enumerable[2]);
        default:
          if (instance == null)
          {
            if (count == 4)
              return Expression.Call(method, enumerable[0], enumerable[1], enumerable[2], enumerable[3]);
            if (count == 5)
              return Expression.Call(method, enumerable[0], enumerable[1], enumerable[2], enumerable[3], enumerable[4]);
          }
          ContractUtils.RequiresNotNull((object) method, nameof (method));
          ReadOnlyCollection<Expression> arguments1 = enumerable.ToReadOnly<Expression>();
          Expression.ValidateMethodInfo(method, nameof (method));
          Expression.ValidateStaticOrInstanceMethod(instance, method);
          Expression.ValidateArgumentTypes((MethodBase) method, ExpressionType.Call, ref arguments1, nameof (method));
          if (instance == null)
            return (MethodCallExpression) new MethodCallExpressionN(method, (IReadOnlyList<Expression>) arguments1);
          return (MethodCallExpression) new InstanceMethodCallExpressionN(method, instance, (IReadOnlyList<Expression>) arguments1);
      }
    }

    private static ParameterInfo[] ValidateMethodAndGetParameters(
      Expression instance,
      MethodInfo method)
    {
      Expression.ValidateMethodInfo(method, nameof (method));
      Expression.ValidateStaticOrInstanceMethod(instance, method);
      return Expression.GetParametersForValidation((MethodBase) method, ExpressionType.Call);
    }

    private static void ValidateStaticOrInstanceMethod(Expression instance, MethodInfo method)
    {
      if (method.IsStatic)
      {
        if (instance != null)
          throw Error.OnlyStaticMethodsHaveNullInstance();
      }
      else
      {
        if (instance == null)
          throw Error.OnlyStaticMethodsHaveNullInstance();
        ExpressionUtils.RequiresCanRead(instance, nameof (instance));
        Expression.ValidateCallInstanceType(instance.Type, method);
      }
    }

    private static void ValidateCallInstanceType(Type instanceType, MethodInfo method)
    {
      if (!TypeUtils.IsValidInstanceType((MemberInfo) method, instanceType))
        throw Error.InstanceAndMethodTypeMismatch((object) method, (object) method.DeclaringType, (object) instanceType);
    }

    private static void ValidateArgumentTypes(
      MethodBase method,
      ExpressionType nodeKind,
      ref ReadOnlyCollection<Expression> arguments,
      string methodParamName)
    {
      ExpressionUtils.ValidateArgumentTypes(method, nodeKind, ref arguments, methodParamName);
    }

    private static ParameterInfo[] GetParametersForValidation(
      MethodBase method,
      ExpressionType nodeKind)
    {
      return ExpressionUtils.GetParametersForValidation(method, nodeKind);
    }

    private static void ValidateArgumentCount(
      MethodBase method,
      ExpressionType nodeKind,
      int count,
      ParameterInfo[] pis)
    {
      ExpressionUtils.ValidateArgumentCount(method, nodeKind, count, pis);
    }

    private static Expression ValidateOneArgument(
      MethodBase method,
      ExpressionType nodeKind,
      Expression arg,
      ParameterInfo pi,
      string methodParamName,
      string argumentParamName)
    {
      return ExpressionUtils.ValidateOneArgument(method, nodeKind, arg, pi, methodParamName, argumentParamName, -1);
    }

    private static bool TryQuote(Type parameterType, ref Expression argument)
    {
      return ExpressionUtils.TryQuote(parameterType, ref argument);
    }

    private static MethodInfo FindMethod(
      Type type,
      string methodName,
      Type[] typeArgs,
      Expression[] args,
      BindingFlags flags)
    {
      int num = 0;
      MethodInfo methodInfo1 = (MethodInfo) null;
      foreach (MethodInfo method in type.GetMethods(flags))
      {
        if (method.Name.Equals(methodName, StringComparison.OrdinalIgnoreCase))
        {
          MethodInfo methodInfo2 = Expression.ApplyTypeArgs(method, typeArgs);
          if (methodInfo2 != (MethodInfo) null && Expression.IsCompatible((MethodBase) methodInfo2, args))
          {
            if (methodInfo1 == (MethodInfo) null || !methodInfo1.IsPublic && methodInfo2.IsPublic)
            {
              methodInfo1 = methodInfo2;
              num = 1;
            }
            else if (methodInfo1.IsPublic == methodInfo2.IsPublic)
              ++num;
          }
        }
      }
      if (num == 0)
      {
        if (typeArgs != null && typeArgs.Length != 0)
          throw Error.GenericMethodWithArgsDoesNotExistOnType((object) methodName, (object) type);
        throw Error.MethodWithArgsDoesNotExistOnType((object) methodName, (object) type);
      }
      if (num > 1)
        throw Error.MethodWithMoreThanOneMatch((object) methodName, (object) type);
      return methodInfo1;
    }

    private static bool IsCompatible(MethodBase m, Expression[] arguments)
    {
      ParameterInfo[] parametersCached = m.GetParametersCached();
      if (parametersCached.Length != arguments.Length)
        return false;
      for (int index = 0; index < arguments.Length; ++index)
      {
        Expression expression = arguments[index];
        ContractUtils.RequiresNotNull((object) expression, nameof (arguments));
        Type type1 = expression.Type;
        Type type2 = parametersCached[index].ParameterType;
        if (type2.IsByRef)
          type2 = type2.GetElementType();
        if (!TypeUtils.AreReferenceAssignable(type2, type1) && (!TypeUtils.IsSameOrSubclass(typeof (LambdaExpression), type2) || !type2.IsAssignableFrom(expression.GetType())))
          return false;
      }
      return true;
    }

    private static MethodInfo ApplyTypeArgs(MethodInfo m, Type[] typeArgs)
    {
      if (typeArgs == null || typeArgs.Length == 0)
      {
        if (!m.IsGenericMethodDefinition)
          return m;
      }
      else if (m.IsGenericMethodDefinition && m.GetGenericArguments().Length == typeArgs.Length)
        return m.MakeGenericMethod(typeArgs);
      return (MethodInfo) null;
    }

    public static MethodCallExpression ArrayIndex(
      Expression array,
      params Expression[] indexes)
    {
      return Expression.ArrayIndex(array, (IEnumerable<Expression>) indexes);
    }

    public static MethodCallExpression ArrayIndex(
      Expression array,
      IEnumerable<Expression> indexes)
    {
      ExpressionUtils.RequiresCanRead(array, nameof (array), -1);
      ContractUtils.RequiresNotNull((object) indexes, nameof (indexes));
      Type type = array.Type;
      if (!type.IsArray)
        throw Error.ArgumentMustBeArray(nameof (array));
      ReadOnlyCollection<Expression> readOnlyCollection = indexes.ToReadOnly<Expression>();
      if (type.GetArrayRank() != readOnlyCollection.Count)
        throw Error.IncorrectNumberOfIndexes();
      int index = 0;
      for (int count = readOnlyCollection.Count; index < count; ++index)
      {
        Expression expression = readOnlyCollection[index];
        ExpressionUtils.RequiresCanRead(expression, nameof (indexes), index);
        if (expression.Type != typeof (int))
          throw Error.ArgumentMustBeArrayIndexType(nameof (indexes), index);
      }
      MethodInfo method = array.Type.GetMethod("Get", BindingFlags.Instance | BindingFlags.Public);
      return Expression.Call(array, method, (IEnumerable<Expression>) readOnlyCollection);
    }

    public static NewArrayExpression NewArrayInit(
      Type type,
      params Expression[] initializers)
    {
      return Expression.NewArrayInit(type, (IEnumerable<Expression>) initializers);
    }

    public static NewArrayExpression NewArrayInit(
      Type type,
      IEnumerable<Expression> initializers)
    {
      ContractUtils.RequiresNotNull((object) type, nameof (type));
      ContractUtils.RequiresNotNull((object) initializers, nameof (initializers));
      if (type == typeof (void))
        throw Error.ArgumentCannotBeOfTypeVoid(nameof (type));
      TypeUtils.ValidateType(type, nameof (type));
      ReadOnlyCollection<Expression> expressions = initializers.ToReadOnly<Expression>();
      Expression[] expressionArray = (Expression[]) null;
      int idx = 0;
      for (int count = expressions.Count; idx < count; ++idx)
      {
        Expression expression = expressions[idx];
        ExpressionUtils.RequiresCanRead(expression, nameof (initializers), idx);
        if (!TypeUtils.AreReferenceAssignable(type, expression.Type))
        {
          if (!Expression.TryQuote(type, ref expression))
            throw Error.ExpressionTypeCannotInitializeArrayType((object) expression.Type, (object) type);
          if (expressionArray == null)
          {
            expressionArray = new Expression[expressions.Count];
            for (int index = 0; index < idx; ++index)
              expressionArray[index] = expressions[index];
          }
        }
        if (expressionArray != null)
          expressionArray[idx] = expression;
      }
      if (expressionArray != null)
        expressions = (ReadOnlyCollection<Expression>) new TrueReadOnlyCollection<Expression>(expressionArray);
      return NewArrayExpression.Make(ExpressionType.NewArrayInit, type.MakeArrayType(), expressions);
    }

    public static NewArrayExpression NewArrayBounds(
      Type type,
      params Expression[] bounds)
    {
      return Expression.NewArrayBounds(type, (IEnumerable<Expression>) bounds);
    }

    public static NewArrayExpression NewArrayBounds(
      Type type,
      IEnumerable<Expression> bounds)
    {
      ContractUtils.RequiresNotNull((object) type, nameof (type));
      ContractUtils.RequiresNotNull((object) bounds, nameof (bounds));
      if (type == typeof (void))
        throw Error.ArgumentCannotBeOfTypeVoid(nameof (type));
      TypeUtils.ValidateType(type, nameof (type));
      ReadOnlyCollection<Expression> expressions = bounds.ToReadOnly<Expression>();
      int count = expressions.Count;
      if (count <= 0)
        throw Error.BoundsCannotBeLessThanOne(nameof (bounds));
      for (int index = 0; index < count; ++index)
      {
        Expression expression = expressions[index];
        ExpressionUtils.RequiresCanRead(expression, nameof (bounds), index);
        if (!expression.Type.IsInteger())
          throw Error.ArgumentMustBeInteger(nameof (bounds), index);
      }
      return NewArrayExpression.Make(ExpressionType.NewArrayBounds, count != 1 ? type.MakeArrayType(count) : type.MakeArrayType(), expressions);
    }

    public static NewExpression New(ConstructorInfo constructor)
    {
      return Expression.New(constructor, (IEnumerable<Expression>) null);
    }

    public static NewExpression New(
      ConstructorInfo constructor,
      params Expression[] arguments)
    {
      return Expression.New(constructor, (IEnumerable<Expression>) arguments);
    }

    public static NewExpression New(
      ConstructorInfo constructor,
      IEnumerable<Expression> arguments)
    {
      ContractUtils.RequiresNotNull((object) constructor, nameof (constructor));
      ContractUtils.RequiresNotNull((object) constructor.DeclaringType, "constructor.DeclaringType");
      TypeUtils.ValidateType(constructor.DeclaringType, nameof (constructor), true, true);
      Expression.ValidateConstructor(constructor, nameof (constructor));
      ReadOnlyCollection<Expression> arguments1 = arguments.ToReadOnly<Expression>();
      Expression.ValidateArgumentTypes((MethodBase) constructor, ExpressionType.New, ref arguments1, nameof (constructor));
      return new NewExpression(constructor, (IReadOnlyList<Expression>) arguments1, (ReadOnlyCollection<MemberInfo>) null);
    }

    public static NewExpression New(
      ConstructorInfo constructor,
      IEnumerable<Expression> arguments,
      IEnumerable<MemberInfo> members)
    {
      ContractUtils.RequiresNotNull((object) constructor, nameof (constructor));
      ContractUtils.RequiresNotNull((object) constructor.DeclaringType, "constructor.DeclaringType");
      TypeUtils.ValidateType(constructor.DeclaringType, nameof (constructor), true, true);
      Expression.ValidateConstructor(constructor, nameof (constructor));
      ReadOnlyCollection<MemberInfo> members1 = members.ToReadOnly<MemberInfo>();
      ReadOnlyCollection<Expression> arguments1 = arguments.ToReadOnly<Expression>();
      Expression.ValidateNewArgs(constructor, ref arguments1, ref members1);
      return new NewExpression(constructor, (IReadOnlyList<Expression>) arguments1, members1);
    }

    public static NewExpression New(
      ConstructorInfo constructor,
      IEnumerable<Expression> arguments,
      params MemberInfo[] members)
    {
      return Expression.New(constructor, arguments, (IEnumerable<MemberInfo>) members);
    }

    public static NewExpression New(Type type)
    {
      ContractUtils.RequiresNotNull((object) type, nameof (type));
      if (type == typeof (void))
        throw Error.ArgumentCannotBeOfTypeVoid(nameof (type));
      TypeUtils.ValidateType(type, nameof (type));
      if (type.IsValueType)
        return (NewExpression) new NewValueTypeExpression(type, EmptyReadOnlyCollection<Expression>.Instance, (ReadOnlyCollection<MemberInfo>) null);
      ConstructorInfo constructor = ((IEnumerable<ConstructorInfo>) type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)).SingleOrDefault<ConstructorInfo>((Func<ConstructorInfo, bool>) (c => c.GetParametersCached().Length == 0));
      if (constructor == (ConstructorInfo) null)
        throw Error.TypeMissingDefaultConstructor((object) type, nameof (type));
      return Expression.New(constructor);
    }

    private static void ValidateNewArgs(
      ConstructorInfo constructor,
      ref ReadOnlyCollection<Expression> arguments,
      ref ReadOnlyCollection<MemberInfo> members)
    {
      ParameterInfo[] parametersCached;
      if ((parametersCached = constructor.GetParametersCached()).Length != 0)
      {
        if (arguments.Count != parametersCached.Length)
          throw Error.IncorrectNumberOfConstructorArguments();
        if (arguments.Count != members.Count)
          throw Error.IncorrectNumberOfArgumentsForMembers();
        Expression[] expressionArray = (Expression[]) null;
        MemberInfo[] memberInfoArray = (MemberInfo[]) null;
        int index1 = 0;
        for (int count = arguments.Count; index1 < count; ++index1)
        {
          Expression expression = arguments[index1];
          ExpressionUtils.RequiresCanRead(expression, nameof (arguments), index1);
          MemberInfo member = members[index1];
          ContractUtils.RequiresNotNull((object) member, nameof (members), index1);
          if (!TypeUtils.AreEquivalent(member.DeclaringType, constructor.DeclaringType))
            throw Error.ArgumentMemberNotDeclOnType((object) member.Name, (object) constructor.DeclaringType.Name, nameof (members), index1);
          Type memberType;
          Expression.ValidateAnonymousTypeMember(ref member, out memberType, nameof (members), index1);
          if (!TypeUtils.AreReferenceAssignable(memberType, expression.Type) && !Expression.TryQuote(memberType, ref expression))
            throw Error.ArgumentTypeDoesNotMatchMember((object) expression.Type, (object) memberType, nameof (arguments), index1);
          Type type = parametersCached[index1].ParameterType;
          if (type.IsByRef)
            type = type.GetElementType();
          if (!TypeUtils.AreReferenceAssignable(type, expression.Type) && !Expression.TryQuote(type, ref expression))
            throw Error.ExpressionTypeDoesNotMatchConstructorParameter((object) expression.Type, (object) type, nameof (arguments), index1);
          if (expressionArray == null && expression != arguments[index1])
          {
            expressionArray = new Expression[arguments.Count];
            for (int index2 = 0; index2 < index1; ++index2)
              expressionArray[index2] = arguments[index2];
          }
          if (expressionArray != null)
            expressionArray[index1] = expression;
          if (memberInfoArray == null && member != members[index1])
          {
            memberInfoArray = new MemberInfo[members.Count];
            for (int index2 = 0; index2 < index1; ++index2)
              memberInfoArray[index2] = members[index2];
          }
          if (memberInfoArray != null)
            memberInfoArray[index1] = member;
        }
        if (expressionArray != null)
          arguments = (ReadOnlyCollection<Expression>) new TrueReadOnlyCollection<Expression>(expressionArray);
        if (memberInfoArray == null)
          return;
        members = (ReadOnlyCollection<MemberInfo>) new TrueReadOnlyCollection<MemberInfo>(memberInfoArray);
      }
      else
      {
        if (arguments != null && arguments.Count > 0)
          throw Error.IncorrectNumberOfConstructorArguments();
        if (members != null && members.Count > 0)
          throw Error.IncorrectNumberOfMembersForGivenConstructor();
      }
    }

    private static void ValidateAnonymousTypeMember(
      ref MemberInfo member,
      out Type memberType,
      string paramName,
      int index)
    {
      FieldInfo fieldInfo = member as FieldInfo;
      if (fieldInfo != (FieldInfo) null)
      {
        if (fieldInfo.IsStatic)
          throw Error.ArgumentMustBeInstanceMember(paramName, index);
        memberType = fieldInfo.FieldType;
      }
      else
      {
        PropertyInfo propertyInfo = member as PropertyInfo;
        if (propertyInfo != (PropertyInfo) null)
        {
          if (!propertyInfo.CanRead)
            throw Error.PropertyDoesNotHaveGetter((object) propertyInfo, paramName, index);
          if (propertyInfo.GetGetMethod().IsStatic)
            throw Error.ArgumentMustBeInstanceMember(paramName, index);
          memberType = propertyInfo.PropertyType;
        }
        else
        {
          MethodInfo mi = member as MethodInfo;
          if (!(mi != (MethodInfo) null))
            throw Error.ArgumentMustBeFieldInfoOrPropertyInfoOrMethod(paramName, index);
          if (mi.IsStatic)
            throw Error.ArgumentMustBeInstanceMember(paramName, index);
          PropertyInfo property = Expression.GetProperty(mi, paramName, index);
          member = (MemberInfo) property;
          memberType = property.PropertyType;
        }
      }
    }

    private static void ValidateConstructor(ConstructorInfo constructor, string paramName)
    {
      if (constructor.IsStatic)
        throw Error.NonStaticConstructorRequired(paramName);
    }

    public static ParameterExpression Parameter(Type type)
    {
      return Expression.Parameter(type, (string) null);
    }

    public static ParameterExpression Variable(Type type)
    {
      return Expression.Variable(type, (string) null);
    }

    public static ParameterExpression Parameter(Type type, string name)
    {
      Expression.Validate(type, true);
      bool isByRef = type.IsByRef;
      if (isByRef)
        type = type.GetElementType();
      return ParameterExpression.Make(type, name, isByRef);
    }

    public static ParameterExpression Variable(Type type, string name)
    {
      Expression.Validate(type, false);
      return ParameterExpression.Make(type, name, false);
    }

    private static void Validate(Type type, bool allowByRef)
    {
      ContractUtils.RequiresNotNull((object) type, nameof (type));
      TypeUtils.ValidateType(type, nameof (type), allowByRef, false);
      if (type == typeof (void))
        throw Error.ArgumentCannotBeOfTypeVoid(nameof (type));
    }

    public static RuntimeVariablesExpression RuntimeVariables(
      params ParameterExpression[] variables)
    {
      return Expression.RuntimeVariables((IEnumerable<ParameterExpression>) variables);
    }

    public static RuntimeVariablesExpression RuntimeVariables(
      IEnumerable<ParameterExpression> variables)
    {
      ContractUtils.RequiresNotNull((object) variables, nameof (variables));
      ReadOnlyCollection<ParameterExpression> variables1 = variables.ToReadOnly<ParameterExpression>();
      for (int index = 0; index < variables1.Count; ++index)
        ContractUtils.RequiresNotNull((object) variables1[index], nameof (variables), index);
      return new RuntimeVariablesExpression(variables1);
    }

    public static SwitchCase SwitchCase(Expression body, params Expression[] testValues)
    {
      return Expression.SwitchCase(body, (IEnumerable<Expression>) testValues);
    }

    public static SwitchCase SwitchCase(
      Expression body,
      IEnumerable<Expression> testValues)
    {
      ExpressionUtils.RequiresCanRead(body, nameof (body));
      ReadOnlyCollection<Expression> testValues1 = testValues.ToReadOnly<Expression>();
      ContractUtils.RequiresNotEmpty<Expression>((ICollection<Expression>) testValues1, nameof (testValues));
      Expression.RequiresCanRead((IReadOnlyList<Expression>) testValues1, nameof (testValues));
      return new SwitchCase(body, testValues1);
    }

    public static SwitchExpression Switch(
      Expression switchValue,
      params SwitchCase[] cases)
    {
      return Expression.Switch(switchValue, (Expression) null, (MethodInfo) null, (IEnumerable<SwitchCase>) cases);
    }

    public static SwitchExpression Switch(
      Expression switchValue,
      Expression defaultBody,
      params SwitchCase[] cases)
    {
      return Expression.Switch(switchValue, defaultBody, (MethodInfo) null, (IEnumerable<SwitchCase>) cases);
    }

    public static SwitchExpression Switch(
      Expression switchValue,
      Expression defaultBody,
      MethodInfo comparison,
      params SwitchCase[] cases)
    {
      return Expression.Switch(switchValue, defaultBody, comparison, (IEnumerable<SwitchCase>) cases);
    }

    public static SwitchExpression Switch(
      Type type,
      Expression switchValue,
      Expression defaultBody,
      MethodInfo comparison,
      params SwitchCase[] cases)
    {
      return Expression.Switch(type, switchValue, defaultBody, comparison, (IEnumerable<SwitchCase>) cases);
    }

    public static SwitchExpression Switch(
      Expression switchValue,
      Expression defaultBody,
      MethodInfo comparison,
      IEnumerable<SwitchCase> cases)
    {
      return Expression.Switch((Type) null, switchValue, defaultBody, comparison, cases);
    }

    public static SwitchExpression Switch(
      Type type,
      Expression switchValue,
      Expression defaultBody,
      MethodInfo comparison,
      IEnumerable<SwitchCase> cases)
    {
      ExpressionUtils.RequiresCanRead(switchValue, nameof (switchValue));
      if (switchValue.Type == typeof (void))
        throw Error.ArgumentCannotBeOfTypeVoid(nameof (switchValue));
      ReadOnlyCollection<SwitchCase> cases1 = cases.ToReadOnly<SwitchCase>();
      ContractUtils.RequiresNotNullItems<SwitchCase>((IList<SwitchCase>) cases1, nameof (cases));
      Type type1 = !(type != (Type) null) ? (cases1.Count == 0 ? (defaultBody == null ? typeof (void) : defaultBody.Type) : cases1[0].Body.Type) : type;
      bool customType = type != (Type) null;
      if (comparison != (MethodInfo) null)
      {
        Expression.ValidateMethodInfo(comparison, nameof (comparison));
        ParameterInfo[] parametersCached = comparison.GetParametersCached();
        if (parametersCached.Length != 2)
          throw Error.IncorrectNumberOfMethodCallArguments((object) comparison, nameof (comparison));
        ParameterInfo pi1 = parametersCached[0];
        bool flag = false;
        if (!Expression.ParameterIsAssignable(pi1, switchValue.Type))
        {
          flag = Expression.ParameterIsAssignable(pi1, switchValue.Type.GetNonNullableType());
          if (!flag)
            throw Error.SwitchValueTypeDoesNotMatchComparisonMethodParameter((object) switchValue.Type, (object) pi1.ParameterType);
        }
        ParameterInfo pi2 = parametersCached[1];
        foreach (SwitchCase switchCase in cases1)
        {
          ContractUtils.RequiresNotNull((object) switchCase, nameof (cases));
          Expression.ValidateSwitchCaseType(switchCase.Body, customType, type1, nameof (cases));
          int index = 0;
          for (int count = switchCase.TestValues.Count; index < count; ++index)
          {
            Type type2 = switchCase.TestValues[index].Type;
            if (flag)
            {
              if (!type2.IsNullableType())
                throw Error.TestValueTypeDoesNotMatchComparisonMethodParameter((object) type2, (object) pi2.ParameterType);
              type2 = type2.GetNonNullableType();
            }
            if (!Expression.ParameterIsAssignable(pi2, type2))
              throw Error.TestValueTypeDoesNotMatchComparisonMethodParameter((object) type2, (object) pi2.ParameterType);
          }
        }
        if (comparison.ReturnType != typeof (bool))
          throw Error.EqualityMustReturnBoolean((object) comparison, nameof (comparison));
      }
      else if (cases1.Count != 0)
      {
        Expression testValue = cases1[0].TestValues[0];
        foreach (SwitchCase switchCase in cases1)
        {
          ContractUtils.RequiresNotNull((object) switchCase, nameof (cases));
          Expression.ValidateSwitchCaseType(switchCase.Body, customType, type1, nameof (cases));
          int index = 0;
          for (int count = switchCase.TestValues.Count; index < count; ++index)
          {
            if (!TypeUtils.AreEquivalent(testValue.Type, switchCase.TestValues[index].Type))
              throw Error.AllTestValuesMustHaveSameType(nameof (cases));
          }
        }
        comparison = Expression.Equal(switchValue, testValue, false, comparison).Method;
      }
      if (defaultBody == null)
      {
        if (type1 != typeof (void))
          throw Error.DefaultBodyMustBeSupplied(nameof (defaultBody));
      }
      else
        Expression.ValidateSwitchCaseType(defaultBody, customType, type1, nameof (defaultBody));
      return new SwitchExpression(type1, switchValue, defaultBody, comparison, cases1);
    }

    private static void ValidateSwitchCaseType(
      Expression @case,
      bool customType,
      Type resultType,
      string parameterName)
    {
      if (customType)
      {
        if (resultType != typeof (void) && !TypeUtils.AreReferenceAssignable(resultType, @case.Type))
          throw Error.ArgumentTypesMustMatch(parameterName);
      }
      else if (!TypeUtils.AreEquivalent(resultType, @case.Type))
        throw Error.AllCaseBodiesMustHaveSameType(parameterName);
    }

    public static SymbolDocumentInfo SymbolDocument(string fileName)
    {
      return new SymbolDocumentInfo(fileName);
    }

    public static SymbolDocumentInfo SymbolDocument(
      string fileName,
      Guid language)
    {
      return (SymbolDocumentInfo) new SymbolDocumentWithGuids(fileName, ref language);
    }

    public static SymbolDocumentInfo SymbolDocument(
      string fileName,
      Guid language,
      Guid languageVendor)
    {
      return (SymbolDocumentInfo) new SymbolDocumentWithGuids(fileName, ref language, ref languageVendor);
    }

    public static SymbolDocumentInfo SymbolDocument(
      string fileName,
      Guid language,
      Guid languageVendor,
      Guid documentType)
    {
      return (SymbolDocumentInfo) new SymbolDocumentWithGuids(fileName, ref language, ref languageVendor, ref documentType);
    }

    public static TryExpression TryFault(Expression body, Expression fault)
    {
      return Expression.MakeTry((Type) null, body, (Expression) null, fault, (IEnumerable<CatchBlock>) null);
    }

    public static TryExpression TryFinally(Expression body, Expression @finally)
    {
      return Expression.MakeTry((Type) null, body, @finally, (Expression) null, (IEnumerable<CatchBlock>) null);
    }

    public static TryExpression TryCatch(Expression body, params CatchBlock[] handlers)
    {
      return Expression.MakeTry((Type) null, body, (Expression) null, (Expression) null, (IEnumerable<CatchBlock>) handlers);
    }

    public static TryExpression TryCatchFinally(
      Expression body,
      Expression @finally,
      params CatchBlock[] handlers)
    {
      return Expression.MakeTry((Type) null, body, @finally, (Expression) null, (IEnumerable<CatchBlock>) handlers);
    }

    public static TryExpression MakeTry(
      Type type,
      Expression body,
      Expression @finally,
      Expression fault,
      IEnumerable<CatchBlock> handlers)
    {
      ExpressionUtils.RequiresCanRead(body, nameof (body));
      ReadOnlyCollection<CatchBlock> handlers1 = handlers.ToReadOnly<CatchBlock>();
      ContractUtils.RequiresNotNullItems<CatchBlock>((IList<CatchBlock>) handlers1, nameof (handlers));
      Expression.ValidateTryAndCatchHaveSameType(type, body, handlers1);
      if (fault != null)
      {
        if (@finally != null || handlers1.Count > 0)
          throw Error.FaultCannotHaveCatchOrFinally(nameof (fault));
        ExpressionUtils.RequiresCanRead(fault, nameof (fault));
      }
      else if (@finally != null)
        ExpressionUtils.RequiresCanRead(@finally, nameof (@finally));
      else if (handlers1.Count == 0)
        throw Error.TryMustHaveCatchFinallyOrFault();
      Type type1 = type;
      if ((object) type1 == null)
        type1 = body.Type;
      return new TryExpression(type1, body, @finally, fault, handlers1);
    }

    private static void ValidateTryAndCatchHaveSameType(
      Type type,
      Expression tryBody,
      ReadOnlyCollection<CatchBlock> handlers)
    {
      if (type != (Type) null)
      {
        if (!(type != typeof (void)))
          return;
        if (!TypeUtils.AreReferenceAssignable(type, tryBody.Type))
          throw Error.ArgumentTypesMustMatch();
        foreach (CatchBlock handler in handlers)
        {
          if (!TypeUtils.AreReferenceAssignable(type, handler.Body.Type))
            throw Error.ArgumentTypesMustMatch();
        }
      }
      else if (tryBody.Type == typeof (void))
      {
        foreach (CatchBlock handler in handlers)
        {
          if (handler.Body.Type != typeof (void))
            throw Error.BodyOfCatchMustHaveSameTypeAsBodyOfTry();
        }
      }
      else
      {
        type = tryBody.Type;
        foreach (CatchBlock handler in handlers)
        {
          if (!TypeUtils.AreEquivalent(handler.Body.Type, type))
            throw Error.BodyOfCatchMustHaveSameTypeAsBodyOfTry();
        }
      }
    }

    public static TypeBinaryExpression TypeIs(Expression expression, Type type)
    {
      ExpressionUtils.RequiresCanRead(expression, nameof (expression));
      ContractUtils.RequiresNotNull((object) type, nameof (type));
      if (type.IsByRef)
        throw Error.TypeMustNotBeByRef(nameof (type));
      return new TypeBinaryExpression(expression, type, ExpressionType.TypeIs);
    }

    public static TypeBinaryExpression TypeEqual(
      Expression expression,
      Type type)
    {
      ExpressionUtils.RequiresCanRead(expression, nameof (expression));
      ContractUtils.RequiresNotNull((object) type, nameof (type));
      if (type.IsByRef)
        throw Error.TypeMustNotBeByRef(nameof (type));
      return new TypeBinaryExpression(expression, type, ExpressionType.TypeEqual);
    }

    public static UnaryExpression MakeUnary(
      ExpressionType unaryType,
      Expression operand,
      Type type)
    {
      return Expression.MakeUnary(unaryType, operand, type, (MethodInfo) null);
    }

    public static UnaryExpression MakeUnary(
      ExpressionType unaryType,
      Expression operand,
      Type type,
      MethodInfo method)
    {
      switch (unaryType)
      {
        case ExpressionType.ArrayLength:
          return Expression.ArrayLength(operand);
        case ExpressionType.Convert:
          return Expression.Convert(operand, type, method);
        case ExpressionType.ConvertChecked:
          return Expression.ConvertChecked(operand, type, method);
        case ExpressionType.Negate:
          return Expression.Negate(operand, method);
        case ExpressionType.UnaryPlus:
          return Expression.UnaryPlus(operand, method);
        case ExpressionType.NegateChecked:
          return Expression.NegateChecked(operand, method);
        case ExpressionType.Not:
          return Expression.Not(operand, method);
        case ExpressionType.Quote:
          return Expression.Quote(operand);
        case ExpressionType.TypeAs:
          return Expression.TypeAs(operand, type);
        case ExpressionType.Decrement:
          return Expression.Decrement(operand, method);
        case ExpressionType.Increment:
          return Expression.Increment(operand, method);
        case ExpressionType.Throw:
          return Expression.Throw(operand, type);
        case ExpressionType.Unbox:
          return Expression.Unbox(operand, type);
        case ExpressionType.PreIncrementAssign:
          return Expression.PreIncrementAssign(operand, method);
        case ExpressionType.PreDecrementAssign:
          return Expression.PreDecrementAssign(operand, method);
        case ExpressionType.PostIncrementAssign:
          return Expression.PostIncrementAssign(operand, method);
        case ExpressionType.PostDecrementAssign:
          return Expression.PostDecrementAssign(operand, method);
        case ExpressionType.OnesComplement:
          return Expression.OnesComplement(operand, method);
        case ExpressionType.IsTrue:
          return Expression.IsTrue(operand, method);
        case ExpressionType.IsFalse:
          return Expression.IsFalse(operand, method);
        default:
          throw Error.UnhandledUnary((object) unaryType, nameof (unaryType));
      }
    }

    private static UnaryExpression GetUserDefinedUnaryOperatorOrThrow(
      ExpressionType unaryType,
      string name,
      Expression operand)
    {
      UnaryExpression definedUnaryOperator = Expression.GetUserDefinedUnaryOperator(unaryType, name, operand);
      if (definedUnaryOperator == null)
        throw Error.UnaryOperatorNotDefined((object) unaryType, (object) operand.Type);
      Expression.ValidateParamswithOperandsOrThrow(definedUnaryOperator.Method.GetParametersCached()[0].ParameterType, operand.Type, unaryType, name);
      return definedUnaryOperator;
    }

    private static UnaryExpression GetUserDefinedUnaryOperator(
      ExpressionType unaryType,
      string name,
      Expression operand)
    {
      Type type = operand.Type;
      Type[] types = new Type[1]{ type };
      Type nonNullableType = type.GetNonNullableType();
      MethodInfo staticMethodValidated1 = nonNullableType.GetAnyStaticMethodValidated(name, types);
      if (staticMethodValidated1 != (MethodInfo) null)
        return new UnaryExpression(unaryType, operand, staticMethodValidated1.ReturnType, staticMethodValidated1);
      if (type.IsNullableType())
      {
        types[0] = nonNullableType;
        MethodInfo staticMethodValidated2 = nonNullableType.GetAnyStaticMethodValidated(name, types);
        if (staticMethodValidated2 != (MethodInfo) null && staticMethodValidated2.ReturnType.IsValueType && !staticMethodValidated2.ReturnType.IsNullableType())
          return new UnaryExpression(unaryType, operand, staticMethodValidated2.ReturnType.GetNullableType(), staticMethodValidated2);
      }
      return (UnaryExpression) null;
    }

    private static UnaryExpression GetMethodBasedUnaryOperator(
      ExpressionType unaryType,
      Expression operand,
      MethodInfo method)
    {
      Expression.ValidateOperator(method);
      ParameterInfo[] parametersCached = method.GetParametersCached();
      if (parametersCached.Length != 1)
        throw Error.IncorrectNumberOfMethodCallArguments((object) method, nameof (method));
      if (Expression.ParameterIsAssignable(parametersCached[0], operand.Type))
      {
        Expression.ValidateParamswithOperandsOrThrow(parametersCached[0].ParameterType, operand.Type, unaryType, method.Name);
        return new UnaryExpression(unaryType, operand, method.ReturnType, method);
      }
      if (operand.Type.IsNullableType() && Expression.ParameterIsAssignable(parametersCached[0], operand.Type.GetNonNullableType()) && (method.ReturnType.IsValueType && !method.ReturnType.IsNullableType()))
        return new UnaryExpression(unaryType, operand, method.ReturnType.GetNullableType(), method);
      throw Error.OperandTypesDoNotMatchParameters((object) unaryType, (object) method.Name);
    }

    private static UnaryExpression GetUserDefinedCoercionOrThrow(
      ExpressionType coercionType,
      Expression expression,
      Type convertToType)
    {
      UnaryExpression userDefinedCoercion = Expression.GetUserDefinedCoercion(coercionType, expression, convertToType);
      if (userDefinedCoercion != null)
        return userDefinedCoercion;
      throw Error.CoercionOperatorNotDefined((object) expression.Type, (object) convertToType);
    }

    private static UnaryExpression GetUserDefinedCoercion(
      ExpressionType coercionType,
      Expression expression,
      Type convertToType)
    {
      MethodInfo definedCoercionMethod = TypeUtils.GetUserDefinedCoercionMethod(expression.Type, convertToType);
      if (definedCoercionMethod != (MethodInfo) null)
        return new UnaryExpression(coercionType, expression, convertToType, definedCoercionMethod);
      return (UnaryExpression) null;
    }

    private static UnaryExpression GetMethodBasedCoercionOperator(
      ExpressionType unaryType,
      Expression operand,
      Type convertToType,
      MethodInfo method)
    {
      Expression.ValidateOperator(method);
      ParameterInfo[] parametersCached = method.GetParametersCached();
      if (parametersCached.Length != 1)
        throw Error.IncorrectNumberOfMethodCallArguments((object) method, nameof (method));
      if (Expression.ParameterIsAssignable(parametersCached[0], operand.Type) && TypeUtils.AreEquivalent(method.ReturnType, convertToType))
        return new UnaryExpression(unaryType, operand, method.ReturnType, method);
      if ((operand.Type.IsNullableType() || convertToType.IsNullableType()) && Expression.ParameterIsAssignable(parametersCached[0], operand.Type.GetNonNullableType()) && (TypeUtils.AreEquivalent(method.ReturnType, convertToType.GetNonNullableType()) || TypeUtils.AreEquivalent(method.ReturnType, convertToType)))
        return new UnaryExpression(unaryType, operand, convertToType, method);
      throw Error.OperandTypesDoNotMatchParameters((object) unaryType, (object) method.Name);
    }

    public static UnaryExpression Negate(Expression expression)
    {
      return Expression.Negate(expression, (MethodInfo) null);
    }

    public static UnaryExpression Negate(Expression expression, MethodInfo method)
    {
      ExpressionUtils.RequiresCanRead(expression, nameof (expression));
      if (!(method == (MethodInfo) null))
        return Expression.GetMethodBasedUnaryOperator(ExpressionType.Negate, expression, method);
      if (expression.Type.IsArithmetic() && !expression.Type.IsUnsignedInt())
        return new UnaryExpression(ExpressionType.Negate, expression, expression.Type, (MethodInfo) null);
      return Expression.GetUserDefinedUnaryOperatorOrThrow(ExpressionType.Negate, "op_UnaryNegation", expression);
    }

    public static UnaryExpression UnaryPlus(Expression expression)
    {
      return Expression.UnaryPlus(expression, (MethodInfo) null);
    }

    public static UnaryExpression UnaryPlus(Expression expression, MethodInfo method)
    {
      ExpressionUtils.RequiresCanRead(expression, nameof (expression));
      if (!(method == (MethodInfo) null))
        return Expression.GetMethodBasedUnaryOperator(ExpressionType.UnaryPlus, expression, method);
      if (expression.Type.IsArithmetic())
        return new UnaryExpression(ExpressionType.UnaryPlus, expression, expression.Type, (MethodInfo) null);
      return Expression.GetUserDefinedUnaryOperatorOrThrow(ExpressionType.UnaryPlus, "op_UnaryPlus", expression);
    }

    public static UnaryExpression NegateChecked(Expression expression)
    {
      return Expression.NegateChecked(expression, (MethodInfo) null);
    }

    public static UnaryExpression NegateChecked(
      Expression expression,
      MethodInfo method)
    {
      ExpressionUtils.RequiresCanRead(expression, nameof (expression));
      if (!(method == (MethodInfo) null))
        return Expression.GetMethodBasedUnaryOperator(ExpressionType.NegateChecked, expression, method);
      if (expression.Type.IsArithmetic() && !expression.Type.IsUnsignedInt())
        return new UnaryExpression(ExpressionType.NegateChecked, expression, expression.Type, (MethodInfo) null);
      return Expression.GetUserDefinedUnaryOperatorOrThrow(ExpressionType.NegateChecked, "op_UnaryNegation", expression);
    }

    public static UnaryExpression Not(Expression expression)
    {
      return Expression.Not(expression, (MethodInfo) null);
    }

    public static UnaryExpression Not(Expression expression, MethodInfo method)
    {
      ExpressionUtils.RequiresCanRead(expression, nameof (expression));
      if (!(method == (MethodInfo) null))
        return Expression.GetMethodBasedUnaryOperator(ExpressionType.Not, expression, method);
      if (expression.Type.IsIntegerOrBool())
        return new UnaryExpression(ExpressionType.Not, expression, expression.Type, (MethodInfo) null);
      return Expression.GetUserDefinedUnaryOperator(ExpressionType.Not, "op_LogicalNot", expression) ?? Expression.GetUserDefinedUnaryOperatorOrThrow(ExpressionType.Not, "op_OnesComplement", expression);
    }

    public static UnaryExpression IsFalse(Expression expression)
    {
      return Expression.IsFalse(expression, (MethodInfo) null);
    }

    public static UnaryExpression IsFalse(Expression expression, MethodInfo method)
    {
      ExpressionUtils.RequiresCanRead(expression, nameof (expression));
      if (!(method == (MethodInfo) null))
        return Expression.GetMethodBasedUnaryOperator(ExpressionType.IsFalse, expression, method);
      if (expression.Type.IsBool())
        return new UnaryExpression(ExpressionType.IsFalse, expression, expression.Type, (MethodInfo) null);
      return Expression.GetUserDefinedUnaryOperatorOrThrow(ExpressionType.IsFalse, "op_False", expression);
    }

    public static UnaryExpression IsTrue(Expression expression)
    {
      return Expression.IsTrue(expression, (MethodInfo) null);
    }

    public static UnaryExpression IsTrue(Expression expression, MethodInfo method)
    {
      ExpressionUtils.RequiresCanRead(expression, nameof (expression));
      if (!(method == (MethodInfo) null))
        return Expression.GetMethodBasedUnaryOperator(ExpressionType.IsTrue, expression, method);
      if (expression.Type.IsBool())
        return new UnaryExpression(ExpressionType.IsTrue, expression, expression.Type, (MethodInfo) null);
      return Expression.GetUserDefinedUnaryOperatorOrThrow(ExpressionType.IsTrue, "op_True", expression);
    }

    public static UnaryExpression OnesComplement(Expression expression)
    {
      return Expression.OnesComplement(expression, (MethodInfo) null);
    }

    public static UnaryExpression OnesComplement(
      Expression expression,
      MethodInfo method)
    {
      ExpressionUtils.RequiresCanRead(expression, nameof (expression));
      if (!(method == (MethodInfo) null))
        return Expression.GetMethodBasedUnaryOperator(ExpressionType.OnesComplement, expression, method);
      if (expression.Type.IsInteger())
        return new UnaryExpression(ExpressionType.OnesComplement, expression, expression.Type, (MethodInfo) null);
      return Expression.GetUserDefinedUnaryOperatorOrThrow(ExpressionType.OnesComplement, "op_OnesComplement", expression);
    }

    public static UnaryExpression TypeAs(Expression expression, Type type)
    {
      ExpressionUtils.RequiresCanRead(expression, nameof (expression));
      ContractUtils.RequiresNotNull((object) type, nameof (type));
      TypeUtils.ValidateType(type, nameof (type));
      if (type.IsValueType && !type.IsNullableType())
        throw Error.IncorrectTypeForTypeAs((object) type, nameof (type));
      return new UnaryExpression(ExpressionType.TypeAs, expression, type, (MethodInfo) null);
    }

    public static UnaryExpression Unbox(Expression expression, Type type)
    {
      ExpressionUtils.RequiresCanRead(expression, nameof (expression));
      ContractUtils.RequiresNotNull((object) type, nameof (type));
      if (!expression.Type.IsInterface && expression.Type != typeof (object))
        throw Error.InvalidUnboxType(nameof (expression));
      if (!type.IsValueType)
        throw Error.InvalidUnboxType(nameof (type));
      TypeUtils.ValidateType(type, nameof (type));
      return new UnaryExpression(ExpressionType.Unbox, expression, type, (MethodInfo) null);
    }

    public static UnaryExpression Convert(Expression expression, Type type)
    {
      return Expression.Convert(expression, type, (MethodInfo) null);
    }

    public static UnaryExpression Convert(
      Expression expression,
      Type type,
      MethodInfo method)
    {
      ExpressionUtils.RequiresCanRead(expression, nameof (expression));
      ContractUtils.RequiresNotNull((object) type, nameof (type));
      TypeUtils.ValidateType(type, nameof (type));
      if (!(method == (MethodInfo) null))
        return Expression.GetMethodBasedCoercionOperator(ExpressionType.Convert, expression, type, method);
      if (expression.Type.HasIdentityPrimitiveOrNullableConversionTo(type) || expression.Type.HasReferenceConversionTo(type))
        return new UnaryExpression(ExpressionType.Convert, expression, type, (MethodInfo) null);
      return Expression.GetUserDefinedCoercionOrThrow(ExpressionType.Convert, expression, type);
    }

    public static UnaryExpression ConvertChecked(Expression expression, Type type)
    {
      return Expression.ConvertChecked(expression, type, (MethodInfo) null);
    }

    public static UnaryExpression ConvertChecked(
      Expression expression,
      Type type,
      MethodInfo method)
    {
      ExpressionUtils.RequiresCanRead(expression, nameof (expression));
      ContractUtils.RequiresNotNull((object) type, nameof (type));
      TypeUtils.ValidateType(type, nameof (type));
      if (!(method == (MethodInfo) null))
        return Expression.GetMethodBasedCoercionOperator(ExpressionType.ConvertChecked, expression, type, method);
      if (expression.Type.HasIdentityPrimitiveOrNullableConversionTo(type))
        return new UnaryExpression(ExpressionType.ConvertChecked, expression, type, (MethodInfo) null);
      if (expression.Type.HasReferenceConversionTo(type))
        return new UnaryExpression(ExpressionType.Convert, expression, type, (MethodInfo) null);
      return Expression.GetUserDefinedCoercionOrThrow(ExpressionType.ConvertChecked, expression, type);
    }

    public static UnaryExpression ArrayLength(Expression array)
    {
      ExpressionUtils.RequiresCanRead(array, nameof (array));
      if (array.Type.IsSZArray)
        return new UnaryExpression(ExpressionType.ArrayLength, array, typeof (int), (MethodInfo) null);
      if (!array.Type.IsArray || !typeof (Array).IsAssignableFrom(array.Type))
        throw Error.ArgumentMustBeArray(nameof (array));
      throw Error.ArgumentMustBeSingleDimensionalArrayType(nameof (array));
    }

    public static UnaryExpression Quote(Expression expression)
    {
      ExpressionUtils.RequiresCanRead(expression, nameof (expression));
      LambdaExpression lambdaExpression = expression as LambdaExpression;
      if (lambdaExpression == null)
        throw Error.QuotedExpressionMustBeLambda(nameof (expression));
      return new UnaryExpression(ExpressionType.Quote, (Expression) lambdaExpression, lambdaExpression.PublicType, (MethodInfo) null);
    }

    public static UnaryExpression Rethrow()
    {
      return Expression.Throw((Expression) null);
    }

    public static UnaryExpression Rethrow(Type type)
    {
      return Expression.Throw((Expression) null, type);
    }

    public static UnaryExpression Throw(Expression value)
    {
      return Expression.Throw(value, typeof (void));
    }

    public static UnaryExpression Throw(Expression value, Type type)
    {
      ContractUtils.RequiresNotNull((object) type, nameof (type));
      TypeUtils.ValidateType(type, nameof (type));
      if (value != null)
      {
        ExpressionUtils.RequiresCanRead(value, nameof (value));
        if (value.Type.IsValueType)
          throw Error.ArgumentMustNotHaveValueType(nameof (value));
      }
      return new UnaryExpression(ExpressionType.Throw, value, type, (MethodInfo) null);
    }

    public static UnaryExpression Increment(Expression expression)
    {
      return Expression.Increment(expression, (MethodInfo) null);
    }

    public static UnaryExpression Increment(Expression expression, MethodInfo method)
    {
      ExpressionUtils.RequiresCanRead(expression, nameof (expression));
      if (!(method == (MethodInfo) null))
        return Expression.GetMethodBasedUnaryOperator(ExpressionType.Increment, expression, method);
      if (expression.Type.IsArithmetic())
        return new UnaryExpression(ExpressionType.Increment, expression, expression.Type, (MethodInfo) null);
      return Expression.GetUserDefinedUnaryOperatorOrThrow(ExpressionType.Increment, "op_Increment", expression);
    }

    public static UnaryExpression Decrement(Expression expression)
    {
      return Expression.Decrement(expression, (MethodInfo) null);
    }

    public static UnaryExpression Decrement(Expression expression, MethodInfo method)
    {
      ExpressionUtils.RequiresCanRead(expression, nameof (expression));
      if (!(method == (MethodInfo) null))
        return Expression.GetMethodBasedUnaryOperator(ExpressionType.Decrement, expression, method);
      if (expression.Type.IsArithmetic())
        return new UnaryExpression(ExpressionType.Decrement, expression, expression.Type, (MethodInfo) null);
      return Expression.GetUserDefinedUnaryOperatorOrThrow(ExpressionType.Decrement, "op_Decrement", expression);
    }

    public static UnaryExpression PreIncrementAssign(Expression expression)
    {
      return Expression.MakeOpAssignUnary(ExpressionType.PreIncrementAssign, expression, (MethodInfo) null);
    }

    public static UnaryExpression PreIncrementAssign(
      Expression expression,
      MethodInfo method)
    {
      return Expression.MakeOpAssignUnary(ExpressionType.PreIncrementAssign, expression, method);
    }

    public static UnaryExpression PreDecrementAssign(Expression expression)
    {
      return Expression.MakeOpAssignUnary(ExpressionType.PreDecrementAssign, expression, (MethodInfo) null);
    }

    public static UnaryExpression PreDecrementAssign(
      Expression expression,
      MethodInfo method)
    {
      return Expression.MakeOpAssignUnary(ExpressionType.PreDecrementAssign, expression, method);
    }

    public static UnaryExpression PostIncrementAssign(Expression expression)
    {
      return Expression.MakeOpAssignUnary(ExpressionType.PostIncrementAssign, expression, (MethodInfo) null);
    }

    public static UnaryExpression PostIncrementAssign(
      Expression expression,
      MethodInfo method)
    {
      return Expression.MakeOpAssignUnary(ExpressionType.PostIncrementAssign, expression, method);
    }

    public static UnaryExpression PostDecrementAssign(Expression expression)
    {
      return Expression.MakeOpAssignUnary(ExpressionType.PostDecrementAssign, expression, (MethodInfo) null);
    }

    public static UnaryExpression PostDecrementAssign(
      Expression expression,
      MethodInfo method)
    {
      return Expression.MakeOpAssignUnary(ExpressionType.PostDecrementAssign, expression, method);
    }

    private static UnaryExpression MakeOpAssignUnary(
      ExpressionType kind,
      Expression expression,
      MethodInfo method)
    {
      ExpressionUtils.RequiresCanRead(expression, nameof (expression));
      Expression.RequiresCanWrite(expression, nameof (expression));
      UnaryExpression unaryExpression;
      if (method == (MethodInfo) null)
      {
        if (expression.Type.IsArithmetic())
          return new UnaryExpression(kind, expression, expression.Type, (MethodInfo) null);
        string name = kind == ExpressionType.PreIncrementAssign || kind == ExpressionType.PostIncrementAssign ? "op_Increment" : "op_Decrement";
        unaryExpression = Expression.GetUserDefinedUnaryOperatorOrThrow(kind, name, expression);
      }
      else
        unaryExpression = Expression.GetMethodBasedUnaryOperator(kind, expression, method);
      if (!TypeUtils.AreReferenceAssignable(expression.Type, unaryExpression.Type))
        throw Error.UserDefinedOpMustHaveValidReturnType((object) kind, (object) method.Name);
      return unaryExpression;
    }

    private class ExtensionInfo
    {
      internal readonly ExpressionType NodeType;
      internal readonly Type Type;

      public ExtensionInfo(ExpressionType nodeType, Type type)
      {
        this.NodeType = nodeType;
        this.Type = type;
      }
    }

    internal class BinaryExpressionProxy
    {
      private readonly BinaryExpression _node;

      public BinaryExpressionProxy(BinaryExpression node)
      {
        ContractUtils.RequiresNotNull((object) node, nameof (node));
        this._node = node;
      }

      public bool CanReduce
      {
        get
        {
          return this._node.CanReduce;
        }
      }

      public LambdaExpression Conversion
      {
        get
        {
          return this._node.Conversion;
        }
      }

      public string DebugView
      {
        get
        {
          return this._node.DebugView;
        }
      }

      public bool IsLifted
      {
        get
        {
          return this._node.IsLifted;
        }
      }

      public bool IsLiftedToNull
      {
        get
        {
          return this._node.IsLiftedToNull;
        }
      }

      public Expression Left
      {
        get
        {
          return this._node.Left;
        }
      }

      public MethodInfo Method
      {
        get
        {
          return this._node.Method;
        }
      }

      public ExpressionType NodeType
      {
        get
        {
          return this._node.NodeType;
        }
      }

      public Expression Right
      {
        get
        {
          return this._node.Right;
        }
      }

      public Type Type
      {
        get
        {
          return this._node.Type;
        }
      }
    }

    internal class BlockExpressionProxy
    {
      private readonly BlockExpression _node;

      public BlockExpressionProxy(BlockExpression node)
      {
        ContractUtils.RequiresNotNull((object) node, nameof (node));
        this._node = node;
      }

      public bool CanReduce
      {
        get
        {
          return this._node.CanReduce;
        }
      }

      public string DebugView
      {
        get
        {
          return this._node.DebugView;
        }
      }

      public ReadOnlyCollection<Expression> Expressions
      {
        get
        {
          return this._node.Expressions;
        }
      }

      public ExpressionType NodeType
      {
        get
        {
          return this._node.NodeType;
        }
      }

      public Expression Result
      {
        get
        {
          return this._node.Result;
        }
      }

      public Type Type
      {
        get
        {
          return this._node.Type;
        }
      }

      public ReadOnlyCollection<ParameterExpression> Variables
      {
        get
        {
          return this._node.Variables;
        }
      }
    }

    internal class CatchBlockProxy
    {
      private readonly CatchBlock _node;

      public CatchBlockProxy(CatchBlock node)
      {
        ContractUtils.RequiresNotNull((object) node, nameof (node));
        this._node = node;
      }

      public Expression Body
      {
        get
        {
          return this._node.Body;
        }
      }

      public Expression Filter
      {
        get
        {
          return this._node.Filter;
        }
      }

      public Type Test
      {
        get
        {
          return this._node.Test;
        }
      }

      public ParameterExpression Variable
      {
        get
        {
          return this._node.Variable;
        }
      }
    }

    internal class ConditionalExpressionProxy
    {
      private readonly ConditionalExpression _node;

      public ConditionalExpressionProxy(ConditionalExpression node)
      {
        ContractUtils.RequiresNotNull((object) node, nameof (node));
        this._node = node;
      }

      public bool CanReduce
      {
        get
        {
          return this._node.CanReduce;
        }
      }

      public string DebugView
      {
        get
        {
          return this._node.DebugView;
        }
      }

      public Expression IfFalse
      {
        get
        {
          return this._node.IfFalse;
        }
      }

      public Expression IfTrue
      {
        get
        {
          return this._node.IfTrue;
        }
      }

      public ExpressionType NodeType
      {
        get
        {
          return this._node.NodeType;
        }
      }

      public Expression Test
      {
        get
        {
          return this._node.Test;
        }
      }

      public Type Type
      {
        get
        {
          return this._node.Type;
        }
      }
    }

    internal class ConstantExpressionProxy
    {
      private readonly ConstantExpression _node;

      public ConstantExpressionProxy(ConstantExpression node)
      {
        ContractUtils.RequiresNotNull((object) node, nameof (node));
        this._node = node;
      }

      public bool CanReduce
      {
        get
        {
          return this._node.CanReduce;
        }
      }

      public string DebugView
      {
        get
        {
          return this._node.DebugView;
        }
      }

      public ExpressionType NodeType
      {
        get
        {
          return this._node.NodeType;
        }
      }

      public Type Type
      {
        get
        {
          return this._node.Type;
        }
      }

      public object Value
      {
        get
        {
          return this._node.Value;
        }
      }
    }

    internal class DebugInfoExpressionProxy
    {
      private readonly DebugInfoExpression _node;

      public DebugInfoExpressionProxy(DebugInfoExpression node)
      {
        ContractUtils.RequiresNotNull((object) node, nameof (node));
        this._node = node;
      }

      public bool CanReduce
      {
        get
        {
          return this._node.CanReduce;
        }
      }

      public string DebugView
      {
        get
        {
          return this._node.DebugView;
        }
      }

      public SymbolDocumentInfo Document
      {
        get
        {
          return this._node.Document;
        }
      }

      public int EndColumn
      {
        get
        {
          return this._node.EndColumn;
        }
      }

      public int EndLine
      {
        get
        {
          return this._node.EndLine;
        }
      }

      public bool IsClear
      {
        get
        {
          return this._node.IsClear;
        }
      }

      public ExpressionType NodeType
      {
        get
        {
          return this._node.NodeType;
        }
      }

      public int StartColumn
      {
        get
        {
          return this._node.StartColumn;
        }
      }

      public int StartLine
      {
        get
        {
          return this._node.StartLine;
        }
      }

      public Type Type
      {
        get
        {
          return this._node.Type;
        }
      }
    }

    internal class DefaultExpressionProxy
    {
      private readonly DefaultExpression _node;

      public DefaultExpressionProxy(DefaultExpression node)
      {
        ContractUtils.RequiresNotNull((object) node, nameof (node));
        this._node = node;
      }

      public bool CanReduce
      {
        get
        {
          return this._node.CanReduce;
        }
      }

      public string DebugView
      {
        get
        {
          return this._node.DebugView;
        }
      }

      public ExpressionType NodeType
      {
        get
        {
          return this._node.NodeType;
        }
      }

      public Type Type
      {
        get
        {
          return this._node.Type;
        }
      }
    }

    internal class GotoExpressionProxy
    {
      private readonly GotoExpression _node;

      public GotoExpressionProxy(GotoExpression node)
      {
        ContractUtils.RequiresNotNull((object) node, nameof (node));
        this._node = node;
      }

      public bool CanReduce
      {
        get
        {
          return this._node.CanReduce;
        }
      }

      public string DebugView
      {
        get
        {
          return this._node.DebugView;
        }
      }

      public GotoExpressionKind Kind
      {
        get
        {
          return this._node.Kind;
        }
      }

      public ExpressionType NodeType
      {
        get
        {
          return this._node.NodeType;
        }
      }

      public LabelTarget Target
      {
        get
        {
          return this._node.Target;
        }
      }

      public Type Type
      {
        get
        {
          return this._node.Type;
        }
      }

      public Expression Value
      {
        get
        {
          return this._node.Value;
        }
      }
    }

    internal class IndexExpressionProxy
    {
      private readonly IndexExpression _node;

      public IndexExpressionProxy(IndexExpression node)
      {
        ContractUtils.RequiresNotNull((object) node, nameof (node));
        this._node = node;
      }

      public ReadOnlyCollection<Expression> Arguments
      {
        get
        {
          return this._node.Arguments;
        }
      }

      public bool CanReduce
      {
        get
        {
          return this._node.CanReduce;
        }
      }

      public string DebugView
      {
        get
        {
          return this._node.DebugView;
        }
      }

      public PropertyInfo Indexer
      {
        get
        {
          return this._node.Indexer;
        }
      }

      public ExpressionType NodeType
      {
        get
        {
          return this._node.NodeType;
        }
      }

      public Expression Object
      {
        get
        {
          return this._node.Object;
        }
      }

      public Type Type
      {
        get
        {
          return this._node.Type;
        }
      }
    }

    internal class InvocationExpressionProxy
    {
      private readonly InvocationExpression _node;

      public InvocationExpressionProxy(InvocationExpression node)
      {
        ContractUtils.RequiresNotNull((object) node, nameof (node));
        this._node = node;
      }

      public ReadOnlyCollection<Expression> Arguments
      {
        get
        {
          return this._node.Arguments;
        }
      }

      public bool CanReduce
      {
        get
        {
          return this._node.CanReduce;
        }
      }

      public string DebugView
      {
        get
        {
          return this._node.DebugView;
        }
      }

      public Expression Expression
      {
        get
        {
          return this._node.Expression;
        }
      }

      public ExpressionType NodeType
      {
        get
        {
          return this._node.NodeType;
        }
      }

      public Type Type
      {
        get
        {
          return this._node.Type;
        }
      }
    }

    internal class LabelExpressionProxy
    {
      private readonly LabelExpression _node;

      public LabelExpressionProxy(LabelExpression node)
      {
        ContractUtils.RequiresNotNull((object) node, nameof (node));
        this._node = node;
      }

      public bool CanReduce
      {
        get
        {
          return this._node.CanReduce;
        }
      }

      public string DebugView
      {
        get
        {
          return this._node.DebugView;
        }
      }

      public Expression DefaultValue
      {
        get
        {
          return this._node.DefaultValue;
        }
      }

      public ExpressionType NodeType
      {
        get
        {
          return this._node.NodeType;
        }
      }

      public LabelTarget Target
      {
        get
        {
          return this._node.Target;
        }
      }

      public Type Type
      {
        get
        {
          return this._node.Type;
        }
      }
    }

    internal class LambdaExpressionProxy
    {
      private readonly LambdaExpression _node;

      public LambdaExpressionProxy(LambdaExpression node)
      {
        ContractUtils.RequiresNotNull((object) node, nameof (node));
        this._node = node;
      }

      public Expression Body
      {
        get
        {
          return this._node.Body;
        }
      }

      public bool CanReduce
      {
        get
        {
          return this._node.CanReduce;
        }
      }

      public string DebugView
      {
        get
        {
          return this._node.DebugView;
        }
      }

      public string Name
      {
        get
        {
          return this._node.Name;
        }
      }

      public ExpressionType NodeType
      {
        get
        {
          return this._node.NodeType;
        }
      }

      public ReadOnlyCollection<ParameterExpression> Parameters
      {
        get
        {
          return this._node.Parameters;
        }
      }

      public Type ReturnType
      {
        get
        {
          return this._node.ReturnType;
        }
      }

      public bool TailCall
      {
        get
        {
          return this._node.TailCall;
        }
      }

      public Type Type
      {
        get
        {
          return this._node.Type;
        }
      }
    }

    internal class ListInitExpressionProxy
    {
      private readonly ListInitExpression _node;

      public ListInitExpressionProxy(ListInitExpression node)
      {
        ContractUtils.RequiresNotNull((object) node, nameof (node));
        this._node = node;
      }

      public bool CanReduce
      {
        get
        {
          return this._node.CanReduce;
        }
      }

      public string DebugView
      {
        get
        {
          return this._node.DebugView;
        }
      }

      public ReadOnlyCollection<ElementInit> Initializers
      {
        get
        {
          return this._node.Initializers;
        }
      }

      public NewExpression NewExpression
      {
        get
        {
          return this._node.NewExpression;
        }
      }

      public ExpressionType NodeType
      {
        get
        {
          return this._node.NodeType;
        }
      }

      public Type Type
      {
        get
        {
          return this._node.Type;
        }
      }
    }

    internal class LoopExpressionProxy
    {
      private readonly LoopExpression _node;

      public LoopExpressionProxy(LoopExpression node)
      {
        ContractUtils.RequiresNotNull((object) node, nameof (node));
        this._node = node;
      }

      public Expression Body
      {
        get
        {
          return this._node.Body;
        }
      }

      public LabelTarget BreakLabel
      {
        get
        {
          return this._node.BreakLabel;
        }
      }

      public bool CanReduce
      {
        get
        {
          return this._node.CanReduce;
        }
      }

      public LabelTarget ContinueLabel
      {
        get
        {
          return this._node.ContinueLabel;
        }
      }

      public string DebugView
      {
        get
        {
          return this._node.DebugView;
        }
      }

      public ExpressionType NodeType
      {
        get
        {
          return this._node.NodeType;
        }
      }

      public Type Type
      {
        get
        {
          return this._node.Type;
        }
      }
    }

    internal class MemberExpressionProxy
    {
      private readonly MemberExpression _node;

      public MemberExpressionProxy(MemberExpression node)
      {
        ContractUtils.RequiresNotNull((object) node, nameof (node));
        this._node = node;
      }

      public bool CanReduce
      {
        get
        {
          return this._node.CanReduce;
        }
      }

      public string DebugView
      {
        get
        {
          return this._node.DebugView;
        }
      }

      public Expression Expression
      {
        get
        {
          return this._node.Expression;
        }
      }

      public MemberInfo Member
      {
        get
        {
          return this._node.Member;
        }
      }

      public ExpressionType NodeType
      {
        get
        {
          return this._node.NodeType;
        }
      }

      public Type Type
      {
        get
        {
          return this._node.Type;
        }
      }
    }

    internal class MemberInitExpressionProxy
    {
      private readonly MemberInitExpression _node;

      public MemberInitExpressionProxy(MemberInitExpression node)
      {
        ContractUtils.RequiresNotNull((object) node, nameof (node));
        this._node = node;
      }

      public ReadOnlyCollection<MemberBinding> Bindings
      {
        get
        {
          return this._node.Bindings;
        }
      }

      public bool CanReduce
      {
        get
        {
          return this._node.CanReduce;
        }
      }

      public string DebugView
      {
        get
        {
          return this._node.DebugView;
        }
      }

      public NewExpression NewExpression
      {
        get
        {
          return this._node.NewExpression;
        }
      }

      public ExpressionType NodeType
      {
        get
        {
          return this._node.NodeType;
        }
      }

      public Type Type
      {
        get
        {
          return this._node.Type;
        }
      }
    }

    internal class MethodCallExpressionProxy
    {
      private readonly MethodCallExpression _node;

      public MethodCallExpressionProxy(MethodCallExpression node)
      {
        ContractUtils.RequiresNotNull((object) node, nameof (node));
        this._node = node;
      }

      public ReadOnlyCollection<Expression> Arguments
      {
        get
        {
          return this._node.Arguments;
        }
      }

      public bool CanReduce
      {
        get
        {
          return this._node.CanReduce;
        }
      }

      public string DebugView
      {
        get
        {
          return this._node.DebugView;
        }
      }

      public MethodInfo Method
      {
        get
        {
          return this._node.Method;
        }
      }

      public ExpressionType NodeType
      {
        get
        {
          return this._node.NodeType;
        }
      }

      public Expression Object
      {
        get
        {
          return this._node.Object;
        }
      }

      public Type Type
      {
        get
        {
          return this._node.Type;
        }
      }
    }

    internal class NewArrayExpressionProxy
    {
      private readonly NewArrayExpression _node;

      public NewArrayExpressionProxy(NewArrayExpression node)
      {
        ContractUtils.RequiresNotNull((object) node, nameof (node));
        this._node = node;
      }

      public bool CanReduce
      {
        get
        {
          return this._node.CanReduce;
        }
      }

      public string DebugView
      {
        get
        {
          return this._node.DebugView;
        }
      }

      public ReadOnlyCollection<Expression> Expressions
      {
        get
        {
          return this._node.Expressions;
        }
      }

      public ExpressionType NodeType
      {
        get
        {
          return this._node.NodeType;
        }
      }

      public Type Type
      {
        get
        {
          return this._node.Type;
        }
      }
    }

    internal class NewExpressionProxy
    {
      private readonly NewExpression _node;

      public NewExpressionProxy(NewExpression node)
      {
        ContractUtils.RequiresNotNull((object) node, nameof (node));
        this._node = node;
      }

      public ReadOnlyCollection<Expression> Arguments
      {
        get
        {
          return this._node.Arguments;
        }
      }

      public bool CanReduce
      {
        get
        {
          return this._node.CanReduce;
        }
      }

      public ConstructorInfo Constructor
      {
        get
        {
          return this._node.Constructor;
        }
      }

      public string DebugView
      {
        get
        {
          return this._node.DebugView;
        }
      }

      public ReadOnlyCollection<MemberInfo> Members
      {
        get
        {
          return this._node.Members;
        }
      }

      public ExpressionType NodeType
      {
        get
        {
          return this._node.NodeType;
        }
      }

      public Type Type
      {
        get
        {
          return this._node.Type;
        }
      }
    }

    internal class ParameterExpressionProxy
    {
      private readonly ParameterExpression _node;

      public ParameterExpressionProxy(ParameterExpression node)
      {
        ContractUtils.RequiresNotNull((object) node, nameof (node));
        this._node = node;
      }

      public bool CanReduce
      {
        get
        {
          return this._node.CanReduce;
        }
      }

      public string DebugView
      {
        get
        {
          return this._node.DebugView;
        }
      }

      public bool IsByRef
      {
        get
        {
          return this._node.IsByRef;
        }
      }

      public string Name
      {
        get
        {
          return this._node.Name;
        }
      }

      public ExpressionType NodeType
      {
        get
        {
          return this._node.NodeType;
        }
      }

      public Type Type
      {
        get
        {
          return this._node.Type;
        }
      }
    }

    internal class RuntimeVariablesExpressionProxy
    {
      private readonly RuntimeVariablesExpression _node;

      public RuntimeVariablesExpressionProxy(RuntimeVariablesExpression node)
      {
        ContractUtils.RequiresNotNull((object) node, nameof (node));
        this._node = node;
      }

      public bool CanReduce
      {
        get
        {
          return this._node.CanReduce;
        }
      }

      public string DebugView
      {
        get
        {
          return this._node.DebugView;
        }
      }

      public ExpressionType NodeType
      {
        get
        {
          return this._node.NodeType;
        }
      }

      public Type Type
      {
        get
        {
          return this._node.Type;
        }
      }

      public ReadOnlyCollection<ParameterExpression> Variables
      {
        get
        {
          return this._node.Variables;
        }
      }
    }

    internal class SwitchCaseProxy
    {
      private readonly SwitchCase _node;

      public SwitchCaseProxy(SwitchCase node)
      {
        ContractUtils.RequiresNotNull((object) node, nameof (node));
        this._node = node;
      }

      public Expression Body
      {
        get
        {
          return this._node.Body;
        }
      }

      public ReadOnlyCollection<Expression> TestValues
      {
        get
        {
          return this._node.TestValues;
        }
      }
    }

    internal class SwitchExpressionProxy
    {
      private readonly SwitchExpression _node;

      public SwitchExpressionProxy(SwitchExpression node)
      {
        ContractUtils.RequiresNotNull((object) node, nameof (node));
        this._node = node;
      }

      public bool CanReduce
      {
        get
        {
          return this._node.CanReduce;
        }
      }

      public ReadOnlyCollection<SwitchCase> Cases
      {
        get
        {
          return this._node.Cases;
        }
      }

      public MethodInfo Comparison
      {
        get
        {
          return this._node.Comparison;
        }
      }

      public string DebugView
      {
        get
        {
          return this._node.DebugView;
        }
      }

      public Expression DefaultBody
      {
        get
        {
          return this._node.DefaultBody;
        }
      }

      public ExpressionType NodeType
      {
        get
        {
          return this._node.NodeType;
        }
      }

      public Expression SwitchValue
      {
        get
        {
          return this._node.SwitchValue;
        }
      }

      public Type Type
      {
        get
        {
          return this._node.Type;
        }
      }
    }

    internal class TryExpressionProxy
    {
      private readonly TryExpression _node;

      public TryExpressionProxy(TryExpression node)
      {
        ContractUtils.RequiresNotNull((object) node, nameof (node));
        this._node = node;
      }

      public Expression Body
      {
        get
        {
          return this._node.Body;
        }
      }

      public bool CanReduce
      {
        get
        {
          return this._node.CanReduce;
        }
      }

      public string DebugView
      {
        get
        {
          return this._node.DebugView;
        }
      }

      public Expression Fault
      {
        get
        {
          return this._node.Fault;
        }
      }

      public Expression Finally
      {
        get
        {
          return this._node.Finally;
        }
      }

      public ReadOnlyCollection<CatchBlock> Handlers
      {
        get
        {
          return this._node.Handlers;
        }
      }

      public ExpressionType NodeType
      {
        get
        {
          return this._node.NodeType;
        }
      }

      public Type Type
      {
        get
        {
          return this._node.Type;
        }
      }
    }

    internal class TypeBinaryExpressionProxy
    {
      private readonly TypeBinaryExpression _node;

      public TypeBinaryExpressionProxy(TypeBinaryExpression node)
      {
        ContractUtils.RequiresNotNull((object) node, nameof (node));
        this._node = node;
      }

      public bool CanReduce
      {
        get
        {
          return this._node.CanReduce;
        }
      }

      public string DebugView
      {
        get
        {
          return this._node.DebugView;
        }
      }

      public Expression Expression
      {
        get
        {
          return this._node.Expression;
        }
      }

      public ExpressionType NodeType
      {
        get
        {
          return this._node.NodeType;
        }
      }

      public Type Type
      {
        get
        {
          return this._node.Type;
        }
      }

      public Type TypeOperand
      {
        get
        {
          return this._node.TypeOperand;
        }
      }
    }

    internal class UnaryExpressionProxy
    {
      private readonly UnaryExpression _node;

      public UnaryExpressionProxy(UnaryExpression node)
      {
        ContractUtils.RequiresNotNull((object) node, nameof (node));
        this._node = node;
      }

      public bool CanReduce
      {
        get
        {
          return this._node.CanReduce;
        }
      }

      public string DebugView
      {
        get
        {
          return this._node.DebugView;
        }
      }

      public bool IsLifted
      {
        get
        {
          return this._node.IsLifted;
        }
      }

      public bool IsLiftedToNull
      {
        get
        {
          return this._node.IsLiftedToNull;
        }
      }

      public MethodInfo Method
      {
        get
        {
          return this._node.Method;
        }
      }

      public ExpressionType NodeType
      {
        get
        {
          return this._node.NodeType;
        }
      }

      public Expression Operand
      {
        get
        {
          return this._node.Operand;
        }
      }

      public Type Type
      {
        get
        {
          return this._node.Type;
        }
      }
    }

    private enum TryGetFuncActionArgsResult
    {
      Valid,
      ArgumentNull,
      ByRef,
      PointerOrVoid,
    }
  }
}
