using System;
using System.Globalization;
using System.Reflection;

namespace Scrutor
{
    internal class DecoratedType : Type
    {
        private readonly Type _proxiedType;

        public DecoratedType(Type type) => _proxiedType = type;

        public override bool Equals(Type o) => object.ReferenceEquals(this, o);

        public override bool Equals(object o) => object.ReferenceEquals(this, o);

        public override int GetHashCode() => base.GetHashCode();

        public override string Name => "Decorated " + _proxiedType.Name;

        #region Implementing the abstract Type class, forward all methods to _proxiedType

        public override Guid GUID => _proxiedType.GUID;

        public override Module Module => _proxiedType.Module;

        public override Assembly Assembly => _proxiedType.Assembly;

        public override string FullName => _proxiedType.FullName;

        public override string Namespace => _proxiedType.Namespace;

        public override string AssemblyQualifiedName => _proxiedType.AssemblyQualifiedName;

        public override Type BaseType => _proxiedType.BaseType;

        public override Type UnderlyingSystemType => _proxiedType.UnderlyingSystemType;

        public override ConstructorInfo[] GetConstructors(BindingFlags bindingAttr) => _proxiedType.GetConstructors(bindingAttr);

        public override object[] GetCustomAttributes(bool inherit) => _proxiedType.GetCustomAttributes(inherit);

        public override object[] GetCustomAttributes(Type attributeType, bool inherit) => _proxiedType.GetCustomAttributes(attributeType, inherit);

        public override Type GetElementType() => _proxiedType.GetElementType();

        public override EventInfo GetEvent(string name, BindingFlags bindingAttr) => _proxiedType.GetEvent(name, bindingAttr);

        public override EventInfo[] GetEvents(BindingFlags bindingAttr) => _proxiedType.GetEvents(bindingAttr);

        public override FieldInfo GetField(string name, BindingFlags bindingAttr) => _proxiedType.GetField(name, bindingAttr);

        public override FieldInfo[] GetFields(BindingFlags bindingAttr) => _proxiedType.GetFields(bindingAttr);

        public override Type GetInterface(string name, bool ignoreCase) => _proxiedType.GetInterface(name, ignoreCase);

        public override Type[] GetInterfaces() => _proxiedType.GetInterfaces();

        public override MemberInfo[] GetMembers(BindingFlags bindingAttr) => _proxiedType.GetMembers(bindingAttr);

        public override MethodInfo[] GetMethods(BindingFlags bindingAttr) => _proxiedType.GetMethods(bindingAttr);

        public override Type GetNestedType(string name, BindingFlags bindingAttr) => _proxiedType.GetNestedType(name, bindingAttr);

        public override Type[] GetNestedTypes(BindingFlags bindingAttr) => _proxiedType.GetNestedTypes(bindingAttr);

        public override PropertyInfo[] GetProperties(BindingFlags bindingAttr) => _proxiedType.GetProperties(bindingAttr);

        public override object InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters)
            => _proxiedType.InvokeMember(name, invokeAttr, binder, target, args, modifiers, culture, namedParameters);

        public override bool IsDefined(Type attributeType, bool inherit) => _proxiedType.IsDefined(attributeType, inherit);

        protected override TypeAttributes GetAttributeFlagsImpl()
            => _proxiedType.Attributes;

        protected override ConstructorInfo GetConstructorImpl(BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
            => _proxiedType.GetConstructor(bindingAttr, binder, callConvention, types, modifiers);

        protected override MethodInfo GetMethodImpl(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
            => _proxiedType.GetMethod(name, bindingAttr, binder, callConvention, types, modifiers);

        protected override PropertyInfo GetPropertyImpl(string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers)
            => _proxiedType.GetProperty(name, bindingAttr, binder, returnType, types, modifiers);

        protected override bool HasElementTypeImpl()
            => _proxiedType.HasElementType;

        protected override bool IsArrayImpl()
            => _proxiedType.HasElementType;

        protected override bool IsByRefImpl()
            => _proxiedType.IsByRef;

        protected override bool IsCOMObjectImpl()
            => _proxiedType.IsCOMObject;

        protected override bool IsPointerImpl()
            => _proxiedType.IsPointer;

        protected override bool IsPrimitiveImpl()
            => _proxiedType.IsPrimitive;

        #endregion
    }
}
