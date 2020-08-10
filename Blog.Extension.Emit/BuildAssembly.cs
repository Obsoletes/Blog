using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Collections.Generic;

namespace Blog.Extension.Emit
{
	public class BuildAssembly
	{
		private static readonly Regex regex = new Regex(@"^((?<Property>\w+?)|((?<Function>\S+?)\(\)))(\[(?<Index>\d+)\])*$");
		public static Assembly Build(Assembly assembly)
		{
			string namespaceName = $"{assembly.GetName().Name}.{UUID.GenerateRandomString()}";
			AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(namespaceName), AssemblyBuilderAccess.Run);
			ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule($"Module.{UUID.GenerateRandomString()}.dll");
			Lokad.ILPack.AssemblyGenerator generator = new Lokad.ILPack.AssemblyGenerator();
			foreach (Type source in assembly.GetTypes().Where(type => !type.IsSealed))
			{
				TypeBuilder typeBuilder = moduleBuilder.DefineType($"{namespaceName}.{source.Name}", TypeAttributes.BeforeFieldInit, source);
				var properties = source.GetProperties().Where(pro => pro.GetGetMethod()?.IsAbstract ?? false).ToArray();
				var constructorParameterTypes = properties.SelectMany(property =>
				{
					SpecialActionAttribute? atttribute = property.GetCustomAttribute<SpecialActionAttribute>();
					if (atttribute == null)
						return Enumerable.Repeat(property.PropertyType, 1);
					else if (atttribute is ReadValueFromAttribute readValueFrom)
						return Enumerable.Repeat(readValueFrom.Type, 1);
					else if (atttribute is SetValueThroughAttribute setValueThrough)
						return setValueThrough.Types;
					return Enumerable.Empty<Type>();
				}).ToArray();
				if (properties.Length == 0)
				{
					typeBuilder.CreateType();
					continue;
				}
				ConstructorStack stack = new ConstructorStack(constructorParameterTypes);
				var ctor = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, constructorParameterTypes);
				var ilCtor = ctor.GetILGenerator();
				CallBaseConstructor(source, ilCtor);

				var fields = GenerateFieldsFromProperties(typeBuilder, properties);
				GenerateConstructor(ilCtor, stack, fields, properties.Select(p => p.GetCustomAttribute<SpecialActionAttribute>()).ToArray(), source);

