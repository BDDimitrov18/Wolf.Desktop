namespace Wolf.Desktop.Services;

public interface IAuthService
{
    Task<bool> LoginAsync(string username, string password);
    void Logout();

    bool IsLoggedIn { get; }
    string? CurrentToken { get; }
    string? CurrentUsername { get; }
    int? CurrentEmployeeId { get; }
    string CurrentDisplayName { get; }
    string CurrentInitials { get; }
    string CurrentRole { get; }
}
