module Models

type ErrorViewModel =
    { RequestId: string }

    member this.ShowRequestId = not (System.String.IsNullOrEmpty(this.RequestId))

[<CLIMutable>]
type Envelope = { Number: int16; Name: string; Amount: double }

[<CLIMutable>]
type EnvelopeName = { EnvelopeName: string }

[<CLIMutable>]
type Login = { Email: string; Password: string; }

type LoginSignUpErrors = { EmailError: string; PasswordError: string; ConfirmPasswordError: string }

[<CLIMutable>]
type PasswordConfig = { MiB: int16; Iterations: int16; DegreeOfParallism: int16; Salt: byte array }

[<CLIMutable>]
type SignUp = { Email: string; Password: string; ConfirmPassword: string; }

[<CLIMutable>]
type Transaction = { TransactionNumber: int; Amount: double; [<CsvHelper.Configuration.Attributes.FormatAttribute("yyyy-MM-dd")>]Date: System.DateTime; Note: string; }

[<CLIMutable>]
type Transfer = { DestinationNumber: int16; Amount: double; }

[<CLIMutable>]
type Sel_Transactions_Result = { NumberOfAllTransactions: int; Transactions: Transaction list; EnvelopeName: string }

[<CLIMutable>]
type User = { Id: int; PasswordHash: byte array; LockoutExpiry: System.DateTime; PasswordConfig: PasswordConfig; }

[<CLIMutable>]
type YesNo = { YesNo: string }
