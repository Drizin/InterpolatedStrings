#if NET6_0_OR_GREATER
using System.Runtime.CompilerServices;

namespace InterpolatedStrings
{
    /// <summary>
    /// Just a simple InterpolatedStringHandler that builds a <see cref="InterpolatedStringBuilder"/> without requiring regex parsing.
    /// </summary>
    [InterpolatedStringHandler]
    public class InterpolatedStringAdapter
    {

#region Members
        /// <summary>
        /// Underlying Interpolated String
        /// </summary>
        public InterpolatedStringBuilder InterpolatedStringBuilder => _interpolatedStringBuilder;

        private InterpolatedStringBuilder _interpolatedStringBuilder;
#endregion

#region ctors
        /// <inheritdoc />
        public InterpolatedStringAdapter(int literalLength, int formattedCount) // InterpolatedStringFactory.Create() doesn't provide "this" (InterpolatedStringAdapter will create a new StringBuilder)
        {
            _interpolatedStringBuilder = new InterpolatedStringBuilder(literalLength, formattedCount, null);
        }

        // InterpolatedStringBuilder.Append() will send itself.
        /// <inheritdoc />
        public InterpolatedStringAdapter(int literalLength, int formattedCount, InterpolatedStringBuilder target)
        {
            _interpolatedStringBuilder = target;
        }

        // InterpolatedStringBuilder.AppendIf6(bool condition) will send itself and bool
        public InterpolatedStringAdapter(int literalLength, int formattedCount, InterpolatedStringBuilder target, bool condition, out bool isEnabled)
        {
            isEnabled = condition;
            if (!isEnabled)
                return;
            _interpolatedStringBuilder = target;
        }


        /// <inheritdoc />
        public InterpolatedStringAdapter(int literalLength, int formattedCount, InterpolatedStringBuilderOptions options)
        {
            _interpolatedStringBuilder = new InterpolatedStringBuilder(literalLength, formattedCount, options);
        }

#endregion

#region AppendLiteral / AppendFormatted
        /// <summary>
        /// Appends a literal string. 
        /// Curly braces will be automatically escaped depending on <see cref="InterpolatedStringBuilderOptions.AutoEscapeCurlyBraces"/>.
        /// </summary>
        public void AppendLiteral(string value)
        {
            _interpolatedStringBuilder.AppendLiteral(value);
        }

        /// <summary>
        /// Appends the specified object.
        /// </summary>
        public void AppendFormatted<T>(T t)
        {
            _interpolatedStringBuilder.AppendArgument(t, null);
        }

        /// <summary>
        /// Appends the specified object.
        /// </summary>
        public void AppendFormatted<T>(T t, string format)
        {
            _interpolatedStringBuilder.AppendArgument(t, format);
        }
#endregion

    }
}
#endif

