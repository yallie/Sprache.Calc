Sprache.Calc
============

[![Appveyor build status](https://ci.appveyor.com/api/projects/status/qswh0wj2uv3w502v?svg=true)](https://ci.appveyor.com/project/yallie/sprache-calc)
[![Tests](https://img.shields.io/appveyor/tests/yallie/sprache-calc.svg)](https://ci.appveyor.com/project/yallie/sprache-calc/build/tests)

This library provides easy to use extensible expression evaluator based on [LinqyCalculator sample](https://github.com/sprache/Sprache/blob/master/samples/LinqyCalculator/ExpressionParser.cs).
The evaluator supports arithmetic operations, custom functions and parameters. It takes string
representation of an expression and converts it to a structured LINQ expression instance
which can easily be compiled to an executable delegate. In contrast with interpreted expression
evaluators such as NCalc, compiled expressions perform just as fast as native C# methods.

![Instaco.de](/Sprache.Calc.Icons/Instacode91251.png)

Usage example
-------------

```csharp
var calc = new Sprache.Calc.XtensibleCalculator();

// using expressions
var expr = calc.ParseExpression("Sin(y/x)", x => 2, y => System.Math.PI);
var func = expr.Compile();
Console.WriteLine("Result = {0}", func());

// custom functions
calc.RegisterFunction("Mul", (a, b, c) => a * b * c);
expr = calc.ParseExpression("2 ^ Mul(PI, a, b)", a => 2, b => 10);
Console.WriteLine("Result = {0}", expr.Compile()());

// end-user's functions
calc.RegisterFunction("Add", "a + b", "a", "b");
expr = calc.ParseExpression("Add(353, 181)");
Console.WriteLine("Result = {0}", expr.Compile()());
```

Installation
------------

To use expression evaluator in your projects, install [Sprache.Calc NuGet package](https://www.nuget.org/packages/sprache.calc)
by running the following command in the Package Manager Console:

````
PM> Install-Package Sprache.Calc
````

Grammar inheritance technique
-----------------------------

Sprache.Calc library serves as a demonstration of grammar inheritance technique with Sprache toolkit.
An article describing Sprache.Calc implementation details is currently available in English and Russian:

* [Sprache.Calc: building yet another expression evaluator](http://www.codeproject.com/Articles/795056/Sprache-Calc-building-yet-another-expression-evalu?msg=4858437#xx4858437xx)
* [Наследование грамматик в Sprache на примере калькулятора выражений](http://habrahabr.ru/post/228037/).

TL;DR:

* Declare parsers as virtual properties instead of static fields
* Decompose the grammar into small and reusable rules 
* Write unit tests for every single atomic parser
* Use "protected internal" access modifier to enable unit testing
* Unit tests must be organized in the same hierarchy as parser classes
