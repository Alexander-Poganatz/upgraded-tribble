module HTMLViews

open Giraffe.ViewEngine

let private crossorigin_anonymous = _crossorigin "anonymous"

let _hx_get = attr "hx-get"
let _hx_post = attr "hx-post"
let _hx_target = attr "hx-target"
let _onchange = attr "onchange"

let private formMethodPost = _method "post"

let private layout (isAuthenticated) (titleStr:string) (content: XmlNode list) =

    let accountLink, accountActionText = if isAuthenticated then ("/Account/Logout", "Logout") else ("/Account/Login", "Sign In")

    html [ _lang "en" ] [
        head [] [
            meta [ _charset "utf-8" ]
            meta [ _name "viewport"; _content "width=device-width, initial-scale=1.0"; ]
            title [] [ encodedText ( titleStr + " - Envelope" ) ]
            link [ _rel "stylesheet"; _href "/lib/picnic/picnic.min.css"; _integrity "sha512-HZgZOfcUw1rxWuEBlzDis5U4HlbzR0wcWmb3FrLSKV6uhZiZpT9JSTzPJplHDmJZJFNfAReW+iDELJ1kADYHtA=="; crossorigin_anonymous ]
            link [ _rel "stylesheet"; _href "/css/site.css"; crossorigin_anonymous ]
        ]
        body [ _class "js-off" ] [
            header [] [
                nav [] [
                    a [ _class "pseudo button"; _href "/"; ] [ encodedText "EnvelopeWeb" ]
                    div [ _class "menu" ] [
                        a [ _class "pseudo button"; _href "/Envelope" ] [ encodedText "Envelopes" ]
                        a [ _class "pseudo button"; _href accountLink ] [ encodedText accountActionText ]
                    ]
                ]
            ]
            div [] [
                div [ _style "height: 1em;" ] []
                main [] content
                div [ _style "height: 60px;" ] []
            ]
            footer [ _class "footer" ] [
                div [ _class "container" ] [ rawText "&copy; 2023 - EnvelopeWeb" ]
            ]
            script [ _src "/js/site.js"; crossorigin_anonymous ] []
    #if DEBUG
            script [ _src "/lib/htmx/htmx.js"; _integrity "sha512-ykNAloLcNUzuRU5fTY3o9GYUCTQqT1nUqYTwioO6as9B0OR8J9UGctJVYB+fR8rFqXUO836nzcO3OGD+5L0Nyw=="; crossorigin_anonymous ] []
    #else
            script [ _src "/lib/htmx/htmx.min.js"; _integrity "sha512-ULbUWm8wCS6zRoxK/2v51vUHGhKvK8PSiqA02tyUYlYoeQm5wB8xr8lObq5zmNGpYaZsED0NLhaiPAAm2VbhXw=="; crossorigin_anonymous ] []
    #endif
        ]
    ]

let homeView isAuthenticated =
    let nodes = 
        if isAuthenticated then
            [
                div [ _class "text-center" ] [
                    h1 [ _class "pure-menu-heading" ] [ encodedText "Welcome" ]
                    a [ _class "button primary"; _href "/Envelope" ] [ encodedText "Go To Envelopes" ]
                    div [] []
                    a [ _class "button warning"; _href "/Account/PasswordReset" ] [ encodedText "Password Reset" ]
                ]
            ]
        else
            [
                div [ _class "text-center" ] [
                    h1 [ _class "pure-menu-heading" ] [ encodedText "Welcome" ]
                    a [ _class "pure button"; _href "/Account/SignUp" ] [ encodedText "Don't have an account? Sign Up!" ]
                ]
            ]
    nodes
    |> layout isAuthenticated "Home Page"

