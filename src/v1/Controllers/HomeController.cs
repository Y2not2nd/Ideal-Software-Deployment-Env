using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;

namespace ClickCounterApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly string _connString;

        // ✅ Pull connection string from app configuration
        public HomeController(IConfiguration configuration)
        {
            _connString = configuration.GetConnectionString("DefaultConnection");
        }

        public IActionResult Index()
        {
            int current = 0;
            using (var conn = new SqlConnection(_connString))
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT TOP 1 Count FROM Clicks WHERE Id=1", conn);
                var result = cmd.ExecuteScalar();

                if (result != null)
                    current = Convert.ToInt32(result);
                else
                {
                    // Initialize first row if table is empty
                    var initCmd = new SqlCommand("INSERT INTO Clicks (Count) VALUES (0)", conn);
                    initCmd.ExecuteNonQuery();
                }

                ViewBag.Count = current;
            }

            return View();
        }

        [HttpPost]
        public IActionResult Increment()
        {
            using (var conn = new SqlConnection(_connString))
            {
                conn.Open();
                // ✅ Increment counter
                var cmd = new SqlCommand("UPDATE Clicks SET Count = Count + 1 WHERE Id=1", conn);
                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("Index");
        }
    }
}
