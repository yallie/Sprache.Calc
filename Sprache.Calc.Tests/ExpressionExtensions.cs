using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Sprache.Calc.Tests
{
	/// <summary>
	/// Helper class for <see cref="Expression"/>.
	/// </summary>
	internal static class ExpressionExtensions
	{
		/// <summary>
		/// Executes the specified <see cref="Expression"/> and returns result as <see cref="Double"/> value.
		/// </summary>
		/// <param name="x">The expression to execute.</param>
		public static double Execute(this Expression x)
		{
			var lambda = Expression.Lambda<Func<double>>(x);
			return lambda.Compile()();
		}
	}
}