let loginView verificationInput (errorModel: Models.LoginSignUpErrors) (model: Models.Login) =
    [
        div [] [
            h1 [] [ encodedText "Sign In" ]
            form [ formMethodPost ] [
                label [ _for "Email"; ] [ encodedText "Email" ]
                input [ _type "text"; _required; _id "Email"; _name "Email"; _value model.Email ]
                p [] [ encodedText errorModel.EmailError ]
                label [ _for "Password" ] [ encodedText "Password" ]
                input [ _type "password"; _required; _id "Password"; _name "Password"; _value model.Password]
                p [] [ encodedText errorModel.PasswordError ]
                input [ _type "submit" ]
                verificationInput
            ]
        ]
    ]
    |> layout false "Login"

let private signupOrResetView isSignUp verificationInput (errorModel: Models.LoginSignUpErrors) (model: Models.SignUp) =
    let title, emailFieldType, emailTitle = if isSignUp then ("Sign Up", "text", "Email") else ("Password Reset", "password", "Current Password")
    [
        div [] [
            h1 [] [ encodedText title ]
            form [ formMethodPost ] [
                label [ _for "Email"; ] [ encodedText emailTitle ]
                input [ _type emailFieldType; _required; _id "Email"; _name "Email"; _value model.Email ]
                p [] [ encodedText errorModel.EmailError ]
                label [ _for "Password" ] [ encodedText "Password" ]
                input [ _type "password"; _required; _id "Password"; _name "Password"; _value model.Password]
                p [] [ encodedText errorModel.PasswordError ]
                input [ _type "password"; _required; _id "ConfirmPassword"; _name "ConfirmPassword"; _value model.ConfirmPassword]
                p [] [ encodedText errorModel.ConfirmPasswordError ]
                input [ _type "submit" ]
                verificationInput
            ]
        ]
    ]
    |> layout false title

let signupView = signupOrResetView true
let resetPasswordView = signupOrResetView false

let deleteEmoji = rawText "&#10060;"
let updateEmoji = rawText "&#128397;";
let viewTransactionEmoji = rawText "&#128233;"

let private envelopeModelToTableRow (model: Models.Envelope) =
    let encodedName = System.Web.HttpUtility.UrlEncode(model.Name)
    let deleteUrl = sprintf "/Envelope/Delete/%i?name=%s" model.Number encodedName
    let updateUrl = sprintf "/Envelope/Update/%i?name=%s" model.Number encodedName
    tr [] [
        td [] [ encodedText model.Name ]
        td [] [ encodedText (model.Amount.ToString()) ]
        td [] [
            noscript [] [ a [ _href deleteUrl; _class "error"; _title "Delete Envelope"; ] [ deleteEmoji ] ]
            label [ _for "modal1"; _class "error js-on"; _hx_get deleteUrl; _hx_target "#modalBody" ] [ deleteEmoji ]
        ]
        td [] [
            noscript [] [ a [ _href updateUrl; _class "button"; _title "Update Envelope Name"] [ updateEmoji ] ]
            label [ _for "modal1"; _class "button js-on"; _hx_get updateUrl; _hx_target "#modalBody"; _title "Update Envelope Name" ] [ updateEmoji ]
        ]
        td [] [ a [ _href (sprintf "/Transaction/%i" model.Number); _title "View Transactions" ] [ viewTransactionEmoji ] ]
    ]

let private genericHTMXModal =
    div [ _class "modal" ] [
        input [ _id "modal1"; _type "checkbox" ]
        label [ _for "modal1"; _class "overlay" ] []
        div [ _class "modal-body"; _id "modalBody" ] []
    ]

let envelopeIndex (model:Models.Envelope list) =
    let tableOrDiv = 
        if List.isEmpty model then 
            div [] []
        else
            let rowList = model |> List.map envelopeModelToTableRow
            table [ _class "success" ] [
                thead [] [
                    tr [] [
                        th [] [ encodedText "Envelope" ]
                        th [] [ encodedText "Amount" ]
                        th [] []
                        th [] []
                        th [] []
                    ]
                ]
                tbody [] rowList
            ]

    [
        noscript [] [
            a [ _href "/Envelope/Add" ] [ encodedText "Add Envelope"]
        ]
        label [ _for "modal1"; _class "button js-on"; _hx_get "/Envelope/Add"; _hx_target "#modalBody" ] [ encodedText "Add Envelope" ]
        div [ _style "height: 1em;" ] []
        
        tableOrDiv

        genericHTMXModal
    ]
    |> layout true "Envelope"

