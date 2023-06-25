using NUnit.Framework;
using System;
using System.Globalization;

namespace InterpolatedStrings.Tests
{
    public class FormattableStringsCompatibilityTests
    {
        #region InterpolatedStringBuilder implements FormattableString and can be used to replace your existing instances of FormattableString
        static double val1 = 123.45;
        static int val2 = 7;
        static string val3 = "hello";
        static string val4 = "world";
        static CultureInfo culture1 = CultureInfo.CurrentCulture;
        static CultureInfo culture2 = CultureInfo.InvariantCulture;
        static CultureInfo culture3 = CultureInfo.CreateSpecificCulture("pt-BR");
        static CultureInfo culture4 = CultureInfo.CreateSpecificCulture("de-CH");

        private static readonly object[] _source = {
            new object[] { (FormattableString)$"Test"},
            new object[] { (FormattableString)$"Test {val1}"},
            new object[] { (FormattableString)$"Test {val1} {val2}"},
            new object[] { (FormattableString)$"Test {val3}"},
            new object[] { (FormattableString)$"Test {val3} {val4}"},
            new object[] { (FormattableString)$"Test {val3}{val4}"},
            new object[] { (FormattableString)$"Test {val1}{val2}{val3}{val4}"},
        };

        [TestCaseSource("_source")]
        [Test(Description = "InterpolatedStringBuilder implements FormattableString and can be used to replace your existing instances of FormattableString")]
        public void Can_be_used_like_FormattableString(FormattableString value)
        {
            var s = new InterpolatedStringBuilder(value);
            Assert.AreEqual(s.Format, value.Format);
            Assert.AreEqual(s.ArgumentCount, value.ArgumentCount);
            for (int i = 0; i < s.ArgumentCount; i++)
            {
                Assert.AreEqual(s.GetArgument(i), value.GetArgument(i));
            }

           
            Assert.AreEqual(s.ToString(), value.ToString());

            System.Diagnostics.Debug.WriteLine(s.ToString(culture1));
            System.Diagnostics.Debug.WriteLine(s.ToString(culture2));
            System.Diagnostics.Debug.WriteLine(s.ToString(culture3));
            System.Diagnostics.Debug.WriteLine(s.ToString(culture4));

            Assert.AreEqual(s.ToString(culture1), value.ToString(culture1));
            Assert.AreEqual(s.ToString(culture2), value.ToString(culture2));
            Assert.AreEqual(s.ToString(culture3), value.ToString(culture3));
            Assert.AreEqual(s.ToString(culture4), value.ToString(culture4));
        }
        #endregion
    }
}