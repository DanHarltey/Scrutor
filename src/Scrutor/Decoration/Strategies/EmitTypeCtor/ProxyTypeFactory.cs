using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace Scrutor.Decoration.Strategies.EmitTypeCtor
{
    internal class ProxyTypeFactory
    {
        private static int TypeCounter = 0;
        private static readonly AssemblyName AssemblyName = new("TypeDecoratorAssembly");

        public static Type CreateWrapperType(Type toProxy)
        {
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(AssemblyName, AssemblyBuilderAccess.Run);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule($"TypeDecorator_Module");

            var baseClass = typeof(InstanceWrapper);
            var baseClassConstructor = baseClass.GetConstructor(new[] { typeof(object) })!;

            var typeBuilder = moduleBuilder.DefineType($"TypeDecorator_{TypeCounter++}_{toProxy}", TypeAttributes.Sealed, baseClass);

            var constructors = toProxy.GetConstructors();
            foreach (var ctor in constructors)
            {
                AddConstructor(typeBuilder, ctor, baseClassConstructor);
            }

            return typeBuilder.CreateType()!;
        }

        private static void AddConstructor(TypeBuilder typeBuilder, ConstructorInfo ctor, ConstructorInfo baseClassConstructor)
        {
            var parameters = ctor.GetParameters();

            var parameterTypes = new Type[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                parameterTypes[i] = parameters[i].ParameterType;
            }

            ConstructorBuilder pointCtor = typeBuilder.DefineConstructor(
                ctor.Attributes,
                ctor.CallingConvention,
                parameterTypes);

            ILGenerator ctorIL = pointCtor.GetILGenerator();

            ctorIL.Emit(OpCodes.Ldarg_0);

            for (int i = 0; i < parameterTypes.Length; i++)
            {
                ctorIL.Emit(OpCodes.Ldarg, i + 1);

            }
            ctorIL.Emit(OpCodes.Newobj, ctor);
            ctorIL.Emit(OpCodes.Call, baseClassConstructor);

            ctorIL.Emit(OpCodes.Ret);
        }

        public static Func<IServiceProvider, object> ImplementationTypeToFactory(Type implementationType, Type decoratedWrapper)
        {
            var decoratedCtor = decoratedWrapper.GetConstructor(new[] { typeof(object) })!;

            var gEmptyArray = typeof(Array).GetMethod("Empty")!;
            var emptyArray = gEmptyArray.MakeGenericMethod(typeof(object));

            var createInstance = typeof(ActivatorUtilities).GetMethod("CreateInstance", new Type[] { typeof(IServiceProvider), typeof(Type), typeof(object[]) })!;

            var getTypeFromHandle = typeof(Type).GetMethod("GetTypeFromHandle")!;

            var dynamicMethod = new DynamicMethod(
                nameof(ImplementationTypeToFactory),
                MethodAttributes.Public | MethodAttributes.Static,
                CallingConventions.Standard,
                typeof(object),
                new[] { typeof(IServiceProvider) },
                typeof(ProxyTypeFactory),
                true);

            var il = dynamicMethod.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldtoken, implementationType);
            il.Emit(OpCodes.Call, getTypeFromHandle);
            il.Emit(OpCodes.Call, emptyArray);

            il.Emit(OpCodes.Call, createInstance);
            il.Emit(OpCodes.Newobj, decoratedCtor);
            il.Emit(OpCodes.Ret);


            return (Func<IServiceProvider, object>)dynamicMethod.CreateDelegate(typeof(Func<IServiceProvider, object>));
            ////            ctorIL.Emit(OpCodes.Ret);
            ////return (serviceProvider) =>
            ////{
            ////    var instanceToDecorate = ActivatorUtilities.CreateInstance(serviceProvider, implementationType);
            ////    return ctor.Invoke(new[] { instanceToDecorate });
            ////};
        }
        ////        var dynamicMethod = new DynamicMethod(
        ////    "f2Dynamic",
        ////    typeof(double),
        ////    new Type[] { typeof(Func<double, double>), typeof(double) });

        ////        var il = dynamicMethod.GetILGenerator();

        ////        il.Emit(OpCodes.Ldarg_0);
        ////il.Emit(OpCodes.Ldarg_1);
        ////il.Emit(OpCodes.Callvirt, typeof(Func<double, double>).GetMethod("Invoke"));
        ////il.Emit(OpCodes.Ret);

        ////var f2Dynamic =
        ////    (Func<Func<double, double>, double, double>)dynamicMethod.CreateDelegate(
        ////        typeof(Func<Func<double, double>, double, double>));

        ////        Console.WriteLine(f2(x => x* x, 10.0));        // prints 100
        ////        Console.WriteLine(f2Dynamic(x => x* x, 10.0)); // prints 100


        ////        var instanceToDecorate = ActivatorUtilities.CreateInstance(serviceProvider, implementationType);
        ////                return ctor.Invoke(new[] { instanceToDecorate
        ////    });
    }
}

