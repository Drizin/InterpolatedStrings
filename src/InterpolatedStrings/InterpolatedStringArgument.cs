﻿using System;

namespace InterpolatedStrings
{
    /// <summary>
    /// Interpolated string arguments are defined like this:  $"My string {arg}", $"My string {val:000}"
    /// InterpolatedStringArgument will hold both the arguments (like arg or val) in <see cref="Argument"/> 
    /// and if there's a format (after colon, like "000") it will be saved in <see cref="Format"/> (else it will be null).
    /// </summary>
    public class InterpolatedStringArgument
    {
        /// <summary>
        /// Value of argument embedded into Interpolated String. Like <see cref="FormattableString.GetArguments"/>
        /// </summary>
        public object? Argument { get; set; }

        /// <summary>
        /// Each argument embedded into FormattableString may have a format specifier which will be kept here.
        /// If there's no format specified then this will be null.
        /// </summary>
        public string? Format { get; set; }

        /// <inheritdoc/>
        public InterpolatedStringArgument(object? argument, string? format)
        {
            Argument = argument;
            Format = format;
        }
    }
}
