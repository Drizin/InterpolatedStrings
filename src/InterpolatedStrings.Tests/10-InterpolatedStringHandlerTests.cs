#if NET6_0_OR_GREATER
using NUnit.Framework;

namespace InterpolatedStrings.Tests
{
    /// <summary>
    ///  InterpolatedStringHandler (<see cref="InterpolatedStringAdapter"/>) allows to parse interpolated strings directly at the compiler level -
    ///  without using regex (<see cref="InterpolatedStringParser"/>). It's only available in net6.0+.
    ///  Funcionality should be identicla to the regex/parser methods.
    /// </summary>
    public class InterpolatedStringHandlerTests
    {

        int val1 = 3;
        int val2 = 5;
        string world = "World";

        [Test]
        public void Test1()
        {

            var s1 = new InterpolatedStringBuilder($"Hello {world}");              // creating using the constructor
            var s2 = InterpolatedStringFactory.Default.Create($"Hello {world}");   // ...is equivalent of using the Create() factory
            var s3 = InterpolatedStringFactory.Default.Create6($"Hello {world}");  // ...while Create6() factory uses InterpolatedStringAdapter.

            Assert.AreEqual("Hello {0}", s1.Format);
            Assert.AreEqual("Hello {0}", s2.Format);
            Assert.AreEqual("Hello {0}", s3.Format);
            
            Assert.AreEqual(1, s1.Arguments.Count);
            Assert.AreEqual(1, s2.Arguments.Count);
            Assert.AreEqual(1, s3.Arguments.Count);

            Assert.AreEqual(world, s1.GetArgument(0));
            Assert.AreEqual(world, s2.GetArgument(0));
            Assert.AreEqual(world, s3.GetArgument(0));

            s1.AppendLiteral("!");
            s2.AppendLiteral("!");
            s3.AppendLiteral("!");

            s1.AppendIf6(true, $" will add {val1}");
            s2.AppendIf6(true, $" will add {val1}");
            s3.AppendIf6(true, $" will add {val1}");

            s1.AppendIf6(false, $" wont add {val1}");
            s2.AppendIf6(false, $" wont add {val1}");
            s3.AppendIf6(false, $" wont add {val1}");

            Assert.AreEqual("Hello {0}! will add {1}", s1.Format);
            Assert.AreEqual("Hello {0}! will add {1}", s2.Format);
            Assert.AreEqual("Hello {0}! will add {1}", s3.Format);

            Assert.AreEqual(2, s1.Arguments.Count);
            Assert.AreEqual(2, s2.Arguments.Count);
            Assert.AreEqual(2, s3.Arguments.Count);
            
            Assert.AreEqual(world, s1.GetArgument(0));
            Assert.AreEqual(world, s2.GetArgument(0));
            Assert.AreEqual(world, s3.GetArgument(0));

            Assert.AreEqual(val1, s1.GetArgument(1));
            Assert.AreEqual(val1, s2.GetArgument(1));
            Assert.AreEqual(val1, s3.GetArgument(1));
        }

    }
}
#endif