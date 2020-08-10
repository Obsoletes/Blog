using Blog.Extension;
using System;

namespace Blog
{
	[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
	sealed public class InjectAttribute : Attribute
	{
		public InjectAttribute(Lifetime lifetime = Lifetime.Transient)
		{
			Lifetime = lifetime;
		}
		public Lifetime Lifetime { get; }
	}
}
