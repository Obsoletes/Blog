using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blog.Service
{
	public interface IRandomGenerator
	{
		string GenerateString(int length, string? set);
		int GenerateInterger(int min, int max);
	}
}
