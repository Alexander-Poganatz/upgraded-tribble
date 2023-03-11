using EnvelopeASP.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Primitives;

namespace EnvelopeASP.Controllers
{
    [Authorize]
    [HTMXDefaultLayoutFilter]
    public class TransactionController : Controller
    {
        public async Task<IActionResult> Index(int? id, uint page)
        {
            if(id.HasValue == false)
            {
                return Redirect("/Envelope");
            }
            var uid = Utils.GetUserIDFromClaims(HttpContext.User);

            page = Math.Max(1, page);

            var transactions = await Procedures.Sel_Transactions(uid, Convert.ToUInt16(Math.Abs(id.Value)), Utils.DEFAULT_PAGINATION_SIZE, page);
            return View((id.Value,transactions, page));
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

                var hasAddAgain = Request.Form.TryGetValue("AddAgain", out StringValues addAgainValue);
                if (hasAddAgain && addAgainValue.Equals("on"))
                {
                    model = new Transaction() { Date = model.Date };
                } else
                {
                    if (Utils.RequestIsHTMX(HttpContext))
                    {
                        Response.Headers.Add("HX-Refresh", "true");
                        return View(model);
                    }
                    
                    return RedirectToAction("Index", new { id });
                }
            }

            model ??= new Transaction();
            return View(model);
        }

        public async Task<IActionResult> Transfer(int? id, [FromForm] TransferModel model)
        {
            if (id.HasValue == false)
            {
                return RedirectToAction("Index");
            }
            if (Request.Method == System.Net.WebRequestMethods.Http.Post)
            {
                if (ModelState.IsValid == false)
                {
                    return View(model);
                }
                if(model.DestinationNumber.HasValue == false)
                {
                    return View(model);
                }
                var uID = Utils.GetUserIDFromClaims(HttpContext.User);
                var eID = Convert.ToUInt16(Math.Abs(id.Value));
                var dID = Convert.ToUInt16(Math.Abs(model.DestinationNumber.Value));

                await Procedures.Transfer(uID, eID, dID, Utils.DoubleMoneyToCents(model.Amount));
                return RedirectToAction("Index", new { id });
            }

            model ??= new TransferModel();
            if(model.Envelopes.Count == 0)
            {
                var envelopes = await Procedures.Sel_Envelope_Summary(Utils.GetUserIDFromClaims(HttpContext.User));
                model.Envelopes = envelopes.Where(f => f.Number != id).Select(f => new SelectListItem(f.Name + " - " + f.Amount, f.Number.ToString())).ToList();
            }
            return View(model);
        }

        [Route("/Transaction/Update/{id}/{tid}")]
        public async Task<IActionResult> Update(ushort id, uint tid, [FromForm] Transaction model)
        {
            ViewData["Operation"] = "Update";
            var uID = Utils.GetUserIDFromClaims(HttpContext.User);
            if (Request.Method == System.Net.WebRequestMethods.Http.Post)
            {
                if (ModelState.IsValid == false)
                {
                    return View("Add", model);
                }

                await Procedures.Upd_EnvelopeTransaction(uID, id, tid, Utils.DoubleMoneyToCents(model.Amount), model.Date, model.Note);
                if (Utils.RequestIsHTMX(HttpContext))
                {
                    Response.Headers.Add("HX-Refresh", "true");
                    return View("Add", model);
                }
                return Redirect("/Transaction/Index/" + id);
            }
            if(model.TransactionNumber == 0)
            {

                var nmodel = await Procedures.Sel_Transaction(uID, id, tid);
                if(nmodel == null)
                {
                    return Redirect("/Transaction/Index/" + id);
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
                if(val.Equals("Yes", StringComparison.OrdinalIgnoreCase))
                {
                    await Procedures.Del_EnvelopeTransaction(uID, eid, tid);
                }
                if (Utils.RequestIsHTMX(HttpContext))
                {
                    Response.Headers.Add("HX-Refresh", "true");
                    return View(model);
                }
                return Redirect("/Transaction/Index/" + eid);
            }
            if (model.TransactionNumber == 0)
            {

                var nmodel = await Procedures.Sel_Transaction(uID, eid, tid);
                if (nmodel == null)
                {
                    return RedirectToAction("Index", new { id = ((int)eid) });
                }
                model = nmodel;
            }

            return View(model);
        }
    }
}
