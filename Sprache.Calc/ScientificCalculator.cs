using System;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Sprache.Calc
{
	/// <summary>
	/// Scientific calculator grammar.
	/// Supports binary and hexadecimal numbers, exponential notation and functions defined in System.Math.
	/// </summary>
	public class ScientificCalculator : SimpleCalculator
	{
		protected internal virtual Parser<string> Binary =>
			Parse.IgnoreCase("0b").Then(x =>
				Parse.Chars("01").AtLeastOnce().Text()).Token();

		protected internal virtual Parser<string> Hexadecimal =>
			Parse.IgnoreCase("0x").Then(x =>
				Parse.Chars("0123456789ABCDEFabcdef").AtLeastOnce().Text()).Token();

		protected internal virtual ulong ConvertBinary(string bin)
		{
			return bin.Aggregate(0ul, (result, c) =>
			{
				if (c < '0' || c > '1')
				{
					throw new ParseException(bin + " cannot be parsed as binary number");
				}

				return result * 2 + c - '0';
			});
		}

		protected internal virtual ulong ConvertHexadecimal(string hex)
		{
			var result = 0ul;
			if (ulong.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out result))
			{
				return result;
			}

			throw new ParseException(hex + " cannot be parsed as hexadecimal number");
		}

		protected internal virtual Parser<string> Exponent =>
			Parse.Chars("Ee").Then(e => Parse.Number.Select(n => "e+" + n).XOr(
				Parse.Chars("+-").Then(s => Parse.Number.Select(n => "e" + s + n))));

		protected internal override Parser<string> Decimal =>
			from d in base.Decimal
			from e in Exponent.Optional()
			select d + e.GetOrElse(string.Empty);

		protected internal override Parser<Expression> Constant =>
			Hexadecimal.Select(x => Expression.Constant((double)ConvertHexadecimal(x)))
				.Or(Binary.Select(b => Expression.Constant((double)ConvertBinary(b))))
				.Or(base.Constant);

		protected internal virtual Parser<string> Identifier =>
			Parse.Letter.AtLeastOnce().Text().Then(h =>
				Parse.LetterOrDigit.Many().Text().Select(t => h + t)).Token();

		protected internal virtual Parser<Expression> FunctionCall =>
			from name in Identifier
			from lparen in Parse.Char('(')
			from expr in Expr.DelimitedBy(Parse.Char(',').Token())
			from rparen in Parse.Char(')')
			select CallFunction(name, expr.ToArray());

		protected internal virtual Expression CallFunction(string name, params Expression[] parameters)
		{
			var methodInfo = typeof(Math).GetMethod(name, parameters.Select(e => e.Type).ToArray());
			if (methodInfo == null)
			{
				throw new ParseException(string.Format("Function '{0}({1})' does not exist.",
					name, string.Join(",", parameters.Select(e => e.Type.Name))));
			}

			return Expression.Call(methodInfo, parameters);
		}

		protected internal override Parser<Expression> Factor =>
			base.Factor.XOr(FunctionCall);
	}
}
