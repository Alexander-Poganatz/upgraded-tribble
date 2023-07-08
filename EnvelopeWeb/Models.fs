module Models

open System.ComponentModel.DataAnnotations
open Microsoft.AspNetCore.Mvc.Rendering

type ErrorViewModel =
    { RequestId: string }

    member this.ShowRequestId = not (System.String.IsNullOrEmpty(this.RequestId))

[<CLIMutable>]
type Envelope = { Number: uint16; Name: string; Amount: double }

[<CLIMutable>]
type Login = { [<Required(AllowEmptyStrings = false)>]Email: string; [<Required(AllowEmptyStrings = false)>] Password: string; }

[<CLIMutable>]
type PasswordConfig = { MiB: byte; Iterations: byte; DegreeOfParallism: byte; Salt: byte array }

[<CLIMutable>]
type SignUp = { 
    [<Required(AllowEmptyStrings = false)>]Email: string; 
    [<Required(AllowEmptyStrings = false)>]Password: string; 
    [<Required(AllowEmptyStrings = false)>]ConfirmPassword: string; }

[<CLIMutable>]
type Transaction = { TransactionNumber: uint; Amount: double; Date: System.DateTime; Note: string; }

[<CLIMutable>]
type Transfer = { DestinationNumber: System.Nullable<uint16>; Envelopes: SelectListItem list; Amount: double; }

[<CLIMutable>]
type Sel_Transactions_Result = { NumberOfAllTransactions: uint; Transactions: Transaction list}

[<CLIMutable>]
type User = { Id: uint; PasswordHash: byte array; LockoutExpiry: System.DateTime; PasswordConfig: PasswordConfig; }
