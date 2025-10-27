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
                var cmd = new SqlCommand("SELECT ISNULL(SUM([Count]), 0) FROM Clicks", conn);
                var result = cmd.ExecuteScalar();

                if (result != null && result != DBNull.Value)
                {
                    current = Convert.ToInt32(result);
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
                var cmd = new SqlCommand(@"
DECLARE @targetId INT;

SELECT TOP (1) @targetId = Id
FROM Clicks
ORDER BY Id;

IF @targetId IS NULL
BEGIN
    INSERT INTO Clicks (Count) VALUES (1);
END
ELSE
BEGIN
    UPDATE Clicks
    SET Count = Count + 1
    WHERE Id = @targetId;
END", conn);
                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("Index");
        }
    }
}
