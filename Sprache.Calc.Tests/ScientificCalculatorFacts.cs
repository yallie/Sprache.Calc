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
		private void ForEachCalculator(Action<ScientificCalculator> fact)
		{
			foreach (var calc in new[] { new ScientificCalculator(), new XtensibleCalculator() })
			{
				fact(calc);
			}
		}

		[Fact]
		public void HexadecimalNumbersStartWith0x()
		{
			ForEachCalculator(calc =>
			{
				Assert.Equal("abc", calc.Hexadecimal.Parse(" 0xabc "));
				Assert.Equal("123", calc.Hexadecimal.Parse("0X123"));
				Assert.Equal("CAFEBABE", calc.Hexadecimal.Parse("0xCAFEBABE"));
				Assert.Throws<ParseException>(() => calc.Hexadecimal.Parse("0xy"));
			});
		}

		[Fact]
		public void BinaryNumbersStartWith0b()
		{
			ForEachCalculator(calc =>
			{
				Assert.Equal("1011", calc.Binary.Parse(" 0b1011"));
				Assert.Equal("0", calc.Binary.Parse("0B0"));
				Assert.Equal("11111111", calc.Binary.Parse("0b11111111"));
				Assert.Throws<ParseException>(() => calc.Binary.Parse("0b"));
			});
		}

		[Fact]
		public void ConvertHexadecimalSupportsHexadecimalNumbers()
		{
			ForEachCalculator(calc =>
			{
				Assert.Equal(0xABCul, calc.ConvertHexadecimal("abc"));
				Assert.Equal(0x123ul, calc.ConvertHexadecimal("123"));
				Assert.Equal(0xCAFEBABEul, calc.ConvertHexadecimal("CAFEBABE"));
				Assert.Throws<ParseException>(() => calc.ConvertHexadecimal("y"));
			});
		}

		[Fact]
		public void ConvertBinarySupportsBinaryNumbers()
		{
			ForEachCalculator(calc =>
			{
				Assert.Equal(0ul, calc.ConvertBinary("0"));
				Assert.Equal(1ul, calc.ConvertBinary("1"));
				Assert.Equal(11ul, calc.ConvertBinary("1011"));
				Assert.Equal(45ul, calc.ConvertBinary("101101"));
				Assert.Throws<ParseException>(() => calc.ConvertBinary("123"));
			});
		}

		[Fact]
		public void ExponentIsAWholeNumberFollowingEChar()
		{
			ForEachCalculator(calc =>
			{
				Assert.Equal("e+1", calc.Exponent.Parse("e1"));
				Assert.Equal("e-10", calc.Exponent.Parse("E-10"));
				Assert.Equal("e+12345", calc.Exponent.Parse("E12345"));
				Assert.Throws<ParseException>(() => calc.Exponent.Parse("e"));
			});
		}

		[Fact]
		public void DecimalNumbersMayHaveOptionalExponentPart()
		{
			ForEachCalculator(calc =>
			{
				Assert.Equal(".1e+1", calc.Decimal.Parse(".1e1"));
				Assert.Equal("3.14e-10", calc.Decimal.Parse("3.14E-10"));
				Assert.Equal("2.718e+20", calc.Decimal.Parse("2.718e+20"));
				Assert.Equal("54321e+12345", calc.Decimal.Parse("54321E12345"));
				Assert.Equal(".5", calc.Decimal.Parse(".5"));
				Assert.Equal("123", calc.Decimal.Parse("123"));
				Assert.Throws<ParseException>(() => calc.Decimal.Parse("e"));
			});
		}

		[Fact]
		public void ConstantCanBeHexadecimalOrDecimalWithOrWithoutExponent()
		{
			ForEachCalculator(calc =>
			{
				Assert.Equal((double)0xABC, calc.Constant.Parse("0xabc").Execute());
				Assert.Equal((double)0x123ul, calc.Constant.Parse(" 0X123").Execute());
				Assert.Equal((double)0xCAFEBABE, calc.Constant.Parse("0b11001010111111101011101010111110").Execute());
				Assert.Equal(1d, calc.Constant.Parse("0B1").Execute());
				Assert.Equal(123d, calc.Constant.Parse("123 ").Execute());
				Assert.Equal(123e15d, calc.Constant.Parse("123e15").Execute());
				Assert.Equal(2.718e-5d, calc.Constant.Parse("2.718e-5 ").Execute());
				Assert.Equal(010110100d, calc.Constant.Parse("010110100").Execute());
				Assert.Throws<ParseException>(() => calc.Constant.End().Parse("0x"));
			});
		}

		[Fact]
		public void IdentifierIsAWordStartingWithLetter()
		{
			ForEachCalculator(calc =>
			{
				Assert.Equal("abc", calc.Identifier.Parse(" abc "));
				Assert.Equal("Test123", calc.Identifier.Parse("Test123"));
				Assert.Throws<ParseException>(() => calc.Identifier.Parse("1"));
			});
		}

		[Fact]
		public void FunctionCallIsAnIdentifierFollowedByArgumentsInParentheses()
		{
			ForEachCalculator(calc =>
			{
				Assert.Equal(0d, calc.FunctionCall.Parse("Sin(0x0)").Execute());
				Assert.Equal(1d, calc.FunctionCall.Parse("Cos(0X0)").Execute());
				Assert.Equal(2.71828d, calc.FunctionCall.Parse("Min(3.14159, 2.71828)").Execute());
				Assert.Throws<ParseException>(() => calc.FunctionCall.Parse("(3)"));
			});
		}

		[Fact]
		public void ScientificCalculatorSupportsArithmeticOperationsIntermixedWithFunctions()
		{
			ForEachCalculator(calc =>
			{
				Assert.Equal(1d, calc.ParseExpression("Sin(3.141592653589793238462643383279/2)").Compile()());
				Assert.Equal(6, (int)calc.ParseExpression("Log10(1234567)^(Log(2.718281828459045)-Cos(3.141592653589793238462643383279/2))").Compile()());
				Assert.Throws<ParseException>(() => calc.ParseExpression("Log10()"));
			});
		}
	}
}
