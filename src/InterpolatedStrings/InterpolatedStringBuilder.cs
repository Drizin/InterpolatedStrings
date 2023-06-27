using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
#if NET6_0_OR_GREATER
using System.Runtime.CompilerServices;
#endif

namespace InterpolatedStrings
{
    /// <summary>
    /// InterpolatedStringBuilder is like a StringBuilder but for Interpolated Strings (FormattableString).
    /// It's an implementation of `FormattableString` with support for concatenating strings, replace, insert, etc.
    /// </summary>
    [DebuggerDisplay("{Format}")]
    public class InterpolatedStringBuilder : FormattableString
    {
#region Members
        /// <inheritdoc cref="InterpolatedStringBuilderOptions"/>
        public InterpolatedStringBuilderOptions Options { get; set; }

        /// <summary>
        /// For <see cref="InterpolatedStringBuilder"/> this is like <see cref="FormattableString.Format"/> which is the underlying format of the interpolated string.
        /// If you inherit and override the Append methods then this may have a different meaning.
        /// If you don't use indexed placeholders (numbered placeholders like "{0}", "{1}", etc.) you can set <see cref="InterpolatedStringBuilderOptions.AutoEscapeCurlyBraces"/> to false for better performance.
        /// </summary>
        public override string Format
        { 
            get 
            {
                if (_cachedFormat == null)
                    _cachedFormat = _format.ToString();
                return _cachedFormat;
            }
        }
        
        /// <summary>
        /// <see cref="Format"/> uses <see cref="StringBuilder.ToString()"/> which is slow - this is a cache for faster reuse. Cache can be invalidated by setting it to null, so it gets recalculated when needed
        /// </summary>
        protected string? _cachedFormat = null;

        /// <summary>
        /// Like <see cref="FormattableString.GetArguments"/>
        /// </summary>
        public IReadOnlyList<InterpolatedStringArgument> Arguments => _arguments;

        /// <inheritdoc cref="Arguments" />
        protected List<InterpolatedStringArgument> _arguments;

        /// <inheritdoc cref="Format" />
        protected StringBuilder _format;

        /// <summary>
        /// Like <see cref="FormattableString.ArgumentCount"/>
        /// </summary>
        public override int ArgumentCount => _arguments.Count;

        /// <summary>
        /// Like <see cref="FormattableString.GetArgument(int)"/>
        /// </summary>
        public override object? GetArgument(int index) => _arguments[index].Argument;

        /// <summary>
        /// Like <see cref="FormattableString.GetArgument(int)"/> but returning the Argument Format
        /// </summary>
        public string? GetArgumentFormat(int index) => _arguments[index].Format;

        /// <summary>
        /// Like <see cref="FormattableString.GetArguments"/>
        /// </summary>
        public override object?[] GetArguments() => _arguments.Select(x => x.Argument).ToArray();

#endregion


#region ctor
        /// <inheritdoc />
        protected InterpolatedStringBuilder(InterpolatedStringBuilderOptions? options, StringBuilder? format, List<InterpolatedStringArgument>? arguments)
        {
            Options = options ?? DefaultOptions;
            _format = format ?? new StringBuilder();
            _arguments = arguments ?? new List<InterpolatedStringArgument>();
        }

        /// <inheritdoc />
        public InterpolatedStringBuilder(InterpolatedStringBuilderOptions? options = null) : this(options: options, format: null, arguments: null)
        {
            // Options can be defined in constructor but can also be set/modified after constructor (e.g. in initializer) - as long as it's set before parsing the first string
        }


