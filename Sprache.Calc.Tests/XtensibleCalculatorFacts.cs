using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Sprache.Calc.Tests
{
	public class XtensibleCalculatorFacts : ScientificCalculatorFacts
	{
		private XtensibleCalculator Сalc
		{
			get { return (XtensibleCalculator)CreateCalculator(); }
		}

		protected override SimpleCalculator CreateCalculator()
		{
			return new XtensibleCalculator()
				.RegisterFunction("Min", (x, y, z) => Math.Min(x, Math.Min(y, z)))
				.RegisterFunction("Mul", (a, b, c, d, e) => a * b * c * d * e);
		}

		protected override object[] LambdaArguments
		{
			get { return new object[] { null }; }
		}

		[Fact]
		public void UserFunctionCanWork()
		{
			var calc = (XtensibleCalculator) CreateCalculator();
			calc.RegisterFunction("User", (aU,bU,cU) => calc.ParseExpression("Min(a,b,c)", new Dictionary<string,double>(){{"a",aU},{"b",bU},{"c",cU}}).Compile().Invoke());

			var func = calc.ParseExpression("User(a,b,c)", new Dictionary<string,double>(){{"a",1},{"b",2},{"c",3}}).Compile();
			Assert.Equal(1, func());
		}

		[Fact]
		public void StringUserFunctionCanWork()
		{
			var calc = (XtensibleCalculator) CreateCalculator();
			calc.RegisterFunction("UserB", "Min(a,b,c)", "a", "b", "c");
			var func = calc.ParseExpression("UserB(a,b,c)",
				new Dictionary<string, double>() {{"a", 1}, {"b", 2}, {"c", 3}}).Compile();

			Assert.Equal(1, func());
		}

		[Fact]
		public void GetParameterExpressionSupportsSystemMathConstantsAndCustomVariables()
		{
			Assert.Equal(System.Math.PI, Сalc.GetParameterExpression("PI").Execute());
			Assert.Equal(System.Math.E, Сalc.GetParameterExpression("E").Execute());
			Assert.Equal("Parameters.get_Item(\"Dummy\")", Сalc.GetParameterExpression("Dummy").ToString());
		}

		[Fact]
		public void ParameterIsAnIdentifierNotFollowedByParens()
		{
			Assert.Equal(System.Math.PI, Сalc.Parameter.Parse("PI").Execute());
			Assert.Equal(System.Math.E, Сalc.Parameter.Parse("E").Execute());
			Assert.Throws<ParseException>(() => Сalc.Parameter.Parse("E(1)").Execute());
		}

		[Fact]
		public void FactorSupportsParametersAndFunctions()
		{
			Assert.Equal(0, Сalc.Factor.Parse("Sin(0)").Execute());
			Assert.Equal(System.Math.PI, Сalc.Factor.Parse("PI").Execute());
			Assert.Equal("Parameters.get_Item(\"Hello\")", Сalc.Factor.Parse("Hello").ToString());
		}

		[Fact]
		public void ParseFunctionProducesExpressionWithParameters()
		{
			var compiledFunction = Сalc.ParseFunction("Dummy+Yummy").Compile();
			var parameters = new Dictionary<string, double> { { "Dummy", 2 }, { "Yummy", 3 } };
			Assert.Equal(5d, compiledFunction(parameters));
		}

		[Fact]
		public void ParseExpressionAcceptsDefaultParameterDictionary()
		{
			var parameters = new Dictionary<string, double> { { "Dummy", 123 }, { "Yummy", 456 } };
			var compiledFunction = Сalc.ParseExpression("Dummy+Yummy", parameters).Compile();
			Assert.Equal(579d, compiledFunction());
		}

		[Fact]
		public void ParseExpressionAcceptsDefaultParameterExpressions()
		{
			var compiledFunction = Сalc.ParseExpression("Dummy+Yummy", Dummy => 123, Yummy => 456).Compile();
			Assert.Equal(579d, compiledFunction());
		}

		[Fact]
		public void ParseExpressionAcceptsDefaultParameterAnonymousClass()
		{
			var compiledFunction = Сalc.ParseExpression("Dummy+Yummy", new { Dummy = 123, Yummy = 456 }).Compile();
			Assert.Equal(579d, compiledFunction());
		}

		[Fact]
		public void OverriddenParseExpressionMethodStillWorks()
		{
			Assert.Equal(0d, Сalc.ParseExpression("0").Compile()());
			Assert.Equal(System.Math.E, Сalc.ParseExpression("E").Compile()());
		}

		[Fact]
		public void MangleNameProducesMangledFunctionName()
		{
			Assert.Equal(":0", Сalc.MangleName(null, 0));
			Assert.Equal("MySine:2", Сalc.MangleName("MySine", 2));
		}

		[Fact]
		public void CallFunctionSupportsRegisteredFunctionsAsWellAsSystemMathFunctions()
		{
			Assert.Equal(2d, Сalc.CallFunction("Min", Expression.Constant(2d), Expression.Constant(3d)).Execute()); // System.Math.Min
			Assert.Equal(1d, Сalc.CallFunction("Min", Expression.Constant(2d), Expression.Constant(3d), Expression.Constant(1d)).Execute()); // custom Min
			Assert.Throws<ParseException>(() => Сalc.CallFunction("Mul", Expression.Constant(0d)));
		}
	}
}
