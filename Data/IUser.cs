using SampleSecureWeb.Models;

namespace SampleSecureWeb.Data
{
    public interface IUser
    {
        User Login(User user);
        void Registration(User user); // Metode ini sudah benar
        User GetUserByUsername(string username);
        User GetUserById(int id); // Menambahkan metode ini
        void UpdateUser(User user); // Menambahkan metode ini
    }
}
