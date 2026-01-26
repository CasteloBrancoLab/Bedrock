namespace Bedrock.BuildingBlocks.Persistence.Abstractions.Utils;

public static class IdUtils
{
    public static Guid HexIdToGuid(string hexId)
    {
        ArgumentException.ThrowIfNullOrEmpty(hexId);

        if (hexId.Length != 24)
        {
            throw new ArgumentException("O ID hexadecimal deve conter exatamente 24 caracteres.", nameof(hexId));
        }

        Span<byte> guidBytes = stackalloc byte[16];
        for (int i = 0; i < 12; i++)
        {
            int hi = GetHexVal(hexId[i * 2]);
            int lo = GetHexVal(hexId[i * 2 + 1]);
            if (hi == -1 || lo == -1)
            {
                throw new FormatException("Hexadecimal inválido.");
            }

            guidBytes[i] = (byte)((hi << 4) | lo);
        }

        guidBytes[12] = guidBytes[13] = guidBytes[14] = guidBytes[15] = 0;

        return new Guid(guidBytes);
    }

    public static string GuidToHexId(Guid value)
    {
        Span<byte> guidBytes = stackalloc byte[16];
        _ = value.TryWriteBytes(guidBytes);

        for (int i = 12; i < 16; i++)
        {
            if (guidBytes[i] != 0)
            {
                throw new ArgumentException("O Guid fornecido não pode ser convertido para um ID hexadecimal.", nameof(value));
            }
        }

        return Convert.ToHexStringLower(guidBytes[..12]);
    }

    private static int GetHexVal(char hex)
    {
        int val = hex;
        // For uppercase A-F letters:
        if (val is >= '0' and <= '9')
        {
            return val - '0';
        }

        if (val is >= 'a' and <= 'f')
        {
            return val - 'a' + 10;
        }

        if (val is >= 'A' and <= 'F')
        {
            return val - 'A' + 10;
        }

        return -1;
    }
}
