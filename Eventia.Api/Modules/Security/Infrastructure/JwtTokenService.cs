using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Modules.Security.Application;

public class JwtTokenService : IJwtTokenService
{
    private readonly string _key;

    public JwtTokenService(string key) => _key = key;

    public string Issue(int userId, string name, IEnumerable<string> roles, IEnumerable<string> permissions)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, name)
        };

        // Roles como claims
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        // Permisos como claims personalizados
        claims.AddRange(permissions.Select(p => new Claim("permissions", p)));

        var creds = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key)),
            SecurityAlgorithms.HmacSha256
        );

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
