using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using KriptoProyek.Models;
using KriptoProyek.Services;

namespace KriptoProyek.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly JwtService _jwtService;
    private readonly TokenService _tokenService;
    private readonly IConfiguration _configuration;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        JwtService jwtService,
        TokenService tokenService,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtService = jwtService;
        _tokenService = tokenService;
        _configuration = configuration;
    }

    // REGISTER
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterDto model)
    {
        var existingUser = await _userManager.FindByEmailAsync(model.Email);
        if (existingUser != null)
        {
            return BadRequest(new { message = "Email sudah terdaftar" });
        }

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FullName = model.FullName,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (!result.Succeeded)
        {
            return BadRequest(new { errors = result.Errors });
        }

        await _userManager.AddToRoleAsync(user, "User");

        return Ok(new { message = "Registrasi berhasil" });
    }

    // LOGIN
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginDto model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        
        if (user == null)
        {
            return Unauthorized(new { message = "Email atau password salah" });
        }

        if (await _userManager.IsLockedOutAsync(user))
        {
            return Unauthorized(new { message = "Akun terkunci karena terlalu banyak percobaan login gagal" });
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: true);

        if (!result.Succeeded)
        {
            return Unauthorized(new { message = "Email atau password salah" });
        }

        // Generate JWT token
        var roles = await _userManager.GetRolesAsync(user);
        var token = _jwtService.GenerateToken(user, roles);

        // Simpan token ke database dan revoke token lama
        var expiryMinutes = Convert.ToInt32(_configuration["JwtSettings:ExpiryMinutes"]);
        var deviceInfo = Request.Headers["User-Agent"].ToString();
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        await _tokenService.CreateTokenAsync(user.Id, token, expiryMinutes, deviceInfo, ipAddress);

        return Ok(new
        {
            token,
            email = user.Email,
            fullName = user.FullName,
            roles,
            message = "Login berhasil. Sesi lama telah dibatalkan."
        });
    }

    // GET PROFILE
    [HttpGet("profile")]
    [Authorize]
    public async Task<IActionResult> GetProfile()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "Token tidak valid" });
        }
        
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return NotFound(new { message = "User tidak ditemukan" });
        }

        var roles = await _userManager.GetRolesAsync(user);

        return Ok(new
        {
            email = user.Email,
            fullName = user.FullName,
            createdAt = user.CreatedAt,
            roles
        });
    }

    // CHANGE PASSWORD
    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto model)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "Token tidak valid" });
        }
        
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return NotFound(new { message = "User tidak ditemukan" });
        }

        var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

        if (!result.Succeeded)
        {
            return BadRequest(new { errors = result.Errors });
        }

        // Revoke semua token setelah ganti password
        await _tokenService.RevokeAllUserTokensAsync(userId);

        return Ok(new { message = "Password berhasil diubah. Silakan login kembali." });
    }

    // LOGOUT
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        
        await _tokenService.RevokeTokenAsync(token);

        return Ok(new { message = "Logout berhasil" });
    }

    // LOGOUT ALL DEVICES
    [HttpPost("logout-all")]
    [Authorize]
    public async Task<IActionResult> LogoutAll()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "Token tidak valid" });
        }
        
        await _tokenService.RevokeAllUserTokensAsync(userId);

        return Ok(new { message = "Logout dari semua device berhasil" });
    }

    // GET ACTIVE SESSIONS (bonus feature)
    [HttpGet("sessions")]
    [Authorize]
    public async Task<IActionResult> GetActiveSessions()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "Token tidak valid" });
        }
        
        var sessions = await _tokenService.GetActiveUserTokensAsync(userId);

        var result = sessions.Select(s => new
        {
            deviceInfo = s.DeviceInfo,
            ipAddress = s.IpAddress,
            createdAt = s.CreatedAt,
            expiresAt = s.ExpiresAt
        });

        return Ok(result);
    }
}