let private addUpdateEnvelopeArticle (operation:string) (actionPath: string) antiForgeryInput (modelError: string) (model: Models.EnvelopeName) =
    article [] [
        header [] [ h3 [] [ encodedText operation ] ]
        section [] [
            form [ formMethodPost; _id "mainform"; _action actionPath ] [
                label [] [ encodedText (sprintf "%s Envelope Name" operation) ]
                input [ _type "text"; _maxlength "50"; _name "EnvelopeName"; _value model.EnvelopeName; _required ]
                p [ _class "error" ] [ encodedText modelError ]
                antiForgeryInput
            ]
        ]
        footer [] [
            input [ _type "submit"; _form "mainform" ]
            noscript [] [
                a [ _href "/Envelope"; _class "button dangerous" ] [ encodedText "Cancel" ]
            ]
            label [ _for "modal1"; _class "button dangerous js-on" ] [ encodedText "Cancel" ]
        ]
    ]
let private addOperationText = "Add"
let private updateOperationText = "Update"
let private deleteOperationText = "Delete"

let addEnvelopePartial = addUpdateEnvelopeArticle addOperationText
let addEnvelopeFull (actionPath: string) antiForgeryInput (modelError: string) (model: Models.EnvelopeName) =
    [ addUpdateEnvelopeArticle addOperationText actionPath antiForgeryInput modelError model ]
    |> layout true addOperationText

let updateEnvelopePartial = addUpdateEnvelopeArticle updateOperationText
let updateEnvelopeFull (actionPath: string) antiForgeryInput (modelError: string) (model: Models.EnvelopeName) =
    [ addUpdateEnvelopeArticle updateOperationText actionPath antiForgeryInput modelError model ]
    |> layout true updateOperationText

let deleteEnvelopePartial (actionPath: string) antiForgeryInput (model: Models.EnvelopeName) =
    article [] [
        header [] [ h3 [] [ encodedText "Delete?" ] ]
        section [] [
            form [ formMethodPost; _id "mainform"; _action actionPath ] [
                p [] [ encodedText (sprintf "Are you sure you want to delete %s?" model.EnvelopeName) ]
                label [] [ input [ _type "radio"; _name "YesNo"; _value "Yes"; _required ]; span [ _class "checkable" ] [ encodedText "Yes"] ]
                label [] [ input [ _type "radio"; _name "YesNo"; _value "No"; _required ]; span [ _class "checkable" ] [ encodedText "No"] ]
                antiForgeryInput
            ]
        ]
        footer [] [
            input [ _type "submit"; _form "mainform" ]
        ]
    ]

let deleteEnvelopeFull (actionPath: string) antiForgeryInput (model: Models.EnvelopeName) =
    [ deleteEnvelopePartial actionPath antiForgeryInput model ]
    |> layout true deleteOperationText

