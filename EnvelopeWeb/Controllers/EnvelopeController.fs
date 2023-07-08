namespace EnvelopeWeb.Controllers

open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open System.Threading.Tasks

[<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>]
[<EnvelopeWeb.HTMXDefaultLayoutFilterAttribute>]
type EnvelopeController (logger : ILogger<EnvelopeController>, dbConnectionGetter : DbConnection.IDbConnectionGetter) =
    inherit Controller()

    member this.Index() =
        async {
            let uID = Utils.getUserIDFromClaims this.User
            let! model = Procedures.Sel_Envelope_Summary dbConnectionGetter uID
            return this.View(model)
        }
        |> Async.StartAsTask

    member this.Add() : Task<IActionResult> =
        let model = System.String.Empty
        this.ViewData["Operation"] <- "Add"

        async {
            if this.Request.Method = System.Net.WebRequestMethods.Http.Post then
                let b, s = this.Request.Form.TryGetValue "envelopeName"

                let value = if b then s.Item 0 else System.String.Empty

                if System.String.IsNullOrWhiteSpace(value) then
                    this.ModelState.AddModelError("NoName", "Envelopes must have a name")
                    return this.View(model :> System.Object) :> IActionResult
                else
                    let uid = Utils.getUserIDFromClaims this.User
                    let! r = Procedures.Ins_Envelope dbConnectionGetter uid value
                    return this.RedirectToAction("Index") :> IActionResult
            else
                return this.View(model :> System.Object) :> IActionResult
        }
        |> Async.StartAsTask

    member this.Update (id:uint16) (name:string) : Task<IActionResult> =
        let model = if System.String.IsNullOrWhiteSpace name then System.String.Empty else name
        this.ViewData["Operation"] <- "Update"

        async {
            if this.Request.Method = System.Net.WebRequestMethods.Http.Post then
                let b, s = this.Request.Form.TryGetValue "envelopeName"

                let value = if b then s.Item 0 else System.String.Empty

                if System.String.IsNullOrWhiteSpace(value) then
                    this.ModelState.AddModelError("NoName", "Envelopes must have a name")
                    return this.View("Add", model :> System.Object) :> IActionResult
                else
                    let uid = Utils.getUserIDFromClaims this.User
                    let! r = Procedures.Upd_Envelope dbConnectionGetter uid id value
                    return this.RedirectToAction("Index") :> IActionResult
            else
                return this.View("Add", model :> System.Object) :> IActionResult
        }
        |> Async.StartAsTask

    member this.Delete (id:uint16) (name:string) : Task<IActionResult> =
        let model = if System.String.IsNullOrEmpty name then System.String.Empty else name

        async {
            if this.Request.Method = System.Net.WebRequestMethods.Http.Post then
                let b, s = this.Request.Form.TryGetValue "yesno"

                let value = if b then s.Item 0 else System.String.Empty

                if System.String.IsNullOrWhiteSpace(value) then
                    return this.View(model :> System.Object) :> IActionResult
                else if value.Equals("Yes", System.StringComparison.OrdinalIgnoreCase) then
                    let uid = Utils.getUserIDFromClaims this.User
                    let! r = Procedures.Del_Envelope dbConnectionGetter uid id
                    return this.RedirectToAction("Index") :> IActionResult
                else
                    return this.RedirectToAction("Index") :> IActionResult
            else
                return this.View(model :> System.Object) :> IActionResult
        }
        |> Async.StartAsTask