        /// <inheritdoc />
        public InterpolatedStringBuilder(FormattableString? value, InterpolatedStringBuilderOptions? options = null) : this(options: options)
        {
            // This constructor gets a FormattableString to be immediately parsed, and therefore it can be useful to provide Options (and Parser) immediately together
            if (value != null)
                Options.Parser.ParseAppend(value, this);
        }

#if NET6_0_OR_GREATER
        /// <inheritdoc />
        public InterpolatedStringBuilder(int literalLength, int formattedCount, InterpolatedStringBuilderOptions? options = null)
        {
            Options = options ?? DefaultOptions;
            _format = new StringBuilder(literalLength);
            _arguments = new List<InterpolatedStringArgument>(formattedCount);
        }
#endif
#endregion

#region AsFormattableString()
        /// <summary>
        /// Casts to a regular FormattableString
        /// </summary>
        public virtual FormattableString AsFormattableString() => (FormattableString)this;
#endregion

#region Append (InterpolatedStringBuilder/FormattableString/+overload)
        /// <summary>
        /// Appends to this instance another InterpolatedString.
        /// Underlying parameters will be appended (merged), and underlying formats will be concatenated (placeholder positions will be shifted to their new positions).
        /// </summary>
        public virtual InterpolatedStringBuilder Append(InterpolatedStringBuilder value)
        {
            return Insert(_format.Length, value);
        }

        /// <summary>
        /// Appends to this instance another InterpolatedString, depending on a condition.
        /// Underlying parameters will be appended (merged), and underlying formats will be concatenated (placeholder positions will be shifted to their new positions).
        /// If condition is false, FormattableString will be parsed/evaluated but won't be appended.
        /// </summary>
        public virtual InterpolatedStringBuilder AppendIf(bool condition, InterpolatedStringBuilder value)
        {
            if (condition)
                Insert(_format.Length, value);
            return this;
        }


        /// <inheritdoc cref="InterpolatedStringBuilder.Append(InterpolatedStringBuilder)"/>
        public static InterpolatedStringBuilder operator +(InterpolatedStringBuilder interpolatedString, InterpolatedStringBuilder value)
        {
            interpolatedString.Append(value);
            return interpolatedString;
        }

        /// <summary>
        /// Appends to this instance another FormattableString.
        /// Uses regular expression for parsing the FormattableString.
        /// Underlying parameters will be appended (merged), and underlying formats will be concatenated (placeholder positions will be shifted to their new positions).
        /// </summary>
        public virtual InterpolatedStringBuilder Append(FormattableString value)
        {
            Options.Parser.ParseAppend(value, this);
            return this;
        }

        /// <summary>
        /// Appends to this instance another FormattableString, depending on a condition.
        /// Uses regular expression for parsing the FormattableString.
        /// Underlying parameters will be appended (merged), and underlying formats will be concatenated (placeholder positions will be shifted to their new positions).
        /// If condition is false, FormattableString won't be parsed or appended.
        /// </summary>
        public virtual InterpolatedStringBuilder AppendIf(bool condition, FormattableString value)
        {
            if (condition)
                Options.Parser.ParseAppend(value, this);
            return this;
        }

        /// <inheritdoc cref="InterpolatedStringBuilder.Append(FormattableString)"/>
        public static InterpolatedStringBuilder operator +(InterpolatedStringBuilder interpolatedString, FormattableString fs)
        {
            interpolatedString.Append(fs);
            return interpolatedString;
        }

#if NET6_0_OR_GREATER
        /// <summary>
        /// Appends to this instance another interpolated string.
        /// Uses InterpolatedStringHandler (net6.0+) which can be a little faster than using regex.
        /// </summary>
        public InterpolatedStringBuilder Append([InterpolatedStringHandlerArgument("")] InterpolatedStringAdapter adapter)
        {
            // InterpolatedStringAdapter will get this InterpolatedStringBuilder instance
            // and will receive the literals/arguments to be appended to this instance.
            return this;
        }

        /// <summary>
        /// Appends to this instance another interpolated string, depending on a condition.
        /// Uses InterpolatedStringHandler (net6.0+) which can be a little faster than using regex.
        /// If condition is false, interpolated string won't be parsed or appended.
        /// </summary>
        public InterpolatedStringBuilder AppendIf(bool condition, [InterpolatedStringHandlerArgument("", "condition")] InterpolatedStringAdapter adapter)
        {
            // InterpolatedStringAdapter will get this InterpolatedStringBuilder instance, and will also get the bool condition.
            // If condition is false, InterpolatedStringAdapter will just early-abort.
            // Else, it will receive the literals/arguments and will append them to this instance.
            return this;
        }
#endif

#endregion

#region AppendLiteral / AppendRaw

