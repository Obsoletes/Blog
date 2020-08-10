using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace Blog.Extension
{
	public static class IServiceCollectionExtension
	{
		public static IServiceCollection AddService(this IServiceCollection services, string iServiceAssembly, string imServiceAssembly)
		{
			return AddService(services, Assembly.Load(iServiceAssembly), Assembly.Load(imServiceAssembly));
		}
		public static IServiceCollection AddService(this IServiceCollection services, Action<IAddServiceOptions> build)
		{
			AddServiceOptions options = new AddServiceOptions();
			build(options);
			return AddService(services, options);
		}
		static IServiceCollection AddService(this IServiceCollection services, AddServiceOptions options)
		{
			IEnumerable<Type> it = RequireNotNull(options.InterfaceAssembly, nameof(options.InterfaceAssembly)).GetTypes();
			if (options.InterfaceFilter != null)
				it = it.Where(options.InterfaceFilter);
			it = it.Where(type => type.GetCustomAttribute<InjectAttribute>() != null);
			if (options.Override.Count != 0)
				it = it.Where(type => options.Override.ContainsKey(type));
			Dictionary<Type, Type> interfaces = it.ToDictionary(_ => _);
			foreach (var impl in options.ImplementationAssemblys.SelectMany(assembly => assembly.GetTypes()).Where(type => type.IsClass && !type.IsAbstract))
			{
				foreach (var iFace in interfaces.Keys.Where(type => type.IsAssignableFrom(impl)).Where(type =>
				{
					var injectOn = type.GetCustomAttribute<InjectOnAttribute>();
					if (injectOn == null)
						return true;
					else
						return string.Compare(Environment.GetEnvironmentVariable(injectOn.Key), injectOn.Value, injectOn.IgnoreCase) == 0;
				}))
				{
					interfaces[iFace] = impl;
				}
			}
			foreach (var kv in interfaces)
			{
				InjectAttribute inject = kv.Key.GetCustomAttribute<InjectAttribute>()!;
				switch (inject.Lifetime)
				{
					case Lifetime.Singleton:
						services.AddSingleton(kv.Key, kv.Value);
						break;
					case Lifetime.Transient:
						services.AddTransient(kv.Key, kv.Value);
						break;
					case Lifetime.Scoped:
						services.AddScoped(kv.Key, kv.Value);
						break;
				}
			}
			foreach (var kv in options.Override)
			{
				switch (kv.Value.lifetime)
				{
					case Lifetime.Singleton:
						services.AddSingleton(kv.Key, kv.Value.impl);
						break;
					case Lifetime.Transient:
						services.AddTransient(kv.Key, kv.Value.impl);
						break;
					case Lifetime.Scoped:
						services.AddScoped(kv.Key, kv.Value.impl);
						break;
				}
			}
			return services;
		}
		public static IServiceCollection AddService(this IServiceCollection services, Assembly iServiceAssembly, Assembly imServiceAssembly)
		{
			AddServiceOptions options = new AddServiceOptions()
			{
				InterfaceAssembly = iServiceAssembly
			};
			options.AddImplementation(imServiceAssembly);
			return AddService(services, options);
		}
		private static T RequireNotNull<T>(T? obj, string name) where T : class
		{
			if (obj == null)
				throw new ArgumentNullException(name);
			return obj!;
		}
	}
}
