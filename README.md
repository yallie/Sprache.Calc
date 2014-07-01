Sprache.Calc
============

This library provides easy to use extensible expression evaluator based on [LinqyCalculator sample](https://github.com/sprache/Sprache/blob/master/src/LinqyCalculator/ExpressionParser.cs).
The evaluator supports arithmetic operations, custom functions and parameters. It takes string
representation of an expression and converts it to a structured LINQ expression instance
which can easily be compiled to an executable delegate. In contrast with interpreting expression
evaluators such as NCalc, compiled expressions perform just as fast as native C# methods.

Usage example:

```csharp
var calc = new Sprache.Calc.XtensibleCalculator();

// using variables
var expr = calc.ParseExpression("Sin(y/x)", x => 2, y => System.Math.PI);
var func = expr.Compile();
Console.WriteLine("Result = {0}", func());

// custom functions
calc.RegisterFunction("Mul", (a, b, c) => a * b * c);
expr = calc.ParseExpression("2 ^ Mul(PI, a, b)", a => 2, b => 10);
Console.WriteLine("Result = {0}", func.Compile()());
```

Sprache toolkit grammar inheritance
-----------------------------------

Sprache.Calc library serves as a demonstration of grammar inheritance tecnhique with Sprache toolkit.
An article describing this tenchique in details is currently available [in Russian](http://habrahabr.ru/post/228037/).
