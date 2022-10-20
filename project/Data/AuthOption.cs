using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.IdentityModel.Tokens;

#pragma warning disable CS1591

namespace JustBuyApi.Data;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public static class AuthOption
{
    public const string ISSUER = "JustBuyServer"; // издатель токена
    public const string AUDIENCE = "http://localhost:7299/"; // потребитель токена
    private const string KEY = "mysupersecret_secretkey!123";   // ключ для шифрации
    public static SymmetricSecurityKey GetSymmetricSecurityKey() => new(Encoding.UTF8.GetBytes(KEY));
}