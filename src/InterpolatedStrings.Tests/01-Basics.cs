using NUnit.Framework;
using System.Linq;

namespace InterpolatedStrings.Tests
{
    public class BasicTests
    {
        [Test]
        public void Create_Parse_Append()
        {
            //------------------------------------------------------------------------------
            // Creates an InterpolatedStringBuilder, appends some more interpolated strings
            //------------------------------------------------------------------------------
            string arg1 = "FormattableString";
            var s = new InterpolatedStringBuilder($"This is exactly like {arg1}");

            Assert.AreEqual("This is exactly like {0}", s.Format);
            Assert.AreEqual(1, s.Arguments.Count);
            Assert.AreEqual(arg1, s.Arguments.Single().Argument);

            string arg2 = "additional";
            // += is an operator overload, but you can also call s.Append(...);
            s += $"... but you can append {arg2} FormattableString instances";

            Assert.AreEqual("This is exactly like {0}... but you can append {1} FormattableString instances", s.Format);
            Assert.AreEqual(2, s.Arguments.Count);
            Assert.AreEqual(arg1, s.Arguments[0].Argument);
            Assert.AreEqual(arg2, s.Arguments[1].Argument);
        }

        [Test]
        public void Sample_Sql_Building()
        {
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

            Assert.AreEqual("SELECT * FROM Products WHERE CategoryId={0} AND price<={1}", query.Format);
            Assert.AreEqual(2, query.Arguments.Count);
            Assert.AreEqual(categoryId, query.Arguments[0].Argument);
            Assert.AreEqual(maxPrice, query.Arguments[1].Argument);

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
        }

        [Test]
        public void Sample_Conditional_Appends()
        {
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

            Assert.AreEqual("SELECT * FROM Products WHERE 1=1 AND price<={0}", query.Format);
            Assert.AreEqual(1, query.Arguments.Count);
            Assert.AreEqual(maxPrice, query.Arguments[0].Argument);
        }


        [Test]
        public void Sample_Multiline_Replaces()
        {
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

            Assert.AreEqual(1, query.Arguments.Count);
            Assert.AreEqual(categoryId, query.Arguments[0].Argument);
        }

    }
}