        /// <summary>
        /// Appends to this instance a literal string. 
        /// Curly braces will be automatically escaped depending on <see cref="InterpolatedStringBuilderOptions.AutoEscapeCurlyBraces"/>.
        /// </summary>
        public InterpolatedStringBuilder AppendLiteral(string value)
        {
            return AppendLiteral(value, 0, value.Length);
        }

        /// <summary>
        /// Appends to this instance a substring as literal string.
        /// Curly braces will be automatically escaped depending on <see cref="InterpolatedStringBuilderOptions.AutoEscapeCurlyBraces"/>.
        /// </summary>
        public virtual InterpolatedStringBuilder AppendLiteral(string value, int startIndex, int count)
        {
            if (Options.AutoEscapeCurlyBraces == false || value.IndexOfAny(new char[] { '{', '}' }, startIndex, count) == -1)
            {
                _format.Append(value, startIndex, count);
                _cachedFormat = null;
                return this;
            }
            for (int i = 0; i < count; i++)
            {
                switch (value[startIndex + i])
                {
                    case '{':
                        _format.Append("{{");
                        break;
                    case '}':
                        _format.Append("}}");
                        break;
                    default:
                        _format.Append(value[startIndex + i]); break;
                }
            }
            _cachedFormat = null;
            return this;
        }

        /// <summary>
        /// Appends a raw string. 
        /// This is like <see cref="AppendLiteral(string)"/> but it's a little faster since it does not escape curly-braces 
        /// </summary>
        public InterpolatedStringBuilder AppendRaw(string value)
        {
            _format.Append(value);
            return this;
        }

#endregion

#region Insert/Replace/ShiftPlaceholderPositions/ReplacePlaceholderPosition
        /// <summary>
        /// Inserts another InterpolatedString at a specific position. Similar to <see cref="Append(InterpolatedStringBuilder)"/>
        /// </summary>
        public virtual InterpolatedStringBuilder Insert(int index, InterpolatedStringBuilder value)
        {
            int shift = Arguments.Count;
            if (shift > 0)
                Options.Parser.ShiftPlaceholderPositions(value._format, shift);
            // if (index < _format.Length-1) then the placeholder positions might become out of order - but that's fine
            _format.Insert(index, value._format);

            _cachedFormat = null;
            _arguments.AddRange(value.Arguments);
            return this;
        }

        /// <summary>
        /// Inserts another FormattableString at a specific position. Similar to <see cref="Append(FormattableString)"/>
        /// </summary>
        public virtual InterpolatedStringBuilder Insert(int index, FormattableString value)
        {
            var temp = new InterpolatedStringBuilder();
            Options.Parser.ParseAppend(value, temp);
            return Insert(index, temp);
        }

        /// <summary>
        /// Searches for a literal text in the InterpolatedString, and if found it will be replaced by another InterpolatedString (parameters will be merged, placeholder positions will be shifted).
        /// </summary>
        public virtual bool Replace(string oldValue, InterpolatedStringBuilder newValue)
        {
            int index = IndexOf(oldValue, 0, false);
            if (index < 0)
                return false;

            _format.Remove(index, oldValue.Length);
            Insert(index, newValue);
            return true;
        }

        /// <inheritdoc cref="Replace(string, InterpolatedStringBuilder)"/>
        public virtual bool Replace(string oldValue, FormattableString newValue)
        {
            int index = IndexOf(oldValue, 0, false);
            if (index < 0)
                return false;

            _format.Remove(index, oldValue.Length);
            Insert(index, newValue);
            return true;
        }

