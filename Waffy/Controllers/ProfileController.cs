
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using Waffy.Models;

      

namespace Waffy.Controllers
    {
        public class ProfileController : Controller
        {


            public IActionResult Index()
            {
                return View();
            }
            public IActionResult Edit_Profile()
            {
                return View();
            }
            public IActionResult Contact_Info()
            {
                return View();
            }
            public IActionResult My_Wallet()
            {
                return View();
            }

            public IActionResult Rewards()
            {
                return View();
            }

            public IActionResult Appointments()
            {
                return View();
            }

            public IActionResult Orders()
            {
                return View();
            }

            public IActionResult Lost_Reports()
            {
                return View();
            }

            public IActionResult Adoption_Ads()
            {
                return View();
            }

            public IActionResult Timeline()
            {
                return View();
            }

            public IActionResult Complaints()
            {
                return View();
            }

            public IActionResult Security()
            {
                return View();
            }

            


    }
    }
