using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Sprache.Calc.Tests
{
	public class XtensibleCalculatorFacts
	{
		private XtensibleCalculator calc = new XtensibleCalculator()
			.RegisterFunction("Min", (x, y, z) => Math.Min(x, Math.Min(y, z)))
			.RegisterFunction("Mul", (a, b, c, d, e) => a * b * c * d * e);

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
		public void ParseExpressionAcceptsDefaultParameterDictionary()
		{
			var parameters = new Dictionary<string, double> { { "Dummy", 123 }, { "Yummy", 456 } };
			var compiledFunction = calc.ParseExpression("Dummy+Yummy", parameters).Compile();
			Assert.Equal(579d, compiledFunction());
		}

		[Fact]
		public void ParseExpressionAcceptsDefaultParameterExpressions()
		{
			var compiledFunction = calc.ParseExpression("Dummy+Yummy", Dummy => 123, Yummy => 456).Compile();
			Assert.Equal(579d, compiledFunction());
		}

		[Fact]
		public void ParseExpressionAcceptsDefaultParameterAnonymousClass()
		{
			var compiledFunction = calc.ParseExpression("Dummy+Yummy", new { Dummy = 123, Yummy = 456 }).Compile();
			Assert.Equal(579d, compiledFunction());
		}

		[Fact]
		public void OverriddenParseExpressionMethodStillWorks()
		{
			Assert.Equal(0d, calc.ParseExpression("0").Compile()());
			Assert.Equal(System.Math.E, calc.ParseExpression("E").Compile()());
		}

		[Fact]
		public void MangleNameProducesMangledFunctionName()
		{
			Assert.Equal(":0", calc.MangleName(null, 0));
			Assert.Equal("MySine:2", calc.MangleName("MySine", 2));
		}

		[Fact]
		public void CallFunctionSupportsRegisteredFunctionsAsWellAsSystemMathFunctions()
		{
			Assert.Equal(2d, calc.CallFunction("Min", Expression.Constant(2d), Expression.Constant(3d)).Execute()); // System.Math.Min
			Assert.Equal(1d, calc.CallFunction("Min", Expression.Constant(2d), Expression.Constant(3d), Expression.Constant(1d)).Execute()); // custom Min
			Assert.Throws<ParseException>(() => calc.CallFunction("Mul", Expression.Constant(0d)));
		}
	}
}