        // From https://stackoverflow.com/questions/1359948/why-doesnt-stringbuilder-have-indexof-method  - it's a shame that StringBuilder has Replace but does not have IndexOf
        /// <summary>
        /// Returns the index of the start of the contents in a StringBuilder
        /// </summary>        
        /// <param name="value">The string to find</param>
        /// <param name="startIndex">The starting index.</param>
        /// <param name="ignoreCase">if set to <c>true</c> it will ignore case</param>
        public int IndexOf(string value, int startIndex, bool ignoreCase)
        {
            int index;
            int length = value.Length;
            int maxSearchLength = (_format.Length - length) + 1;

            if (ignoreCase)
            {
                for (int i = startIndex; i < maxSearchLength; ++i)
                {
                    if (char.ToLower(_format[i]) == char.ToLower(value[0]))
                    {
                        index = 1;
                        while ((index < length) && (char.ToLower(_format[i + index]) == char.ToLower(value[index])))
                            ++index;

                        if (index == length)
                            return i;
                    }
                }

                return -1;
            }

            for (int i = startIndex; i < maxSearchLength; ++i)
            {
                if (_format[i] == value[0])
                {
                    index = 1;
                    while ((index < length) && (_format[i + index] == value[index]))
                        ++index;

                    if (index == length)
                        return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Removes the specified range of characters from this instance.
        /// </summary>
        public InterpolatedStringBuilder Remove(int startIndex, int length)
        {
            _format.Remove(startIndex, length);
            return this;
        }
#endregion

#region AppendArgument / AddArgument / IsArgumentEqual
        /// <summary>
        /// Appends an argument.
        /// This will both add the argument object to the list of arguments (see <see cref="AddArgument(InterpolatedStringArgument)"/>)
        /// and will also add the placeholder to the format StringBuilder (e.g. "{2}")
        /// </summary>
        protected internal virtual void AppendArgument(object argument, string? format)
        {
            if (format == "raw")
            {
                AppendRaw(argument?.ToString() ?? "");
                return;
            }

            int argumentPos = AddArgument(new InterpolatedStringArgument(argument, format));
            _format.Append("{");
            _format.Append(argumentPos);
            if (Options.PreserveArgumentFormatting  && !string.IsNullOrEmpty(format))
                _format.Append(":").Append(format);
            _format.Append("}");
            _cachedFormat = null;
        }

        /// <summary>
        /// Appends an argument and returns the index of the argument in the list <see cref="Arguments"/>.
        /// This method will only add the argument object to the list of arguments - it will NOT add the placeholder to the format StringBuilder (e.g. "{2}").
        /// Appending parameters and adding them to the underlying StringBuilder is usually done by concatenating or (equivalent) by calling <see cref="AppendArgument(object, string)"/>.
        /// If <see cref="InterpolatedStringBuilderOptions.ReuseIdenticalParameters"/> is true then this method will try to reuse existing parameters, according to 
        /// </summary>
        /// <returns>Position where parameter was added</returns>
        protected internal virtual int AddArgument(InterpolatedStringArgument argument)
        {
            if (Options.ReuseIdenticalParameters && Options.ArgumentComparer != null)
            {
                // Reuse existing parameters (don't pass duplicates)
                for (int i = 0; i < Arguments.Count; i++)
                    if (Options.ArgumentComparer.Equals(Arguments[i], argument))
                        return i;
            }

            _arguments.Add(argument);
            return _arguments.Count - 1;
        }
#endregion

#region ToString
        /// <inheritdoc/>
        public override string ToString()
        {
            return ToString(CultureInfo.InvariantCulture);
        }
        /// <inheritdoc/>
        public override string ToString(IFormatProvider? formatProvider)
        {
            return string.Format(
                formatProvider,
                Format,
                _arguments.Select(arg => arg.Argument is IFormattable ? ((IFormattable)arg.Argument!).ToString(arg.Format!, formatProvider) : arg.Argument).ToArray()
                );
        }
#endregion

#region DefaultOptions
        /// <summary>
        /// Default options used when options is not defined in constructor
        /// </summary>
        public static InterpolatedStringBuilderOptions DefaultOptions { get; set; } = new InterpolatedStringBuilderOptions();
#endregion

    }
}