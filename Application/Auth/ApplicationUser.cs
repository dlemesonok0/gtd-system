namespace Domain.Auth;

public class ApplicationUser
{
    public Guid Id { get; set; } =  Guid.NewGuid();
    public string Email { get; set; }
    public string PasswordHash { get; set; }
}
