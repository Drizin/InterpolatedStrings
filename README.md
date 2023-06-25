[![Nuget](https://img.shields.io/nuget/v/InterpolatedStrings?label=InterpolatedStrings)](https://www.nuget.org/packages/InterpolatedStrings)
[![Downloads](https://img.shields.io/nuget/dt/InterpolatedStrings.svg)](https://www.nuget.org/packages/InterpolatedStrings)
[![Nuget](https://img.shields.io/nuget/v/InterpolatedStrings.StrongName?label=InterpolatedStrings.StrongName)](https://www.nuget.org/packages/InterpolatedStrings.StrongName)
[![Downloads](https://img.shields.io/nuget/dt/InterpolatedStrings.StrongName.svg)](https://www.nuget.org/packages/InterpolatedStrings.StrongName)

# Interpolated String Builder

**[InterpolatedStringBuilder](src/InterpolatedStrings/InterpolatedStringBuilder.cs) is a `FormattableString` with support for concatenating other interpolated strings, replace(), insert(), etc**

It's similar to a StringBuilder but for Interpolated Strings (FormattableString)

# Quickstart

1. Install the [NuGet package InterpolatedStrings](https://www.nuget.org/packages/InterpolatedStrings) or [NuGet package InterpolatedStrings.StrongName](https://www.nuget.org/packages/InterpolatedStrings.StrongName)
1. Add `using InterpolatedStrings;` to your usings.
1. See examples below.


## Basics

How to create an InterpolatedStringBuilder, append some more interpolated strings

```cs
string arg1 = "FormattableString";
var s = new InterpolatedStringBuilder($"This is exactly like {arg1}");

// s.Format now is equal to "This is exactly like {0}"
// s.Arguments contain [arg1]

string arg2 = "additional";

// += is an operator overload, but you can also call s.Append(...);
s += $"... but you can append {arg2} FormattableString instances";

// s.Format now is equal to "This is exactly like {0}... but you can append {1} FormattableString instances"
// s.Arguments now contains [arg1, arg2]
```

## Sample Usage: Dynamic SQL building

```cs
int categoryId = 1;
double maxPrice = 20.50;

//------------------------------------------------------------------------------
// Creates an initial SQL query, and appends more conditions.
// Embedded objects are NOT converted to strings: they are still kept 
// as objects (in Arguments list), and the underlying format string just keeps
// the numbered placeholders
//------------------------------------------------------------------------------
var query = new InterpolatedStringBuilder($"SELECT * FROM Products");
query += $" WHERE CategoryId={categoryId}";
query += $" AND price<={maxPrice}";

// query.Format now is "SELECT * FROM Products WHERE CategoryId={0} AND price<={1}"
// query.Arguments now is [categoryId, maxPrice]

//------------------------------------------------------------------------------
// Then you can create your own methods (or extensions) to convert back from
// InterpolatedStringBuilder into a valid SQL statement
//------------------------------------------------------------------------------
string sql = string.Format(query.Format, query.Arguments.Select((arg, i) => "@p" + i.ToString()).ToArray());
Assert.AreEqual("SELECT * FROM Products WHERE CategoryId=@p0 AND price<=@p1", sql);

// If you were using Dapper you could pass parameters like this:
// var sqlParms = new DynamicParameters();
// for (int i = 0; i < query.Arguments.Count; i++) { dbArgs.Add("p" + i.ToString(), query.Arguments[i].Argument); }
// var products = connection.Query<Product>(sql, sqlParms)
```

## Fluent API and Conditional Appends

```cs
int? categoryId = null;
double? maxPrice = 20.50;

//------------------------------------------------------------------------------
// Fluent API allows short syntax for appending multiple blocks,
// and using conditions
//------------------------------------------------------------------------------
var query = 
    new InterpolatedStringBuilder($"SELECT * FROM Products WHERE 1=1")
    .AppendIf(categoryId != null, $" AND CategoryId={categoryId}")
    .AppendIf(maxPrice != null, $" AND price<={maxPrice}");

// Now query.Format is "SELECT * FROM Products WHERE 1=1 AND price<={0}"
```

## Multiline Blocks, Replaces, Inserts, etc.

Using Raw String Literals:

```cs
int? categoryId = 3;
double? maxPrice = null;

//------------------------------------------------------------------------------
// Raw String Literals allows us to easily write multiline blocks
//------------------------------------------------------------------------------
var query = new InterpolatedStringBuilder($$"""
    SELECT * FROM Products
    /***where***/
    ORDER BY Category, Name
    """);

var wheres = new InterpolatedStringBuilder();
wheres.AppendIf(categoryId != null, $" AND CategoryId={categoryId}");
wheres.AppendIf(maxPrice != null, $" AND price<={maxPrice}");

if (wheres.Format.Length> 0)
{
    wheres.Remove(0, " AND ".Length).Insert(0, $"WHERE ");
    query.Replace("/***where***/", wheres);
}

Assert.AreEqual("""
    SELECT * FROM Products
    WHERE CategoryId={0}
    ORDER BY Category, Name
    """, 
    query.Format);
```

## Interpolated String Handlers

FormattableStrings are [parsed](/src/InterpolatedStrings/InterpolatedStringParser.cs) using regex.
If you're using .net6.0+ you can use the methods that use an [InterpolatedStringHandler](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/tutorials/interpolated-string-handler).

```cs
// InterpolatedStringFactory.Create() will use regex, while Create6() will use InterpolatedStringHandler
var builder = InterpolatedStringFactory.Default.Create6($"Hello {world}"); 
builder.Append6($" something...");
builder.AppendIf6(true, $" something else...");
```

## Extensibility

One of the nice things of this library is that you can extend the `InterpolatedStringBuilder` class and override methods like `AppendLiteral()`, `AppendArgument()`, or `AddArgument()`, and manipulate the way that `Format` is built or the way that `Arguments` are created.
And you can do things like [this](https://github.com/Drizin/DapperQueryBuilder).

## More Examples
See more examples in [unit tests](/src/InterpolatedStrings.Tests/).


# FAQ

## What is FormattableString?

Whenever you write an interpolated string, the compiler can either convert it to a plain string or (if you specify the right type) it can keep the interpolated string as a `FormattableString`.  
The nice part of FormattableString (as compared to a plain string) is that it keeps the `Arguments` (the objects that you interpolate) and the `Format` (the literals around the arguments) isolated from each other.
And this allows a lot of clever usages like [this](https://github.com/Drizin/DapperQueryBuilder).  

## What is wrong with FormattableString?

The major limitation of FormattableString is that it's immutable: you can't append new interpolated strings, or modify it (Replace()/Insert()/Remove()).

PS: Actually `FormattableString` is an abstract class (which we also implement). The limitation in case is from `ConcreteFormattableString` which is the concrete type that the compiler uses when it creates an interpolated string.


## Is this a replacement for StringBuilder?

**No, it's NOT.**

It's more like a replacement for `ConcreteFormattableString`, but it's **similar** to a StringBuilder only in the sense that it's mutable (we can concatenate new interpolated strings), which is not possible in ConcreteFormattableString.

So in other words, `InterpolatedStringBuilder` is a `FormattableString` implementation that allows us to concatenate other interpolated strings, and offers some methods similar to methods that you would also have in a StringBuilder (`Replace()`, `Insert()`, `Remove()`) etc).  
So our methods are named like StringBuilder methods, but instead of operating on plain strings (like a StringBuilder), it wraps both `Arguments` and Literals (`Format`) - like a `FormattableString` would do.

## How is this any better than using a plain StringBuilder?

Having a single wrapper (which wraps both `Arguments` and `Format`, and lets them "walk side-by-side" - always in synch) makes things easier.

In a single statement you can both append one or more literals and one or more arguments.

```cs
// Using a StringBuilder we have to keep Arguments and Literals individually
var sql  = new StringBuilder();
var dynamicParams = new DynamicParameters();

sql.Append("SELECT * FROM Product WHERE 1=1");

sql.Append(" AND Name LIKE @p0"); 
dynamicParams.Add("p0", productName);

sql.Append(" AND ProductSubcategoryID = @p1");
dynamicParams.Add("p1", subCategoryId);
```

```cs
// Using InterpolatedStringBuilder the Arguments and Literals walk side-by-side
var sql  = new InterpolatedStringBuilder();

sql.Append($"SELECT * FROM Product WHERE 1=1");

sql.Append($" AND Name LIKE {productName}"); 
sql.Append($" AND ProductSubcategoryID = {subCategoryId}");
```

And by inheriting from `InterpolatedStringBuilder` we can even hack the way that literals and arguments are processed (e.g. automatically add spaces, or even parse hints like `sql.Append($" AND Name LIKE {productName:nvarchar(200)}")` ).



## Why the regex parsing?

Starting with net6.0 the interpolated strings can be parsed using an `InterpolatedStringHandler`, which processes the interpolated strings block by block (literal by literal, argument by argument).  
This step-by-step processing is very interesting because derived classes have a chance to modify the underlying format - like automatically adding spaces, adding or removing quotes, extracting IFormattable formats, or anything else.  
Before `InterpolatedStringHandler` the only way to do that (process each literal one by one) was using regular expressions.  
Our `StringInterpolationBuilder` works both with net5.0 or older (using regex) and with net6.0+ (using `InterpolatedStringHandler`). You can inherit `StringInterpolationBuilder` and override `AppendLiteral()` and `AppendArgument()`, and do your own magic.
If you don't need to override those methods then probably we wouldn't need to parse the format using regex (we'll improve that).


# Stargazers over time

[![Star History Chart](https://api.star-history.com/svg?repos=Drizin/InterpolatedStrings&type=Date)](https://star-history.com/#Drizin/InterpolatedStrings&Date)


## License
MIT License
