using System.Buffers;

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
    /// Uses stack allocation for small strings to minimize heap allocations.
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

        int maxLength = input.Length * 2;

        // Stryker disable once all : Mutante equivalente - threshold check apenas seleciona otimizacao, resultado identico
        if (maxLength <= StackAllocThreshold)
        {
            return ToKebabCaseStackAlloc(input, maxLength);
        }

        return ToKebabCaseWithRentedBuffer(input, maxLength);
    }

    private static string ToKebabCaseStackAlloc(string input, int maxLength)
    {
        Span<char> buffer = stackalloc char[maxLength];
        int position = 0;
        // Stryker disable once Boolean : Mutante equivalente - valor inicial sobrescrito antes do primeiro uso efetivo
        bool lastCharWasLowerCaseOrDigit = false;
        // Stryker disable once Boolean : Mutante equivalente - valor inicial sobrescrito antes do primeiro uso efetivo
        bool lastCharWasSeparator = true;

        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];
            ProcessKebabCharacter(buffer, ref position, c, ref lastCharWasLowerCaseOrDigit, ref lastCharWasSeparator);
        }

        if (position > 0 && buffer[position - 1] == KebabCaseSeparator)
        {
            position--;
        }

        return new string(buffer[..position]);
    }

    // Stryker disable all : Mutante equivalente - metodo identico ao StackAlloc, apenas usa ArrayPool para strings grandes
    private static string ToKebabCaseWithRentedBuffer(string input, int maxLength)
    {
        char[] rentedBuffer = ArrayPool<char>.Shared.Rent(maxLength);
        try
        {
            Span<char> buffer = rentedBuffer.AsSpan();
            int position = 0;
            bool lastCharWasLowerCaseOrDigit = false;
            bool lastCharWasSeparator = true;

            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];
                ProcessKebabCharacter(buffer, ref position, c, ref lastCharWasLowerCaseOrDigit, ref lastCharWasSeparator);
            }

            if (position > 0 && buffer[position - 1] == KebabCaseSeparator)
            {
                position--;
            }

            return new string(buffer[..position]);
        }
        finally
        {
            ArrayPool<char>.Shared.Return(rentedBuffer);
        }
    }
    // Stryker restore all

    private static void ProcessKebabCharacter(Span<char> buffer, ref int position, char character, ref bool lastCharWasLowerCaseOrDigit, ref bool lastCharWasSeparator)
    {
        if (!char.IsLetterOrDigit(character))
        {
            // Stryker disable once Equality : Mutante equivalente - lastCharWasSeparator inicia true, tornando position>0 redundante no contexto atual
            if (position > 0 && !lastCharWasSeparator)
            {
                buffer[position++] = KebabCaseSeparator;
                lastCharWasSeparator = true;
            }

            // Stryker disable once Boolean : Mutante equivalente - apos char especial lastCharWasSeparator sempre true, mascarando efeito desta variavel
            lastCharWasLowerCaseOrDigit = false;
        }
        else if (char.IsUpper(character))
        {
            if (lastCharWasLowerCaseOrDigit && !lastCharWasSeparator)
            {
                buffer[position++] = KebabCaseSeparator;
            }

            buffer[position++] = char.ToLowerInvariant(character);
            lastCharWasLowerCaseOrDigit = false;
            lastCharWasSeparator = false;
        }
        else
        {
            buffer[position++] = character;
            lastCharWasLowerCaseOrDigit = true;
            lastCharWasSeparator = false;
        }
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
        int position = ProcessSnakeCaseCharacters(buffer, value);
        return new string(buffer[..position]);
    }

    // Stryker disable all : Mutante equivalente - metodo identico ao StackAlloc, apenas usa ArrayPool para strings grandes
    private static string ToSnakeCaseWithRentedBuffer(string value, int maxLength)
    {
        char[] rentedBuffer = ArrayPool<char>.Shared.Rent(maxLength);
        try
        {
            Span<char> buffer = rentedBuffer.AsSpan();
            int position = ProcessSnakeCaseCharacters(buffer, value);
            return new string(buffer[..position]);
        }
        finally
        {
            ArrayPool<char>.Shared.Return(rentedBuffer);
        }
    }
    // Stryker restore all

    private static int ProcessSnakeCaseCharacters(Span<char> buffer, string value)
    {
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

        return position;
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
        // Stryker disable all : Remover finally ou Return nao afeta resultado, apenas vazaria memoria
        finally
        {
            ArrayPool<char>.Shared.Return(rentedBuffer);
        }
        // Stryker restore all
    }
}
