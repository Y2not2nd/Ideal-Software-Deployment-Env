using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace ClickCounterApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly string _connString;
        private readonly bool _darkMode;

        public HomeController(string connString, IConfiguration config)
        {
            _connString = connString;
            // Read feature flag from config (App Setting "DarkModeEnabled", default false)
            _darkMode = config.GetValue<bool>("DarkModeEnabled");
        }

        public IActionResult Index()
        {
            int current = 0;
            using(var conn = new SqlConnection(_connString))
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT TOP 1 Count FROM Clicks WHERE Id=1", conn);
                var result = cmd.ExecuteScalar();
                if (result != null) current = Convert.ToInt32(result);
                ViewBag.Count = current;
            }
            ViewBag.DarkMode = _darkMode; // Pass flag to view
            return View();
        }

        [HttpPost]
        public IActionResult Increment()
        {
            using(var conn = new SqlConnection(_connString))
            {
                conn.Open();
                var cmd = new SqlCommand("UPDATE Clicks SET Count = Count + 1 WHERE Id=1", conn);
                cmd.ExecuteNonQuery();
            }
            return RedirectToAction("Index");
        }
    }
}
