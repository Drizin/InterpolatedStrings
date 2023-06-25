using System;
#if NET6_0_OR_GREATER
using System.Runtime.CompilerServices;
#endif

namespace InterpolatedStrings
{
    /// <summary>
    /// Creates <see cref="InterpolatedStringBuilder"/>
    /// </summary>
    public class InterpolatedStringFactory
    {
#if NET6_0_OR_GREATER
        /// <summary>
        /// Creates a new InterpolatedStringBuilder using an InterpolatedStringHandler (net6.0+) which can be a little faster than using regex.
        /// </summary>
        public virtual InterpolatedStringBuilder Create6(InterpolatedStringAdapter value)
        {
            return value.InterpolatedStringBuilder;
        }

        /// <summary>
        /// Creates a new InterpolatedStringBuilder using an InterpolatedStringHandler (net6.0+) which can be a little faster than using regex.
        /// </summary>
        public virtual InterpolatedStringBuilder Create6([InterpolatedStringHandlerArgument("options")] InterpolatedStringAdapter value, InterpolatedStringBuilderOptions options)
        {
            return value.InterpolatedStringBuilder;
        }

#endif

        /// <summary>
        /// Creates a new InterpolatedStringBuilder using regular expression for parsing the FormattableString. 
        /// If you're using net6.0+ please consider using <see cref="Create6(InterpolatedStringAdapter)"/> which can be a little faster than using regex.
        /// </summary>
        public virtual InterpolatedStringBuilder Create(FormattableString source)
        {
            var target = new InterpolatedStringBuilder();
            Parser.ParseAppend(source, target);
            return target;
        }

#region Default Parser Instance
        /// <summary>
        /// Default Parser
        /// </summary>
        public InterpolatedStringParser Parser = new InterpolatedStringParser();
#endregion

#region Default Factory Instance
        /// <summary>
        /// Default Factory
        /// </summary>
        public static InterpolatedStringFactory Default = new InterpolatedStringFactory();
#endregion
    }
}
