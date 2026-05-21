using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using RagQnA.Contracts.Options;

namespace RagQnA.Infrastructure.Security;

public sealed class QStashSignatureVerifier
{
    private readonly string _currentKey;
    private readonly string _nextKey;

    public QStashSignatureVerifier(IOptions<QStashOptions> options)
    {
        _currentKey = options.Value.CurrentSigningKey;
        _nextKey = options.Value.NextSigningKey;
    }

    /// <summary>
    /// Verifies the Upstash-Signature JWT. Returns false if invalid or expired.
    /// Tries the current signing key first, then the next key (rotation support).
    /// </summary>
    public bool Verify(string jwt, string rawBody, string requestUrl)
    {
        return TryVerifyWithKey(jwt, rawBody, requestUrl, _currentKey)
            || TryVerifyWithKey(jwt, rawBody, requestUrl, _nextKey);
    }

    private static bool TryVerifyWithKey(string jwt, string rawBody, string requestUrl, string signingKey)
    {
        if (string.IsNullOrEmpty(signingKey)) return false;

        try
        {
            var handler = new JwtSecurityTokenHandler();

            // QStash signs with HMAC-SHA256 using the signing key as secret
            var keyBytes = Encoding.UTF8.GetBytes(signingKey);
            var validationParams = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(keyBytes),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromSeconds(30)
            };

            var principal = handler.ValidateToken(jwt, validationParams, out var validatedToken);

            // Verify the body hash claim
            var bodyHash = ComputeSha256(rawBody);
            var bodyClaim = principal.FindFirst("body")?.Value;
            if (bodyClaim != null && !string.Equals(bodyClaim, bodyHash, StringComparison.OrdinalIgnoreCase))
                return false;

            return true;
        }
        catch
        {
            return false;
        }
    }

    private static string ComputeSha256(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(bytes);
    }
}
