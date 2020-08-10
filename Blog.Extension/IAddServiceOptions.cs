using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Blog.Extension
{
	public interface IAddServiceOptions
	{
		Assembly InterfaceAssembly { set; }
		Func<Type, bool> InterfaceFilter { set; }
		IAddServiceOptions Add<TInterface, TImplementation>(Lifetime lifetime);
		IAddServiceOptions Add(Type tInterface, Type tImplementation, Lifetime lifetime);
		IAddServiceOptions AddImplementation(Assembly assembly);
	}
}
