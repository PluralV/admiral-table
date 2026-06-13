using System.Security.Cryptography;
using System.Text;

namespace AdmiralsTable.Domain.Common;

/// <summary>
/// Generates record identifiers. Per spec, an id is the SHA-256 hash of the
/// record's name concatenated with 8 random bytes, rendered as a lowercase hex string.
/// </summary>
public static class Ids
{
    private const int RandomByteCount = 8;

    public static string New(string name)
    {
        byte[] nameBytes = Encoding.UTF8.GetBytes(name ?? string.Empty);
        byte[] random = new byte[RandomByteCount];
        RandomNumberGenerator.Fill(random);

        byte[] buffer = new byte[nameBytes.Length + random.Length];
        Buffer.BlockCopy(nameBytes, 0, buffer, 0, nameBytes.Length);
        Buffer.BlockCopy(random, 0, buffer, nameBytes.Length, random.Length);

        return Convert.ToHexString(SHA256.HashData(buffer)).ToLowerInvariant();
    }
}
