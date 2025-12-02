using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using KriptoProyek.Models;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly JwtService _jwtService;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        JwtService jwtService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtService = jwtService;
    }

    // REGISTER - Daftar user baru
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterDto model)
    {
        // Cek email sudah terdaftar
        var existingUser = await _userManager.FindByEmailAsync(model.Email);
        if (existingUser != null)
        {
            return BadRequest(new { message = "Email sudah terdaftar" });
        }

        // Buat user baru
        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FullName = model.FullName
        };

        // Password otomatis di-hash oleh Identity
        var result = await _userManager.CreateAsync(user, model.Password);

        if (!result.Succeeded)
        {
            return BadRequest(new { errors = result.Errors });
        }

        // Assign default role "User"
        await _userManager.AddToRoleAsync(user, "User");

        return Ok(new { message = "Registrasi berhasil" });
    }

    // LOGIN - Masuk ke aplikasi
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginDto model)
    {
        // Cari user berdasarkan email
        var user = await _userManager.FindByEmailAsync(model.Email);
        
        if (user == null)
        {
            return Unauthorized(new { message = "Email atau password salah" });
        }

        // Cek apakah akun terkunci (brute force protection)
        if (await _userManager.IsLockedOutAsync(user))
        {
            return Unauthorized(new { message = "Akun terkunci karena terlalu banyak percobaan login gagal" });
        }

        // Verifikasi password
        var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: true);

        if (!result.Succeeded)
        {
            return Unauthorized(new { message = "Email atau password salah" });
        }

        // Generate JWT token
        var roles = await _userManager.GetRolesAsync(user);
        var token = _jwtService.GenerateToken(user, roles);

        return Ok(new
        {
            token,
            email = user.Email,
            fullName = user.FullName,
            roles
        });
    }

    // GET PROFILE - Info user yang login
    [HttpGet("profile")]
    [Authorize]
    public async Task<IActionResult> GetProfile()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
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

    // CHANGE PASSWORD - Ganti password
    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto model)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
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

        return Ok(new { message = "Password berhasil diubah" });
    }

    // LOGOUT - Keluar (untuk JWT biasanya cukup hapus token di client)
    [HttpPost("logout")]
    [Authorize]
    public IActionResult Logout()
    {
        // Untuk JWT, logout dilakukan di client side dengan menghapus token
        // Server tidak perlu menyimpan blacklist token untuk aplikasi sederhana
        return Ok(new { message = "Logout berhasil, hapus token di client" });
    }
}