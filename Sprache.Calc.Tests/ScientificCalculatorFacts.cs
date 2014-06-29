using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Sprache.Calc.Tests
{
	public class ScientificCalculatorFacts : SimpleCalculatorFacts
	{
		private ScientificCalculator Calc
		{
			get { return (ScientificCalculator)CreateCalculator(); }
		}

		protected override SimpleCalculator CreateCalculator()
		{
			return new ScientificCalculator();
		}

		[Fact]
		public void HexadecimalNumbersStartWith0x()
		{
			Assert.Equal("abc", Calc.Hexadecimal.Parse(" 0xabc "));
			Assert.Equal("123", Calc.Hexadecimal.Parse("0X123"));
			Assert.Equal("CAFEBABE", Calc.Hexadecimal.Parse("0xCAFEBABE"));
			Assert.Throws<ParseException>(() => Calc.Hexadecimal.Parse("0xy"));
		}

		[Fact]
		public void BinaryNumbersStartWith0b()
		{
			Assert.Equal("1011", Calc.Binary.Parse(" 0b1011"));
			Assert.Equal("0", Calc.Binary.Parse("0B0"));
			Assert.Equal("11111111", Calc.Binary.Parse("0b11111111"));
			Assert.Throws<ParseException>(() => Calc.Binary.Parse("0b"));
		}

		[Fact]
		public void ConvertHexadecimalSupportsHexadecimalNumbers()
		{
			Assert.Equal(0xABCul, Calc.ConvertHexadecimal("abc"));
			Assert.Equal(0x123ul, Calc.ConvertHexadecimal("123"));
			Assert.Equal(0xCAFEBABEul, Calc.ConvertHexadecimal("CAFEBABE"));
			Assert.Throws<ParseException>(() => Calc.ConvertHexadecimal("y"));
		}

		[Fact]
		public void ConvertBinarySupportsBinaryNumbers()
		{
			Assert.Equal(0ul, Calc.ConvertBinary("0"));
			Assert.Equal(1ul, Calc.ConvertBinary("1"));
			Assert.Equal(11ul, Calc.ConvertBinary("1011"));
			Assert.Equal(45ul, Calc.ConvertBinary("101101"));
			Assert.Throws<ParseException>(() => Calc.ConvertBinary("123"));
		}

		[Fact]
		public void ExponentIsAWholeNumberFollowingEChar()
		{
			Assert.Equal("e+1", Calc.Exponent.Parse("e1"));
			Assert.Equal("e-10", Calc.Exponent.Parse("E-10"));
			Assert.Equal("e+12345", Calc.Exponent.Parse("E12345"));
			Assert.Throws<ParseException>(() => Calc.Exponent.Parse("e"));
		}

		[Fact]
		public void DecimalNumbersMayHaveOptionalExponentPart()
		{
			Assert.Equal(".1e+1", Calc.Decimal.Parse(".1e1"));
			Assert.Equal("3.14e-10", Calc.Decimal.Parse("3.14E-10"));
			Assert.Equal("2.718e+20", Calc.Decimal.Parse("2.718e+20"));
			Assert.Equal("54321e+12345", Calc.Decimal.Parse("54321E12345"));
			Assert.Equal(".5", Calc.Decimal.Parse(".5"));
			Assert.Equal("123", Calc.Decimal.Parse("123"));
			Assert.Throws<ParseException>(() => Calc.Decimal.Parse("e"));
		}

		[Fact]
		public void ConstantCanBeHexadecimalOrDecimalWithOrWithoutExponent()
		{
			Assert.Equal((double)0xABC, Calc.Constant.Parse("0xabc").Execute());
			Assert.Equal((double)0x123ul, Calc.Constant.Parse(" 0X123").Execute());
			Assert.Equal((double)0xCAFEBABE, Calc.Constant.Parse("0b11001010111111101011101010111110").Execute());
			Assert.Equal(1d, Calc.Constant.Parse("0B1").Execute());
			Assert.Equal(123d, Calc.Constant.Parse("123 ").Execute());
			Assert.Equal(123e15d, Calc.Constant.Parse("123e15").Execute());
			Assert.Equal(2.718e-5d, Calc.Constant.Parse("2.718e-5 ").Execute());
			Assert.Equal(010110100d, Calc.Constant.Parse("010110100").Execute());
			Assert.Throws<ParseException>(() => Calc.Constant.End().Parse("0x"));
		}

		[Fact]
		public void IdentifierIsAWordStartingWithLetter()
		{
			Assert.Equal("abc", Calc.Identifier.Parse(" abc "));
			Assert.Equal("Test123", Calc.Identifier.Parse("Test123"));
			Assert.Throws<ParseException>(() => Calc.Identifier.Parse("1"));
		}

		[Fact]
		public void FunctionCallIsAnIdentifierFollowedByArgumentsInParentheses()
		{
			Assert.Equal(0d, Calc.FunctionCall.Parse("Sin(0x0)").Execute());
			Assert.Equal(1d, Calc.FunctionCall.Parse("Cos(0X0)").Execute());
			Assert.Equal(2.71828d, Calc.FunctionCall.Parse("Min(3.14159, 2.71828)").Execute());
			Assert.Throws<ParseException>(() => Calc.FunctionCall.Parse("(3)"));
		}

		[Fact]
		public void ScientificCalculatorSupportsArithmeticOperationsIntermixedWithFunctions()
		{
			Assert.Equal(1d, Calc.ParseExpression("Sin(3.141592653589793238462643383279/2)").Compile()());
			Assert.Equal(6, (int)Calc.ParseExpression("Log10(1234567)^(Log(2.718281828459045)-Cos(3.141592653589793238462643383279/2))").Compile()());
			Assert.Throws<ParseException>(() => Calc.ParseExpression("Log10()"));
		}
	}
}