////using System.Runtime.CompilerServices;
////using System;
////using System.Threading;
////using System.Reflection;
////using System.Reflection.Emit;
////////[assembly: InternalsVisibleTo("MyDynamicAssembly")]

////namespace Emit
////{
////    public class TestClass2
////    {
////        public TestClass2(int _) { }
////    }

////    public class TestClass
////    {
////        public int Foo { get; }
////        public TestClass()
////        {
////            Foo = -1;
////        }

////        public TestClass(int i)
////        //: base(new TestClass2(i))
////        {
////            Foo = i;
////        }


////        ////public object? Instance { get; }
////    }

////    public class BaseClass
////    {
////        public object Instance { get; }
////        public BaseClass(object instance)
////            => Instance = instance;
////    }

////    internal class HiddenClass
////    { }

////    public class TypeProxyBuilder
////    {
////        private static int Counter = 0;
////        private static readonly AssemblyName AssemblyName = new AssemblyName("TypeProxyBuilderAssembly");

////        public Type CreateProxyBuilder(Type toProxy)
////        {
////            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(AssemblyName, AssemblyBuilderAccess.Run);

////            var moduleBuilder = assemblyBuilder.DefineDynamicModule($"TypeDecorator_{Counter++}_{toProxy}");

////            //var genericBaseClass = typeof(BaseClass<>);
////            var baseClass = typeof(BaseClass);// genericBaseClass.MakeGenericType(toProxy);
////            var baseClassConstructor = baseClass.GetConstructor(new[] { typeof(object) });// baseClass.GetConstructor(new[] { toProxy })!;
////            var typeBuilder = moduleBuilder.DefineType($"TypeDecorator_{Counter++}_{toProxy}", TypeAttributes.Sealed, baseClass);

////            var constructors = toProxy.GetConstructors();
////            foreach (var ctor in constructors)
////            {
////                AddConstructor(typeBuilder, ctor, baseClassConstructor);
////            }
////            return typeBuilder.CreateType()!;
////        }

////        private static void AddConstructor(TypeBuilder typeBuilder, ConstructorInfo ctor, ConstructorInfo baseClassConstructor)
////        {
////            var parameters = ctor.GetParameters();

////            var parameterTypes = new Type[parameters.Length];
////            for (int i = 0; i < parameters.Length; i++)
////            {
////                parameterTypes[i] = parameters[i].ParameterType;
////            }

////            ConstructorBuilder pointCtor = typeBuilder.DefineConstructor(
////                ctor.Attributes,
////                ctor.CallingConvention,
////                parameterTypes);

////            ILGenerator ctorIL = pointCtor.GetILGenerator();

////            // NOTE: ldarg.0 holds the "this" reference - ldarg.1, ldarg.2, and ldarg.3
////            // hold the actual passed parameters. ldarg.0 is used by instance methods
////            // to hold a reference to the current calling object instance. Static methods
////            // do not use arg.0, since they are not instantiated and hence no reference
////            // is needed to distinguish them.

////            //ctorIL.Emit(OpCodes.Stfld, xField);
////            // Here, we wish to create an instance of System.Object by invoking its
////            // constructor, as specified above.

////            ////ConstructorInfor ctor = typeToCreate.GetConstructor(System.Type.EmptyTypes);

////            ////ILGenerator il = createHeadersMethod.GetILGenerator();
////            ctorIL.Emit(OpCodes.Ldarg_0);

////            for (int i = 0; i < parameterTypes.Length; i++)
////            {
////                ctorIL.Emit(OpCodes.Ldarg, i + 1);

