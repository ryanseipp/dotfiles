// Decompiled with JetBrains decompiler
// Type: System.Reflection.MethodBase
// Assembly: System.Private.CoreLib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e
// MVID: 1E12AA92-1E57-4067-A322-8F82407CE9DD
// Assembly location: /opt/dotnet/shared/Microsoft.NETCore.App/2.2.3/System.Private.CoreLib.dll

using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading;

namespace System.Reflection
{
  public abstract class MethodBase : MemberInfo
  {
    public static MethodBase GetMethodFromHandle(RuntimeMethodHandle handle)
    {
      if (handle.IsNullHandle())
        throw new ArgumentException(SR.Argument_InvalidHandle);
      MethodBase methodBase = RuntimeType.GetMethodBase(handle.GetMethodInfo());
      Type declaringType = methodBase.DeclaringType;
      if (declaringType != (Type) null && declaringType.IsGenericType)
        throw new ArgumentException(string.Format((IFormatProvider) CultureInfo.CurrentCulture, SR.Argument_MethodDeclaringTypeGeneric, (object) methodBase, (object) declaringType.GetGenericTypeDefinition()));
      return methodBase;
    }

    public static MethodBase GetMethodFromHandle(
      RuntimeMethodHandle handle,
      RuntimeTypeHandle declaringType)
    {
      if (handle.IsNullHandle())
        throw new ArgumentException(SR.Argument_InvalidHandle);
      return RuntimeType.GetMethodBase(declaringType.GetRuntimeType(), handle.GetMethodInfo());
    }

    public static MethodBase GetCurrentMethod()
    {
      StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
      return RuntimeMethodInfo.InternalGetCurrentMethod(ref stackMark);
    }

    private IntPtr GetMethodDesc()
    {
      return this.MethodHandle.Value;
    }

    internal virtual ParameterInfo[] GetParametersNoCopy()
    {
      return this.GetParameters();
    }

    internal static string ConstructParameters(
      Type[] parameterTypes,
      CallingConventions callingConvention,
      bool serialization)
    {
      StringBuilder stringBuilder = new StringBuilder();
      string str1 = "";
      for (int index = 0; index < parameterTypes.Length; ++index)
      {
        Type parameterType = parameterTypes[index];
        stringBuilder.Append(str1);
        string str2 = parameterType.FormatTypeName(serialization);
        if (parameterType.IsByRef && !serialization)
        {
          stringBuilder.Append(str2.TrimEnd('&'));
          stringBuilder.Append(" ByRef");
        }
        else
          stringBuilder.Append(str2);
        str1 = ", ";
      }
      if ((callingConvention & CallingConventions.VarArgs) == CallingConventions.VarArgs)
      {
        stringBuilder.Append(str1);
        stringBuilder.Append("...");
      }
      return stringBuilder.ToString();
    }

    internal string FormatNameAndSig()
    {
      return this.FormatNameAndSig(false);
    }

    internal virtual string FormatNameAndSig(bool serialization)
    {
      StringBuilder stringBuilder = new StringBuilder(this.Name);
      stringBuilder.Append("(");
      stringBuilder.Append(MethodBase.ConstructParameters(this.GetParameterTypes(), this.CallingConvention, serialization));
      stringBuilder.Append(")");
      return stringBuilder.ToString();
    }

    internal virtual Type[] GetParameterTypes()
    {
      ParameterInfo[] parametersNoCopy = this.GetParametersNoCopy();
      Type[] typeArray = new Type[parametersNoCopy.Length];
      for (int index = 0; index < parametersNoCopy.Length; ++index)
        typeArray[index] = parametersNoCopy[index].ParameterType;
      return typeArray;
    }

    internal object[] CheckArguments(
      object[] parameters,
      Binder binder,
      BindingFlags invokeAttr,
      CultureInfo culture,
      Signature sig)
    {
      object[] objArray = new object[parameters.Length];
      ParameterInfo[] parameterInfoArray = (ParameterInfo[]) null;
      for (int index = 0; index < parameters.Length; ++index)
      {
        object obj = parameters[index];
        RuntimeType runtimeType = sig.Arguments[index];
        if (obj == Type.Missing)
        {
          if (parameterInfoArray == null)
            parameterInfoArray = this.GetParametersNoCopy();
          if (parameterInfoArray[index].DefaultValue == DBNull.Value)
            throw new ArgumentException(SR.Arg_VarMissNull, nameof (parameters));
          obj = parameterInfoArray[index].DefaultValue;
        }
        objArray[index] = runtimeType.CheckValue(obj, binder, culture, invokeAttr);
      }
      return objArray;
    }

    public abstract ParameterInfo[] GetParameters();

    public abstract MethodAttributes Attributes { get; }

