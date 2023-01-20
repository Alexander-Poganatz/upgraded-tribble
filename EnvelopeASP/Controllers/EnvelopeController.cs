using EnvelopeASP.Models;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using System.Security.Cryptography;

namespace EnvelopeASP.Controllers
{
    [Microsoft.AspNetCore.Authorization.Authorize]
    public class EnvelopeController : Controller
    {
        
        public async Task<IActionResult> Index()
        {
            var uID = Utils.GetUserIDFromClaims(HttpContext.User);
            var model = await Procedures.Sel_Envelope_Summary(uID);
            return View(model);
        }

        public async Task<IActionResult> Add()
        {
            string model = string.Empty;
            ViewData["Operation"] = "Add";
            if (Request.Method == System.Net.WebRequestMethods.Http.Post)
            {

                var keyPair = Request.Form.FirstOrDefault(f => f.Key == "envelopeName");

                var value = keyPair.Value.ToString().Trim();
                if (string.IsNullOrEmpty(value) == false)
                {
                    await Procedures.Ins_Envelope(Utils.GetUserIDFromClaims(HttpContext.User), value);
                    return RedirectToAction("Index");
                } else
                {
                    ModelState.AddModelError("NoName", "Envelopes must have a name");
                }
            }
            return View((object)model);
        }

        public async Task<IActionResult> Update(ushort id, string? name)
        {
            var value = string.Empty;
            ViewData["Operation"] = "Update";
            if (Request.Method == System.Net.WebRequestMethods.Http.Post)
            {
                var keyPair = Request.Form.FirstOrDefault(f => f.Key == "envelopeName");
                value = keyPair.Value.ToString().Trim();
                if (string.IsNullOrEmpty(value) == false)
                {
                    await Procedures.Upd_Envelope(Utils.GetUserIDFromClaims(HttpContext.User), id, value);
                    return RedirectToAction("Index");
                } else
                {
                    ModelState.AddModelError("NoName", "Envelopes must have a name");
                }
            }
            if(value == string.Empty && name != null)
            {
                value = name;
            }
            return View("Add",(object)value);
        }

        public async Task<IActionResult> Delete(ushort id, string? name)
        {
            name ??= string.Empty;
            var uID = Utils.GetUserIDFromClaims(HttpContext.User);
            if (Request.Method == System.Net.WebRequestMethods.Http.Post)
            {
                var yesno = Request.Form.FirstOrDefault(f => f.Key == "yesno");
                var val = yesno.Value.ToString();
                if (string.IsNullOrEmpty(val))
                {
                    return View((object)name);
                }
                if (val.Equals("No", StringComparison.OrdinalIgnoreCase))
                {
                    return RedirectToAction("Index");
                }
                if (val.Equals("Yes", StringComparison.OrdinalIgnoreCase))
                {
                    await Procedures.Del_Envelope(uID, id);
                    return RedirectToAction("Index");
                }

            }

            return View((object)name);
        }
    }
}
