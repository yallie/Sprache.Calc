using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Sprache.Calc.Tests
{
	public class ScientificCalculatorFacts
	{
		private ScientificCalculator calc = new ScientificCalculator();

		[Fact]
		public void IdentifierIsAWordStartingWithLetter()
		{
			Assert.Equal("abc", calc.Identifier.Parse(" abc "));
			Assert.Equal("Test123", calc.Identifier.Parse("Test123"));
			Assert.Throws<ParseException>(() => calc.Identifier.Parse("1"));
		}

		[Fact]
		public void FunctionCallIsAnIdentifierFollowedByArgumentsInParentheses()
		{
			Assert.Equal(0d, calc.FunctionCall.Parse("Sin(0)").Execute());
			Assert.Equal(1d, calc.FunctionCall.Parse("Cos(0)").Execute());
			Assert.Equal(2.71828d, calc.FunctionCall.Parse("Min(3.14159, 2.71828)").Execute());
			Assert.Throws<ParseException>(() => calc.FunctionCall.Parse("(3)"));
		}

		[Fact]
		public void ScientificCalculatorSupportsArithmeticOperationsIntermixedWithFunctions()
		{
			Assert.Equal(1d, calc.ParseExpression("Sin(3.141592653589793238462643383279/2)").Compile()());
			Assert.Equal(6, (int)calc.ParseExpression("Log10(1234567)^(Log(2.718281828459045)-Cos(3.141592653589793238462643383279/2))").Compile()());
			Assert.Throws<ParseException>(() => calc.ParseExpression("Log10()"));
		}
	}
}
