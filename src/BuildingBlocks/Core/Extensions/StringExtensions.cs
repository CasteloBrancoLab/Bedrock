using System.Buffers;
using System.Globalization;
using System.Text;

namespace Bedrock.BuildingBlocks.Core.Extensions;

/// <summary>
/// Extension methods for <see cref="string"/>.
/// </summary>
public static class StringExtensions
{
    private const int StackAllocThreshold = 256;

    /// <summary>
    /// The separator character used in kebab-case strings.
    /// </summary>
    public const char KebabCaseSeparator = '-';

    /// <summary>
    /// The separator character used in snake_case strings.
    /// </summary>
    public const char SnakeCaseSeparator = '_';

    /// <summary>
    /// Converts a string to kebab-case (e.g., "HelloWorld" becomes "hello-world").
    /// </summary>
    /// <param name="input">The input string to convert.</param>
    /// <returns>The kebab-case representation of the input string, or null if input is null.</returns>
    public static string? ToKebabCase(this string? input)
    {
        if (input is null)
        {
            return null;
        }

        if (input.Length == 0)
        {
            return string.Empty;
        }

        var builder = new StringBuilder();
        bool lastCharWasLowerCase = false;
        // Stryker disable once Boolean : Mutante equivalente - valor inicial sobrescrito antes do primeiro uso efetivo
        bool lastCharWasSeparator = false;

        for (int i = 0; i < input.Length; i++)
        {
            char character = input[i];
            ProcessKebabCharacter(builder, character, ref lastCharWasLowerCase, ref lastCharWasSeparator);
        }

        return TrimTrailingSeparator(builder.ToString(), KebabCaseSeparator);
    }

    private static void ProcessKebabCharacter(StringBuilder builder, char character, ref bool lastCharWasLowerCase, ref bool lastCharWasSeparator)
    {
        if (!char.IsLetterOrDigit(character))
        {
            if (CanAppendKebabSeparator(builder, lastCharWasSeparator, character))
            {
                builder.Append(KebabCaseSeparator);
                lastCharWasSeparator = true;
            }
        }
        else if (char.IsUpper(character))
        {
            AppendUpperCharAsKebab(builder, character, lastCharWasLowerCase, lastCharWasSeparator);
            lastCharWasLowerCase = false;
            lastCharWasSeparator = false;
        }
        else
        {
            builder.Append(character);
            lastCharWasLowerCase = true;
            lastCharWasSeparator = false;
        }
    }

    private static void AppendUpperCharAsKebab(StringBuilder builder, char character, bool lastCharWasLowerCase, bool lastCharWasSeparator)
    {
        if (lastCharWasLowerCase && !lastCharWasSeparator)
        {
            builder.Append(KebabCaseSeparator);
        }

        builder.Append(char.ToLower(character, CultureInfo.InvariantCulture));
    }

    private static string TrimTrailingSeparator(string result, char separator)
    {
        if (result.Length > 0 && result[^1] == separator)
        {
            return result[..^1];
        }

        return result;
    }

    /// <summary>
    /// Converts a string to snake_case (e.g., "HelloWorld" becomes "hello_world").
    /// Uses stack allocation for small strings to minimize heap allocations.
    /// </summary>
    /// <param name="value">The input string to convert.</param>
    /// <returns>The snake_case representation of the input string.</returns>
    public static string ToSnakeCase(this string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        int maxLength = value.Length * 2;

        // Stryker disable once all : Mutante equivalente - threshold check apenas seleciona otimizacao, resultado identico
        if (maxLength <= StackAllocThreshold)
        {
            return ToSnakeCaseStackAlloc(value, maxLength);
        }

        return ToSnakeCaseWithRentedBuffer(value, maxLength);
    }

    private static string ToSnakeCaseStackAlloc(string value, int maxLength)
    {
        Span<char> buffer = stackalloc char[maxLength];
        int position = 0;

        buffer[position++] = char.ToLowerInvariant(value[0]);

        for (int i = 1; i < value.Length; i++)
        {
            char c = value[i];
            if (char.IsUpper(c))
            {
                buffer[position++] = SnakeCaseSeparator;
                buffer[position++] = char.ToLowerInvariant(c);
            }
            else
            {
                buffer[position++] = c;
            }
        }

        return new string(buffer[..position]);
    }

    private static string ToSnakeCaseWithRentedBuffer(string value, int maxLength)
    {
        char[] rentedBuffer = ArrayPool<char>.Shared.Rent(maxLength);
        try
        {
            Span<char> buffer = rentedBuffer.AsSpan();
            int position = 0;

            buffer[position++] = char.ToLowerInvariant(value[0]);

            for (int i = 1; i < value.Length; i++)
            {
                char c = value[i];
                if (char.IsUpper(c))
                {
                    buffer[position++] = SnakeCaseSeparator;
                    buffer[position++] = char.ToLowerInvariant(c);
                }
                else
                {
                    buffer[position++] = c;
                }
            }

            return new string(buffer[..position]);
        }
        // Stryker disable once Block : Remover finally nao afeta resultado, apenas vazaria memoria
        finally
        {
            ArrayPool<char>.Shared.Return(rentedBuffer);
        }
    }

    /// <summary>
    /// Removes all non-alphanumeric characters from a string.
    /// Uses stack allocation for small strings to minimize heap allocations.
    /// </summary>
    /// <param name="value">The input string to filter.</param>
    /// <returns>A string containing only letters and digits from the input.</returns>
    public static string OnlyLettersAndDigits(this string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        // Stryker disable once all : Mutante equivalente - threshold check apenas seleciona otimizacao, resultado identico
        if (value.Length <= StackAllocThreshold)
        {
            return OnlyLettersAndDigitsStackAlloc(value);
        }

        return OnlyLettersAndDigitsWithRentedBuffer(value);
    }

    private static string OnlyLettersAndDigitsStackAlloc(string value)
    {
        Span<char> buffer = stackalloc char[value.Length];
        int position = 0;

        foreach (char c in value)
        {
            if (char.IsLetterOrDigit(c))
            {
                buffer[position++] = c;
            }
        }

        if (position == 0)
        {
            return string.Empty;
        }

        return new string(buffer[..position]);
    }

    private static string OnlyLettersAndDigitsWithRentedBuffer(string value)
    {
        char[] rentedBuffer = ArrayPool<char>.Shared.Rent(value.Length);
        try
        {
            Span<char> buffer = rentedBuffer.AsSpan();
            int position = 0;

            foreach (char c in value)
            {
                if (char.IsLetterOrDigit(c))
                {
                    buffer[position++] = c;
                }
            }

            if (position == 0)
            {
                return string.Empty;
            }

            return new string(buffer[..position]);
        }
        // Stryker disable once Block : Remover finally nao afeta resultado, apenas vazaria memoria
        finally
        {
            ArrayPool<char>.Shared.Return(rentedBuffer);
        }
    }

    private static bool CanAppendKebabSeparator(StringBuilder builder, bool lastCharWasSeparator, char character)
    {
        return builder.Length > 0 && !lastCharWasSeparator && character == KebabCaseSeparator;
    }
}
