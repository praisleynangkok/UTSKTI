using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SampleSecureWeb.Data;
using SampleSecureWeb.Models;
using SampleSecureWeb.ViewModels;
using SampleSecureWeb.Services;

namespace SampleSecureWeb.Controllers
{
    [AllowAnonymous] // Mengizinkan akses tanpa autentikasi ke semua aksi di controller ini
    public class AccountController : Controller
    {
        private readonly IUser _userData;
        private readonly EmailService _emailService;

        public AccountController(IUser user, EmailService emailService)
        {
            _userData = user;
            _emailService = emailService; // Inisialisasi EmailService
        }

        // Menampilkan halaman ganti kata sandi (GET)
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        // Memproses permintaan ganti kata sandi (POST)
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Ambil pengguna saat ini berdasarkan username
                var currentUser = _userData.GetUserByUsername(User.Identity.Name);
                if (currentUser == null)
                {
                    return RedirectToAction("Login");
                }

                // Verifikasi kata sandi saat ini
                if (!BCrypt.Net.BCrypt.Verify(model.CurrentPassword, currentUser.Password))
                {
                    ModelState.AddModelError("CurrentPassword", "Kata sandi saat ini tidak sesuai.");
                    return View(model);
                }

                // Update kata sandi jika sesuai
                currentUser.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
                _userData.UpdateUser(currentUser);

                ViewBag.Message = "Kata sandi berhasil diubah.";
                return RedirectToAction("ChangePasswordConfirmation");
            }

            // Jika validasi gagal
            return View(model);
        }

        public IActionResult ChangePasswordConfirmation()
        {
            return View();
        }
      
        // Menampilkan halaman pendaftaran
        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        // Memproses pendaftaran
        [HttpPost]
        public async Task<ActionResult> Register(RegistrationViewModel registrationViewModel)
        {
            if (ModelState.IsValid)
            {
                var existingUser = _userData.GetUserByUsername(registrationViewModel.Username);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Username", "Username sudah terpakai.");
                    return View(registrationViewModel);
                }

                var user = new User
                {
                    Username = registrationViewModel.Username,
                    Password = BCrypt.Net.BCrypt.HashPassword(registrationViewModel.Password),
                    RoleName = "contributor",
                    IsVerified = false // Tambahkan properti untuk verifikasi
                };

                try
                {
                    _userData.Registration(user);
                    ViewBag.Message = "Pendaftaran berhasil! Silakan cek email Anda untuk verifikasi.";

                    // Kirim email verifikasi
                    var verificationLink = Url.Action("VerifyEmail", "Account", new { userId = user.Id }, Request.Scheme);
                    await _emailService.SendEmailAsync(user.Username, "Verifikasi Email", $"Silakan klik <a href='{verificationLink}'>di sini</a> untuk memverifikasi akun Anda.");

                    return RedirectToAction("Login");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Terjadi kesalahan saat menyimpan data: " + ex.Message);
                }
            }
            return View(registrationViewModel);
        }

        // Memverifikasi email pengguna
        [HttpGet]
        public ActionResult VerifyEmail(int userId)
        {
            var user = _userData.GetUserById(userId); // Metode untuk mengambil pengguna berdasarkan ID
            if (user != null)
            {
                user.IsVerified = true; // Set pengguna sebagai terverifikasi
                _userData.UpdateUser(user); // Simpan perubahan
                ViewBag.Message = "Email Anda telah terverifikasi!";
            }
            else
            {
                ViewBag.Message = "Pengguna tidak ditemukan.";
            }

            return RedirectToAction("Index", "Home");
        }

        // Menampilkan halaman login
        [HttpGet]
        public ActionResult Login(string returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // Memproses login
        [HttpPost]
        public async Task<ActionResult> Login(LoginViewModel loginViewModel)
        {
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine("Error: " + error.ErrorMessage);
                }
                return View(loginViewModel);
            }

            try
            {
                var loginUser = _userData.Login(new User
                {
                    Username = loginViewModel.Username,
                    Password = loginViewModel.Password
                });

                if (loginUser != null && loginUser.IsVerified) // Pastikan pengguna terverifikasi
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, loginUser.Username)
                    };
                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        principal,
                        new AuthenticationProperties { IsPersistent = loginViewModel.RememberLogin }
                    );

                    if (!string.IsNullOrEmpty(loginViewModel.ReturnUrl) && Url.IsLocalUrl(loginViewModel.ReturnUrl))
                    {
                        return Redirect(loginViewModel.ReturnUrl);
                    }

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Percobaan login tidak valid atau email belum terverifikasi.");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            return View(loginViewModel);
        }
    }
}
