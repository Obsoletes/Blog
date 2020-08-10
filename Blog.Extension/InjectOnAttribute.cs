using System;

namespace Blog
{
	[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
	sealed public class InjectOnAttribute : Attribute
	{
		public InjectOnAttribute(string key, string value,bool ignoreCase=false)
		{
			Key = key;
			Value = value;
			IgnoreCase = ignoreCase;
		}
		public string Key { get; }
		public string Value { get; }
		public bool IgnoreCase{ get; }
	}
}
