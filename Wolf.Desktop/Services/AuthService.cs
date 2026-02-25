using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using Wolf.Dtos;

namespace Wolf.Desktop.Services;

public class AuthService : IAuthService
{
    private readonly ApiClient _client;

    public bool IsLoggedIn { get; private set; }
    public string? CurrentToken { get; private set; }
    public string? CurrentUsername { get; private set; }
    public int? CurrentEmployeeId { get; private set; }
    public string CurrentRole { get; private set; } = "User";

    public string CurrentDisplayName =>
        CurrentUsername is not null ? FormatDisplayName(CurrentUsername) : "Guest";

    public string CurrentInitials =>
        CurrentUsername is not null ? MakeInitials(CurrentUsername) : "?";

    public AuthService(ApiClient client)
    {
        _client = client;
    }

    public async Task<bool> LoginAsync(string username, string password)
    {
        try
        {
            var result = await _client.PostAsync<JsonElement>("api/auth/login", new LoginDto
            {
                Username = username,
                Password = password
            });

            var token = result.GetProperty("token").GetString();
            if (string.IsNullOrEmpty(token)) return false;

            _client.SetToken(token);
            IsLoggedIn = true;
            CurrentToken = token;
            CurrentUsername = username;
            CurrentEmployeeId = ParseEmployeeId(token);
            CurrentRole = ParseRole(token);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public void Logout()
    {
        _client.ClearToken();
        IsLoggedIn = false;
        CurrentToken = null;
        CurrentUsername = null;
        CurrentEmployeeId = null;
        CurrentRole = "User";
    }

    private static int? ParseEmployeeId(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            var claim = jwt.Claims.FirstOrDefault(c => c.Type == "employeeid");
            if (claim is not null && int.TryParse(claim.Value, out var id))
                return id;
        }
        catch { }
        return null;
    }

    private static string ParseRole(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            var roleClaim = jwt.Claims.FirstOrDefault(c =>
                c.Type == System.Security.Claims.ClaimTypes.Role || c.Type == "role");
            if (roleClaim is not null)
                return roleClaim.Value;
        }
        catch { }
        return "User";
    }

    private static string FormatDisplayName(string username)
    {
        // Capitalize first letter and limit length
        if (string.IsNullOrWhiteSpace(username)) return username;
        var name = username.Length > 1
            ? char.ToUpper(username[0]) + username[1..]
            : username.ToUpper();
        return name.Length > 20 ? name[..20] + "." : name;
    }

    private static string MakeInitials(string username)
    {
        if (string.IsNullOrWhiteSpace(username)) return "?";
        var parts = username.Split([' ', '.', '_', '-'], StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 2)
            return $"{char.ToUpper(parts[0][0])}{char.ToUpper(parts[1][0])}";
        return username.Length >= 2
            ? username[..2].ToUpper()
            : username[..1].ToUpper();
    }
}
