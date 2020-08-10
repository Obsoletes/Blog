using Blog.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blog
{
	[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
	public sealed class SetValueThroughAttribute : SpecialActionAttribute
	{
		public SetValueThroughAttribute(string funcionName, params Type[] para)
		{
			Function = funcionName;
			Types = para;
		}
		public string Function { get; }
		public Type[] Types { get; }
	}
}