////            }
////            ctorIL.Emit(OpCodes.Newobj, ctor);
////            ctorIL.Emit(OpCodes.Call, baseClassConstructor);
////            //ctorIL.Emit(OpCodes.Ldarg_0);
////            ////ctorIL.Emit(OpCodes.Pop);
////            ctorIL.Emit(OpCodes.Ret);
////        }
////    }

////    class TestCtorBuilder
////    {
////        public static Type DynamicPointTypeGen()
////        {
////            Type pointType = null;
////            Type[] ctorParams = new Type[]
////            {
////                typeof(int),
////                typeof(int),
////                typeof(int)
////            };

////            AppDomain myDomain = Thread.GetDomain();
////            AssemblyName myAsmName = new AssemblyName();
////            myAsmName.Name = "MyDynamicAssembly";

////            AssemblyBuilder myAsmBuilder = AssemblyBuilder.DefineDynamicAssembly(
////                           myAsmName,
////                           AssemblyBuilderAccess.Run);

////            ////var asdasdasda = myAsmBuilder.GetReferencedAssemblies();
////            ModuleBuilder pointModule = myAsmBuilder.DefineDynamicModule("PointModule");

////            // We want to create a type that inherits "MyBaseType" and calls its default constructor.
////            var baseType = typeof(object); //genericBaseClass.MakeGenericType(toProxy);typeof(BaseClass<HiddenClass>);
////            var baseConstructor = baseType.GetConstructor(new Type[0]);

////            TypeBuilder pointTypeBld = pointModule.DefineType("Point", TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.NotPublic, baseType);
////            ////pointTypeBld.AddInterfaceImplementation(typeof(IBaseClass));
////            FieldBuilder xField = pointTypeBld.DefineField("x", typeof(int),
////                                                               FieldAttributes.Public);
////            FieldBuilder yField = pointTypeBld.DefineField("y", typeof(int),
////                                                               FieldAttributes.Public);
////            FieldBuilder zField = pointTypeBld.DefineField("z", typeof(int),
////                                                               FieldAttributes.Public);

////            ////Type objType = Type.GetType("System.Object");
////            ////ConstructorInfo objCtor = objType.GetConstructor(new Type[0]);

////            ConstructorBuilder pointCtor = pointTypeBld.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, ctorParams);
////            ILGenerator ctorIL = pointCtor.GetILGenerator();

////            // NOTE: ldarg.0 holds the "this" reference - ldarg.1, ldarg.2, and ldarg.3
////            // hold the actual passed parameters. ldarg.0 is used by instance methods
////            // to hold a reference to the current calling object instance. Static methods
////            // do not use arg.0, since they are not instantiated and hence no reference
////            // is needed to distinguish them.

////            ctorIL.Emit(OpCodes.Ldarg_0);

////            // Here, we wish to create an instance of System.Object by invoking its
////            // constructor, as specified above.

////            ctorIL.Emit(OpCodes.Call, baseConstructor);

////            // Now, we'll load the current instance ref in arg 0, along
////            // with the value of parameter "x" stored in arg 1, into stfld.

////            ctorIL.Emit(OpCodes.Ldarg_0);
////            ctorIL.Emit(OpCodes.Ldarg_1);
////            ctorIL.Emit(OpCodes.Stfld, xField);

////            // Now, we store arg 2 "y" in the current instance with stfld.

////            ctorIL.Emit(OpCodes.Ldarg_0);
////            ctorIL.Emit(OpCodes.Ldarg_2);
////            ctorIL.Emit(OpCodes.Stfld, yField);

////            // Last of all, arg 3 "z" gets stored in the current instance.

////            ctorIL.Emit(OpCodes.Ldarg_0);
////            ctorIL.Emit(OpCodes.Ldarg_3);
////            ctorIL.Emit(OpCodes.Stfld, zField);

////            // Our work complete, we return.

////            ctorIL.Emit(OpCodes.Ret);

////            // Now, let's create three very simple methods so we can see our fields.

////            string[] mthdNames = new string[] { "GetX", "GetY", "GetZ" };

////            foreach (string mthdName in mthdNames)
////            {
////                MethodBuilder getFieldMthd = pointTypeBld.DefineMethod(
////                             mthdName,
////                             MethodAttributes.Public,
////                                             typeof(int),
////                                             null);
////                ILGenerator mthdIL = getFieldMthd.GetILGenerator();

