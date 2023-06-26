using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace InterpolatedStrings
{
    /// <summary>
    /// Parses FormattableString into <see cref="InterpolatedStringBuilder"/> using regex.
    /// </summary>
    public interface IInterpolatedStringParser
    {
        /// <summary>
        /// Parses a FormattableString and Appends it to an existing <see cref="InterpolatedStringBuilder"/>
        /// </summary>
        void ParseAppend(FormattableString value, InterpolatedStringBuilder target);

        /// <summary>
        /// When a FormattableString is appended to an existing InterpolatedString, 
        /// the underlying format (where there are numeric placeholders) needs to be shifted because the arguments will have new positions in the final array
        /// This method is used to shift a format by a number of positions.
        /// </summary>
        void ShiftPlaceholderPositions(StringBuilder format, int shift);
    }

    /// <summary>
    /// Parses FormattableString into <see cref="InterpolatedStringBuilder"/>
    /// </summary>
    public class InterpolatedStringParser : IInterpolatedStringParser
    {
        #region statics/constants
        /// <summary>
        /// Regex to parse FormattableString
        /// </summary>
        internal static Regex _formattableArgumentRegex = new Regex(
              "{(?<ArgPos>\\d*)(:(?<Format>[^}]*))?}",
            RegexOptions.IgnoreCase
            | RegexOptions.Singleline
            | RegexOptions.CultureInvariant
            | RegexOptions.IgnorePatternWhitespace
            | RegexOptions.Compiled
            );

        /// <summary>
        /// Identify all types of line-breaks
        /// </summary>
        protected static readonly Regex _lineBreaksRegex = new Regex(@"(\r\n|\n|\r)", RegexOptions.Compiled);
        #endregion

        #region Members
        /// <summary>
        /// This is for legacy compatibility - it adjusts blocks similarly to what is currently available with C#11 Raw String Literals
        /// <see href="https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-11.0/raw-string-literal" />
        /// </summary>
        public bool AutoAdjustMultilineString { get; set; } = false;
        #endregion

        #region ctor
        /// <inheritdoc />
        public InterpolatedStringParser()
        {
        }
        #endregion

        #region ParseTo
#if NET6_0_OR_GREATER
        /// <summary>
        /// Parses a FormattableString (using <see cref="InterpolatedStringAdapter"/> handler) and Appends it to an existing <see cref="InterpolatedStringBuilder"/>.
        /// </summary>
        public virtual void ParseAppend(InterpolatedStringAdapter value, InterpolatedStringBuilder target)
        {
            target.Append(value.InterpolatedStringBuilder);
        }
#endif

        /// <summary>
        /// Parses a FormattableString and Appends it to an existing <see cref="InterpolatedStringBuilder"/>
        /// </summary>
        public virtual void ParseAppend(FormattableString value, InterpolatedStringBuilder target)
        {
            if (value == null || string.IsNullOrEmpty(value.Format))
                return;
            object?[] arguments = value.GetArguments();
            if (arguments == null || arguments.Length == 0)
            {
                target.AppendLiteral(value.Format);
                return;
            }

            string format = value.Format;

            if (AutoAdjustMultilineString)
                format = AdjustMultilineString(format);


            // Regex will find all placeholders, and iterate through the string processing the placeholders and the blocks before and after it
            // E.g. block, placeholder, block, placeholder, lastBlock
            var matches = _formattableArgumentRegex.Matches(format);
            int currentPos = 0;
            for (int i = 0; i < matches.Count; i++)
            {
                int literalStart = currentPos; // previous pointer
                int literalEnd = matches[i].Index; // position of next placeholder 

                //string block = format.Substring(currentPos, matches[i].Index - currentPos);
                currentPos = matches[i].Index + matches[i].Length;

                // arguments[i] may not work because same argument can be used multiple times
                int argPos = int.Parse(matches[i].Groups["ArgPos"].Value);
                var formatMatch = matches[i].Groups["Format"];
                string? argumentFormat = formatMatch.Success ? formatMatch.Value : null;
                object argument = arguments[argPos]!;

                AppendLiteral(target, format, literalStart, literalEnd - literalStart);
                AppendArgument(target, argument, argumentFormat);
            }
            // Last literal
            AppendLiteral(target, format, currentPos, format.Length - currentPos);
        }

        /// <summary></summary>
        protected virtual void AppendLiteral(InterpolatedStringBuilder target, string value, int startIndex, int count)
        {
            target.AppendLiteral(value, startIndex, count);
        }

        /// <summary></summary>
        protected virtual void AppendArgument(InterpolatedStringBuilder target, object argument, string? argumentFormat)
        {
            // If we get a nested InterpolatedStringBuilder, it's already parsed - we just merge to current one
            if (argument is InterpolatedStringBuilder isbArg)
            {
                target.Append(isbArg); // this will automatically shift the arguments
                return;
            }

            // If we get a nested FormattableString, we parse it (recursively) and merge it to current one
            if (argument is FormattableString fsArg)
            {
                var nestedStatement = new InterpolatedStringBuilder();
                ParseAppend(fsArg, nestedStatement);
                target.Append(nestedStatement); // this will automatically shift the arguments
                return;
            }

            // Else, just keep the same {argPos} (e.g. "{2}")
            target.AppendArgument(argument, argumentFormat);
        }
        #endregion

        #region ShiftPlaceholderPositions
        /// <inheritdoc cref="IInterpolatedStringParser.ShiftPlaceholderPositions(StringBuilder, int)"/>
        public void ShiftPlaceholderPositions(StringBuilder format, int shift)
        {
            if (shift == 0)
                return;
            string newFormat = InterpolatedStringParser._formattableArgumentRegex.Replace(format.ToString(), match => ReplacePlaceholderPosition(match, shift));
            format.Clear().Append(newFormat);
            //TODO: micro optimize this with foreach and manually replacing each placeholder?
        }
        /// <summary>
        /// When a FormattableString is appended to an existing InterpolatedString, 
        /// the underlying format (where there are numeric placeholders) needs to be shifted because the arguments will have new positions in the final array
        /// </summary>
        protected virtual string ReplacePlaceholderPosition(Match match, int shift)
        {
            Group parm = match.Groups[2];
            int newPos = int.Parse(parm.Value) + shift;
            string replace = newPos.ToString();
            string newPlaceholder = string.Format("{0}{1}{2}", match.Value.Substring(0, parm.Index - match.Index), replace, match.Value.Substring(parm.Index - match.Index + parm.Length));
            return newPlaceholder;
        }

        #endregion

        #region Multi-line blocks can be conveniently used with any indentation, and we will correctly adjust the indentation of those blocks (TrimLeftPadding and TrimFirstEmptyLine)
        /// <summary>
        /// Given a text block (multiple lines), this removes the left padding of the block, by calculating the minimum number of spaces which happens in EVERY line.
        /// Then, other methods writes the lines one by one, which in case will respect the current indent of the writer.
        /// This is legacy (backwards compatibility), but new code should just use C# Raw String Literals which will do the same (remove block indentation and first empty line)
        /// </summary>
        protected string AdjustMultilineString(string block)
        {
            // copied from https://github.com/CodegenCS/CodegenCS/

            string[] parts = _lineBreaksRegex.Split(block);
            if (parts.Length <= 1) // no linebreaks at all
                return block;
            var nonEmptyLines = parts.Where(line => line.TrimEnd().Length > 0).ToList();
            if (nonEmptyLines.Count <= 1) // if there's not at least 2 non-empty lines, assume that we don't need to adjust anything
                return block;

            Match m = _lineBreaksRegex.Match(block);
            if (m != null && m.Success && m.Index == 0)
            {
                block = block.Substring(m.Length); // remove first empty line
                parts = _lineBreaksRegex.Split(block);
                nonEmptyLines = parts.Where(line => line.TrimEnd().Length > 0).ToList();
            }


            int minNumberOfSpaces = nonEmptyLines.Select(nonEmptyLine => nonEmptyLine.Length - nonEmptyLine.TrimStart().Length).Min();

            StringBuilder sb = new StringBuilder();

            var matches = _lineBreaksRegex.Matches(block);
            int lastPos = 0;
            for (int i = 0; i < matches.Count; i++)
            {
                string line = block.Substring(lastPos, matches[i].Index - lastPos);
                string lineBreak = block.Substring(matches[i].Index, matches[i].Length);
                lastPos = matches[i].Index + matches[i].Length;

                sb.Append(line.Substring(Math.Min(line.Length, minNumberOfSpaces)));
                sb.Append(lineBreak);
            }
            string lastLine = block.Substring(lastPos);
            sb.Append(lastLine.Substring(Math.Min(lastLine.Length, minNumberOfSpaces)));

            return sb.ToString();
        }
        #endregion


    }
}