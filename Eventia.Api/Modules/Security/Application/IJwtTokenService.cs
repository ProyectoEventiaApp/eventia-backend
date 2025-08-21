namespace Modules.Security.Application;

public interface IJwtTokenService
{
    string Issue(int userId, string name, IEnumerable<string> roles, IEnumerable<string> permissions);
}