				for (int i = 0; i < properties.Length; i++)
				{
					PropertyBuilder propertyBuilder = typeBuilder.DefineProperty(properties[i].Name, PropertyAttributes.None, properties[i].PropertyType, null);
					GenerateGetMethod(typeBuilder, propertyBuilder, fields[i]);
				}
				typeBuilder.CreateType();
			}
			try
			{
				generator.GenerateAssembly(assemblyBuilder, @"D:\out.dll");
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				Console.WriteLine(ex.InnerException?.ToString());
				throw;
			}
			return assemblyBuilder;
		}
		static private void CallBaseConstructor(Type baseType, ILGenerator ilCtor)
		{
			ConstructorInfo? constructorInfo = baseType.GetConstructor(Type.EmptyTypes);
			if (constructorInfo == null)
				return;
			ilCtor.Emit(OpCodes.Ldarg_0);
			ilCtor.Emit(OpCodes.Call, constructorInfo);
			return;
		}
		static private FieldBuilder[] GenerateFieldsFromProperties(TypeBuilder tb, PropertyInfo[] pbs)
		{
			return pbs.Select(info =>
				tb.DefineField($"{info.Name}.{UUID.GenerateRandomString()}",
				 info.PropertyType, FieldAttributes.Private)).ToArray();
		}
		static private void GenerateConstructor(ILGenerator ilCtor, ConstructorStack stack, FieldBuilder[] fbs, SpecialActionAttribute?[] attributes, Type source)
		{
			SpecialActionAttribute? attribute;
			for (int i = 0; i < fbs.Length; i++)
			{
				ilCtor.Emit(OpCodes.Ldarg_0);
				attribute = attributes[i];
				if (attribute == null)
				{
					stack.Pop(1, ilCtor);
				}
				else if (attribute is ReadValueFromAttribute readValueFrom)
				{
					stack.Pop(1, ilCtor);
					CalcPath(ilCtor, readValueFrom);
				}
				else if (attribute is SetValueThroughAttribute setValueThrough)
				{
					CallSetFunction(ilCtor, source, setValueThrough, stack);
				}
				ilCtor.Emit(OpCodes.Stfld, fbs[i]);
			}
			ilCtor.Emit(OpCodes.Ret);
		}
		static private void GenerateGetMethod(TypeBuilder tb, PropertyBuilder pb, FieldInfo fi)
		{
			MethodBuilder propertyGetter = tb.DefineMethod("get_" + pb.Name,
					MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Virtual,
					pb.PropertyType, Type.EmptyTypes);
			ILGenerator ilGetMethod = propertyGetter.GetILGenerator();
			ilGetMethod.Emit(OpCodes.Ldarg_0);
			ilGetMethod.Emit(OpCodes.Ldfld, fi);
			ilGetMethod.Emit(OpCodes.Ret);
			pb.SetGetMethod(propertyGetter);
		}

		static private void CalcPath(ILGenerator il, ReadValueFromAttribute attribute)
		{
			string[] paths = attribute.Path.Split('.');
			Type lastType = attribute.Type;
			foreach (var path in paths)
			{
				Match match = regex.Match(path);
				if (!match.Success)
					throw new InvalidOperationException($"Bad Path {attribute.Path}");
				var propertyGroup = match.Groups["Property"];
				if (propertyGroup.Success)
				{
					MethodInfo info = GetGetMethod(lastType, propertyGroup.Value);
					il.Emit(OpCodes.Callvirt, info);
					lastType = info.ReturnType;
				}
				var functionGroup = match.Groups["Function"];
				if (functionGroup.Success)
				{
					MethodInfo info = GetMethod(lastType, functionGroup.Value);
					il.Emit(OpCodes.Callvirt, info);
					lastType = info.ReturnType;
				}
				var indexGroup = match.Groups["Index"];
				if (indexGroup.Success)
				{
					foreach (var index in indexGroup.Captures.Select(c =>
					{
						if (int.TryParse(c.Value, out int result))
							return (true, result);
						else
							return (false, 0);
					}).Where(c => c.Item1).Select(c => c.Item2))
					{
						lastType = GetIndexReturnType(lastType);
						il.Emit(OpCodes.Ldc_I4, index);
						il.Emit(OpCodes.Ldelem_Ref);
					}
				}
			}
		}
		static private MethodInfo GetGetMethod(Type type, string propertyName)
		{
			MethodInfo? memberInfo;
			PropertyInfo? property = type.GetProperty(propertyName);
			if (property == null)
			{
				throw new InvalidOperationException($"The Type({type.FullName}) don't has property {propertyName}");
			}
			memberInfo = property.GetGetMethod();
			if (memberInfo == null)
			{
				throw new InvalidOperationException($"The Property({type.FullName}.{propertyName}) can't get method");
			}
			return memberInfo;
		}
		static private MethodInfo GetMethod(Type type, string functionName)
		{
			MethodInfo? memberInfo = type.GetMethod(functionName, Type.EmptyTypes);
			if (memberInfo == null)
			{
				throw new InvalidOperationException($"The Function({type.FullName}.{functionName}) don't exist");
			}
			return memberInfo;
		}
		static private Type GetIndexReturnType(Type type)
		{
			try
			{
				if (type.IsArray)
					return type.GetElementType()!;
				var arr = type.GetProperties().Where(pro => pro.GetIndexParameters().Length > 0).ToArray();
				return type.GetProperties().Where(pro => pro.GetIndexParameters().Length > 0).First().PropertyType;
			}
			catch (NullReferenceException)
			{
				throw new InvalidOperationException($"The Type({type.FullName}) don't has Indexers");
			}
		}
		static private void CallSetFunction(ILGenerator il, Type source, SetValueThroughAttribute setValueThrough, ConstructorStack stack)
		{
			var functionName = setValueThrough.Function;
			var types = setValueThrough.Types;
			MethodInfo? info = source.GetMethod(functionName, types);
			if (info == null)
				throw new InvalidOperationException($"The Function({source.FullName}.{functionName}({string.Join(", ", types.Select(t => t.FullName))})) don't exist");
			if (!info.IsStatic)
				il.Emit(OpCodes.Ldarg_0);
			stack.Pop(types.Length, il);
			if (info.IsStatic)
				il.Emit(OpCodes.Call, info);
			else
				il.Emit(OpCodes.Callvirt, info);

		}
	}
}
