using EnvelopeASP.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EnvelopeASP.Controllers
{
    public class AccountController : Controller
    {

        public AccountController()
        {
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> SignUp(SignUp model)
        {
            if(Request.Method== "POST" && ModelState.IsValid)
            {
                model.Email = model.Email.Trim();
                if(model.Password != model.ConfirmPassword)
                {
                    ViewBag.Error = "Passwords do not match";
                    return View(model);
                }
                var hash = BCrypt.Net.BCrypt.EnhancedHashPassword(model.Password);
                await Procedures.InsertUser(model.Email, hash);

                return Redirect("Login");
            } else
            {
                model ??= new SignUp();
                return View(model);
            }
        }

        public async Task<IActionResult> Login(Login model, string? ReturnUrl)
        {
            if (string.IsNullOrWhiteSpace(ReturnUrl))
            {
                ReturnUrl = "/";
            }
            if (User.Identity.IsAuthenticated)
            {
                return Redirect(ReturnUrl);
            }
            if (Request.Method == "POST" && ModelState.IsValid)
            {
                model.Email = model.Email.Trim();
                
                var dbUserN = await Procedures.Sel_UserByEmail(model.Email);

                if (dbUserN != null)
                {
                    var isValid = DateTime.UtcNow > dbUserN.LockoutExpiry && BCrypt.Net.BCrypt.EnhancedVerify(model.Password, dbUserN.PasswordHash);

                    await Procedures.Upd_User_Login(dbUserN.Id, isValid);

                    var claims = new Claim[2];
                    claims[0] = new Claim(ClaimTypes.NameIdentifier, dbUserN.Id.ToString());
                    claims[1] = new Claim(ClaimTypes.Name, model.Email);

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    var principle = new ClaimsPrincipal(claimsIdentity);

                    await HttpContext.SignInAsync(principle);
                    
                    return Redirect(ReturnUrl);
                }

                return View(model);
            }
            else
            {
                model ??= new Login();
                return View(model);
            }
        }

        public async Task<IActionResult> Logout()
        {
            if (User.Identity.IsAuthenticated)
            {
                await HttpContext.SignOutAsync();
            }
            return Redirect("/");
        }
    }
}
