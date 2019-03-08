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
				.RegisterFunction("Multiply", (a, b, c) => a * b * c)
				.RegisterFunction("MultiplySquare", "Multiply(a, b, c) * Multiply(a, b, c)", "a","b","c");

			var func = calc.ParseExpression("Multiply(x, y, PI)", x => 2, y => 2 + 3).Compile();
			var product = calc.ParseExpression("MultiplySquare(a, b, c)",
				new Dictionary<string, double> {{"a", 1}, {"b", 2}, {"c", 3}}).Compile();
			Console.WriteLine($"Multiply: {func()}");
			Console.WriteLine($"Product: {product()}");
			Console.ReadKey();
		}
	}
}