let transactionIndex (envelopeNum: uint16) (pagePath: string) (currentPageNumber: uint) (model: Models.Sel_Transactions_Result) =
    let transferUrl = sprintf "/Transaction/Transfer/%i" envelopeNum
    let addUrl = sprintf "/Transaction/Add/%i" envelopeNum

    let maxNumberOfPages = (model.NumberOfAllTransactions / Utils.DefaultPaginationSize) + (if model.NumberOfAllTransactions % Utils.DefaultPaginationSize > 0u then 1u else 0u)
    let numberOfPagesRange = seq { 1u .. maxNumberOfPages }

    let mapNumberToSelectList (num: uint) =
        let numStr = num.ToString()

        let attrList =
            if num = currentPageNumber then
                [ _value numStr; _selected ]
            else
                [ _value numStr; ]

        option attrList [ encodedText numStr ]
        
    let mapEnvelopeNumberToAnchorForNoScriptBlock (num: uint) =
        let numStr = num.ToString()
        let transactionUrl = sprintf "%s?page=%s" pagePath numStr
        a [ _href transactionUrl; _class "button"; ] [ encodedText numStr ]

    let optionsList = numberOfPagesRange |> Seq.map mapNumberToSelectList |> List.ofSeq
    let anchorList = numberOfPagesRange |> Seq.map mapEnvelopeNumberToAnchorForNoScriptBlock |> List.ofSeq

    let mapTransactionToTablerow (m: Models.Transaction) =
        let transactionNumber = m.TransactionNumber.ToString()
        let deleteLink = sprintf "/Transaction/Delete/%i/%s" envelopeNum transactionNumber
        let updateLink = sprintf "/Transaction/Update/%i/%s" envelopeNum transactionNumber

        tr [] [
            td [] [ encodedText transactionNumber ]
            td [] [ encodedText m.Note ]
            td [] [ encodedText (m.Amount.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)) ]
            td [] [ encodedText (m.Date.ToString("yyyy-MM-dd")) ]
            td [] [
                noscript [] [ a [ _href deleteLink; _title "Delete Transaction" ] [ deleteEmoji ] ]
                label [ _for "modal1"; _hx_get deleteLink; _hx_target "#modalBody"; _class "js-on"; _title "Delete Transaction" ] [ deleteEmoji ]
            ]
            td [] [
                noscript [] [ a [ _href updateLink; _title "Update Transaction"; ] [ updateEmoji ] ]
                label [ _for "modal1"; _hx_get updateLink; _hx_target "#modalBody"; _class "button js-on"; _title "Update Transaction"; ] [ updateEmoji ]
            ]
        ]

    let tableOrDiv =
        if List.length model.Transactions = 0 then
            div [] []
        else
            table [ _class "success" ] [
                thead [] [
                    tr [] [
                        th [] []
                        th [] [ encodedText "Note" ]
                        th [] [ encodedText "Amount" ]
                        th [] [ encodedText "Date" ]
                        th [] []
                        th [] []
                    ]
                ]
                tbody [] (model.Transactions |> List.map mapTransactionToTablerow)
            ]

    [
        noscript [] [
            a [ _href transferUrl; _class "button" ] [ encodedText "Transfer" ]
            a [ _href addUrl; _class "button" ] [ encodedText "Add Transaction" ]
        ]
        label [ _for "modal1"; _hx_get transferUrl; _hx_target "#modalBody"; _class "button js-on" ] [ encodedText "Transfer"]
        label [ _for "modal1"; _hx_get addUrl; _hx_target "#modalBody"; _class "button js-on" ] [ encodedText "Add Transaction"]
        div [ _style "height: 1em;" ] []
        select [ _value (currentPageNumber.ToString()); _onchange "OnPageNumSelectChange(this)"; _class "js-on" ] optionsList
        tableOrDiv
        noscript [] anchorList

        genericHTMXModal
    ]
    |> layout true "Transactions"

let private addOrUpdateTransactionViewPartial (operation: string) (submitPath: string) (eid : uint16) antiForgeryNode (model: Models.Transaction) =

    let addAgain =
        if addOperationText = operation then
            label [] [ input [ _type "checkbox"; _name "AddAgain"; ]; span [ _class "checkable" ] [ encodedText "Add Again" ] ]
        else
            span [] []

    article [] [
        header [] [ encodedText operation ]
        section [] [
            form [ formMethodPost; _id "mainform"; _hx_post submitPath; _hx_target "#modalBody"; _action submitPath ] [
                input [ _type "hidden"; _name "TransactionNumber"; _value "0" ]
                label [ _for "Amount"; ] [ encodedText "Amount" ]
                input [ _name "Amount"; _type "number"; _step "0.01"; _required; _value (if model.Amount = 0.0 then "" else model.Amount.ToString()) ]
                label [ _for "Date"; ] [ encodedText "Date" ]
                input [ _name "Date"; _type "date"; _required; _value (if model.Date = System.DateTime.MinValue then "" else model.Date.ToString("yyyy-MM-dd")) ]
                label [ _for "Note" ] [ encodedText "Note" ]
                input [ _name "Note"; _type "text"; _value model.Note; _required ]
                addAgain
                antiForgeryNode
            ]
        ]
        footer [] [
            input [ _type "submit"; _form "mainform" ]
            a [ _href ( sprintf "/Transaction/%i" eid); _class "button dangerous" ] [ encodedText "Cancel" ]
        ]
    ]

