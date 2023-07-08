namespace EnvelopeWeb.Controllers

open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open System.Threading.Tasks
open Models

[<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>]
[<EnvelopeWeb.HTMXDefaultLayoutFilterAttribute>]
type TransactionController (logger : ILogger<TransactionController>, dbConnectionGetter : DbConnection.IDbConnectionGetter) =
    inherit Controller()

    member this.Index (id: System.Nullable<uint16>) (page: uint32) =
        async {
            if id.HasValue = false then
                return this.Redirect("/Envelope") :> IActionResult
            else
                let page = System.Math.Max(1u, page)
                let uid = Utils.getUserIDFromClaims this.User
                let! transactions = Procedures.Sel_Transactions dbConnectionGetter uid id.Value Utils.DefaultPaginationSize page

                return this.View((id.Value, transactions, page) |> System.TupleExtensions.ToValueTuple) :> IActionResult
        }
        |> Async.StartAsTask
        
    member this.Add (id:System.Nullable<uint16>) ([<FromForm>]model: Models.Transaction) : Task<IActionResult> =
        this.ViewData["Operation"] <- "Add"
        async {
            if id.HasValue = false then
                return this.RedirectToAction("Index") :> IActionResult
            else if this.Request.Method = System.Net.WebRequestMethods.Http.Post && this.ModelState.IsValid then
                let uid = Utils.getUserIDFromClaims this.User
                
                let dollarAsCents = Utils.doubleMoneyToCents model.Amount

                let! r = Procedures.Ins_EnvelopeTransaction dbConnectionGetter uid id.Value dollarAsCents model.Date model.Note

                let hasAddAgain, hasAddAgainValue = this.Request.Form.TryGetValue "AddAgain"

                if hasAddAgain && hasAddAgainValue.Equals("on") then 
                    let model = { Amount = 0.0; Note = System.String.Empty; Date = model.Date; TransactionNumber = 0u }
                    return this.View(model) :> IActionResult
                else if Utils.requestIsHTMX this.HttpContext then
                    this.Response.Headers.Add("HX-Refresh", "true")
                    return this.View(model)
                else
                    return this.RedirectToAction("Index", {| id = id |}) :> IActionResult
                        
            else
                return this.View(model) :> IActionResult
                
        }
        |> Async.StartAsTask

    member this.Transfer (id: uint16) ([<FromForm>]model:Models.Transfer) =
        async {

            if this.Request.Method = System.Net.WebRequestMethods.Http.Post && this.ModelState.IsValid && model.DestinationNumber.HasValue then
                let uid = Utils.getUserIDFromClaims this.User
                let amountAsCents = Utils.doubleMoneyToCents model.Amount
                let! r = Procedures.Transfer dbConnectionGetter uid id model.DestinationNumber.Value amountAsCents

                return this.RedirectToAction("Index", {| id = id |}) :> IActionResult
            else
                let uid = Utils.getUserIDFromClaims this.User
                let! env = Procedures.Sel_Envelope_Summary dbConnectionGetter uid
                let env = env |> List.filter (fun f -> f.Number <> id) |> List.map (fun f -> new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem(f.Name + " - " + f.Amount.ToString(), f.Number.ToString()))
                let model = { DestinationNumber = new System.Nullable<uint16>(); Amount = 0.0; Envelopes = env }
                return this.View(model) :> IActionResult
                
        }
        |> Async.StartAsTask


    [<Route("/Transaction/Update/{id}/{tid}")>]
    member this.Update (id:uint16) (tid:uint) ([<FromForm>]model:Models.Transaction) =
        this.ViewData["Operation"] <- "Update"
        async {
            if this.Request.Method = System.Net.WebRequestMethods.Http.Post && this.ModelState.IsValid then
                let uid = Utils.getUserIDFromClaims this.User
                
                let dollarAsCents = Utils.doubleMoneyToCents model.Amount

                let! r = Procedures.Upd_EnvelopeTransaction dbConnectionGetter uid id tid dollarAsCents model.Date model.Note

                if Utils.requestIsHTMX this.HttpContext then
                    this.Response.Headers.Add("HX-Refresh", "true")
                    return this.View("Add", model) :> IActionResult
                else
                    return this.Redirect("/Transaction/Index/" + id.ToString()) :> IActionResult
            
            else if model.TransactionNumber = 0u then
                let uid = Utils.getUserIDFromClaims this.User
                let! model = Procedures.Sel_Transaction dbConnectionGetter uid id tid
                match model with
                | None -> return this.Redirect("/Transaction/Index/" + id.ToString()) :> IActionResult
                | Some m -> return this.View("Add", m) :> IActionResult
            else
                return this.View("Add", model) :> IActionResult
        }
        |> Async.StartAsTask

    [<Route("/Transaction/Delete/{eid}/{tid}")>]
    member this.Delete (eid:uint16) (tid:uint) ([<FromForm>]model:Models.Transaction) =
        let uid = Utils.getUserIDFromClaims this.User
        async {
            if this.Request.Method = System.Net.WebRequestMethods.Http.Post then
                let hasYesNo, yesno = this.Request.Form.TryGetValue "yesno"
                if hasYesNo = false || System.String.IsNullOrWhiteSpace(yesno.Item 0) then
                    return this.View(model) :> IActionResult
                else
                    if "Yes".Equals(yesno.Item 0, System.StringComparison.OrdinalIgnoreCase) then
                        let! r = Procedures.Del_EnvelopeTransaction dbConnectionGetter uid eid tid
                        r |> ignore
                    
                    if Utils.requestIsHTMX this.HttpContext then
                        this.Response.Headers.Add("HX-Refresh", "true")
                        return this.View(model) :> IActionResult
                    else
                        return this.Redirect("/Transaction/Index/" + eid.ToString()) :> IActionResult

            else
                let! dbModel = Procedures.Sel_Transaction dbConnectionGetter uid eid tid
                match dbModel with
                | None -> return this.RedirectToAction("Index", {| id = eid |}) :> IActionResult
                | Some m -> return this.View(m) :> IActionResult

                

        } |> Async.StartAsTask