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
	public class SimpleCalculatorFacts
	{
		private void ForEachCalculator(Action<SimpleCalculator> fact)
		{
			foreach (var calc in new[] { new SimpleCalculator(), new ScientificCalculator(), new XtensibleCalculator() })
			{
				fact(calc);
			}
		}

		[Fact]
		public void DecimalWithoutLeadingDigitsIsSupported()
		{
			ForEachCalculator(calc =>
			{
				Assert.Equal(".01", calc.DecimalWithoutLeadingDigits.Parse(".01"));
				Assert.Throws<ParseException>(() => calc.DecimalWithoutLeadingDigits.Parse("."));
			});
		}

		[Fact]
		public void DecimalWithLeadingDigitsIsSupported()
		{
			ForEachCalculator(calc =>
			{
				Assert.Equal("1", calc.DecimalWithLeadingDigits.Parse("1"));
				Assert.Equal("1.01", calc.DecimalWithLeadingDigits.Parse("1.01"));
				Assert.Throws<ParseException>(() => calc.DecimalWithLeadingDigits.Parse(".01"));
			});
		}

		[Fact]
		public void DecimalCanBeWithOrWithoutLeadingDigits()
		{
			ForEachCalculator(calc =>
			{
				Assert.Equal("2", calc.Decimal.Parse("2"));
				Assert.Equal("3.14", calc.Decimal.Parse("3.14"));
				Assert.Equal(".271828", calc.Decimal.Parse(".271828"));
				Assert.Throws<ParseException>(() => calc.Decimal.End().Parse("0,1"));
			});
		}

		[Fact]
		public void ConstantReturnsDoubleValue()
		{
			ForEachCalculator(calc =>
			{
				Assert.Equal(1d, calc.Constant.Parse("1").Execute());
				Assert.Equal(3.1415926d, calc.Constant.Parse("3.1415926").Execute());
				Assert.Equal(0.1415926d, calc.Constant.Parse("0.1415926").Execute());
				Assert.Throws<ParseException>(() => calc.Constant.Parse("+"));
			});
		}

		[Fact]
		public void FactorCanBeAConstant()
		{
			ForEachCalculator(calc =>
			{
				Assert.Equal(1d, calc.Factor.Parse("1").Execute());
				Assert.Equal(3.1415926d, calc.Factor.Parse("3.1415926").Execute());
				Assert.Equal(2.718d, calc.Factor.Parse("2.718").Execute());
				Assert.Throws<ParseException>(() => calc.Factor.Parse("-"));
			});
		}

		[Fact]
		public void NegativeFactorReturnsNegativeValues()
		{
			ForEachCalculator(calc =>
			{
				Assert.Equal(-1d, calc.NegativeFactor.Parse("-1").Execute());
				Assert.Equal(-3.1415926d, calc.NegativeFactor.Parse("-3.1415926").Execute());
				Assert.Equal(-2.718d, calc.NegativeFactor.Parse("-2.718").Execute());
				Assert.Throws<ParseException>(() => calc.NegativeFactor.Parse("-"));
			});
		}

		[Fact]
		public void OperandIsAPositiveOrNegativeFactor()
		{
			ForEachCalculator(calc =>
			{
				Assert.Equal(-2718281827d, calc.Operand.Parse("-2718281827").Execute());
				Assert.Equal(3.1415926d, calc.Operand.Parse("3.1415926").Execute());
				Assert.Equal(0d, calc.Operand.Parse("0").Execute());
				Assert.Throws<ParseException>(() => calc.Operand.Parse("-"));
			});
		}

		[Fact]
		public void OperatorParsesAnyStringAsGivenExpressionType()
		{
			ForEachCalculator(calc =>
			{
				Assert.Equal(ExpressionType.Power, calc.Operator("^", ExpressionType.Power).Parse("^"));
				Assert.Equal(ExpressionType.AddAssign, calc.Operator("+=", ExpressionType.AddAssign).Parse("+="));
				Assert.Throws<ParseException>(() => calc.Operator("*", ExpressionType.Multiply).Parse("+"));
			});
		}

		[Fact]
		public void ArithmeticOperationsAreSupported()
		{
			ForEachCalculator(calc =>
			{
				Assert.Equal(ExpressionType.AddChecked, calc.Add.Parse("+"));
				Assert.Equal(ExpressionType.SubtractChecked, calc.Subtract.Parse("-"));
				Assert.Equal(ExpressionType.MultiplyChecked, calc.Multiply.Parse("*"));
				Assert.Equal(ExpressionType.Divide, calc.Divide.Parse("/"));
				Assert.Equal(ExpressionType.Modulo, calc.Modulo.Parse("%"));
				Assert.Equal(ExpressionType.Power, calc.Power.Parse("^"));
				Assert.Throws<ParseException>(() => calc.Add.Parse("*"));
			});
		}

		[Fact]
		public void InnerTermCombinesOperandsWithPowerOperator()
		{
			ForEachCalculator(calc =>
			{
				Assert.Equal(4d, calc.InnerTerm.Parse("2^2").Execute());
				Assert.Equal(512d, calc.InnerTerm.Parse("2^3^2").Execute());
				Assert.Equal(27d, calc.InnerTerm.Parse(" 3 ^ 3").Execute());
				Assert.Throws<ParseException>(() => calc.InnerTerm.Parse("^"));
			});
		}

		[Fact]
		public void TermCombinesInnerTermsWithMulDivModOperators()
		{
			ForEachCalculator(calc =>
			{
				Assert.Equal(4d, calc.Term.Parse("2*2").Execute());
				Assert.Equal(2 / 3d, calc.Term.Parse("2/3^1").Execute());
				Assert.Equal(1d, calc.Term.Parse("10 % 3").Execute());
				Assert.Throws<ParseException>(() => calc.Term.Parse("%"));
			});
		}

		[Fact]
		public void ExprCombinesTermsWithAddSubOperators()
		{
			ForEachCalculator(calc =>
			{
				Assert.Equal(4d, calc.Expr.Parse("2+2").Execute());
				Assert.Equal(2d, calc.Expr.Parse("2*3-4*1").Execute());
				Assert.Throws<ParseException>(() => calc.Expr.Parse("+"));
			});
		}

		[Fact]
		public void ExprInParenthesesIsSelfExplanatory()
		{
			ForEachCalculator(calc =>
			{
				Assert.Equal(0d, calc.Expr.Parse("(2+-2)").Execute());
				Assert.Equal(2d, calc.Expr.Parse("2*(3-4)*-1").Execute());
				Assert.Throws<ParseException>(() => calc.Expr.Parse("()"));
			});
		}

		[Fact]
		public void LambdaProducesExpressionThatCanBeCompiledAndExecuted()
		{
			foreach (var calc in new[] { new SimpleCalculator(), new ScientificCalculator() })
			{
				Assert.Equal(3.14159d, Math.Round((double)calc.Lambda.Parse("4*(1/1-1/3+1/5-1/7+1/9-1/11+1/13-1/15+1/17-1/19+10/401)").Compile().DynamicInvoke(), 5));
				Assert.Equal(2.97215d, Math.Round((double)calc.Lambda.Parse("2*(2/1*2/3*4/3*4/5*6/5*6/7*8/7*8/9)").Compile().DynamicInvoke(), 5));
				Assert.Equal(Math.E.ToString(), calc.Lambda.Parse(string.Format(CultureInfo.InvariantCulture, "{0}", Math.E)).Compile().DynamicInvoke().ToString());
			}
		}

		[Fact]
		public void ParseExpressionReturnsResult()
		{
			ForEachCalculator(calc =>
			{
				Assert.Equal(3.14159d, Math.Round(calc.ParseExpression("4*(1/1-1/3+1/5-1/7+1/9-1/11+1/13-1/15+1/17-1/19+10/401)").Compile()(), 5));
				Assert.Equal(2.97215d, Math.Round(calc.ParseExpression("2*(2/1*2/3*4/3*4/5*6/5*6/7*8/7*8/9)").Compile()(), 5));
				Assert.Equal(Math.E.ToString(), calc.ParseExpression(string.Format(CultureInfo.InvariantCulture, "{0}", Math.E)).Compile()().ToString());
			});
		}
	}
}
