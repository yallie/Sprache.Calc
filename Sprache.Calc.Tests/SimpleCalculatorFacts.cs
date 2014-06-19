using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Sprache.Calc.Tests
{
    public class SimpleCalculatorFacts
    {
		private SimpleCalculator calc = new SimpleCalculator();

		[Fact]
		public void DecimalWithoutLeadingDigitsIsSupported()
		{
			Assert.Equal(".01", calc.DecimalWithoutLeadingDigits.Parse(".01"));
			Assert.Throws<ParseException>(() => calc.DecimalWithoutLeadingDigits.Parse("."));
		}

		[Fact]
		public void DecimalWithLeadingDigitsIsSupported()
		{
			Assert.Equal("1", calc.DecimalWithLeadingDigits.Parse("1"));
			Assert.Equal("1.01", calc.DecimalWithLeadingDigits.Parse("1.01"));
			Assert.Throws<ParseException>(() => calc.DecimalWithLeadingDigits.Parse(".01"));
		}

		[Fact]
		public void DecimalCanBeWithOrWithoutLeadingDigits()
		{
			Assert.Equal("2", calc.Decimal.Parse("2"));
			Assert.Equal("3.14", calc.Decimal.Parse("3.14"));
			Assert.Equal(".271828", calc.Decimal.Parse(".271828"));
			Assert.Throws<ParseException>(() => calc.Decimal.End().Parse("0,1"));
		}

		private double GetValue(Expression x)
		{
			return (double)((ConstantExpression)x).Value;
		}

		[Fact]
		public void ConstantReturnsDoubleValue()
		{
			Assert.Equal(1d, GetValue(calc.Constant.Parse("1")));
			Assert.Equal(3.1415926d, GetValue(calc.Constant.Parse("3.1415926")));
		}

		[Fact]
		public void OperatorParsesAnyStringAsGivenExpressionType()
		{
			Assert.Equal(ExpressionType.Power, calc.Operator("^", ExpressionType.Power).Parse("^"));
			Assert.Equal(ExpressionType.AddAssign, calc.Operator("+=", ExpressionType.AddAssign).Parse("+="));
			Assert.Throws<ParseException>(() => calc.Operator("*", ExpressionType.Multiply).Parse("+"));
		}

		[Fact]
		public void ArithmeticOperationsAreSupported()
		{
			Assert.Equal(ExpressionType.AddChecked, calc.Add.Parse("+"));
			Assert.Equal(ExpressionType.SubtractChecked, calc.Subtract.Parse("-"));
			Assert.Equal(ExpressionType.MultiplyChecked, calc.Multiply.Parse("*"));
			Assert.Equal(ExpressionType.Divide, calc.Divide.Parse("/"));
			Assert.Equal(ExpressionType.Modulo, calc.Modulo.Parse("%"));
			Assert.Equal(ExpressionType.Power, calc.Power.Parse("^"));
			Assert.Throws<ParseException>(() => calc.Add.Parse("*"));
		}
    }
}
