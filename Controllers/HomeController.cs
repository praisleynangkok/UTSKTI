using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SampleSecureWeb.Data;

namespace SampleSecureWeb.Controllers
{
    [Authorize] // Hanya pengguna yang terautentikasi yang dapat mengakses controller ini
    public class HomeController : Controller
    {
        private readonly IStudent _studentData;

        public HomeController(IStudent studentData)
        {
            _studentData = studentData;
        }

        // Menampilkan daftar mahasiswa
        public IActionResult Index()
        {
            var students = _studentData.GetStudents(); // Mengambil data mahasiswa
            return View(students);
        }

        // Menampilkan detail mahasiswa
        public IActionResult Details(int id)
        {
            var student = _studentData.GetStudent(id.ToString());
            if (student == null)
            {
                return NotFound();
            }
            return View(student);
        }

        // Metode untuk menguji autentikasi
        public IActionResult TestAuth()
        {
            if (User.Identity.IsAuthenticated)
            {
                return Content("User is authenticated.");
            }
            return Content("User is NOT authenticated.");
        }
    }
}
