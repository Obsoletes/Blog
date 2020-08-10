using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection.Emit;

namespace Blog.Extension.Emit
{
	class ConstructorStack
	{
		public ConstructorStack(Type[] types)
		{
			length = types.Length;
			pos = 0;
		}
		public void Pop(int count, ILGenerator iL)
		{
			if (pos + count > length)
				throw new InvalidOperationException("Stack is not balanced");
			for (int i = 0; i < count; i++)
			{
				switch (pos + i)
				{
					case 0:
						iL.Emit(OpCodes.Ldarg_1);
						break;
					case 1:
						iL.Emit(OpCodes.Ldarg_2);
						break;
					case 2:
						iL.Emit(OpCodes.Ldarg_3);
						break;
					default:
						iL.Emit(OpCodes.Ldarg_S, pos + i + 1);
						break;
				}
			}
			pos += count;
		}
		private readonly int length;
		private int pos;
	}
}
