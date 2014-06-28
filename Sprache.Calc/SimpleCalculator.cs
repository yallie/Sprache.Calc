using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Sprache.Calc
{
	/// <summary>
	/// Simple calculator grammar.
	/// Supports arithmetic operations and parentheses.
	/// </summary>
	public class SimpleCalculator
	{
		protected internal virtual Parser<string> DecimalWithoutLeadingDigits
		{
			get
			{
				return
					from dot in Parse.Char('.')
					from fraction in Parse.Number
					select dot + fraction;
			}
		}

		protected internal virtual Parser<string> DecimalWithLeadingDigits
		{
			get { return Parse.Number.Then(n => DecimalWithoutLeadingDigits.XOr(Parse.Return(string.Empty)).Select(f => n + f)); }
		}

		protected internal virtual Parser<string> Decimal
		{
			get { return DecimalWithLeadingDigits.XOr(DecimalWithoutLeadingDigits); }
		}

		protected internal virtual Parser<Expression> Constant
		{
			get { return Decimal.Select(x => Expression.Constant(double.Parse(x, CultureInfo.InvariantCulture))).Named("Constant"); }
		}

		protected internal Parser<ExpressionType> Operator(string op, ExpressionType opType)
		{
			return Parse.String(op).Token().Return(opType);
		}

		protected internal virtual Parser<ExpressionType> Add
		{
			get { return Operator("+", ExpressionType.AddChecked); }
		}

		protected internal virtual Parser<ExpressionType> Subtract
		{
			get { return Operator("-", ExpressionType.SubtractChecked); }
		}

		protected internal virtual Parser<ExpressionType> Multiply
		{
			get { return Operator("*", ExpressionType.MultiplyChecked); }
		}

		protected internal virtual Parser<ExpressionType> Divide
		{
			get { return Operator("/", ExpressionType.Divide); }
		}

		protected internal virtual Parser<ExpressionType> Modulo
		{
			get { return Operator("%", ExpressionType.Modulo); }
		}

		protected internal virtual Parser<ExpressionType> Power
		{
			get { return Operator("^", ExpressionType.Power); }
		}

		protected virtual Parser<Expression> ExpressionInParentheses
		{
			get
			{
				return
					from lparen in Parse.Char('(')
					from expr in Expr
					from rparen in Parse.Char(')')
					select expr;
			}
		}

		protected internal virtual Parser<Expression> Factor
		{
			get { return ExpressionInParentheses.XOr(Constant); }
		}

		protected internal virtual Parser<Expression> NegativeFactor
		{
			get
			{
				return
					from sign in Parse.Char('-')
					from factor in Factor
					select Expression.NegateChecked(factor);
			}
		}

		protected internal virtual Parser<Expression> Operand
		{
			get { return (NegativeFactor.XOr(Factor)).Token(); }
		}

		protected internal virtual Parser<Expression> InnerTerm
		{
			get { return Parse.ChainRightOperator(Power, Operand, Expression.MakeBinary); }
		}

		protected internal virtual Parser<Expression> Term
		{
			get { return Parse.ChainOperator(Multiply.Or(Divide).Or(Modulo), InnerTerm, Expression.MakeBinary); }
		}

		protected internal virtual Parser<Expression> Expr
		{
			get { return Parse.ChainOperator(Add.Or(Subtract), Term, Expression.MakeBinary); }
		}

		protected internal virtual Parser<LambdaExpression> Lambda
		{
			get { return Expr.End().Select(body => Expression.Lambda<Func<double>>(body)); }
		}

		public virtual Expression<Func<double>> ParseExpression(string text)
		{
			return Lambda.Parse(text) as Expression<Func<double>>;
		}
	}
}
