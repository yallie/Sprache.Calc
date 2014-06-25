using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Sprache.Calc
{
	/// <summary>
	/// Scientific calculator grammar. Supports System.Math functions.
	/// </summary>
	public class ScientificCalculator : SimpleCalculator
	{
		protected internal virtual Parser<string> Identifier
		{
			get
			{
				return Parse.Letter.AtLeastOnce().Text().Then(h =>
					Parse.LetterOrDigit.Many().Text().Select(t => h + t)).Token();
			}
		}

		protected internal virtual Parser<Expression> FunctionCall
		{
			get
			{
				return
					from name in Identifier
					from lparen in Parse.Char('(')
					from expr in Expr.DelimitedBy(Parse.Char(',').Token())
					from rparen in Parse.Char(')')
					select CallFunction(name, expr.ToArray());
			}
		}

		protected internal virtual Expression CallFunction(string name, Expression[] parameters)
		{
			var methodInfo = typeof(Math).GetMethod(name, parameters.Select(e => e.Type).ToArray());
			if (methodInfo == null)
			{
				throw new ParseException(string.Format("Function '{0}({1})' does not exist.",
					name, string.Join(",", parameters.Select(e => e.Type.Name))));
			}

			return Expression.Call(methodInfo, parameters);
		}

		protected internal override Parser<Expression> Factor
		{
			get { return base.Factor.XOr(FunctionCall); }
		}
	}
}
