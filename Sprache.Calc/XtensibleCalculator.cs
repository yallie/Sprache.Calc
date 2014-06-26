using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CustomFunction = System.Func<double[], double>;
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

		private Dictionary<string, CustomFunction> customFunctions = new Dictionary<string, CustomFunction>();

		protected internal virtual Dictionary<string, CustomFunction> CustomFuctions
		{
			get { return customFunctions; }
		}

		protected internal virtual string MangleName(string name, int paramCount)
		{
			return name + ":" + paramCount;
		}

		protected internal override Expression CallFunction(string name, params Expression[] parameters)
		{
			// look up a custom function first
			var mangledName = MangleName(name, parameters.Length);
			if (CustomFuctions.ContainsKey(mangledName))
			{
				// convert parameters
				var callCustomFunction = new Func<string, double[], double>(CallCustomFunction).Method;
				var newParameters = new List<Expression>();
				newParameters.Add(Expression.Constant(mangledName));
				newParameters.Add(Expression.NewArrayInit(typeof(double), parameters));

				// call this.CallCustomFunction(mangledName, double[]);
				return Expression.Call(Expression.Constant(this), callCustomFunction, newParameters.ToArray());
			}

			// fall back to System.Math functions
			return base.CallFunction(name, parameters);
		}

		protected virtual double CallCustomFunction(string mangledName, double[] parameters)
		{
			return CustomFuctions[mangledName](parameters);
		}

		public XtensibleCalculator RegisterFunction(string name, Func<double> function)
		{
			CustomFuctions[MangleName(name, 0)] = x => function();
			return this;
		}

		public XtensibleCalculator RegisterFunction(string name, Func<double, double> function)
		{
			CustomFuctions[MangleName(name, 1)] = x => function(x[0]);
			return this;
		}

		public XtensibleCalculator RegisterFunction(string name, Func<double, double, double> function)
		{
			CustomFuctions[MangleName(name, 2)] = x => function(x[0], x[1]);
			return this;
		}

		public XtensibleCalculator RegisterFunction(string name, Func<double, double, double, double> function)
		{
			CustomFuctions[MangleName(name, 3)] = x => function(x[0], x[1], x[2]);
			return this;
		}

		public XtensibleCalculator RegisterFunction(string name, Func<double, double, double, double, double> function)
		{
			CustomFuctions[MangleName(name, 4)] = x => function(x[0], x[1], x[2], x[3]);
			return this;
		}

		public XtensibleCalculator RegisterFunction(string name, Func<double, double, double, double, double, double> function)
		{
			CustomFuctions[MangleName(name, 5)] = x => function(x[0], x[1], x[2], x[3], x[4]);
			return this;
		}
	}
}