let private addOrUpdateTransactionViewFull (operation: string) (submitPath: string) (eid : uint16) antiForgeryNode (model: Models.Transaction) =
    let operationTitle = operation + " Transaction"
    [ addOrUpdateTransactionViewPartial operation submitPath eid antiForgeryNode model ]
    |> layout true operationTitle

let addTransactionViewPartial = addOrUpdateTransactionViewPartial addOperationText
let addTransactionView = addOrUpdateTransactionViewFull addOperationText

let updateTransactionViewPartial = addOrUpdateTransactionViewPartial updateOperationText
let updateTransactionViewFull = addOrUpdateTransactionViewFull updateOperationText

let deleteTransactionViewPartial (path: string) antiforgeryNode (model: Models.Transaction) =
    let deleteMessage = 
        sprintf "Are you sure you want to delete %s - %s - %s?" (model.Amount.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)) (model.Date.ToString("yyyy-MM-dd")) model.Note

    article [] [
        header [] [ h3 [] [ encodedText "Delete?" ] ]
        section [] [
            form [ formMethodPost; _action path; _id "mainform"; _hx_post path; _hx_target "#modalBody" ] [
                p [] [ encodedText deleteMessage ]
                label [] [ input [ _type "radio"; _name "YesNo"; _value "Yes"; _required ]; span [ _class "checkable" ] [ encodedText "Yes"] ]
                label [] [ input [ _type "radio"; _name "YesNo"; _value "No"; _required ]; span [ _class "checkable" ] [ encodedText "No"] ]
                antiforgeryNode
            ]
        ]
        footer [] [
            input [ _type "submit"; _form "mainform" ]
        ]
    ]

let deleteTransactionViewFull (path: string) antiforgeryNode (model: Models.Transaction) =
    [ deleteTransactionViewPartial path antiforgeryNode model ]
    |> layout true "Delete Transaction"

let transferViewPartial (path: string) antiforgeryNode (envelopeList: Models.Envelope list) (model: Models.Transfer) =
    
    let mapRazorListToGiraffeList (listItem: Models.Envelope) =
        option [ _value (listItem.Number.ToString()); ] [ encodedText (sprintf "%s - %s" listItem.Name (listItem.Amount.ToString("F2")) ) ]

    article [] [
        header [] [ h3 [] [ encodedText "Transfer" ] ]
        section [] [
            form [ formMethodPost; _id "mainform"; _action path; _hx_post path; _hx_target "#modalBody" ] [
                label [ _for "DestinationNumber" ] [ encodedText "Transfer To" ]
                select [ _name "DestinationNumber"; _required ] ( envelopeList |> List.map mapRazorListToGiraffeList )
                label [ _for "Amount"; ] [ encodedText "Amount" ]
                input [ _type "number"; _name "Amount"; _step "0.01"; _value (if model.Amount = 0.0 then "" else (model.Amount.ToString("F2"))); _required; ]
                antiforgeryNode
            ]
        ]
        footer [] [
            input [ _type "submit"; _form "mainform" ]
            label [ _for "modal1"; _class "button dangerous js-on"; ] [ encodedText "Cancel" ]
        ]
    ]

let transferViewFull (path: string) antiforgeryNode (envelopeList: Models.Envelope list) (model: Models.Transfer) =
    [ transferViewPartial path antiforgeryNode envelopeList model ]
    |> layout true "Transfer Amount"
