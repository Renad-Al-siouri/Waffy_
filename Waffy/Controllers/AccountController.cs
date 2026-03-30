using Microsoft.AspNetCore.Mvc;

namespace Waffy.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Login()
        {
            return View();
        }
        public IActionResult Signup()
        {
            return View();
        }
        
        public IActionResult ForgotPass()
        {
            return View();
        }
        public IActionResult OTP()
        {
            return View();
        }
        public IActionResult ResetPass()
        {
            return View();
        }
        public IActionResult Logout()
        {
            return View();
        }

        public IActionResult user_signup()
        {
            return View();
        }
        //Sign up step for clinic

        public IActionResult Clinic_step1()
        {
            return View();
        }
        public IActionResult Clinic_step2()
        {
            return View();
        }
        public IActionResult Clinic_step3()
        {
            return View();
        }
        public IActionResult Clinic_step4()
        {
            return View();
        }
        public IActionResult Clinic_step5()
        {
            return View();
        }
        public IActionResult Clinic_success()
        {
            return View();
        }


        //Sign up step for store
        public IActionResult Stor_step1()
        {
            return View();
        }
        public IActionResult Stor_step2()
        {
            return View();
        }
        public IActionResult Stor_step3()
        {
            return View();
        }
        public IActionResult Stor_step4()
        {
            return View();
        }
        public IActionResult Stor_step5()
        {
            return View();
        }
        public IActionResult Stor_success()
        {
            return View();
        }

    }
}