    public virtual MethodImplAttributes MethodImplementationFlags
    {
      get
      {
        return this.GetMethodImplementationFlags();
      }
    }

    public abstract MethodImplAttributes GetMethodImplementationFlags();

    public virtual MethodBody GetMethodBody()
    {
      throw new InvalidOperationException();
    }

    public virtual CallingConventions CallingConvention
    {
      get
      {
        return CallingConventions.Standard;
      }
    }

    public bool IsAbstract
    {
      get
      {
        return (uint) (this.Attributes & MethodAttributes.Abstract) > 0U;
      }
    }

    public bool IsConstructor
    {
      get
      {
        if ((object) (this as ConstructorInfo) != null && !this.IsStatic)
          return (this.Attributes & MethodAttributes.RTSpecialName) == MethodAttributes.RTSpecialName;
        return false;
      }
    }

    public bool IsFinal
    {
      get
      {
        return (uint) (this.Attributes & MethodAttributes.Final) > 0U;
      }
    }

    public bool IsHideBySig
    {
      get
      {
        return (uint) (this.Attributes & MethodAttributes.HideBySig) > 0U;
      }
    }

    public bool IsSpecialName
    {
      get
      {
        return (uint) (this.Attributes & MethodAttributes.SpecialName) > 0U;
      }
    }

    public bool IsStatic
    {
      get
      {
        return (uint) (this.Attributes & MethodAttributes.Static) > 0U;
      }
    }

    public bool IsVirtual
    {
      get
      {
        return (uint) (this.Attributes & MethodAttributes.Virtual) > 0U;
      }
    }

    public bool IsAssembly
    {
      get
      {
        return (this.Attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.Assembly;
      }
    }

    public bool IsFamily
    {
      get
      {
        return (this.Attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.Family;
      }
    }

    public bool IsFamilyAndAssembly
    {
      get
      {
        return (this.Attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.FamANDAssem;
      }
    }

    public bool IsFamilyOrAssembly
    {
      get
      {
        return (this.Attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.FamORAssem;
      }
    }

    public bool IsPrivate
    {
      get
      {
        return (this.Attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.Private;
      }
    }

    public bool IsPublic
    {
      get
      {
        return (this.Attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.Public;
      }
    }

    public virtual bool IsConstructedGenericMethod
    {
      get
      {
        if (this.IsGenericMethod)
          return !this.IsGenericMethodDefinition;
        return false;
      }
    }

    public virtual bool IsGenericMethod
    {
      get
      {
        return false;
      }
    }

    public virtual bool IsGenericMethodDefinition
    {
      get
      {
        return false;
      }
    }

    public virtual Type[] GetGenericArguments()
    {
      throw new NotSupportedException(SR.NotSupported_SubclassOverride);
    }

    public virtual bool ContainsGenericParameters
    {
      get
      {
        return false;
      }
    }

    [DebuggerHidden]
    [DebuggerStepThrough]
    public object Invoke(object obj, object[] parameters)
    {
      return this.Invoke(obj, BindingFlags.Default, (Binder) null, parameters, (CultureInfo) null);
    }

    public abstract object Invoke(
      object obj,
      BindingFlags invokeAttr,
      Binder binder,
      object[] parameters,
      CultureInfo culture);

    public abstract RuntimeMethodHandle MethodHandle { get; }

    public virtual bool IsSecurityCritical
    {
      get
      {
        throw NotImplemented.ByDesign;
      }
    }

    public virtual bool IsSecuritySafeCritical
    {
      get
      {
        throw NotImplemented.ByDesign;
      }
    }

    public virtual bool IsSecurityTransparent
    {
      get
      {
        throw NotImplemented.ByDesign;
      }
    }

    public override bool Equals(object obj)
    {
      return base.Equals(obj);
    }

    public override int GetHashCode()
    {
      return base.GetHashCode();
    }

    public static bool operator ==(MethodBase left, MethodBase right)
    {
      if ((object) left == (object) right)
        return true;
      if ((object) left == null || (object) right == null)
        return false;
      MethodInfo methodInfo1;
      MethodInfo methodInfo2;
      if ((methodInfo1 = left as MethodInfo) != (MethodInfo) null && (methodInfo2 = right as MethodInfo) != (MethodInfo) null)
        return methodInfo1 == methodInfo2;
      ConstructorInfo constructorInfo1;
      ConstructorInfo constructorInfo2;
      if ((constructorInfo1 = left as ConstructorInfo) != (ConstructorInfo) null && (constructorInfo2 = right as ConstructorInfo) != (ConstructorInfo) null)
        return constructorInfo1 == constructorInfo2;
      return false;
    }

    public static bool operator !=(MethodBase left, MethodBase right)
    {
      return !(left == right);
    }
  }
}
