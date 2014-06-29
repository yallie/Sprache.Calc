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
		private SimpleCalculator Calc
		{
			get { return CreateCalculator(); }
		}

		protected virtual SimpleCalculator CreateCalculator()
		{
			return new SimpleCalculator();
		}

		protected virtual object[] LambdaArguments
		{
			get { return new object[0]; }
		}

		[Fact]
		public void DecimalWithoutLeadingDigitsIsSupported()
		{
			Assert.Equal(".01", Calc.DecimalWithoutLeadingDigits.Parse(".01"));
			Assert.Throws<ParseException>(() => Calc.DecimalWithoutLeadingDigits.Parse("."));
		}

		[Fact]
		public void DecimalWithLeadingDigitsIsSupported()
		{
			Assert.Equal("1", Calc.DecimalWithLeadingDigits.Parse("1"));
			Assert.Equal("1.01", Calc.DecimalWithLeadingDigits.Parse("1.01"));
			Assert.Throws<ParseException>(() => Calc.DecimalWithLeadingDigits.Parse(".01"));
		}

		[Fact]
		public void DecimalCanBeWithOrWithoutLeadingDigits()
		{
			Assert.Equal("2", Calc.Decimal.Parse("2"));
			Assert.Equal("3.14", Calc.Decimal.Parse("3.14"));
			Assert.Equal(".271828", Calc.Decimal.Parse(".271828"));
			Assert.Throws<ParseException>(() => Calc.Decimal.End().Parse("0,1"));
		}

		[Fact]
		public void ConstantReturnsDoubleValue()
		{
			Assert.Equal(1d, Calc.Constant.Parse("1").Execute());
			Assert.Equal(3.1415926d, Calc.Constant.Parse("3.1415926").Execute());
			Assert.Equal(0.1415926d, Calc.Constant.Parse("0.1415926").Execute());
			Assert.Throws<ParseException>(() => Calc.Constant.Parse("+"));
		}

		[Fact]
		public void FactorCanBeAConstant()
		{
			Assert.Equal(1d, Calc.Factor.Parse("1").Execute());
			Assert.Equal(3.1415926d, Calc.Factor.Parse("3.1415926").Execute());
			Assert.Equal(2.718d, Calc.Factor.Parse("2.718").Execute());
			Assert.Throws<ParseException>(() => Calc.Factor.Parse("-"));
		}

		[Fact]
		public void NegativeFactorReturnsNegativeValues()
		{
			Assert.Equal(-1d, Calc.NegativeFactor.Parse("-1").Execute());
			Assert.Equal(-3.1415926d, Calc.NegativeFactor.Parse("-3.1415926").Execute());
			Assert.Equal(-2.718d, Calc.NegativeFactor.Parse("-2.718").Execute());
			Assert.Throws<ParseException>(() => Calc.NegativeFactor.Parse("-"));
		}

		[Fact]
		public void OperandIsAPositiveOrNegativeFactor()
		{
			Assert.Equal(-2718281827d, Calc.Operand.Parse("-2718281827").Execute());
			Assert.Equal(3.1415926d, Calc.Operand.Parse("3.1415926").Execute());
			Assert.Equal(0d, Calc.Operand.Parse("0").Execute());
			Assert.Throws<ParseException>(() => Calc.Operand.Parse("-"));
		}

		[Fact]
		public void OperatorParsesAnyStringAsGivenExpressionType()
		{
			Assert.Equal(ExpressionType.Power, Calc.Operator("^", ExpressionType.Power).Parse("^"));
			Assert.Equal(ExpressionType.AddAssign, Calc.Operator("+=", ExpressionType.AddAssign).Parse("+="));
			Assert.Throws<ParseException>(() => Calc.Operator("*", ExpressionType.Multiply).Parse("+"));
		}

		[Fact]
		public void ArithmeticOperationsAreSupported()
		{
			Assert.Equal(ExpressionType.AddChecked, Calc.Add.Parse("+"));
			Assert.Equal(ExpressionType.SubtractChecked, Calc.Subtract.Parse("-"));
			Assert.Equal(ExpressionType.MultiplyChecked, Calc.Multiply.Parse("*"));
			Assert.Equal(ExpressionType.Divide, Calc.Divide.Parse("/"));
			Assert.Equal(ExpressionType.Modulo, Calc.Modulo.Parse("%"));
			Assert.Equal(ExpressionType.Power, Calc.Power.Parse("^"));
			Assert.Throws<ParseException>(() => Calc.Add.Parse("*"));
		}

		[Fact]
		public void InnerTermCombinesOperandsWithPowerOperator()
		{
			Assert.Equal(4d, Calc.InnerTerm.Parse("2^2").Execute());
			Assert.Equal(512d, Calc.InnerTerm.Parse("2^3^2").Execute());
			Assert.Equal(27d, Calc.InnerTerm.Parse(" 3 ^ 3").Execute());
			Assert.Throws<ParseException>(() => Calc.InnerTerm.Parse("^"));
		}

		[Fact]
		public void TermCombinesInnerTermsWithMulDivModOperators()
		{
			Assert.Equal(4d, Calc.Term.Parse("2*2").Execute());
			Assert.Equal(2 / 3d, Calc.Term.Parse("2/3^1").Execute());
			Assert.Equal(1d, Calc.Term.Parse("10 % 3").Execute());
			Assert.Throws<ParseException>(() => Calc.Term.Parse("%"));
		}

		[Fact]
		public void ExprCombinesTermsWithAddSubOperators()
		{
			Assert.Equal(4d, Calc.Expr.Parse("2+2").Execute());
			Assert.Equal(2d, Calc.Expr.Parse("2*3-4*1").Execute());
			Assert.Throws<ParseException>(() => Calc.Expr.Parse("+"));
		}

		[Fact]
		public void ExprInParenthesesIsSelfExplanatory()
		{
			Assert.Equal(0d, Calc.Expr.Parse("(2+-2)").Execute());
			Assert.Equal(2d, Calc.Expr.Parse("2*(3-4)*-1").Execute());
			Assert.Throws<ParseException>(() => Calc.Expr.Parse("()"));
		}

		[Fact]
		public virtual void LambdaProducesExpressionThatCanBeCompiledAndExecuted()
		{
			//foreach (var calc in new[] { new SimpleCalculator(), new ScientificCalculator() })
			Assert.Equal(3.14159d, Math.Round((double)Calc.Lambda.Parse("4*(1/1-1/3+1/5-1/7+1/9-1/11+1/13-1/15+1/17-1/19+10/401)").Compile().DynamicInvoke(LambdaArguments), 5));
			Assert.Equal(2.97215d, Math.Round((double)Calc.Lambda.Parse("2*(2/1*2/3*4/3*4/5*6/5*6/7*8/7*8/9)").Compile().DynamicInvoke(LambdaArguments), 5));
			Assert.Equal(Math.E.ToString(), Calc.Lambda.Parse(string.Format(CultureInfo.InvariantCulture, "{0}", Math.E)).Compile().DynamicInvoke(LambdaArguments).ToString());
		}

		[Fact]
		public void ParseExpressionReturnsResult()
		{
			Assert.Equal(3.14159d, Math.Round(Calc.ParseExpression("4*(1/1-1/3+1/5-1/7+1/9-1/11+1/13-1/15+1/17-1/19+10/401)").Compile()(), 5));
			Assert.Equal(2.97215d, Math.Round(Calc.ParseExpression("2*(2/1*2/3*4/3*4/5*6/5*6/7*8/7*8/9)").Compile()(), 5));
			Assert.Equal(Math.E.ToString(), Calc.ParseExpression(string.Format(CultureInfo.InvariantCulture, "{0}", Math.E)).Compile()().ToString());
		}
	}
}
