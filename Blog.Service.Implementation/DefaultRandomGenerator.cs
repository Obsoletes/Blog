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
		[ReadValueFrom(typeof(IOptions<Config.Config>), "Value")]
		public abstract Config.Config Config { get; }
		public Random Random { get; }
		public int GenerateInterger(int min, int max)
		{
			return Random.Next(min, max);
		}

		public string GenerateString(int length, string? set)
		{
			throw new NotImplementedException();
		}
	}
}
