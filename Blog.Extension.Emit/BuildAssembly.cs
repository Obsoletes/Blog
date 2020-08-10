using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;
using System.Text.RegularExpressions;
using System.Diagnostics;

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
				var properties = source.GetProperties();
				var types = properties.Select(property =>
				{
					ReadValueFromAttribute? readValueFrom = property.GetCustomAttribute<ReadValueFromAttribute>(); 
					if (readValueFrom == null)
						return property.PropertyType;
					else
						return readValueFrom.Type;
				}).ToArray();
				if (properties.Length == 0)
				{
					typeBuilder.CreateType();
					continue;
				}
				var ctor = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, types);
				var ilCtor = ctor.GetILGenerator();
				CallBaseConstructor(source, ilCtor);

				var fields = GenerateFieldsFromProperties(typeBuilder, properties);
				GenerateConstructor(ilCtor, fields, properties.Select(p => p.GetCustomAttribute<ReadValueFromAttribute>()).ToArray());

				for (int i = 0; i < properties.Length; i++)
				{
					PropertyBuilder propertyBuilder = typeBuilder.DefineProperty(properties[i].Name, PropertyAttributes.None, properties[i].PropertyType, null);
					GenerateGetMethod(typeBuilder, propertyBuilder, fields[i]);
				}
				typeBuilder.CreateType();
			}
			try
			{
				generator.GenerateAssembly(assemblyBuilder, @"C:\Users\Devil\Desktop\out.dll");
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
		static private void GenerateConstructor(ILGenerator ilCtor, FieldBuilder[] fbs, ReadValueFromAttribute?[] attributes)
		{
			ReadValueFromAttribute? attribute;
			if (fbs.Length > 0)
			{
				ilCtor.Emit(OpCodes.Ldarg_0);
				ilCtor.Emit(OpCodes.Ldarg_1);
				attribute = attributes[0];
				if (attribute != null)
				{
					CalcPath(ilCtor, attribute);
				}
				ilCtor.Emit(OpCodes.Stfld, fbs[0]);
			}
			if (fbs.Length > 1)
			{
				ilCtor.Emit(OpCodes.Ldarg_0);
				ilCtor.Emit(OpCodes.Ldarg_2);
				attribute = attributes[1];
				if (attribute != null)
				{
					CalcPath(ilCtor, attribute);
				}
				ilCtor.Emit(OpCodes.Stfld, fbs[1]);
			}
			if (fbs.Length > 2)
			{
				ilCtor.Emit(OpCodes.Ldarg_0);
				ilCtor.Emit(OpCodes.Ldarg_3);
				attribute = attributes[2];
				if (attribute != null)
				{
					CalcPath(ilCtor, attribute);
				}
				ilCtor.Emit(OpCodes.Stfld, fbs[2]);
			}
			for (int i = 3; i < fbs.Length; i++)
			{
				ilCtor.Emit(OpCodes.Ldarg_0);
				ilCtor.Emit(OpCodes.Ldarg_S, i + 1);
				attribute = attributes[i];
				if (attribute != null)
				{
					CalcPath(ilCtor, attribute);
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
			MethodInfo? memberInfo;
			memberInfo = type.GetMethod(functionName, Type.EmptyTypes);
			if (memberInfo == null)
			{
				throw new InvalidOperationException($"The Function({type.FullName}.{functionName}) don't exist");
			}
			return memberInfo;
		}
	}
}