////                mthdIL.Emit(OpCodes.Ldarg_0);
////                switch (mthdName)
////                {
////                    case "GetX":
////                        mthdIL.Emit(OpCodes.Ldfld, xField);
////                        break;
////                    case "GetY":
////                        mthdIL.Emit(OpCodes.Ldfld, yField);
////                        break;
////                    case "GetZ":
////                        mthdIL.Emit(OpCodes.Ldfld, zField);
////                        break;
////                }
////                mthdIL.Emit(OpCodes.Ret);
////            }
////            // Finally, we create the type.

////            pointType = pointTypeBld.CreateType();

////            // Let's save it, just for posterity.

////            ////myAsmBuilder.Save("Point.dll");

////            var asdasdasda2 = myAsmBuilder.GetReferencedAssemblies();

////            return pointType;
////        }

////        public static void Main()
////        {
////            Type myDynamicType = new TypeProxyBuilder().CreateProxyBuilder(typeof(TestClass));
////            DescribeType(myDynamicType);

////            var ctor = myDynamicType.GetConstructor(new Type[0]);
////            var lala = ctor.Invoke(new object[0]);

////            ctor = myDynamicType.GetConstructor(new Type[] { typeof(int) });
////            lala = ctor.Invoke(new object[] { 22 });

////            Console.WriteLine();

////            ////Type myDynamicType = null;
////            ////object aPoint = null;
////            ////Type[] aPtypes = new Type[] { typeof(int), typeof(int), typeof(int) };
////            ////object[] aPargs = new object[] { 4, 5, 6 };

////            ////// Call the  method to build our dynamic class.

////            ////myDynamicType = DynamicPointTypeGen();
////            ////ConstructorInfo myDTctor = DescribeType(myDynamicType, aPtypes);

////            // Now, we get to use our dynamically-created class by invoking the constructor.

////            ////aPoint = myDTctor.Invoke(aPargs);
////            ////Console.WriteLine("aPoint is type {0}.", aPoint.GetType());

////            ////// Finally, let's reflect on the instance of our new type - aPoint - and
////            ////// make sure everything proceeded according to plan.

////            ////Console.WriteLine("aPoint.x = {0}",
////            ////          myDynamicType.InvokeMember("GetX",
////            ////                         BindingFlags.InvokeMethod,
////            ////                     null,
////            ////                     aPoint,
////            ////                     new object[0]));
////            ////Console.WriteLine("aPoint.y = {0}",
////            ////          myDynamicType.InvokeMember("GetY",
////            ////                         BindingFlags.InvokeMethod,
////            ////                     null,
////            ////                     aPoint,
////            ////                     new object[0]));
////            ////Console.WriteLine("aPoint.z = {0}",
////            ////          myDynamicType.InvokeMember("GetZ",
////            ////                         BindingFlags.InvokeMethod,
////            ////                     null,
////            ////                     aPoint,
////            ////                     new object[0]));

////            // +++ OUTPUT +++
////            // Some information about my new Type 'Point':
////            // Assembly: 'MyDynamicAssembly, Version=0.0.0.0'
////            // Attributes: 'AutoLayout, AnsiClass, NotPublic, Public'
////            // Module: 'PointModule'
////            // Members:
////            // -- Field x;
////            // -- Field y;
////            // -- Field z;
////            // -- Method GetHashCode;
////            // -- Method Equals;
////            // -- Method ToString;
////            // -- Method GetType;
////            // -- Constructor .ctor;
////            // ---
////            // Constructor: Void .ctor(Int32, Int32, Int32);
////            // ---
////            // aPoint is type Point.
////            // aPoint.x = 4
////            // aPoint.y = 5
////            // aPoint.z = 6
////        }

////        private static void DescribeType(Type myDynamicType)
////        {
////            Console.WriteLine("Some information about my new Type '{0}':", myDynamicType.FullName);
////            Console.WriteLine("Assembly: '{0}'", myDynamicType.Assembly);
////            Console.WriteLine("Attributes: '{0}'", myDynamicType.Attributes);
////            Console.WriteLine("Module: '{0}'", myDynamicType.Module);
////            Console.WriteLine("Members: ");
////            foreach (MemberInfo member in myDynamicType.GetMembers(BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
////            {
////                Console.WriteLine("-- {0} {1};", member.MemberType, member.Name);
////            }

////            Console.WriteLine("---");
////            Console.WriteLine("---");
////        }
////    }
////}
