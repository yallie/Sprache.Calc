using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprache.Calc.Sample
{
	class Program
	{
		static void Main(string[] args)
		{
			var calc = new XtensibleCalculator()
				.RegisterFunction("Multiply", (a, b, c) => a * b * c);

			var func = calc.ParseExpression("Multiply(x, y, PI)", x => 2, y => 2 + 3).Compile();

			Console.WriteLine("Result: {0}", func());
			Console.ReadKey();
		}
	}
}
