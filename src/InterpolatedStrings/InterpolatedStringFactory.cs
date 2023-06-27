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
        /// Creates a new InterpolatedStringBuilder using an InterpolatedStringHandler.
        /// </summary>
        public virtual InterpolatedStringBuilder Create(InterpolatedStringAdapter value)
        {
            return value.InterpolatedStringBuilder;
        }

        /// <summary>
        /// Creates a new InterpolatedStringBuilder using an InterpolatedStringHandler.
        /// </summary>
        public virtual InterpolatedStringBuilder Create([InterpolatedStringHandlerArgument("options")] InterpolatedStringAdapter value, InterpolatedStringBuilderOptions options)
        {
            return value.InterpolatedStringBuilder;
        }
#else
        /// <summary>
        /// Creates a new InterpolatedStringBuilder using regular expression for parsing the FormattableString. 
        /// </summary>
        public virtual InterpolatedStringBuilder Create(FormattableString source)
        {
            var target = new InterpolatedStringBuilder();
            Parser.ParseAppend(source, target);
            return target;
        }
#endif



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
