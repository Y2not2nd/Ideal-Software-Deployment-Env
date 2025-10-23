using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace ClickCounterApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly string _connString;

        public HomeController(string connString)
        {
            _connString = connString;
        }

        public IActionResult Index()
        {
            int current = 0;
            using(var conn = new SqlConnection(_connString))
            {
                conn.Open();
                // Read or initialize count
                var cmd = new SqlCommand("SELECT TOP 1 Count FROM Clicks WHERE Id=1", conn);
                var result = cmd.ExecuteScalar();
                if (result != null) current = Convert.ToInt32(result);
                ViewBag.Count = current;
            }
            return View();
        }

        [HttpPost]
        public IActionResult Increment()
        {
            using(var conn = new SqlConnection(_connString))
            {
                conn.Open();
                // Increment count
                var cmd = new SqlCommand("UPDATE Clicks SET Count = Count + 1 WHERE Id=1", conn);
                cmd.ExecuteNonQuery();
            }
            return RedirectToAction("Index");
        }
    }
}
