using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ParameterList = System.Collections.Generic.Dictionary<string, double>;

namespace Sprache.Calc
{
	/// <summary>
	/// Extensible calculator can be extended with custom functions and parameters.
	/// </summary>
	public class XtensibleCalculator : ScientificCalculator
	{
		protected internal virtual Parser<Expression> Parameter
		{
			get
			{
				// identifier not followed by a '(' is a parameter reference
				return
					from id in Identifier
					from n in Parse.Not(Parse.Char('('))
					select GetParameterExpression(id);
			}
		}

		protected internal override Parser<Expression> Factor
		{
			get { return Parameter.Or(base.Factor); }
		}

		protected internal virtual Expression GetParameterExpression(string name)
		{
			// try to find a constant in System.Math
			var systemMathConstants = typeof(System.Math).GetFields(BindingFlags.Public | BindingFlags.Static);
			var constant = systemMathConstants.FirstOrDefault(c => c.Name == name);
			if (constant != null)
			{
				// return System.Math constant value
				return Expression.Constant(constant.GetValue(null));
			}

			// return parameter value: Parameters[name]
			var getItemMethod = typeof(ParameterList).GetMethod("get_Item");
			return Expression.Call(ParameterExpression, getItemMethod, Expression.Constant(name));
		}

		private ParameterExpression parameterInstance = Expression.Parameter(typeof(ParameterList), "Parameters");

		protected internal virtual ParameterExpression ParameterExpression
		{
			get { return parameterInstance; }
		}

		protected internal override Parser<LambdaExpression> Lambda
		{
			get { return Expr.End().Select(body => Expression.Lambda<Func<ParameterList, double>>(body, ParameterExpression)); }
		}

		public virtual Expression<Func<ParameterList, double>> ParseFunction(string text)
		{
			return Lambda.Parse(text) as Expression<Func<ParameterList, double>>;
		}

		public override Expression<Func<double>> ParseExpression(string text)
		{
			// VariableList => double is converted to () => double 
			var sourceExpression = ParseFunction(text);
			var newBody = Expression.Invoke(sourceExpression, Expression.Constant(null, typeof(ParameterList)));
			return Expression.Lambda<Func<double>>(newBody);
		}
	}
}
