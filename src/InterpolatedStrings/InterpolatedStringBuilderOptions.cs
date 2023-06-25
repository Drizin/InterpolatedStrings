using System;
using System.Collections.Generic;

namespace InterpolatedStrings
{
    /// <summary>
    /// Settings for InterpolatedStringBuilder
    /// </summary>
    public class InterpolatedStringBuilderOptions
    {
        /// <summary>
        /// All <see cref="InterpolatedStringBuilder"/> methods that take a <see cref="FormattableString"/> will use this Parser to parse the FormattableString.
        /// By default it's <see cref="InterpolatedStringParser"/>
        /// </summary>
        public IInterpolatedStringParser Parser { get; set; } = new InterpolatedStringParser();

        /// <summary>
        /// If true (default is true) then all curly braces are escaped (with double curly braces).
        /// By default <see cref="System.FormattableString.Format"/> uses indexed placeholders (numbered placeholders like "{0}", "{1}", etc.) to indicate the arguments,
        /// and so does <see cref="InterpolatedStringBuilder.Format"/>.
        /// If your derived type does NOT need those indexed placeholders (e.g. you're writing your own format and do not need the standard one) or
        /// if your derived type will never write curly braces through <see cref="InterpolatedStringBuilder.AppendLiteral(string)"/>, 
        /// then you can disable this (set to false) so that it won't escape curly braces (and it will be faster).
        /// You can also use <see cref="InterpolatedStringBuilder.AppendRaw(string)"/>) which does not escape anything.
        /// </summary>
        public bool AutoEscapeCurlyBraces { get; set; } = true;

        /// <summary>
        /// Argument Formatting (what comes after colon, like $"My string {val:000}") is always extracted into <see cref="InterpolatedStringArgument.Format"/>.
        /// If this is true (default is true) then this formatting will also be preserved in the underlying <see cref="InterpolatedStringBuilder.Format"/>
        /// (else, <see cref="InterpolatedStringBuilder.Format"/> will only have the numeric placeholder like {0} but without the extracted formatting).
        /// </summary>
        public bool PreserveArgumentFormatting { get; set; } = true;

        /// <summary>
        /// If true (default is false) each added parameter will check if identical parameter (same type and value)
        /// was already added, and if so will reuse the existing parameter.
        /// </summary>
        public bool ReuseIdenticalParameters { get; set; } = false;

        /// <summary>
        /// Compares two arguments for reusing. Only used if <see cref="ReuseIdenticalParameters"/> is true.
        /// By default it's <see cref="InterpolatedStringArgumentComparer"/>
        /// </summary>
        public IEqualityComparer<InterpolatedStringArgument> ArgumentComparer { get; set; } = new InterpolatedStringArgumentComparer();
    }
}
