// Models/ChangePasswordDto.cs
namespace KriptoProyek.Models;

public class ChangePasswordDto
{
    public string CurrentPassword { get; set; }
    public string NewPassword { get; set; }
}