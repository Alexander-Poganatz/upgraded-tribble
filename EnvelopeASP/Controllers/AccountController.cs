using EnvelopeASP.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
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
                if(model.Password.Length > 1024)
                {
                    ViewBag.Error = "The password is a bit big, rejected";
                    return View(model);
                }
                if(model.Password != model.ConfirmPassword)
                {
                    ViewBag.Error = "Passwords do not match";
                    return View(model);
                }
                var passwordConfig = new PasswordConfig();
                var hash = passwordConfig.GenerateHash(model.Password);
                await Procedures.InsertUser(model.Email, hash, passwordConfig);

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
            if (User.Identity is not null && User.Identity.IsAuthenticated)
            {
                return Redirect(ReturnUrl);
            }
            if (Request.Method == "POST" && ModelState.IsValid)
            {
                model.Email = model.Email.Trim();
                
                var dbUserN = await Procedures.Sel_UserByEmail(model.Email);

                if (dbUserN != null)
                {
                    var isValid = DateTime.UtcNow > dbUserN.LockoutExpiry && dbUserN.PasswordConfig.Verify(model.Password, dbUserN.PasswordHash);

                    await Procedures.Upd_User_Login(dbUserN.Id, isValid);

                    if (isValid)
                    {
                        var claims = new Claim[2];
                        claims[0] = new Claim(ClaimTypes.NameIdentifier, dbUserN.Id.ToString());
                        claims[1] = new Claim(ClaimTypes.Name, model.Email);

                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                        var principle = new ClaimsPrincipal(claimsIdentity);

                        await HttpContext.SignInAsync(principle);

                        return Redirect(ReturnUrl);
                    }
                    
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
            if (User.Identity is not null && User.Identity.IsAuthenticated)
            {
                await HttpContext.SignOutAsync();
            }
            return Redirect("/");
        }

        [Authorize]
        public async Task<IActionResult> PasswordReset(SignUp model)
        {
            if (Request.Method == "POST" && ModelState.IsValid)
            {
                var oldPassword = model.Email.Trim();

                if (model.Password.Length > 1024)
                {
                    ViewBag.Error = "The new password is a bit big, rejected";
                    return View(model);
                }
                if (model.Password != model.ConfirmPassword)
                {
                    ViewBag.Error = "The new passwords do not match";
                    return View(model);
                }

                // Check Old Password
                var userNull = await Procedures.Sel_UserByEmail( User.Claims.First(f => f.Type == ClaimTypes.Name).Value);
                User user;
                if(userNull is null)
                {
                    ViewBag.Error = "Strange account not found error occured";
                    return View(model);
                }
                user = userNull;

                var isValid = DateTime.UtcNow > user.LockoutExpiry && user.PasswordConfig.Verify(oldPassword, user.PasswordHash);

                await Procedures.Upd_User_Login(user.Id, isValid);

                if(user.LockoutExpiry > DateTime.UtcNow)
                {
                    // password change is getting kinda sus
                    await HttpContext.SignOutAsync();
                    return Redirect("/");
                }

                if(isValid)
                {
                    // do on new password
                    var passwordConfig = new PasswordConfig();
                    var hash = passwordConfig.GenerateHash(model.Password);
                    var rowsEffected = await Procedures.UpdateUserPassword(Utils.GetUserIDFromClaims(User), hash, passwordConfig);
                } else
                {
                    ViewBag.Error = "Current password is invalid";
                    return View(model);
                }

                return Redirect("/");
            }
            else
            {
                model ??= new SignUp();
                return View(model);
            }
        }
    }
}
