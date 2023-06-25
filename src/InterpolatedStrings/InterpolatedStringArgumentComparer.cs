using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace InterpolatedStrings
{
    /// <summary>
    /// Compares two arguments for reusing. Only used if <see cref="InterpolatedStringBuilderOptions.ReuseIdenticalParameters"/> is true.
    /// </summary>
    public class InterpolatedStringArgumentComparer : IEqualityComparer<InterpolatedStringArgument>
    {
        /// <inheritdoc/>
        public bool Equals(InterpolatedStringArgument? arg1, InterpolatedStringArgument? arg2)
        {
            if (arg1 == null && arg2 == null)
                return true;
            if (arg1 == null || arg2 == null)
                return false;
            if (arg1.Format != arg2.Format)
                return false;
            if (arg1.Argument == null && arg2.Argument == null)
                return true;
            if (arg1.Argument == null || arg2.Argument == null)
                return false;
            if (arg1.Argument.GetType() == arg2.Argument.GetType() && arg1.Argument.Equals(arg2.Argument))
                return true;
            return false;
        }

        /// <inheritdoc/>
        public int GetHashCode([DisallowNull] InterpolatedStringArgument obj)
        {
            return (obj.Argument?.GetHashCode() ?? 0) ^ (obj.Format?.GetHashCode() ?? 0);
        }
    }
}
