using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Sprache.Calc.Tests
{
	public class XtensibleCalculatorFacts
	{
		private XtensibleCalculator calc = new XtensibleCalculator();

		[Fact]
		public void GetParameterExpressionSupportsSystemMathConstantsAndCustomVariables()
		{
			Assert.Equal(System.Math.PI, calc.GetParameterExpression("PI").Execute());
			Assert.Equal(System.Math.E, calc.GetParameterExpression("E").Execute());
			Assert.Equal("Parameters.get_Item(\"Dummy\")", calc.GetParameterExpression("Dummy").ToString());
		}

		[Fact]
		public void ParameterIsAnIdentifierNotFollowedByParens()
		{
			Assert.Equal(System.Math.PI, calc.Parameter.Parse("PI").Execute());
			Assert.Equal(System.Math.E, calc.Parameter.Parse("E").Execute());
			Assert.Throws<ParseException>(() => calc.Parameter.Parse("E(1)").Execute());
		}

		[Fact]
		public void FactorSupportsParametersAndFunctions()
		{
			Assert.Equal(0, calc.Factor.Parse("Sin(0)").Execute());
			Assert.Equal(System.Math.PI, calc.Factor.Parse("PI").Execute());
			Assert.Equal("Parameters.get_Item(\"Hello\")", calc.Factor.Parse("Hello").ToString());
		}

		[Fact]
		public void ParseFunctionProducesExpressionWithParameters()
		{
			var compiledFunction = calc.ParseFunction("Dummy+Yummy").Compile();
			var parameters = new Dictionary<string, double> { { "Dummy", 2 }, { "Yummy", 3 } };
			Assert.Equal(5d, compiledFunction(parameters));
		}

		[Fact]
		public void OverriddenParseExpressionMethodStillWorks()
		{
			Assert.Equal("() => Invoke(Parameters => 0, null)", calc.ParseExpression("0").ToString());
			Assert.Equal(System.Math.E, calc.ParseExpression("E").Compile().DynamicInvoke());
		}
	}
}
