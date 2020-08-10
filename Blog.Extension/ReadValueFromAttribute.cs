using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blog
{
	[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
	sealed public class ReadValueFromAttribute : Attribute
	{
		public ReadValueFromAttribute(Type type, string path)
		{
			Type = type;
			Path = path;
		}
		public Type Type { get; private set; }
		public string Path { get; private set; }
	}

}
