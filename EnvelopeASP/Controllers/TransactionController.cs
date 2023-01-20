using EnvelopeASP.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnvelopeASP.Controllers
{
    [Authorize]
    public class TransactionController : Controller
    {
        public async Task<IActionResult> Index(int? id)
        {
            if(id.HasValue == false)
            {
                return Redirect("/Envelope");
            }
            var uid = Utils.GetUserIDFromClaims(HttpContext.User);

            var transactions = await Procedures.Sel_Transactions(uid, Convert.ToUInt16(Math.Abs(id.Value)));

            return View((id.Value,transactions));
        }

        public async Task<IActionResult> Add(int? id, [FromForm]Transaction model)
        {
            ViewData["Operation"] = "Add";
            if(id.HasValue == false)
            {
                return RedirectToAction("Index");
            }
            if(Request.Method == System.Net.WebRequestMethods.Http.Post)
            {
                if (ModelState.IsValid == false)
                {
                    return View(model);
                }
                var uID = Utils.GetUserIDFromClaims(HttpContext.User);
                var eID = Convert.ToUInt16(Math.Abs(id.Value));

                //todo, dollar to cent conversion
                await Procedures.Ins_EnvelopeTransaction(uID, eID, Utils.DoubleMoneyToCents(model.Amount), model.Date, model.Note);
                return RedirectToAction("Index", id);
            }

            model ??= new Transaction();
            return View(model);
        }

        [Route("/Transaction/Update/{eid}/{tid}")]
        public async Task<IActionResult> Update(ushort eid, uint tid, [FromForm] Transaction model)
        {
            ViewData["Operation"] = "Update";
            var uID = Utils.GetUserIDFromClaims(HttpContext.User);
            if (Request.Method == System.Net.WebRequestMethods.Http.Post)
            {
                if (ModelState.IsValid == false)
                {
                    return View(model);
                }
                //var eID = Convert.ToUInt16(Math.Abs(id));

                //todo, dollar to cent conversion
                await Procedures.Upd_EnvelopeTransaction(uID, eid, tid, Utils.DoubleMoneyToCents(model.Amount), model.Date, model.Note);
                return Redirect("/Transaction/Index/" + eid);
            }
            if(model.TransactionNumber == 0)
            {

                var nmodel = await Procedures.Sel_Transaction(uID, eid, tid);
                if(nmodel == null)
                {
                    return Redirect("/Transaction/Index/" + eid);
                }
                model = nmodel;
            }
            
            return View("Add", model);
        }

        [Route("/Transaction/Delete/{eid}/{tid}")]
        public async Task<IActionResult> Delete(ushort eid, uint tid, [FromForm] Transaction model)
        {
            var uID = Utils.GetUserIDFromClaims(HttpContext.User);
            if (Request.Method == System.Net.WebRequestMethods.Http.Post)
            {
                var yesno = Request.Form.FirstOrDefault(f => f.Key == "yesno");
                var val = yesno.Value.ToString();
                if (string.IsNullOrEmpty(val))
                {
                    return View(model);
                }
                if(val.Equals("No", StringComparison.OrdinalIgnoreCase))
                {
                    return Redirect("/Transaction/Index/" + eid);
                }
                if(val.Equals("Yes", StringComparison.OrdinalIgnoreCase))
                {
                    await Procedures.Del_EnvelopeTransaction(uID, eid, tid);
                    return Redirect("/Transaction/Index/" + eid);
                }

            }
            if (model.TransactionNumber == 0)
            {

                var nmodel = await Procedures.Sel_Transaction(uID, eid, tid);
                if (nmodel == null)
                {
                    return RedirectToAction("Index");
                }
                model = nmodel;
            }

            return View(model);
        }
    }
}
