using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Blog.Extension
{
	class AddServiceOptions : IAddServiceOptions
	{
		public AddServiceOptions()
		{
			Override = new Dictionary<Type, (Type impl, Lifetime lifetime)>();
			ImplementationAssemblys = new List<Assembly>();
		}
		public Assembly? InterfaceAssembly { set; get; }
		public Func<Type, bool>? InterfaceFilter { set; get; }
		internal Dictionary<Type, (Type impl, Lifetime lifetime)> Override { get; }
		internal List<Assembly> ImplementationAssemblys { get; }

		public IAddServiceOptions Add<TInterface, TImplementation>(Lifetime lifetime)
		{
			return Add(typeof(TInterface), typeof(TImplementation), lifetime);
		}

		public IAddServiceOptions Add(Type tInterface, Type tImplementation, Lifetime lifetime)
		{
			Override.Add(tInterface, (tImplementation, lifetime));
			return this;
		}

		public IAddServiceOptions AddImplementation(Assembly assembly)
		{
			ImplementationAssemblys.Add(assembly);
			return this;
		}
	}
}
