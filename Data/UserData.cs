using System;
using System.Linq;
using SampleSecureWeb.Models;

namespace SampleSecureWeb.Data
{
    public class UserData : IUser
    {
        private readonly ApplicationDbContext _db;

        public UserData(ApplicationDbContext db)
        {
            _db = db;
        }

        // Metode untuk login pengguna
        public User Login(User user)
        {
            var _user = _db.Users.FirstOrDefault(u => u.Username == user.Username);
            if (_user == null)
            {
                throw new Exception("User not found");
            }
            if (!BCrypt.Net.BCrypt.Verify(user.Password, _user.Password))
            {
                throw new Exception("Password is incorrect");
            }
            return _user; // Kembalikan pengguna jika login berhasil
        }

        // Metode untuk registrasi pengguna
        public void Registration(User user)
        {
            try
            {
                // Hash kata sandi sebelum menyimpannya ke database
                user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
                user.IsVerified = false; // Atur status verifikasi default
                _db.Users.Add(user); // Tambahkan pengguna ke DbSet
                _db.SaveChanges(); // Simpan perubahan ke database
            }
            catch (Exception ex)
            {
                throw new Exception("Terjadi kesalahan saat registrasi: " + ex.Message); // Tangkap dan sampaikan kesalahan
            }
        }

        // Metode untuk mendapatkan pengguna berdasarkan username
        public User GetUserByUsername(string username)
        {
            return _db.Users.FirstOrDefault(u => u.Username == username);
        }

        // Metode untuk mendapatkan pengguna berdasarkan ID
        public User GetUserById(int id)
        {
            return _db.Users.Find(id); // Menggunakan Find untuk mencari pengguna berdasarkan ID
        }

        // Metode untuk memperbarui pengguna
        public void UpdateUser(User user)
        {
            var existingUser = _db.Users.Find(user.Id); // Temukan pengguna berdasarkan ID
            if (existingUser == null)
            {
                throw new Exception("Pengguna tidak ditemukan untuk diperbarui.");
            }

            // Update properti pengguna yang diperlukan
            existingUser.Username = user.Username;

            // Periksa apakah ada password baru untuk diupdate
            if (!string.IsNullOrEmpty(user.Password))
            {
                existingUser.Password = BCrypt.Net.BCrypt.HashPassword(user.Password); // Hanya update password jika diberikan
            }

            existingUser.IsVerified = user.IsVerified; // Update status verifikasi jika ada

            _db.SaveChanges(); // Simpan perubahan ke database
        }
    }
}
