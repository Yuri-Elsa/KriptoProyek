using Microsoft.EntityFrameworkCore;
using KriptoProyek.Models;
using KriptoProyek.Data;

namespace KriptoProyek.Services;

public class TokenService
{
    private readonly ApplicationDbContext _context;

    public TokenService(ApplicationDbContext context)
    {
        _context = context;
    }

    // Simpan token baru dan revoke token lama
    public async Task<UserToken> CreateTokenAsync(string userId, string token, int expiryMinutes, string? deviceInfo = null, string? ipAddress = null)
    {
        // Revoke semua token aktif user ini
        var activeTokens = await _context.RefreshTokens
            .Where(t => t.UserId == userId && !t.IsRevoked && t.ExpiresAt > DateTime.UtcNow)
            .ToListAsync();

        foreach (var oldToken in activeTokens)
        {
            oldToken.IsRevoked = true;
        }

        // Buat token baru
        var userToken = new UserToken
        {
            UserId = userId,
            Token = token,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes),
            IsRevoked = false,
            DeviceInfo = deviceInfo,
            IpAddress = ipAddress
        };

        _context.RefreshTokens.Add(userToken);
        await _context.SaveChangesAsync();

        return userToken;
    }

    // Validasi apakah token masih valid
    public async Task<bool> IsTokenValidAsync(string token)
    {
        var userToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == token);

        if (userToken == null)
            return false;

        // Cek apakah token sudah direvoke atau expired
        if (userToken.IsRevoked || userToken.ExpiresAt <= DateTime.UtcNow)
            return false;

        return true;
    }

    // Revoke token (untuk logout)
    public async Task<bool> RevokeTokenAsync(string token)
    {
        var userToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == token);

        if (userToken == null)
            return false;

        userToken.IsRevoked = true;
        await _context.SaveChangesAsync();

        return true;
    }

    // Revoke semua token user (untuk logout semua device)
    public async Task RevokeAllUserTokensAsync(string userId)
    {
        var userTokens = await _context.RefreshTokens
            .Where(t => t.UserId == userId && !t.IsRevoked)
            .ToListAsync();

        foreach (var token in userTokens)
        {
            token.IsRevoked = true;
        }

        await _context.SaveChangesAsync();
    }

    // Hapus token yang sudah expired (cleanup)
    public async Task CleanupExpiredTokensAsync()
    {
        var expiredTokens = await _context.RefreshTokens
            .Where(t => t.ExpiresAt <= DateTime.UtcNow)
            .ToListAsync();

        _context.RefreshTokens.RemoveRange(expiredTokens);
        await _context.SaveChangesAsync();
    }

    // Get active tokens untuk user (untuk monitoring)
    public async Task<List<UserToken>> GetActiveUserTokensAsync(string userId)
    {
        return await _context.RefreshTokens
            .Where(t => t.UserId == userId && !t.IsRevoked && t.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }
}