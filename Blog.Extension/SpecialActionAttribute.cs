using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blog.Extension
{
	[System.AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
	public abstract class SpecialActionAttribute : Attribute
	{
		public SpecialActionAttribute()
		{
		}
	}
}
