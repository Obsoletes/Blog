using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blog.Service.Implementation
{
	public abstract class DefaultRandomGenerator : IRandomGenerator
	{
		public DefaultRandomGenerator()
		{
			Random = new Random();
		}
		public Random Random { get; }
		public int GenerateInterger(int min, int max)
		{
			return Random.Next(min, max);
		}

		public string GenerateString(int length, string set)
		{
			StringBuilder @string = new StringBuilder(length);
			for (int i = 0; i < length; i++)
			{
				@string.Append(set[GenerateInterger(0, set.Length)]);
			}
			return @string.ToString();
		}
	}
}
