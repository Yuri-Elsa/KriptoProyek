namespace KriptoProyek.Models;

public class UserToken
{
    public int Id { get; set; }
    public required string UserId { get; set; }
    public required string Token { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
    public string? DeviceInfo { get; set; } // Optional: info device
    public string? IpAddress { get; set; } // Optional: IP address
    
    // Navigation property
    public ApplicationUser? User { get; set; }
}