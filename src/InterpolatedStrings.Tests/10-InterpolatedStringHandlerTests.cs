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
        string world = "World";

        [Test]
        public void Test1()
        {

            var s1 = new InterpolatedStringBuilder($"Hello {world}");              // creating using the constructor will always use regex
            var s2 = InterpolatedStringFactory.Default.Create($"Hello {world}");   // ...while Create() factory (when using net6.0+) uses InterpolatedStringAdapter - but both should get identical behavior

            Assert.AreEqual("Hello {0}", s1.Format);
            Assert.AreEqual("Hello {0}", s2.Format);
            
            Assert.AreEqual(1, s1.Arguments.Count);
            Assert.AreEqual(1, s2.Arguments.Count);

            Assert.AreEqual(world, s1.GetArgument(0));
            Assert.AreEqual(world, s2.GetArgument(0));

            s1.AppendLiteral("!");
            s2.AppendLiteral("!");

            s1.AppendIf(true, $" will add {val1}");
            s2.AppendIf(true, $" will add {val1}");

            s1.AppendIf(false, $" wont add {val1}");
            s2.AppendIf(false, $" wont add {val1}");

            Assert.AreEqual("Hello {0}! will add {1}", s1.Format);
            Assert.AreEqual("Hello {0}! will add {1}", s2.Format);

            Assert.AreEqual(2, s1.Arguments.Count);
            Assert.AreEqual(2, s2.Arguments.Count);
            
            Assert.AreEqual(world, s1.GetArgument(0));
            Assert.AreEqual(world, s2.GetArgument(0));

            Assert.AreEqual(val1, s1.GetArgument(1));
            Assert.AreEqual(val1, s2.GetArgument(1));
        }

    }
}
#endif