[![Nuget](https://img.shields.io/nuget/v/InterpolatedStrings?label=InterpolatedStrings)](https://www.nuget.org/packages/InterpolatedStrings)
[![Downloads](https://img.shields.io/nuget/dt/InterpolatedStrings.svg)](https://www.nuget.org/packages/InterpolatedStrings)
[![Nuget](https://img.shields.io/nuget/v/InterpolatedStrings.StrongName?label=InterpolatedStrings.StrongName)](https://www.nuget.org/packages/InterpolatedStrings.StrongName)
[![Downloads](https://img.shields.io/nuget/dt/InterpolatedStrings.StrongName.svg)](https://www.nuget.org/packages/InterpolatedStrings.StrongName)

# Interpolated String Builder

**[InterpolatedStringBuilder](src/InterpolatedStrings/InterpolatedStringBuilder.cs) is like a StringBuilder but for Interpolated Strings (FormattableString)**

It's an implementation of `FormattableString` with support for concatenating strings, replace, insert, etc.

This project was based on [DapperQueryBuilder](https://github.com/Drizin/DapperQueryBuilder).

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

## Conditional Appends

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
    .AppendIf(maxPrice != null, $" AND price<={maxPrice}")
    ;

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



See more examples in [unit tests](/src/InterpolatedStrings.Tests/).



## How to Collaborate?

Please submit a pull-request or if you want to make a sugestion you can [create an issue](https://github.com/Drizin/InterpolatedStrings/issues) or [contact me](https://rickdrizin.com/pages/Contact/).

## Stargazers over time

[![Star History Chart](https://api.star-history.com/svg?repos=Drizin/InterpolatedStrings&type=Date)](https://star-history.com/#Drizin/InterpolatedStrings&Date)


## License
MIT License
