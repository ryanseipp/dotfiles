// Decompiled with JetBrains decompiler
// Type: System.Reflection.FieldInfo
// Assembly: System.Private.CoreLib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e
// MVID: 1E12AA92-1E57-4067-A322-8F82407CE9DD
// Assembly location: /opt/dotnet/shared/Microsoft.NETCore.App/2.2.3/System.Private.CoreLib.dll

using System.Diagnostics;
using System.Globalization;

namespace System.Reflection
{
  public abstract class FieldInfo : MemberInfo
  {
    public static FieldInfo GetFieldFromHandle(RuntimeFieldHandle handle)
    {
      if (handle.IsNullHandle())
        throw new ArgumentException(SR.Argument_InvalidHandle, nameof (handle));
      FieldInfo fieldInfo = RuntimeType.GetFieldInfo(handle.GetRuntimeFieldInfo());
      Type declaringType = fieldInfo.DeclaringType;
      if (declaringType != (Type) null && declaringType.IsGenericType)
        throw new ArgumentException(string.Format((IFormatProvider) CultureInfo.CurrentCulture, SR.Argument_FieldDeclaringTypeGeneric, (object) fieldInfo.Name, (object) declaringType.GetGenericTypeDefinition()));
      return fieldInfo;
    }

    public static FieldInfo GetFieldFromHandle(
      RuntimeFieldHandle handle,
      RuntimeTypeHandle declaringType)
    {
      if (handle.IsNullHandle())
        throw new ArgumentException(SR.Argument_InvalidHandle);
      return RuntimeType.GetFieldInfo(declaringType.GetRuntimeType(), handle.GetRuntimeFieldInfo());
    }

    public override MemberTypes MemberType
    {
      get
      {
        return MemberTypes.Field;
      }
    }

    public abstract FieldAttributes Attributes { get; }

    public abstract Type FieldType { get; }

    public bool IsInitOnly
    {
      get
      {
        return (uint) (this.Attributes & FieldAttributes.InitOnly) > 0U;
      }
    }

    public bool IsLiteral
    {
      get
      {
        return (uint) (this.Attributes & FieldAttributes.Literal) > 0U;
      }
    }

    public bool IsNotSerialized
    {
      get
      {
        return (uint) (this.Attributes & FieldAttributes.NotSerialized) > 0U;
      }
    }

    public bool IsPinvokeImpl
    {
      get
      {
        return (uint) (this.Attributes & FieldAttributes.PinvokeImpl) > 0U;
      }
    }

    public bool IsSpecialName
    {
      get
      {
        return (uint) (this.Attributes & FieldAttributes.SpecialName) > 0U;
      }
    }

    public bool IsStatic
    {
      get
      {
        return (uint) (this.Attributes & FieldAttributes.Static) > 0U;
      }
    }

    public bool IsAssembly
    {
      get
      {
        return (this.Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.Assembly;
      }
    }

    public bool IsFamily
    {
      get
      {
        return (this.Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.Family;
      }
    }

    public bool IsFamilyAndAssembly
    {
      get
      {
        return (this.Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.FamANDAssem;
      }
    }

    public bool IsFamilyOrAssembly
    {
      get
      {
        return (this.Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.FamORAssem;
      }
    }

    public bool IsPrivate
    {
      get
      {
        return (this.Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.Private;
      }
    }

    public bool IsPublic
    {
      get
      {
        return (this.Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.Public;
      }
    }

    public virtual bool IsSecurityCritical
    {
      get
      {
        return true;
      }
    }

    public virtual bool IsSecuritySafeCritical
    {
      get
      {
        return false;
      }
    }

    public virtual bool IsSecurityTransparent
    {
      get
      {
        return false;
      }
    }

    public abstract RuntimeFieldHandle FieldHandle { get; }

    public override bool Equals(object obj)
    {
      return base.Equals(obj);
    }

    public override int GetHashCode()
    {
      return base.GetHashCode();
    }

    public static bool operator ==(FieldInfo left, FieldInfo right)
    {
      if ((object) left == (object) right)
        return true;
      if ((object) left == null || (object) right == null)
        return false;
      return left.Equals((object) right);
    }

    public static bool operator !=(FieldInfo left, FieldInfo right)
    {
      return !(left == right);
    }

    public abstract object GetValue(object obj);

    [DebuggerStepThrough]
    [DebuggerHidden]
    public void SetValue(object obj, object value)
    {
      this.SetValue(obj, value, BindingFlags.Default, Type.DefaultBinder, (CultureInfo) null);
    }

    public abstract void SetValue(
      object obj,
      object value,
      BindingFlags invokeAttr,
      Binder binder,
      CultureInfo culture);

    [CLSCompliant(false)]
    public virtual void SetValueDirect(TypedReference obj, object value)
    {
      throw new NotSupportedException(SR.NotSupported_AbstractNonCLS);
    }

    [CLSCompliant(false)]
    public virtual object GetValueDirect(TypedReference obj)
    {
      throw new NotSupportedException(SR.NotSupported_AbstractNonCLS);
    }

    public virtual object GetRawConstantValue()
    {
      throw new NotSupportedException(SR.NotSupported_AbstractNonCLS);
    }

    public virtual Type[] GetOptionalCustomModifiers()
    {
      throw NotImplemented.ByDesign;
    }

    public virtual Type[] GetRequiredCustomModifiers()
    {
      throw NotImplemented.ByDesign;
    }
  }
}
