using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace JustBuyApi.Data;

public class AuthOption
{
    public const string ISSUER = "JustBuyServer"; // издатель токена
    public const string AUDIENCE = "http://localhost:7299/"; // потребитель токена
    public const string KEY = "mysupersecret_secretkey!123";   // ключ для шифрации
    public static SymmetricSecurityKey GetSymmetricSecurityKey() => new(Encoding.UTF8.GetBytes(KEY));
}