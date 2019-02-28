using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TournamentPage.Models;
using TournamentPage.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using Hangfire;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Owin;


namespace TournamentPage.Controllers
{
    public class HomeController : Controller
    {

        private ApplicationDbContext db;

        public HomeController(ApplicationDbContext db)
        {
            this.db = db;
        }

        public IActionResult Index()
        {
            // Create a list with featured tournaments
            // The query should be changed to only take tournaments that haven't yet started in the future
            var tournaments = db.Tournament.Take(6).ToListAsync().Result; // Fetch data

            return View(tournaments);
        }
        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}