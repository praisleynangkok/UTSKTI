using System;
using System.ComponentModel.DataAnnotations;


namespace SampleSecureWeb.ViewModels
{

public class ChangePasswordViewModel
{
    [Required(ErrorMessage = "Kata sandi saat ini diperlukan.")]
    [DataType(DataType.Password)]
    public string CurrentPassword { get; set; }

    [Required(ErrorMessage = "Kata sandi baru diperlukan.")]
    [DataType(DataType.Password)]
    [MinLength(12, ErrorMessage = "Kata sandi baru harus minimal 12 karakter.")]
    public string NewPassword { get; set; }

    [DataType(DataType.Password)]
    [Compare("NewPassword", ErrorMessage = "Kata sandi konfirmasi tidak cocok.")]
    public string ConfirmPassword { get; set; }
}
}
